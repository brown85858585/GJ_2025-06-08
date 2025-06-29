using System;
using Game.Quests;
using Player;
using UI.Flower;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames.Flower
{
    public class FlowerMiniGame : IMiniGame
    {

        private readonly IPlayerController _playerController;
        private readonly PressIndicator _pressIndicator;
        private readonly FlowerMiniGameView _flowerView;
        public QuestType QType { get; } = QuestType.Flower;
        public int Level { get; set; } = 0;

        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;

        public FlowerMiniGame(IPlayerController playerController, MiniGamePrefabAccumulator prefabAccumulator, Canvas miniGameCanvas)
        {
            _playerController = playerController;

            Level = MiniGameCoordinator.DayLevel;
           
            _flowerView = Object.Instantiate(prefabAccumulator.FlowerMiniGameViews[Level], miniGameCanvas.transform);
            _pressIndicator = Object.Instantiate(prefabAccumulator.PressIndicator, _flowerView.PressPoint);
            
            _pressIndicator.gameObject.SetActive(false);
            _flowerView.gameObject.SetActive(false);

            _pressIndicator.OnCompleteIndicator += CompleteFlowerMiniGame;
        }

        public void StartGame()
        {
            OnMiniGameStart?.Invoke(QType);
            _pressIndicator.gameObject.SetActive(true);
            
            _flowerView.gameObject.SetActive(true);
        }

        private void CompleteFlowerMiniGame(bool isSuccess)
        {
            _pressIndicator.OnCompleteIndicator -= CompleteFlowerMiniGame;
            
            OnMiniGameComplete?.Invoke(QType);
            
            _pressIndicator.gameObject.SetActive(false);
            _flowerView.gameObject.SetActive(false);
            
            SetReward(isSuccess);
        }

        private void SetReward(bool isSuccess)
        {
            if (isSuccess)
            {
                _playerController.Model.Score += _flowerView.WinScore;
            }
            else
            {
                _playerController.Model.Score -= _flowerView.WinScore;
            }
        }

        public void OnActionButtonClick()
        {
        }


        private void OnMiniGameCompleted()
        {
            OnMiniGameComplete?.Invoke(QType);
        }

        public void Dispose()
        {
            _pressIndicator.OnCompleteIndicator -= CompleteFlowerMiniGame;
  
            Object.Destroy(_flowerView);
            Object.Destroy(_pressIndicator);
        }
    }
}