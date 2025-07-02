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
        
        private MenuStateType _menuStateType;
        public MenuStateType State => _menuStateType;

        public MainMenuNavigator(MainMenu mainMenu, SettingsMenuView settingsMenuView)
        {
            _mainMenu = mainMenu;
            _settings = settingsMenuView;

            _mainMenu.OnPlayClick += HandlePlayClicked;
            _mainMenu.OnSettingsClick += HandleSettingClicked;
            _mainMenu.OnQuitClick += HandleQuitClicked;
            
            settingsMenuView.SettingsBackButton.onClick.AddListener(HandleSettingsBack);
            
            _menuStateType = MenuStateType.MainMenu;
        }

        private void HandleSettingsBack()
        {
            _settings.gameObject.SetActive(false);
            _mainMenu.gameObject.SetActive(true);
            _menuStateType = MenuStateType.MainMenu;
        }

        private void HandlePlayClicked()
        {
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            _menuStateType = MenuStateType.None;
           // LoadScene("MainMenu");
        }

        private void HandleSettingClicked()
        {
            _mainMenu.gameObject.SetActive(false);
            _settings.gameObject.SetActive(true);
            _menuStateType = MenuStateType.SettingsMenu;
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

    public enum MenuStateType
    {
        None,
        MainMenu,
        SettingsMenu,
    }
}