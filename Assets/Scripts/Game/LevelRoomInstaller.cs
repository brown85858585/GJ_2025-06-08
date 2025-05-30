using CameraField;
using Cinemachine;
using Game.Models;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Game
{
    public class LevelRoomInstaller :  MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GameObject secondLevelPrefab;

        private PlayerModel _playerModel;
        private PlayerController _playerController;
        private PlayerMovementView _playerMovementView;
        private InputAdapter _inputAdapter;

        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _playerModel = new PlayerModel(new CommonQuestModel(), new DayModel());
            _inputAdapter = new InputAdapter(playerInput);
            _playerController = new PlayerController(_playerModel, _inputAdapter);

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
            PlayerInit();

            CameraInit();
        }

        private void PlayerInit()
        {
            var go = Instantiate(playerPrefab);
            _playerMovementView = go.GetComponent<PlayerMovementView>();
            
            _playerController.Initialize(_playerMovementView);
            _playerMovementView.Initialize(_playerController);
            
        }

        private void CameraInit()
        {
            virtualCamera.Follow = _playerMovementView.TransformPlayer;
            virtualCamera.LookAt = _playerMovementView.TransformPlayer;
            var cameraRotation = virtualCamera.gameObject.AddComponent<CameraRotation>();
            cameraRotation.Initialization(_inputAdapter, virtualCamera.transform);
        }
    }
}