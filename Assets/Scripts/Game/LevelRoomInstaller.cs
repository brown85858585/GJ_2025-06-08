using Game.Models;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Game
{
    public class LevelRoomInstaller :  MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GameObject secondLevelPrefab;

        private PlayerModel _playerModel;
        private PlayerController _playerController;

        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _playerModel = new PlayerModel(new CommonQuestModel(), new DayModel());
            var inputAdapter = new InputAdapter(playerInput);
            _playerController = new PlayerController(_playerModel, inputAdapter);

            _playerController.OnDied += SecondLevel;
        }

        private void SecondLevel()
        {
            _playerController.OnDied -= SecondLevel;
            var go = Instantiate(secondLevelPrefab, transform);
               
            var secondInstaller = go.GetComponent<LevelSecondInstaller>();
            secondInstaller.Initialize(_playerController, _playerModel);
        }

        void Start()
        {
            var go = Instantiate(playerPrefab);
            var view = go.GetComponent<PlayerMovement>();
            
            _playerController.Initialize(view);
            
            view.Initialize(_playerController);
        }

    }
}