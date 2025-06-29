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
        private readonly IPlayerController _playerController;
        private ParkLevelView _parkLevelView;
        private EffectAccumulatorView _effectsAccumulatorView;
        private Transform _levelRoom;
        private ParkSprintController _parkSprintController;

        public QuestType QType { get; } = QuestType.Sprint;
        public int Level { get ; set ; }

        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;

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
            _effectsAccumulatorView.FadeIn();
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _parkLevelView.gameObject.SetActive(true);
                _playerController.SetPosition(Vector3.up * 5.1f);
                _playerController.ToggleMovement();
                
                DisableLevelInNextFrame().Forget();
                
                _parkSprintController = new ParkSprintController(_playerController, _parkLevelView);
                _parkSprintController.EndSprint += () =>
                {
                    RunTimer().Forget();
                };
                
                _effectsAccumulatorView.FadeOut();
            });
        }

        public bool IsCompleted { get; set; }

        private async UniTask DisableLevelInNextFrame()
        {
            await UniTask.WaitForFixedUpdate();
            _levelRoom.gameObject.SetActive(false);
        }

        private async UniTask RunTimer()
        {
            Debug.Log("Park Mini Game Started");
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _effectsAccumulatorView.FadeIn();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _parkLevelView.gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _effectsAccumulatorView.FadeOut();
            
            _playerController.ToggleMovement();
            Debug.Log("Park Mini Game Completed");
            _parkSprintController.Dispose();
            OnMiniGameComplete?.Invoke(QType);
        }

        public void OnActionButtonClick()
        {
            
        }

        public void Dispose()
        {
            Object.Destroy(_parkLevelView);
            //TODO: Implement Dispose logic if needed
            // throw new NotImplementedException();
        }
    }
}