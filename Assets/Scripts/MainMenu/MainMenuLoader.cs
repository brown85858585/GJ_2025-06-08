using System.Linq;
using CharacterSelect;
using UnityEngine;

namespace MainMenu
{
    public class MainMenuLoader : MonoBehaviour
    {
        [SerializeField] private Transform uiRoot;
        [SerializeField] private GameObject mainMenuPrefab;
        [SerializeField] private GameObject characterSelectPrefab;
        [SerializeField] private CharacterData[] characters;

        private MainMenu _mainMenu;
        private CharacterSelectView _characterSelectView;
        private CharacterSelectPresenter _presenter;
        private MainMenuNavigator _navigator;

        private void Awake()
        {
            _mainMenu = Instantiate(mainMenuPrefab, uiRoot).GetComponent<MainMenu>();
            
            _characterSelectView = Instantiate(characterSelectPrefab, uiRoot).GetComponent<CharacterSelectView>();
            _characterSelectView.gameObject.SetActive(false);
            InitCharacterSelect();
            
            _navigator = new MainMenuNavigator(_mainMenu, _characterSelectView);
        }

        private void InitCharacterSelect()
        {
            var charModels = characters.Select(d => 
                new CharacterModel(d, Random.Range(1,10), Random.Range(30,150))).ToArray();
            var selectorModel = new SelectorModel(charModels);
            _presenter = new CharacterSelectPresenter(_characterSelectView, selectorModel);
        }

        private void OnDestroy()
        {
            _navigator?.Dispose();
            _presenter?.Dispose();

            if (_mainMenu != null) Destroy(_mainMenu.gameObject);
            if (_characterSelectView != null) Destroy(_characterSelectView.gameObject);
        }
    }
}