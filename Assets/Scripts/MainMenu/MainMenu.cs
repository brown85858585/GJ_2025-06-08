using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class MainMenu : UIElementTweener
    {
        [SerializeField] private List<GameObject> buttonPrefabs;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private UIElementTweener labelTweener;

        private readonly List<UIElementTweener> _buttonTweens = new();

        private Action _playButtonsHandler;
        private readonly List<Action> _peakHandlers = new();

        private Dictionary<MenuBtnType, Button> MenuButtonsByType { get; } = new();

        public event Action OnSettingsClick = delegate { };
        public event Action OnPlayClick = delegate { };
        public event Action OnCharacterSelectClick = delegate { };
        public event Action OnQuitClick = delegate { };

        protected void Awake()
        {
            InstantiateButtons();

            _playButtonsHandler = PlayButtons;
            OnShowComplete += _playButtonsHandler;
        }

        private void OnEnable()
        {
            Show();
            labelTweener.Show();
        }

        private void OnDisable()
        {
            Hide();
            labelTweener.Hide();
            _buttonTweens.ForEach(tweener => tweener.Hide());
        }

        private void InstantiateButtons()
        {
            foreach (var buttonPrefab in buttonPrefabs)
            {
                GameObject button = Instantiate(buttonPrefab, buttonContainer);
                var tween = button.GetComponent<UIElementTweener>();
                tween.transform.localScale = Vector3.zero;
                SaveAndListenerButton(button);

                if (tween == null)
                {
                    Debug.LogWarning($"Prefab '{buttonPrefab.name}' does not have a UIElementTweener component.");
                    continue;
                }

                _buttonTweens.Add(tween);
            }
        }

        private void SaveAndListenerButton(GameObject button)
        {
            var menuButton = button.GetComponent<MenuButton>();
            if (menuButton == null)
            {
                Debug.LogWarning($"Button '{button.name}' does not have a MenuButton component.");
                return;
            }

            MenuButtonsByType.Add(menuButton.ButtonType, menuButton.Button);

            switch (menuButton.ButtonType)
            {
                case MenuBtnType.Play:
                    menuButton.Button.onClick.AddListener(() => { OnPlayClick?.Invoke(); });
                    break;
                case MenuBtnType.Settings:
                    menuButton.Button.onClick.AddListener(() => { OnSettingsClick?.Invoke(); });
                    break;
                case MenuBtnType.CharacterSelect:
                    menuButton.Button.onClick.AddListener(() => { OnCharacterSelectClick?.Invoke(); });
                    break;
                case MenuBtnType.Quit:
                    menuButton.Button.onClick.AddListener(() => { OnQuitClick?.Invoke(); });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayButtons()
        {
            if (_buttonTweens.Count == 0) return;

            _buttonTweens[0].Show();

            for (int i = 1; i < _buttonTweens.Count; i++)
            {
                int index = i;

                Action peakHandler = null;
                peakHandler = () =>
                {
                    _buttonTweens[index].Show();
                    _buttonTweens[index - 1].OnShowPeak -= peakHandler;
                };

                _buttonTweens[index - 1].OnShowPeak += peakHandler;
                _peakHandlers.Add(peakHandler);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        
            if (_playButtonsHandler != null)
                OnShowComplete -= _playButtonsHandler;
        
            for (int i = 1; i < _buttonTweens.Count && i - 1 < _peakHandlers.Count; i++)
            {
                _buttonTweens[i - 1].OnShowPeak -= _peakHandlers[i - 1];
            }
        
            _peakHandlers.Clear();
        
            foreach (var button in MenuButtonsByType.Values)
            {
                button.onClick.RemoveAllListeners();
            }
        
            MenuButtonsByType.Clear();
            _buttonTweens.Clear();
        }
    }
}