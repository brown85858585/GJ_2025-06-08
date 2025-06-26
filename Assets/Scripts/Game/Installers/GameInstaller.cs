using CameraField;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Effects;
using Game.Levels;
using Game.Monolog;
using Game.Quests;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Game.Installers
{
    public class GameInstaller : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject virtualCameraPrefab;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private QuestLogView questLogPrefab;
        [SerializeField] private EffectAccumulatorView effectAccumulator;
        [SerializeField] private Button nextLevelButton;
        
        [SerializeField] private LevelsConfig config;

        private CoreInstaller _core;
        private GameLogicInstaller _logic;
        private bool _allQuestsCompleted;
        
        private LevelManager _levelManager;
        private QuestLogView _questsView;
        private GameObject _virtualCamera;
        private EffectAccumulatorView _effectAccumulator;

        private void Awake()
        {
            Install();
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }

        private void Install()
        {
            _core = new CoreInstaller(playerInput);
            _logic = new GameLogicInstaller(_core);
            
            _levelManager = new LevelManager(config, _core.InteractionSystem, _logic.MiniGameCoordinator);
        }

        private void Start()
        {
            _effectAccumulator = Instantiate(effectAccumulator, transform.parent);
            _effectAccumulator.FadeOut();
            
            Instantiate(playerInput);
            
            InitLevelOne();
            InitPlayer();
            InitCamera();
            InitQuestLog();
            var monologSystem = new MonologSystem(_core.InteractionSystem, _logic.PlayerController, _levelManager);

        }

        private void InitLevelOne()
        {
            _levelManager.LoadLevel(0, transform?.parent, _effectAccumulator);
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
            _virtualCamera = Instantiate(virtualCameraPrefab, transform.parent);
            var vCam = _virtualCamera.GetComponent<CinemachineVirtualCamera>();
            vCam.Follow = _core.PlayerModel.PlayerTransform;
            vCam.LookAt = _core.PlayerModel.PlayerTransform;
            

            var cameraDependence = _virtualCamera.GetComponent<CameraDependence>();
            var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(cameraDependence.Cam);
            
            var cameraRotation = _virtualCamera.GetComponent<CameraRotation>();
            cameraRotation.Initialization(_core.InputAdapter, _virtualCamera.transform);
            var cameraZoom = _virtualCamera.GetComponent<SmoothZoomController>();
            cameraZoom.Initialization(_core.InputAdapter, vCam);
            
            _logic.PlayerController.CamTransform = _virtualCamera.transform;
        }

        private void InitQuestLog()
        {
            _questsView = Instantiate(questLogPrefab, transform);
            _logic.QuestLog.Initialization(_questsView, _logic.MiniGameCoordinator);
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
            if (category == ItemCategory.Bed && _allQuestsCompleted)
                LoadNextLevel();
        }

        private void LoadNextLevel()
        {
            _effectAccumulator.FadeIn();
           
            
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _levelManager.LoadNextLevel(transform.parent);
            
                _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
                _logic.QuestLog.ResetQuests();
                _logic.QuestLog.Initialization(_questsView, _logic.MiniGameCoordinator);
                _allQuestsCompleted = false;
                
                _effectAccumulator.SetWeather(_levelManager.CurrentLevelIndex+1);
                _effectAccumulator.FadeOut();
            });
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