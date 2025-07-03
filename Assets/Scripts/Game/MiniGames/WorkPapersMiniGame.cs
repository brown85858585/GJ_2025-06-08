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
            if(_miniGameController.CardCount == 0)
            { 
                SetupForLanguage();                 //TODO Test Card  закоментировать при добавлении карт
                
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
        public void SetupMorningWork()
        {
            _configurator?.SetupMorningWork();
            SetTargetScore(5);
        }

        public void SetupAfternoonWork()
        {
            _configurator?.SetupAfternoonWork();
            SetTargetScore(4);
        }

        public void SetupDeadlineMode()
        {
            _configurator?.SetupDeadlineMode();
            SetTargetScore(6); // Повышенная сложность для дедлайна
        }

        // Настройки для разных типов профессий
        public void SetupForManager()
        {
            ClearCustomCards();

            // Рабочие задачи менеджера
            _configurator?.AddWorkCards(
                ("Начальник", "Подготовить отчет о работе команды"),
                ("HR", "Провести собеседование с кандидатом"),
                ("Коллега", "Решить конфликт в команде"),
                ("Клиент", "Обсудить условия нового контракта"),
                ("Подчиненный", "Утвердить план работы на неделю")
            );

            // Личные отвлечения
            _configurator?.AddPersonalCards(
                ("Друг", "Обсудить планы на отпуск"),
                ("Семья", "Вопросы по домашним делам"),
                ("Интернет", "Чтение новостей о конкурентах"),
                ("Соцсеть", "Обновления в LinkedIn не по работе"),
                ("Магазин", "Скидки на технику")
            );

            SetTargetScore(5);
        }

        public void SetupForDeveloper()
        {
            ClearCustomCards();

            // Рабочие задачи разработчика
            _configurator?.AddWorkCards(
                ("Tech Lead", "Код-ревью нового модуля"),
                ("PM", "Оценка времени на новую фичу"),
                ("QA", "Исправить баг в продакшене"),
                ("DevOps", "Настроить CI/CD pipeline"),
                ("Коллега", "Помочь с архитектурным решением"),
                ("Заказчик", "Уточнить требования к API")
            );

            // Личные отвлечения программиста
            _configurator?.AddPersonalCards(
                ("Reddit", "Новые мемы про программирование"),
                ("Хабр", "Статья не по текущему проекту"),
                ("GitHub", "Посмотреть trending репозитории"),
                ("Друг", "Обсудить новую игру"),
                ("YouTube", "Видео про новый фреймворк"),
                ("StackOverflow", "Интересный вопрос не по работе")
            );

            SetTargetScore(6); // Программисты должны быть точными
        }

        public void SetupForLanguage()
        {
            ClearCustomCards();

            var Level1 = Level + 1;
            // Рабочие задачи разработчика
            _configurator?.AddWorkCards(
                ($"Day{Level1}_CardHeader0", $"Day{Level1}_CardContent0"),
                ($"Day{Level1}_CardHeader1", $"Day{Level1}_CardContent1"),
                ($"Day{Level1}_CardHeader2", $"Day{Level1}_CardContent2"),
                ($"Day{Level1}_CardHeader3", $"Day{Level1}_CardContent3"),
                ($"Day{Level1}_CardHeader4", $"Day{Level1}_CardContent4"),
                ($"Day{Level1}_CardHeader5", $"Day{Level1}_CardContent5")
            );

            // Личные отвлечения программиста
            _configurator?.AddPersonalCards(
                ($"Day{Level1}_CardHeader6", $"Day{Level1}_CardContent6"),
                ($"Day{Level1}_CardHeader7", $"Day{Level1}_CardContent7"),
                ($"Day{Level1}_CardHeader8", $"Day{Level1}_CardContent8"),
                ($"Day{Level1}_CardHeader9", $"Day{Level1}_CardContent9"),
                ($"Day{Level1}_CardHeader10", $"Day{Level1}_CardContent10"),
                ($"Day{Level1}_CardHeader11", $"Day{Level1}_CardContent11")
            );

            _miniGameController.SetCustomCards(_configurator.GetAllCards());
            SetTargetScore(6); // Программисты должны быть точными
        }

        public void SetupForDesigner()
        {
            ClearCustomCards();

            // Рабочие задачи дизайнера
            _configurator?.AddWorkCards(
                ("Арт-директор", "Создать концепт для нового проекта"),
                ("Клиент", "Внести правки в макет"),
                ("Фронтенд", "Подготовить ассеты для верстки"),
                ("Маркетинг", "Баннер для рекламной кампании"),
                ("PM", "Презентация дизайна для заказчика")
            );

            // Личные отвлечения дизайнера
            _configurator?.AddPersonalCards(
                ("Behance", "Посмотреть вдохновляющие работы"),
                ("Dribbble", "Новые тренды в дизайне"),
                ("Instagram", "Красивые фото не по работе"),
                ("Pinterest", "Идеи для личных проектов"),
                ("Друг", "Обсудить арт-выставку"),
                ("Магазин", "Новые кисти для рисования")
            );

            SetTargetScore(5);
        }

        public void SetupForSales()
        {
            ClearCustomCards();

            // Рабочие задачи продажника
            _configurator?.AddWorkCards(
                ("Менеджер", "Отчет по продажам за месяц"),
                ("Клиент", "Встреча для обсуждения сделки"),
                ("Маркетинг", "Информация о новых лидах"),
                ("Техподдержка", "Помочь клиенту с настройкой"),
                ("Партнер", "Обсудить условия сотрудничества")
            );

            // Личные отвлечения продажника
            _configurator?.AddPersonalCards(
                ("Друг", "Планы на корпоратив"),
                ("Автосалон", "Новые предложения по машинам"),
                ("Ресторан", "Бронирование столика"),
                ("Спортзал", "Расписание тренировок"),
                ("Банк", "Предложение по кредиту")
            );

            SetTargetScore(4); // Продажники общительные, им сложнее
        }

        // Методы для разных временных периодов работы
        public void SetupStartOfDay()
        {
            ClearCustomCards();

            _configurator?.AddWorkCards(
                ("Календарь", "Планирование встреч на день"),
                ("Начальник", "Приоритетные задачи на сегодня"),
                ("Email", "Важные письма за ночь"),
                ("Команда", "Планерка по проекту")
            );

            _configurator?.AddPersonalCards(
                ("Новости", "Что происходит в мире"),
                ("Погода", "Прогноз на день"),
                ("Кофейня", "Заказать кофе"),
                ("Соцсети", "Что делали друзья вчера")
            );

            SetTargetScore(4);
        }

        public void SetupEndOfDay()
        {
            ClearCustomCards();

            _configurator?.AddWorkCards(
                ("Начальник", "Отчет о проделанной работе"),
                ("Коллега", "Передать задачи на завтра"),
                ("Система", "Сохранить все изменения"),
                ("Календарь", "Планирование завтрашнего дня")
            );

            _configurator?.AddPersonalCards(
                ("Семья", "Планы на вечер"),
                ("Друзья", "Встреча после работы"),
                ("Магазин", "Список покупок"),
                ("Транспорт", "Как добраться домой"),
                ("Ресторан", "Заказ ужина")
            );

            SetTargetScore(3); // В конце дня концентрация снижается
        }

        public void SetupLunchBreak()
        {
            ClearCustomCards();

            _configurator?.AddWorkCards(
                ("Начальник", "СРОЧНОЕ письмо"),
                ("Клиент", "Критический вопрос"),
                ("Техподдержка", "Сервер упал!"),
                ("Коллега", "Помочь с блокирующей задачей")
            );

            _configurator?.AddPersonalCards(
                ("Ресторан", "Выбрать обед"),
                ("Друг", "Пообедать вместе"),
                ("Магазин", "Быстро купить продукты"),
                ("Семья", "Узнать как дела"),
                ("Новости", "Что происходит в мире"),
                ("Соцсети", "Пролистать ленту")
            );

            SetTargetScore(2); // На обеде можно расслабиться
        }

        // Сезонные/событийные конфигурации
        public void SetupHolidaySeason()
        {
            ClearCustomCards();

            _configurator?.AddWorkCards(
                ("HR", "Планы на новогодние праздники"),
                ("Бухгалтерия", "Закрытие года"),
                ("Клиент", "Завершить проекты до праздников"),
                ("Команда", "Подготовка к корпоративу")
            );

            _configurator?.AddPersonalCards(
                ("Семья", "Планирование праздника"),
                ("Магазин", "Покупка подарков"),
                ("Друзья", "Новогодние планы"),
                ("Путешествия", "Бронирование отпуска"),
                ("Рецепты", "Что готовить на стол")
            );

            SetTargetScore(3); // В праздники сложно сконцентрироваться
        }

        public void SetupMondayMotivation()
        {
            ClearCustomCards();

            _configurator?.AddWorkCards(
                ("Планировщик", "Цели на неделю"),
                ("Начальник", "Новые проекты"),
                ("Команда", "Планерка понедельника"),
                ("Email", "Накопившиеся задачи"),
                ("Клиент", "Старт новой недели")
            );

            _configurator?.AddPersonalCards(
                ("Выходные", "Воспоминания о отдыхе"),
                ("Спорт", "Планы на спортзал"),
                ("Друзья", "Как провели выходные"),
                ("Сериал", "Новая серия вышла"),
                ("Кофе", "Нужно больше кофеина")
            );

            SetTargetScore(5); // Понедельник - день мотивации
        }

        public void SetupFridayRelax()
        {
            ClearCustomCards();

            _configurator?.AddWorkCards(
                ("Отчеты", "Закрыть неделю"),
                ("Планы", "Подготовка к следующей неделе"),
                ("Команда", "Ретроспектива недели"),
                ("Клиент", "Завершить текущие задачи")
            );

            _configurator?.AddPersonalCards(
                ("Планы", "Что делать на выходных"),
                ("Друзья", "Пятничные посиделки"),
                ("Кино", "Новые фильмы в прокате"),
                ("Ресторан", "Ужин в пятницу"),
                ("Путешествия", "Планы на выходные"),
                ("Соцсети", "Поделиться планами")
            );

            SetTargetScore(3); // В пятницу тяжело работать
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

