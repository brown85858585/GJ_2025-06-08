using System;
using Game.Quests;
using Player;
using UnityEngine;

namespace Game.MiniGames
{
    public class WorkPapersMiniGame : IMiniGame
    {
        private CardSwipeMiniGame _miniGameController;
        private WorkCardGameConfigurator _configurator;
        private readonly GameObject _miniGameObj;
        private IPlayerController playerController;
        public QuestType QType { get; } = QuestType.Work;
        public int Level { get ; set; }

        public event Action<QuestType, bool> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;
        public WorkPapersMiniGame(IPlayerController playerController)
        {
            this.playerController = playerController;
            //var workGame = new WorkPapersMiniGame();
            _miniGameObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/WorkGameManager"));
            Level = MiniGameCoordinator.DayLevel;
            if (_miniGameObj != null)
            {
                _miniGameController = _miniGameObj.GetComponent<CardSwipeMiniGame>();
            }
            // Добавляем компоненты
            _configurator = _miniGameObj.AddComponent<WorkCardGameConfigurator>();

            // Настраиваем основные компоненты
            SetupMiniGameComponents();
        }

        private void SetupMiniGameComponents()
        {
            /*
            Canvas canvas = _miniGameController.MainCanvas;
            if (canvas != null)
            {
                // Создаем панель для мини-игры
                GameObject panel = new GameObject("MiniGamePanel");
                panel.transform.SetParent(canvas.transform, false);

                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0, 0, 0, 0.8f);

                panel.SetActive(false);

                Debug.Log("📋 Панель мини-игры Papers созданa");
            }
            */

            _miniGameController.SetPlayer(playerController.Model);
        }

        public void OnActionButtonClick()
        {
            // Обработка нажатия E (принять карточку)
            if (_miniGameController != null)
            {
                Debug.Log("Внешний вызов принятия карточки");
            }
        }

        public void StartGame()
        {
            //SetupForDeveloper(); // Настройка для программиста
            //-----------------------------------// Тест кард
            if(_miniGameController.CardCount == 0 
                && (MiniGameCoordinator.DayLevel < 5))
            { 
                SetupForLanguage();                 //TODO Test Card  закоментировать при добавлении карт
                
            }
            else if(MiniGameCoordinator.DayLevel >= 5)
            {
                ClearCustomCards();
            }
                //----------------------------------//

                SetDifficulty(DifficultyLevel.Medium);
           
            Debug.Log("📋 Work Papers Mini Game Started");
            OnMiniGameStart?.Invoke(QType);

            if (_miniGameController == null)
            {
                Debug.LogError("CardSwipeMiniGame контроллер не найден!");
                return;
            }

            Debug.Log("🎮 Запуск мини-игры сортировки документов...");

            // Убедимся что панель выключена перед запуском
            var panel = _miniGameController.Panel;
            if (panel != null && panel.activeInHierarchy)
            {
                Debug.Log("📱 Панель была включена, выключаем её перед запуском");
                panel.SetActive(false);
            }

            // Подписываемся на события мини-игры
            _miniGameController.OnMiniGameComplete += OnMiniGameCompleted;
            _miniGameController.OnGameAttempt += OnWorkPapersAttempt;

            // Применяем текущую конфигурацию карточек
            if (_configurator != null)
            {
                _configurator.ApplyConfigurationToGame(_miniGameController);
            }

            // Запускаем мини-игру
            _miniGameController.StartMiniGame();
        }

        public bool IsCompleted { get; set; }
        public bool IsWin { get; private set; }

        private void OnWorkPapersAttempt(bool success)
        {
            //playerController.Model.Score += _miniGameController.gameScore;
            if (success)
            {
                IsWin = true;
                Debug.Log("📋 Успешная обработка документов! Квест выполнен.");
            }
            else
            {
                IsWin = false;
                Debug.Log("📋 Документы обработаны неправильно. Квест не выполнен.");
            }
        }

        private void OnMiniGameCompleted()
        {
            Debug.Log("📋 Мини-игра обработки документов завершена!");

            OnMiniGameComplete?.Invoke(QType, _miniGameController.Victory);

            // Отписываемся от событий
            if (_miniGameController != null)
            {
                _miniGameController.OnMiniGameComplete -= OnMiniGameCompleted;
                _miniGameController.OnGameAttempt -= OnWorkPapersAttempt;
            }

            // Здесь можно добавить логику:
            // - Обновить статистику производительности работы
            // - Дать бонусы за правильную сортировку
            // - Показать результаты работы
            // - Сохранить результаты в профиль игрока
        }

        public void ForceEndMiniGame()
        {
            if (_miniGameController != null && _miniGameController.gameObject.activeSelf)
            {
                Debug.Log("🛑 Принудительное завершение мини-игры Papers");
                _miniGameController.EndMiniGame();
            }
        }

        // Методы для настройки карточек и сложности
        public void SetTargetScore(int score)
        {
            if (_miniGameController != null)
            {
                _miniGameController.SetTargetScore(score);
            }
        }

        public void SetDifficulty(DifficultyLevel difficulty)
        {
            if (_configurator == null) return;

            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    SetTargetScore(3); // Легко - нужно 3 очка
                    break;
                case DifficultyLevel.Medium:
                    SetTargetScore(5); // Средне - нужно 5 очков
                    break;
                case DifficultyLevel.Hard:
                    SetTargetScore(7); // Сложно - нужно 7 очков
                    break;
            }
        }

        // Методы для добавления карточек
        public void AddWorkCard(string sender, string content)
        {
            _configurator?.AddWorkCard(sender, content);
        }

        public void AddPersonalCard(string sender, string content)
        {
            _configurator?.AddPersonalCard(sender, content);
        }

        public void ClearCustomCards()
        {
            _configurator?.ClearRuntimeCards();
        }

        // Предустановленные конфигурации для разных рабочих ситуаций

        public void SetupForLanguage()
        {
            ClearCustomCards();

            var Level1 = Level + 1;
            // Рабочие задачи разработчика
            _configurator?.AddWorkCards(
                ($"Day{Level1}_CardHeader0", $"Day{Level1}_CardContent0"),
                ($"Day{Level1}_CardHeader1", $"Day{Level1}_CardContent1"),
                ($"Day{Level1}_CardHeader2", $"Day{Level1}_CardContent2")
            );

            // Личные отвлечения программиста
            _configurator?.AddSpanCards(
                ($"Day{Level1}_CardHeader6", $"Day{Level1}_CardContent6"),
                ($"Day{Level1}_CardHeader7", $"Day{Level1}_CardContent7"),
                ($"Day{Level1}_CardHeader8", $"Day{Level1}_CardContent8")
            );
            _configurator?.AddFrendCards(
                ($"Day{Level1}_CardHeader6", $"Day{Level1}_CardContent6"),
                ($"Day{Level1}_CardHeader7", $"Day{Level1}_CardContent7"),
                ($"Day{Level1}_CardHeader8", $"Day{Level1}_CardContent8"),
                ($"Day{Level1}_CardHeader9", $"Day{Level1}_CardContent9")
            );

            

            _miniGameController.SetCustomCards(_configurator.GetAllCards());
            SetTargetScore(6); // Программисты должны быть точными
        }
 

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_miniGameObj);
        }
        /*
        ~WorkPapersMiniGame()
        {
            // Очистка при уничтожении
            if (_miniGameObj != null)
            {
                UnityEngine.Object.Destroy(_miniGameObj);
            }
        }
        */
    }


// Вспомогательные енумы
    public enum WorkType
    {
        Manager,
        Developer,
        Designer,
        Sales,
        Support,
        Marketing
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum WorkPeriod
    {
        StartOfDay,
        MidMorning,
        LunchBreak,
        Afternoon,
        EndOfDay
    }
}

