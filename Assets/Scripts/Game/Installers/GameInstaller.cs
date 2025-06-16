using CameraField;
using Cinemachine;
using Game.Levels;
using Game.Quests;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class GameInstaller : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject virtualCameraObj;
        [SerializeField] private PlayerInput playerInput;
        // [SerializeField] private GameObject firstLevelPrefab;
        // [SerializeField] private GameObject secondLevelPrefab;
        [SerializeField] private QuestLogView questLogPrefab;
        
        [SerializeField] private LevelsConfig config;

        private CoreInstaller _core;
        private GameLogicInstaller _logic;
        // private GameObject _currentLevel;
        // private RoomView _roomView;
        private bool _allQuestsCompleted;
        
        private LevelManager _levelManager;

        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _core = new CoreInstaller(playerInput);
            _logic = new GameLogicInstaller(_core, virtualCameraObj.transform);
            
            _levelManager = new LevelManager(config, _core.InteractionSystem, _logic.MiniGameCoordinator);
        }

        private void Start()
        {
            InitLevelOne();
            InitPlayer();
            InitCamera();
            // InitInteractions();
            // InitMiniGames();
            InitQuestLog();
        }

        private void InitLevelOne()
        {
            // _currentLevel = Instantiate(firstLevelPrefab, transform);
            // _roomView = _currentLevel.GetComponentInChildren<RoomView>();
            
            _levelManager.LoadLevel(0, transform.parent);
            _core.InteractionSystem.OnInteraction += HandlePlayerInteraction;
        }

        private void InitPlayer()
        {
            var playerObj = Instantiate(playerPrefab);
            _core.PlayerModel.PlayerTransform = playerObj.transform;
            var playerView = playerObj.GetComponent<PlayerView>();
            _logic.PlayerController.InitView(playerView);
            _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
        }

        private void InitCamera()
        {
            var vCamObj = Instantiate(virtualCameraObj, transform.parent);
            var vCam = vCamObj.GetComponent<CinemachineVirtualCamera>();
            vCam.Follow = _core.PlayerModel.PlayerTransform;
            vCam.LookAt = _core.PlayerModel.PlayerTransform;
            var cameraRotation = vCamObj.AddComponent<CameraRotation>();
            cameraRotation.Initialization(_core.InputAdapter, vCamObj.transform);
        }

        private void InitInteractions()
        {
            var system = _core.InteractionSystem;
            system.AddNewInteractionCollection(_levelManager.CurrentRoomView);
            system.OnInteraction += HandlePlayerInteraction;
        }

        // private void InitMiniGames()
        // {
        //     _logic.MiniGameCoordinator.RegisterGames(_levelManager.CurrentRoomView.transform);
        // }

        private void InitQuestLog()
        {
            var questsView = Instantiate(questLogPrefab, transform);
            _logic.QuestLog.Initialization(questsView, _logic.MiniGameCoordinator);
            _logic.QuestLog.AllQuestsCompleted += () => _allQuestsCompleted = true;
            _core.InteractionSystem.OnInteraction += HandleLevelCompletion;
        }

        private void HandlePlayerInteraction(ItemCategory item)
        {
            var interactable = _core.InteractionSystem.CurrentInteractable;
            _logic.PlayerController.HandleInteraction(item, interactable.transform);
        }

        private void HandleLevelCompletion(ItemCategory category)
        {
            if (category == ItemCategory.Bed && !_allQuestsCompleted)
                LoadSecondLevel();
        }

        private void LoadSecondLevel()
        {
            _levelManager.LoadNextLevel(transform.parent);
            _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
            // var next = Instantiate(secondLevelPrefab, transform);
            // var installer = _levelManager.CurrentRoomView.GetComponent<LevelSecondInstaller>();
            // installer.Initialize(_logic.PlayerController, _core.PlayerModel);
            // Destroy(_currentLevel);
            // _currentLevel = next;
            // _roomView = _currentLevel.GetComponentInChildren<RoomView>();
            // _core.InteractionSystem.AddNewInteractionCollection(_levelManager.CurrentRoomView);
        }

        private void OnDestroy()
        {
            if (_core.InteractionSystem != null)
            {
                _core.InteractionSystem.OnInteraction -= HandlePlayerInteraction;
                _core.InteractionSystem.OnInteraction -= HandleLevelCompletion;
            }

            _core.InteractionSystem?.Dispose();
            _core.InputAdapter?.Dispose();
        }
    }
}