using System;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class MainCanvasUi : MonoBehaviour
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private LanguageSelector languageSelector;
        
        public Button NextLevelButton => nextLevelButton;
        public LanguageSelector LanguageSelector => languageSelector;
    }
}