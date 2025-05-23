using System;
using UnityEngine;

namespace MainMenu
{
    public class MainMenuNavigator : IDisposable
    {
        private readonly MainMenu _mainMenu;
        private readonly CharacterSelect.CharacterSelectView _characterSelectView;
        private bool _isDisposed;

        public MainMenuNavigator(MainMenu mainMenu, CharacterSelect.CharacterSelectView characterSelectView)
        {
            _mainMenu = mainMenu;
            _characterSelectView = characterSelectView;

            _mainMenu.OnPlayClick += HandlePlayClicked;
            _mainMenu.OnSettingsClick += HandleSettingClicked;
            _mainMenu.OnCharacterSelectClick += HandleCharacterSelectClicked;
            _mainMenu.OnQuitClick += HandleQuitClicked;

            _characterSelectView.OnBackClicked += HandleBackCharacterSelectView;
        }

        private void HandleCharacterSelectClicked()
        {
            _mainMenu.gameObject.SetActive(false);
            _characterSelectView.gameObject.SetActive(true);
        }

        private void HandleBackCharacterSelectView()
        {
            _characterSelectView.gameObject.SetActive(false);
            _mainMenu.gameObject.SetActive(true);
        }

        private void HandlePlayClicked()
        {
            Debug.Log("Play button clicked");
        }

        private void HandleSettingClicked()
        {
            Debug.Log("Settings button clicked");
        }

        private void HandleQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _mainMenu.OnPlayClick -= HandlePlayClicked;
            _mainMenu.OnSettingsClick -= HandleSettingClicked;
            _mainMenu.OnCharacterSelectClick -= HandleCharacterSelectClicked;
            _mainMenu.OnQuitClick -= HandleQuitClicked;

            _characterSelectView.OnBackClicked -= HandleBackCharacterSelectView;
        }
    }
}