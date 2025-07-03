using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class MainCanvasUi : MonoBehaviour
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private LanguageSelector languageSelector;

        [SerializeField] private Button btnHelp;
        [SerializeField] private Image helpButtonImage;
        [SerializeField] private TextMeshProUGUI helpButtonText;
        [SerializeField] private GameObject hintTextPanel;

        [SerializeField] private Color inactiveNormalColor = new Color(0.627f, 0.427f, 0.220f);
        [SerializeField] private Color activeNormalColor = new Color(0.467f, 0.345f, 0.235f);
        [SerializeField] private Color highlightedColor = Color.white; // Цвет при наведении
        public Button NextLevelButton => nextLevelButton;
        public LanguageSelector LanguageSelector => languageSelector;

        private bool isHelpButtonActive = false;
        private void Start()
        {
            btnHelp.onClick.AddListener(OnButtonHelpClicked);
        }
        private void OnDestroy()
        {
            btnHelp.onClick.RemoveListener(OnButtonHelpClicked);
        }

        private void OnButtonHelpClicked()
        {
            isHelpButtonActive = !isHelpButtonActive;

            // Получаем текущие цвета
            ColorBlock cb = btnHelp.colors;

            // Устанавливаем нужные цвета
            cb.normalColor = isHelpButtonActive ? activeNormalColor : inactiveNormalColor;
            cb.highlightedColor = highlightedColor;
            cb.pressedColor = cb.normalColor * 0.8f;

            btnHelp.colors = cb;

            // Меняем цвет текста
            helpButtonText.color = isHelpButtonActive ? Color.white : Color.black;

            // Управление панелью
            hintTextPanel.SetActive(isHelpButtonActive);
        }
    }
}
