using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Effects.PostProcess;
using Game.Intertitles;
using Game.Levels;
using Game.MiniGames;
using Game.Monolog;
using Player;
using Scenario;
using UnityEngine;

namespace Game.Installers
{
    public class ScenarioInstaller : MonoBehaviour
    {
        private MonologSystem _monologSystem;
        private LevelManager _levelManager;
        private IntertitleSystem _intertitleSystem;
        private GameOverScenario _gameOverScenario;

        public EffectAccumulatorView EffectAccumulatorView { get; private set; }
        public MiniGameCoordinator MiniGameCoordinator { get; private set; }
        public IPlayerController PlayerController { get; private set; }
        public CinemachineVirtualCamera VirtualCamera { get; private set; }
        public InputAdapter InputAdapter { get; private set; }

        private event Action ActionNextLevel;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(MiniGameCoordinator miniGameCoordinator,
            PlayerController logicPlayerController,
            InputAdapter coreInputAdapter,
            MonologSystem monologSystem, LevelManager levelManager,
            EffectAccumulatorView effectAccumulator,
            IntertitleSystem intertitleSystem,
            CinemachineVirtualCamera virtualCamera,
            Action loadNextLevel)
        {
            MiniGameCoordinator = miniGameCoordinator;
            PlayerController = logicPlayerController;
            InputAdapter = coreInputAdapter;
            _monologSystem = monologSystem;
            _levelManager = levelManager;
            EffectAccumulatorView = effectAccumulator;
            _intertitleSystem = intertitleSystem;
            VirtualCamera = virtualCamera;
            ActionNextLevel += loadNextLevel;

            _gameOverScenario = new GameOverScenario(intertitleSystem);
        }

        public async UniTask NextLevelScenario()
        {
            InputAdapter.SwitchAdapterToMiniGameMode();
            
            _monologSystem.TryOpenDialogue($"Day{_levelManager.CurrentLevelIndex + 1}_Sleep");
            await UniTask.Delay(1000);
            _monologSystem.CloseDialogue();
            var fadeTimer = 1100;
            EffectAccumulatorView.FadeOut(fadeTimer/1000f);
            await UniTask.Delay(fadeTimer);
            await _intertitleSystem.ShowScoreIntertitle(_levelManager.CurrentLevelIndex,
                PlayerController.Model,
                CancellationToken.None);
            if(PlayerController.Model.Score < 0)
            {
                await _gameOverScenario.StartScenario();
            }
            await _intertitleSystem.ShowIntertitle(_levelManager.CurrentLevelIndex+1,
                CancellationToken.None);

            PlayerReset();
            
            
            ActionNextLevel?.Invoke();
        }

        public void PlayerReset()
        {
            (PlayerController as PlayerController)?.ResetPlayer();
        }
        
        private void OnDestroy()
        {
            ActionNextLevel = null;
        }
    }
}