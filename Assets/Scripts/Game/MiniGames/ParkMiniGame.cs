using System;
using Cysharp.Threading.Tasks;
using Effects;
using Game.Quests;
using Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames
{
    public class ParkMiniGame : IMiniGame
    {
        private readonly IPlayerController _playerController;
        private GameObject _parkLevel;
        private EffectAccumulatorView _effectsAccumulatorView;
        private Transform _level;

        public QuestType QType { get; } = QuestType.Sprint;
        public int Level { get ; set ; }

        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;

        public ParkMiniGame(IPlayerController playerController)
        {
            _playerController = playerController;
        }

        public void Initialization(GameObject parkLevel, EffectAccumulatorView effectAccumulatorView, Transform level)
        {
            _parkLevel = parkLevel;
            _parkLevel.SetActive(false);
            _parkLevel.transform.position = Vector3.up * 5f;
            _level = level;
            _effectsAccumulatorView = effectAccumulatorView;
        }
        public void StartGame()
        {
            _effectsAccumulatorView.FadeIn();
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _parkLevel.SetActive(true);
                _playerController.SetPosition(Vector3.up * 5.1f);
                _playerController.ToggleMovement();
                
                DisableLevelInNextFrame().Forget();

                RunTimer().Forget();
                
                _effectsAccumulatorView.FadeOut();
            });
        }

        private async UniTask DisableLevelInNextFrame()
        {
            await UniTask.WaitForFixedUpdate();
            _level.gameObject.SetActive(false);
        }

        private async UniTask RunTimer()
        {
            Debug.Log("Park Mini Game Started");
            await UniTask.Delay(TimeSpan.FromSeconds(9));
            _effectsAccumulatorView.FadeIn();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _parkLevel.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _effectsAccumulatorView.FadeOut();
            
            _playerController.ToggleMovement();
            Debug.Log("Park Mini Game Completed");
            OnMiniGameComplete?.Invoke(QType);
        }

        public void OnActionButtonClick()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Object.Destroy(_parkLevel);
            //TODO: Implement Dispose logic if needed
            // throw new NotImplementedException();
        }
    }
}