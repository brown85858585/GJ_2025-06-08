using System;
using Cysharp.Threading.Tasks;
using Effects;
using Game.MiniGames.Park;
using Game.Quests;
using Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames
{
    public class ParkMiniGame : IMiniGame
    {
        public int Level { get ; set ; }
        public bool IsCompleted { get; set; }
        public bool IsWin { get; private set; }
        public event Action<QuestType> OnMiniGameStart;
        public Action<QuestType, bool> OnMiniGameComplete { get; set; }
        public QuestType QType { get; } = QuestType.Sprint;

        private Transform _levelRoom;
        private ParkLevelView _parkLevelView;
        private ParkSprintController _parkSprintController;
        private readonly IPlayerController _playerController;
        private EffectAccumulatorView _effectsAccumulatorView;
        private int _savedScore;

        public ParkMiniGame(IPlayerController playerController)
        {
            _playerController = playerController;
        }

        public void Initialization(ParkLevelView parkView, EffectAccumulatorView effectAccumulatorView, Transform level)
        {
            _parkLevelView = parkView;
            _parkLevelView.gameObject.SetActive(false);
            _parkLevelView.transform.position = Vector3.up * 5f;
            _levelRoom = level;
            _effectsAccumulatorView = effectAccumulatorView;
        }
        public void StartGame()
        {
            _savedScore = _playerController.Model.Score;
            
            _effectsAccumulatorView.FadeOut();
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _parkLevelView.gameObject.SetActive(true);
                _playerController.SetPosition(Vector3.up * 5.1f);
                _playerController.ToggleMovement(true);
                
                DisableLevelInNextFrame().Forget();
                
                _parkSprintController = new ParkSprintController(_playerController, _parkLevelView);
                _parkSprintController.EndSprint += (win) =>
                {
                    RunCompletingTimer(win).Forget();
                };
                
                _effectsAccumulatorView.FadeIn();
                _effectsAccumulatorView.VignetteToggle();
            });
        }

        private async UniTask DisableLevelInNextFrame()
        {
            await UniTask.WaitForFixedUpdate();
            _levelRoom.gameObject.SetActive(false);
        }

        private async UniTask RunCompletingTimer(bool win)
        {
            _playerController.ToggleMovement(false);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _effectsAccumulatorView.FadeOut();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _parkLevelView.gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _effectsAccumulatorView.FadeIn();

            _parkSprintController.Dispose();
            
            IsWin = win;
            
            _effectsAccumulatorView.VignetteToggle();
            OnMiniGameComplete?.Invoke(QType, IsWin);
        }

        public void OnActionButtonClick()
        {
            
        }

        public void Dispose()
        {
            Object.Destroy(_parkLevelView.gameObject);
            _parkSprintController?.Dispose();
            //TODO: Implement Dispose logic if needed
        }
    }
}