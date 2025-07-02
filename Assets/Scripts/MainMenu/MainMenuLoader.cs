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

        private void Awake()
        {
            _mainMenu = Instantiate(mainMenuPrefab, uiRoot).GetComponent<MainMenu>();
            
            _settingsMenuView = Instantiate(settingsPrefab, uiRoot).GetComponent<SettingsMenuView>();
            
            _navigator = new MainMenuNavigator(_mainMenu, _settingsMenuView);
        }
        private void Start()
        {
            _settingsMenuView.gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            _navigator?.Dispose();

            if (_mainMenu != null) Destroy(_mainMenu.gameObject);
            if (_settingsMenuView != null) Destroy(_settingsMenuView.gameObject);
        }
    }
}