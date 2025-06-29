using System;
using Game.Quests;
using Player;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using Object = UnityEngine.Object;

namespace Game.MiniGames
{
    public class KitchenMiniGame : IMiniGame
    {
        private BaseTimingMiniGame _miniGameController;
        private  GameObject _miniGameObj;
        private IPlayerController playerController;

        public QuestType QType { get; } = QuestType.Kitchen;
        public int Level { get ; set ; }

        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;

        public KitchenMiniGame(IPlayerController playerController)
        {
            this.playerController = playerController;
            // Попробуем найти префаб MiniGameManager1 или создать новый
            _miniGameObj = Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/CookingGameManager"));
            Level = Level = MiniGameCoordinator.DayLevel; ;
            if (_miniGameObj != null)
            {
                _miniGameController = _miniGameObj.GetComponent<CookingMiniGame>();

                if (_miniGameController == null)
                {
                    // Если CookingMiniGame не найден, добавляем его
                    _miniGameController = _miniGameObj.AddComponent<CookingMiniGame>();
                }
            }
            else
            {
                Debug.LogError("Не удалось найти MiniGameManager1 префаб! Создаем новый GameObject...");
                CreateFallbackMiniGame();
            }
            _miniGameController.SetPlayer(playerController.Model);
        }

        private void CreateFallbackMiniGame()
        {
            // Создаем новый GameObject если префаб не найден
            _miniGameObj = new GameObject("KitchenMiniGameManager");
            Object.DontDestroyOnLoad(_miniGameObj);

            _miniGameController = _miniGameObj.AddComponent<CookingMiniGame>();

            // Настраиваем основные компоненты
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // Создаем панель для мини-игры
                GameObject panel = new GameObject("MiniGamePanel1");
                panel.transform.SetParent(canvas.transform, false);

                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0.5f);

                panel.SetActive(false);
            }
        }

        public void OnActionButtonClick()
        {
            // Этот метод вызывается системой ввода, если нужно
            if (_miniGameController != null && _miniGameController.gameObject.activeInHierarchy)
            {
                // Можно вызвать метод контроллера напрямую
                Debug.Log("Внешний вызов действия в кухонной мини-игре");
            }
        }

        public void StartGame()
        {
            Debug.Log("🍳 Kitchen Mini Game Started");
            OnMiniGameStart?.Invoke(QType);

            if (_miniGameController == null)
            {
                if (_miniGameObj != null)
                {
                    _miniGameController = _miniGameObj.GetComponent<CookingMiniGame>();

                    if (_miniGameController == null)
                    {
                        Debug.LogError("CookingMiniGame компонент не найден на объекте!");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Объект мини-игры не создан!");
                    return;
                }
            }

            Debug.Log("🎮 MiniGameController найден! Запуск кухонной мини-игры...");

            // Убедимся что панель выключена перед запуском
            var panel = _miniGameController.Panel;
            if (panel != null && panel.activeInHierarchy)
            {
                Debug.Log("📱 Панель была включена, выключаем её перед запуском");
                panel.SetActive(false);
            }

            // Подписываемся на события мини-игры
            _miniGameController.OnMiniGameComplete += OnMiniGameCompleted;
            _miniGameController.OnGameAttempt += OnCookingAttempt;

            // Устанавливаем сложность (опционально)
            if (_miniGameController is CookingMiniGame cookingGame)
            {
                cookingGame.SetDifficulty(80f, 30f); // Скорость 80, размер зоны 30 градусов
            }

            // Запускаем мини-игру
            _miniGameController.StartMiniGame();
        }

        public bool IsCompleted { get; set; }

        private void OnCookingAttempt(bool success)
        {
            if (success)
            {
               
                Debug.Log("🍽️ Успешная попытка готовки!");
            }
            else
            {
                Debug.Log("🔥 Неудачная попытка готовки!");
            }
        }

        private void OnMiniGameCompleted()
        {

           // playerController.Model.Score += _miniGameController.gameScore;
            Debug.Log("🍳 Кухонная мини-игра завершена!");

            OnMiniGameComplete?.Invoke(QType);

            // Отписываемся от событий
            if (_miniGameController != null)
            {
                _miniGameController.OnMiniGameComplete -= OnMiniGameCompleted;
                _miniGameController.OnGameAttempt -= OnCookingAttempt;
            }

            // Здесь можно добавить дополнительную логику:
            // - Обновить состояние кухни
            // - Дать игроку еду
            // - Показать результат готовки
            // - Сохранить прогресс
        }

        public void ForceEndMiniGame()
        {
            if (_miniGameController != null && _miniGameController.gameObject.activeSelf)
            {
                Debug.Log("🛑 Принудительное завершение кухонной мини-игры");
                _miniGameController.EndMiniGame();
            }
        }

        public void SetDifficulty(float speed, float successZoneSize)
        {
            if (_miniGameController is CookingMiniGame cookingGame)
            {
                cookingGame.SetDifficulty(speed, successZoneSize);
            }
        }

        public void Dispose()
        {
            Object.Destroy(_miniGameController);
            Object.Destroy(_miniGameObj);

        }

       /* ~KitchenMiniGame()
        {
            // Очистка при уничтожении
            if (_miniGameObj != null)
            {
                Object.Destroy(_miniGameObj);
            }
        }
       */
    }
}