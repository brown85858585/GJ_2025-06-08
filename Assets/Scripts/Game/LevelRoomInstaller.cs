using CameraField;
using Cinemachine;
using Game.Interactions;
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
        [SerializeField] private GameObject firstLevelPrefab;
        [SerializeField] private GameObject secondLevelPrefab;

        private PlayerModel _playerModel;
        private PlayerController _playerController;
        private InputAdapter _inputAdapter;
        private InteractionSystem _interactionSystem;
        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _playerModel = new PlayerModel(new CommonQuestModel(), new DayModel());
            _inputAdapter = new InputAdapter(playerInput);
            _playerController = new PlayerController(_playerModel, _inputAdapter, virtualCamera.transform);
            
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
            
            var firstLevel =Instantiate(firstLevelPrefab, transform);
            
            var interactibles = firstLevel.GetComponentInChildren<InteractionItemCollection>();
            _interactionSystem = new InteractionSystem(interactibles, _inputAdapter);
            _interactionSystem.OnInteraction += _playerController.HandleInteraction;
        }

        private void PlayerInit()
        {
            var go = Instantiate(playerPrefab);
            _playerModel.PlayerTransform = go.transform;
            
            var component = go.GetComponent<PlayerView>();
            _playerController.InitView(component);
        }

        private void CameraInit()
        {
            virtualCamera.Follow = _playerModel.PlayerTransform;
            virtualCamera.LookAt = _playerModel.PlayerTransform;

            var cameraRotation = virtualCamera.gameObject.AddComponent<CameraRotation>();
            cameraRotation.Initialization(_inputAdapter, virtualCamera.transform);
        }
    }
}