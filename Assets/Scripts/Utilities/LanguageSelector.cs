using System.Linq;
using Knot.Localization;
using TMPro;
using UnityEngine;

namespace Utilities
{
    public class LanguageSelector : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown languageDropdown;  // назначаем в инспекторе

        private const string PrefKey = "SelectedLanguageCode";
        private void Awake()
        {
            // на всякий случай, если забыли подвязать в инспекторе
            if (languageDropdown == null)
                languageDropdown = GetComponent<TMP_Dropdown>();
        }

        private void Start()
        {
            // 1) Выставляем опции дропдауна
            languageDropdown.ClearOptions();
            var langs = KnotLocalization.Manager.Languages.ToList();
            var names = langs.Select(l => l.NativeName).ToList();
            languageDropdown.AddOptions(names);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        
            if (PlayerPrefs.HasKey(PrefKey))
            {
                string savedCode = PlayerPrefs.GetString(PrefKey);
                int savedIndex = langs.FindIndex(l => l.CultureName  == savedCode);
                if (savedIndex >= 0)
                {
                    languageDropdown.value = savedIndex;
                    // загрузит язык внутри OnValueChanged
                    return;
                }
            }
        }

        private void OnLanguageChanged(int index)
        {
            var lang = KnotLocalization.Manager.Languages[index];
            // 1) Меняем на выбранный
            KnotLocalization.Manager.LoadLanguage(lang);

            // 2) Сохраняем выбор
            PlayerPrefs.SetString(PrefKey, lang.CultureName );
            PlayerPrefs.Save();
        }
    }
}