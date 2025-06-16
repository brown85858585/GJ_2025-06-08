using System;
using Cysharp.Threading.Tasks;
using Game.Quests;
using Player;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Game.MiniGames
{
    public class ParkMiniGame : IMiniGame
    {
        private readonly IPlayerController _playerController;
        private GameObject _parkLevel;

        public QuestType QType { get; } = QuestType.Sprint;
        public event Action<QuestType> OnMiniGameComplete;
        
        public ParkMiniGame(IPlayerController playerController)
        {
            _playerController = playerController;
        }

        public void Initialization(GameObject parkLevel)
        {
            _parkLevel = parkLevel;
            _parkLevel.SetActive(false);
            _parkLevel.transform.position = Vector3.up * 5f;
        }
        public void StartGame()
        {
            _parkLevel.SetActive(true);
            _playerController.SetPosition(Vector3.up * 5.1f);
            _playerController.ToggleMovement();

            RunTimer().Forget();
        }

        private async UniTask RunTimer()
        {
            Debug.Log("Park Mini Game Started");
            await UniTask.Delay(TimeSpan.FromSeconds(10)); // Задержка в 5 секунд для симуляции мини-игры
            
            _parkLevel.SetActive(false);
            
            _playerController.ToggleMovement();
            Debug.Log("Park Mini Game Completed");
            OnMiniGameComplete?.Invoke(QType);
        }
    }
}