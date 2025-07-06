using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Effects.PostProcess;
using Game.Intertitles;
using Game.Levels;
using Game.Monolog;
using Player;
using Scenario;
using UnityEngine;

namespace Game.Installers
{
    public class ScenarioInstaller : MonoBehaviour
    {
        private IPlayerController _playerController;
        private InputAdapter _inputAdapter;
        private MonologSystem _monologSystem;
        private LevelManager _levelManager;
        private EffectAccumulatorView _effectAccumulator;
        private IntertitleSystem _intertitleSystem;
        private CinemachineVirtualCamera _virtualCamera;
        private GameOverScenario _gameOverScenario;

        public IPlayerController PlayerController => _playerController;
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;
        public InputAdapter InputAdapter => _inputAdapter;
        private event Action ActionNextLevel;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(PlayerController logicPlayerController,
            InputAdapter coreInputAdapter,
            MonologSystem monologSystem, LevelManager levelManager,
            EffectAccumulatorView effectAccumulator,
            IntertitleSystem intertitleSystem,
            CinemachineVirtualCamera virtualCamera,
            Action loadNextLevel)
        {
            _playerController = logicPlayerController;
            _inputAdapter = coreInputAdapter;
            _monologSystem = monologSystem;
            _levelManager = levelManager;
            _effectAccumulator = effectAccumulator;
            _intertitleSystem = intertitleSystem;
            _virtualCamera = virtualCamera;
            ActionNextLevel += loadNextLevel;

            _gameOverScenario = new GameOverScenario(intertitleSystem);
        }

        public async UniTask NextLevelScenario()
        {
            _inputAdapter.SwitchAdapterToMiniGameMode();
            
            _monologSystem.TryOpenDialogue($"Day{_levelManager.CurrentLevelIndex + 1}_Sleep");
            await UniTask.Delay(1000);
            _monologSystem.CloseDialogue();
            var fadeTimer = 1100;
            _effectAccumulator.FadeOut(fadeTimer/1000f);
            await UniTask.Delay(fadeTimer);
            await _intertitleSystem.ShowScoreIntertitle(_levelManager.CurrentLevelIndex,
                _playerController.Model,
                CancellationToken.None);
            if(_playerController.Model.Score < 0)
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
            (_playerController as PlayerController)?.ResetPlayer();
        }
        
        private void OnDestroy()
        {
            ActionNextLevel = null;
        }
    }
}