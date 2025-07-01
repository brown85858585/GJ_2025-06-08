using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    public class MainMenuNavigator : IDisposable
    {
        private readonly MainMenu _mainMenu;
        private bool _isDisposed;
        private readonly SettingsMenuView _settings;

        public MainMenuNavigator(MainMenu mainMenu, SettingsMenuView settingsMenuView)
        {
            _mainMenu = mainMenu;
            _settings = settingsMenuView;

            _mainMenu.OnPlayClick += HandlePlayClicked;
            _mainMenu.OnSettingsClick += HandleSettingClicked;
            _mainMenu.OnQuitClick += HandleQuitClicked;
            
            settingsMenuView.SettingsBackButton.onClick.AddListener(HandleSettingsBack);

        }

        private void HandleSettingsBack()
        {
            _settings.gameObject.SetActive(false);
            _mainMenu.gameObject.SetActive(true);
        }

        private void HandlePlayClicked()
        {
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
           // LoadScene("MainMenu");
        }

        private void HandleSettingClicked()
        {
            _mainMenu.gameObject.SetActive(false);
            _settings.gameObject.SetActive(true);
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
            _mainMenu.OnQuitClick -= HandleQuitClicked;
            
            _settings.SettingsBackButton.onClick.RemoveListener(HandleSettingsBack);
        }
    }
}