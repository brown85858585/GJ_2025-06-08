using System;
using System.Linq;
using CharacterSelect;
using UnityEngine;

namespace MainMenu
{
    public class MainMenuLoader : MonoBehaviour
    {
        [SerializeField] private Transform uiRoot;
        [SerializeField] private GameObject mainMenuPrefab;
        [SerializeField] private GameObject settingsPrefab;

        private MainMenu _mainMenu;
        private SettingsMenuView _settingsMenuView;
        private MainMenuNavigator _navigator;
        
        private GameObject[] _objectsDisabledInGame;

        private bool _isGame;

        private void Awake()
        {
            _mainMenu = Instantiate(mainMenuPrefab, uiRoot).GetComponent<MainMenu>();
            
            _settingsMenuView = Instantiate(settingsPrefab, uiRoot).GetComponent<SettingsMenuView>();
            
            _navigator = new MainMenuNavigator(_mainMenu, _settingsMenuView);
            
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(uiRoot);
        }
        
        private void Start()
        {
            _settingsMenuView.gameObject.SetActive(false);

            _objectsDisabledInGame = GameObject.FindGameObjectsWithTag("NoInGame");
            _mainMenu.OnPlayClick += DestroyInGame;
        }

        private void Update()
        {
            if (!_isGame) return;
            
            if (Input.GetKeyDown(KeyCode.Escape) && _navigator.State != MenuStateType.SettingsMenu)
            {
                _mainMenu.gameObject.SetActive(!_mainMenu.gameObject.activeSelf);
            }
        }

        private void DestroyInGame()
        {
            _mainMenu.ButtonTweens.RemoveAt(0);
            _mainMenu.OnPlayClick += DestroyInGame;
            _isGame = true;
            
            if (_objectsDisabledInGame != null && _objectsDisabledInGame.Length > 0)
            {
                foreach (var bg in _objectsDisabledInGame)
                {
                    Destroy(bg.gameObject);
                }
            }
            _mainMenu.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _navigator?.Dispose();
            _mainMenu.OnPlayClick -= DestroyInGame;
            if (_mainMenu != null) Destroy(_mainMenu.gameObject);
            if (_settingsMenuView != null) Destroy(_settingsMenuView.gameObject);
        }
    }
}