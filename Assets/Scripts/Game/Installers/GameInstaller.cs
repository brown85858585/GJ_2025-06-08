using System;
using System.Threading;
using CameraField;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Effects.PostProcess;
using Game.Intertitles;
using Game.Levels;
using Game.MiniGames.Flower;
using Game.Monolog;
using Game.Quests;
using Player;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Game.Installers
{
    public class GameInstaller : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject virtualCameraPrefab;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private QuestLogView questLogPrefab;
        [SerializeField] private EffectAccumulatorView effectAccumulator;
        [SerializeField] private MainCanvasUi mainCanvasPrefab;
        [SerializeField] private MiniGamePrefabAccumulator miniGamePrefabAccumulator;
        
        [SerializeField] private LevelsConfig config;
        [SerializeField] private IntertitleConfig intertitleConfig;

        private CoreInstaller _core;
        private GameLogicInstaller _logic;
        private bool _allQuestsCompleted;
        
        private LevelManager _levelManager;
        private QuestLogView _questsView;
        private GameObject _virtualCamera;
        private EffectAccumulatorView _effectAccumulator;
        private ScoreView _scoreText;

        private GameObject _savedCan;
        private MainCanvasUi _mainCanvas;
        private MonologSystem _monologSystem;
        private ScenarioInstaller _scenario;
        private CinemachineVirtualCamera _vCam;

        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _core = new CoreInstaller(Instantiate(playerInput), intertitleConfig);
            _logic = new GameLogicInstaller(_core, miniGamePrefabAccumulator);
            
            _levelManager = new LevelManager(config, _core.InteractionSystem, _logic.MiniGameCoordinator);
        }

        private void Start()
        {  
            _scenario = Instantiate(Resources.Load<ScenarioInstaller>("Prefabs/ScenarioInstaller"));
            
            _effectAccumulator = Instantiate(effectAccumulator, transform.parent);
            _effectAccumulator.FadeOut(0, PostEffectProfile.Day0);
            InitLevelOne();
            mainCanvasPrefab.LanguageSelector.ApplySavedLanguage();

            _core.InputAdapter.SwitchAdapterToMiniGameMode();
            _core.IntertitleSystem.ShowIntertitle(_levelManager.CurrentLevelIndex, CancellationToken.None).
                ContinueWith(() =>
                {
                    _effectAccumulator.FadeIn(1);
                    
                    InitializeMainObjects();
                });
        }

        private void InitializeMainObjects()
        {
            _mainCanvas = Instantiate(mainCanvasPrefab, transform.parent);
            _mainCanvas.NextLevelButton.onClick.AddListener(LoadNextLevel);
            _scoreText = _mainCanvas.GetComponentInChildren<ScoreView>();
            _core.PlayerModel.CurrentScore.Subscribe(newScore => _scoreText.Score = newScore)
                .AddTo(this);
            
            InitPlayer();
            InitStartWakeUp();
            InitCamera();
            InitQuestLog();
            _monologSystem = new MonologSystem(_core.InteractionSystem, _logic.PlayerController, _levelManager,
                _logic.MiniGameCoordinator, _logic.QuestLog);
            
            _scenario.Initialize(
                _logic.PlayerController, 
                _core.InputAdapter, 
                _monologSystem,
                _levelManager, 
                _effectAccumulator,
                _core.IntertitleSystem,
                _vCam,
                LoadNextLevel);
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
            _logic.PlayerController.Init(playerView, _levelManager);
            _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
            _levelManager.OnNextLevelLoaded += _logic.PlayerController.SwitchHead;
        }

        private void InitStartWakeUp()
        {
            _logic.PlayerController.PlayWakeUpAnimation(2f);
        }

        private void InitCamera()
        {
            _virtualCamera = Instantiate(virtualCameraPrefab, transform.parent);
            _vCam = _virtualCamera.GetComponent<CinemachineVirtualCamera>();
            _vCam.Follow = _core.PlayerModel.PlayerTransform;
            _vCam.LookAt = _core.PlayerModel.PlayerTransform;
            

            var cameraDependence = _virtualCamera.GetComponent<CameraDependence>();
            var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(cameraDependence.Cam);
            
            var cameraRotation = _virtualCamera.GetComponent<CameraRotation>();
            cameraRotation.Initialization(_core.InputAdapter, _virtualCamera.transform);
            var cameraZoom = _virtualCamera.GetComponent<SmoothZoomController>();
            cameraZoom.Initialization(_core.InputAdapter, _vCam);
            
            _logic.PlayerController.CamTransform = _virtualCamera.transform;
            KeepConstantScreenSize[] allComponents = FindObjectsOfType<KeepConstantScreenSize>(true); // true - ������� ����������

            foreach (var component in allComponents)
            {
                component.InitializeFromSceneStart();
            }

            _savedCan = allComponents[0].gameObject;
            _logic.MiniGameCoordinator.SetWateringCanView(_savedCan);
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
            //todo ! for debug
            if (category == ItemCategory.Bed && _allQuestsCompleted )
            {
                _scenario.NextLevelScenario().Forget();
            }
        }

        private void LoadNextLevel()
        {
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _levelManager.LoadNextLevel(transform.parent);
            
                _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
                if(_levelManager.CurrentLevelIndex != 7)
                { 
                    // Play wake up animation only if not the last level
                    _logic.PlayerController.PlayWakeUpAnimation(1);
                }
                else
                { 
                    _core.InputAdapter.SwitchAdapterToGlobalMode();
                }
                _logic.QuestLog.ResetQuests();
                _logic.QuestLog.Initialization(_questsView, _logic.MiniGameCoordinator);
                _allQuestsCompleted = false;
                
                _logic.MiniGameCoordinator.SetWateringCanView(_savedCan);
                
                _effectAccumulator.SetWeather(_levelManager.CurrentLevelIndex+1);

                _effectAccumulator.FadeIn(1, 
                    (PostEffectProfile)Enum.Parse(typeof(PostEffectProfile), $"Day{_levelManager.CurrentLevelIndex}"));
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