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
        private FlowerMiniGameManager _miniGameController;
        private readonly GameObject _miniGameObj;
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
            // _miniGameObj =  Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/FlowerGameManager"));
            // if (_miniGameObj != null)
            // {
            //     _miniGameController = _miniGameObj.GetComponent<FlowerMiniGameManager>();
            // }

            _flowerView = Object.Instantiate(prefabAccumulator.FlowerMiniGameViews[Level], miniGameCanvas.transform);
            _pressIndicator = Object.Instantiate(prefabAccumulator.PressIndicator, _flowerView.PressPoint);
            _pressIndicator.gameObject.SetActive(false);
            _flowerView.gameObject.SetActive(false);

            _pressIndicator.OnCompleteIndicator += SompleteFloverMiniGame;
        }

        private void SompleteFloverMiniGame(bool isSuccess)
        {
            OnMiniGameComplete?.Invoke(QType);
            
            Debug.Log(isSuccess);
            
            _pressIndicator.gameObject.SetActive(false);
            _flowerView.gameObject.SetActive(false);
            
            if (isSuccess)
            {
                _playerController.Model.Score += 500;
            }
            else
            {
                _playerController.Model.Score -= 500;
            }
        }

        private void StartFlowerMiniGame()
        {
            // Найти контроллер мини-игры в текущей сцене
            if (_miniGameController == null)
            {
                if (_miniGameObj != null)
                {
                    _miniGameController = _miniGameObj.GetComponent<FlowerMiniGameManager>();
                    //_miniGameController = FindObjectOfType<MiniGameController>();
                }
            }

            if (_miniGameController != null)
            {
                Debug.Log("✅ MiniGameController найден! Запуск мини-игры...");

                // Дополнительно убедиться что панель выключена перед запуском
                var panel = _miniGameController.Panel;
                if (panel != null && panel.activeInHierarchy)
                {
                    Debug.Log("⚠️ Панель была включена, выключаем её перед запуском");
                    panel.SetActive(false);
                }

                // Подписаться на события мини-игры
                _miniGameController.OnMiniGameComplete += OnMiniGameCompleted;
                _miniGameController.OnWateringAttempt += OnWateringAttempt;

                // Запустить мини-игру (панель включится автоматически)
                _miniGameController.SetPlayer(_playerController.Model);
                _miniGameController.StartMiniGame();
            }
            else
            {
                Debug.LogError("❌ MiniGameController не найден! Проверь что скрипт добавлен на MiniGameManager в префабе комнаты.");
            }
        }
        
        public void StartGame()
        {
            OnMiniGameStart?.Invoke(QType);
            _pressIndicator.gameObject.SetActive(true);
            
            _flowerView.gameObject.SetActive(true);
            // StartFlowerMiniGame();
        }


        private void OnMiniGameCompleted()
        {
            Debug.Log("Мини-игра завершена!");
           // playerController.Model.Score += _miniGameController.GetGameScore;
            OnMiniGameComplete?.Invoke(QType);
            
            // Отписаться от событий чтобы избежать утечек памяти
            if (_miniGameController != null)
            {
                _miniGameController.OnMiniGameComplete -= OnMiniGameCompleted;
                _miniGameController.OnWateringAttempt -= OnWateringAttempt;
            }

            // Здесь можно добавить логику завершения:
            // - Дать награду игроку
            // - Показать анимацию роста цветка
            // - Обновить состояние цветка в комнате
            // - Сохранить прогресс

            Debug.Log("Взаимодействие с цветком завершено");
        }

        // Колбэк для каждой попытки полива
        private void OnWateringAttempt(bool success)
        {
            if (success)
            {
                Debug.Log("🌸 Успешный полив! Цветок доволен!");

                // Тут можно добавить:
                // - Звук успеха
                // - Партиклы воды
                // - Анимацию цветка
                // - Увеличить счетчик успешных поливов
            }
            else
            {
                Debug.Log("💧 Промах! Попробуй еще раз!");

                // Тут можно добавить:
                // - Звук промаха
                // - Анимация неудачи
                // - Feedback для игрока
            }
        }

        public void ForceEndMiniGame()
        {
            if (_miniGameController != null && _miniGameController.gameObject.activeSelf)
            {
                _miniGameController.EndMiniGame();
            }
        }

        public void Dispose()
        {
            Object.Destroy(_miniGameController);
            Object.Destroy(_miniGameObj);

        }
        
        public void OnActionButtonClick()
        {
            //_miniGameController.OnActionButtonClick();
        }
    }
}