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
        //private void OnButtonHelpClicked()
        //{
        //    isHelpButtonActive = !isHelpButtonActive;

        //    ColorBlock colors = btnHelp.colors;

        //    // Сохраняем текущий highlightedColor перед изменением
        //    Color savedHighlighted = colors.highlightedColor;

        //    // Меняем только normalColor
        //    colors.normalColor = isHelpButtonActive ? activeHelpButtonColor : inactiveHelpButtonColor;

        //    // Восстанавливаем исходный highlightedColor
        //    colors.highlightedColor = savedHighlighted;

        //    btnHelp.colors = colors;

        //    helpButtonText.color = isHelpButtonActive ? Color.white : Color.black;

        //    if (isHelpButtonActive)
        //        ShowTextPanel();
        //    else
        //        HideTextPanel();
        //}

        private void OnButtonHelpClicked()
        {
            isHelpButtonActive = !isHelpButtonActive;

            // Получаем текущие цвета
            ColorBlock cb = btnHelp.colors;

            // Устанавливаем нужные цвета
            cb.normalColor = isHelpButtonActive ? activeNormalColor : inactiveNormalColor;
            cb.highlightedColor = highlightedColor; // Фиксируем цвет наведения
            cb.pressedColor = cb.normalColor * 0.8f; // Автоматическое затемнение при нажатии

            // Важно! Сначала применить colors, потом менять другие параметры
            btnHelp.colors = cb;

            // Обновляем визуальное состояние кнопки
            btnHelp.GetComponent<Image>().color = cb.normalColor;

            // Меняем цвет текста
            helpButtonText.color = isHelpButtonActive ? Color.white : Color.black;

            // Управление панелью
            hintTextPanel.SetActive(isHelpButtonActive);
        }
        private void ShowTextPanel()
        {
            Debug.Log("ShowTextPanel");

            if (hintTextPanel == null)
            {
                Debug.Log("hintTextPanel not found!");
                return;
            }

            hintTextPanel.SetActive(true);
        }

        private void HideTextPanel()
        {
            Debug.Log("HideTextPanel");

            if (hintTextPanel == null)
            {
                Debug.Log("hintTextPanel not found!");
                return;
            }

            hintTextPanel.SetActive(false);
        }
    }
}
