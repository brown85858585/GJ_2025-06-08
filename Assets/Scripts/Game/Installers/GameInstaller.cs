using System.Threading;
using CameraField;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Effects;
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
            _effectAccumulator = Instantiate(effectAccumulator, transform.parent);
            _effectAccumulator.FadeOut(0);
            InitLevelOne();
            mainCanvasPrefab.LanguageSelector.ApplySavedLanguage();

            _core.InputAdapter.SwitchAdapterToMiniGameMode();
            _core.IntertitleSystem.ShowIntertitle(_levelManager.CurrentLevelIndex, CancellationToken.None).ContinueWith(
                () =>
                {
                    _effectAccumulator.FadeIn(1,_core.InputAdapter.SwitchAdapterToGlobalMode);
                    
                    _core.InputAdapter.SwitchAdapterToGlobalMode();
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

        private void InitStartWakeUp()
        {
            _logic.PlayerController.PlayWakeUpAnimation();
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
            if (category == ItemCategory.Bed && !_allQuestsCompleted)
            {
                var scenario = new ScenarioInstaller();
                ScenarioNext().Forget();
                async UniTask ScenarioNext()
                {
                    
                    _core.InputAdapter.SwitchAdapterToMiniGameMode();
                    _monologSystem.OpenDialogue($"Day{_levelManager.CurrentLevelIndex + 1}_Sleep");
                    // Старт Анимации
                    
                    await UniTask.Delay(1000);
                    _monologSystem.CloseDialogue();
                    var fadeTimer = 1100;
                    _effectAccumulator.FadeOut(fadeTimer/1000f);
                    await UniTask.Delay(fadeTimer);
                    await _core.IntertitleSystem.ShowScoreIntertitle(_levelManager.CurrentLevelIndex,
                        _logic.PlayerController.Model,
                        CancellationToken.None);
                    await _core.IntertitleSystem.ShowIntertitle(_levelManager.CurrentLevelIndex+1,
                        CancellationToken.None);
                    
                    scenario.NextLevelScenario(LoadNextLevel);
                }
                
            }
        }

        private void LoadNextLevel()
        {
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _levelManager.LoadNextLevel(transform.parent);
            
                _logic.PlayerController.SetPosition(_levelManager.CurrentRoomView.StartPoint.position);
                _logic.QuestLog.ResetQuests();
                _logic.QuestLog.Initialization(_questsView, _logic.MiniGameCoordinator);
                _logic.MiniGameCoordinator.SetWateringCanView(_savedCan);
                _allQuestsCompleted = false;
                
                _effectAccumulator.SetWeather(_levelManager.CurrentLevelIndex+1);

                _effectAccumulator.FadeIn(-1, _core.InputAdapter.SwitchAdapterToGlobalMode);

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