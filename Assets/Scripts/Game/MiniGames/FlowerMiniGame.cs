using UnityEngine;

namespace Game.MiniGames
{
    public class FlowerMiniGame : IMiniGame
    {
        [Header("Mini Game")]
        private MiniGameController _miniGameController;

        public FlowerMiniGame()
        {
            GameObject miniGameObj = GameObject.Find("MiniGameManager");
            if (miniGameObj != null)
            {
                _miniGameController = miniGameObj.GetComponent<MiniGameController>();
            }
        }

        private void StartFlowerMiniGame()
        {
            // Найти контроллер мини-игры в текущей сцене
            if (_miniGameController == null)
            {
                GameObject miniGameObj = GameObject.Find("MiniGameManager");
                if (miniGameObj != null)
                {
                    _miniGameController = miniGameObj.GetComponent<MiniGameController>();
                    //_miniGameController = FindObjectOfType<MiniGameController>();
                }
            }

            if (_miniGameController != null)
            {
                Debug.Log("✅ MiniGameController найден! Запуск мини-игры...");

                // Дополнительно убедиться что панель выключена перед запуском
                GameObject panel = GameObject.Find("MiniGamePanel");
                if (panel != null && panel.activeInHierarchy)
                {
                    Debug.Log("⚠️ Панель была включена, выключаем её перед запуском");
                    panel.SetActive(false);
                }

                // Подписаться на события мини-игры
                _miniGameController.OnMiniGameComplete += OnMiniGameCompleted;
                _miniGameController.OnWateringAttempt += OnWateringAttempt;

                // Запустить мини-игру (панель включится автоматически)
                _miniGameController.StartMiniGame();
            }
            else
            {
                Debug.LogError("❌ MiniGameController не найден! Проверь что скрипт добавлен на MiniGameManager в префабе комнаты.");
            }
        }


        public void StartGame()
        {
            StartFlowerMiniGame();
        }


        private void OnMiniGameCompleted()
        {
            Debug.Log("Мини-игра завершена!");

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

        // Альтернативный метод если нужно вызвать мини-игру напрямую
        [ContextMenu("Test Mini Game")]
        public void TestMiniGame()
        {
            StartFlowerMiniGame();
        }


    }
}