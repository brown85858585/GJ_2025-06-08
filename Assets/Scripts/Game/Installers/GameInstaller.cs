using CameraField;
using Cinemachine;
using Game.Levels;
using Game.Quests;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Installers
{
    public class GameInstaller : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject virtualCameraObj;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private QuestLogView questLogPrefab;
        
        [SerializeField] private LevelsConfig config;

        private CoreInstaller _core;
        private GameLogicInstaller _logic;
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
            InitQuestLog();
        }

        private void InitLevelOne()
        {
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
                LoadNextLevel();
        }

        private void LoadNextLevel()
        {
            _levelManager.LoadNextLevel(transform.parent);
            _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
            _logic.QuestLog.ResetQuests();
            _allQuestsCompleted = false;
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