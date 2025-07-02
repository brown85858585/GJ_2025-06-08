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
            
            languageDropdown.ClearOptions();
            var langs = KnotLocalization.Manager.Languages.ToList();
            var names = langs.Select(l => l.NativeName).ToList();
            languageDropdown.AddOptions(names);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            ApplySavedLanguage();
        }

        public void ApplySavedLanguage()
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                string savedCode = PlayerPrefs.GetString(PrefKey);
                int savedIndex = KnotLocalization.Manager.Languages.ToList().
                    FindIndex(l => l.CultureName  == savedCode);
                if (savedIndex >= 0)
                {
                    languageDropdown.value = savedIndex;
                    // загрузит язык внутри OnValueChanged
                }

                OnLanguageChanged(savedIndex);
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
        
        private void OnDestroy()
        {
            // Отписываемся от события, чтобы избежать утечек памяти
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }
    }
}