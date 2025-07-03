using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Effects;
using Game.Intertitles;
using Game.Levels;
using Game.Monolog;
using Player;
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

        public IPlayerController PlayerController => _playerController;
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
            Action loadNextLevel)
        {
            _playerController = logicPlayerController;
            _inputAdapter = coreInputAdapter;
            _monologSystem = monologSystem;
            _levelManager = levelManager;
            _effectAccumulator = effectAccumulator;
            _intertitleSystem = intertitleSystem;
            ActionNextLevel += loadNextLevel;
        }

        public async UniTask NextLevelScenario()
        {
            _inputAdapter.SwitchAdapterToMiniGameMode();
            
            _monologSystem.TryOpenDialogue($"Day{_levelManager.CurrentLevelIndex + 1}_Sleep");
            // Старт Анимации
                    
            await UniTask.Delay(1000);
            _monologSystem.CloseDialogue();
            var fadeTimer = 1100;
            _effectAccumulator.FadeOut(fadeTimer/1000f);
            await UniTask.Delay(fadeTimer);
            await _intertitleSystem.ShowScoreIntertitle(_levelManager.CurrentLevelIndex,
                _playerController.Model,
                CancellationToken.None);
            await _intertitleSystem.ShowIntertitle(_levelManager.CurrentLevelIndex+1,
                CancellationToken.None);
            
            
            ActionNextLevel?.Invoke();
        }

        private void OnDestroy()
        {
            ActionNextLevel = null;
        }
    }
}