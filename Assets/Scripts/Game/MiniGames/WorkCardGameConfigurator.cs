using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.MiniGames
{
    [CreateAssetMenu(fileName = "WorkCardGameConfig", menuName = "MiniGames/Work Card Game Configuration")]
    public class WorkCardGameConfiguration : ScriptableObject
    {
        [Header("Game Settings")]
        public string configName = "Офисная работа";
        public int targetScore = 5; // Нужно набрать для победы
        public int maxCards = 10; // Максимум карточек в игре

        [Header("Cards")]
        public List<CardSwipeMiniGame.CardData> cards = new List<CardSwipeMiniGame.CardData>();

        [Header("Visual Settings")]
        public Color acceptButtonColor = Color.green;
        public Color rejectButtonColor = Color.red;
        public Sprite cardBackgroundSprite;

        [ContextMenu("Add Sample Work Cards")]
        public void AddSampleWorkCards()
        {
            cards.Clear();

            // Примеры рабочих задач (принять)
            cards.Add(new CardSwipeMiniGame.CardData("Начальник", "Подготовьте отчет по проекту к пятнице", true));
            cards.Add(new CardSwipeMiniGame.CardData("Клиент", "Нужно обсудить детали договора", true));
            cards.Add(new CardSwipeMiniGame.CardData("HR", "Заполните анкету для аттестации", true));
            cards.Add(new CardSwipeMiniGame.CardData("Коллега", "Помогите с код-ревью, пожалуйста", true));
            cards.Add(new CardSwipeMiniGame.CardData("Техподдержка", "Обновите антивирус на рабочем ПК", true));

            // Примеры личных дел (удалить)
            cards.Add(new CardSwipeMiniGame.CardData("Мама", "Не забудь купить молоко по дороге домой", false));
            cards.Add(new CardSwipeMiniGame.CardData("Друг", "Давай встретимся вечером в кафе", false));
            cards.Add(new CardSwipeMiniGame.CardData("Интернет-магазин", "Скидка 50% на всё! Только сегодня!", false));
            cards.Add(new CardSwipeMiniGame.CardData("Соцсеть", "У вас 5 новых уведомлений", false));
            cards.Add(new CardSwipeMiniGame.CardData("YouTube", "Новое видео от вашего любимого блогера", false));
        }
    }

    /// <summary>
    /// Менеджер для управления конфигурациями карточек рабочих задач
    /// </summary>
    public class WorkCardGameConfigurator : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private List<WorkCardGameConfiguration> availableConfigs = new List<WorkCardGameConfiguration>();
        [SerializeField] private WorkCardGameConfiguration currentConfig;

        [Header("Dynamic Cards")]
        [SerializeField] private List<CardSwipeMiniGame.CardData> runtimeCards = new List<CardSwipeMiniGame.CardData>();

        public WorkCardGameConfiguration CurrentConfig => currentConfig;
        public List<WorkCardGameConfiguration> AvailableConfigs => availableConfigs;

        /// <summary>
        /// Устанавливает конфигурацию по имени
        /// </summary>
        public bool SetConfiguration(string configName)
        {
            WorkCardGameConfiguration config = availableConfigs.Find(c => c.configName == configName);
            if (config != null)
            {
                currentConfig = config;
                Debug.Log($"Установлена конфигурация: {configName}");
                return true;
            }

            Debug.LogWarning($"Конфигурация '{configName}' не найдена!");
            return false;
        }

        /// <summary>
        /// Устанавливает конфигурацию по индексу
        /// </summary>
        public bool SetConfiguration(int index)
        {
            if (index >= 0 && index < availableConfigs.Count)
            {
                currentConfig = availableConfigs[index];
                Debug.Log($"Установлена конфигурация: {currentConfig.configName}");
                return true;
            }

            Debug.LogWarning($"Индекс конфигурации {index} вне диапазона!");
            return false;
        }

        /// <summary>
        /// Получает карточки из текущей конфигурации + добавленные во время выполнения
        /// </summary>
        public List<CardSwipeMiniGame.CardData> GetAllCards()
        {
            List<CardSwipeMiniGame.CardData> allCards = new List<CardSwipeMiniGame.CardData>();

            // Добавляем карточки из конфигурации
            if (currentConfig != null && currentConfig.cards != null)
            {
                allCards.AddRange(currentConfig.cards);
            }

            // Добавляем карточки добавленные во время выполнения
            allCards.AddRange(runtimeCards);

            return allCards;
        }

        /// <summary>
        /// Добавляет рабочую карточку во время выполнения
        /// </summary>
        public void AddWorkCard(string sender, string content)
        {
            runtimeCards.Add(new CardSwipeMiniGame.CardData(sender, content, true));
            Debug.Log($"Добавлена рабочая карточка: {sender} - {content}");
        }

        /// <summary>
        /// Добавляет личную карточку во время выполнения
        /// </summary>
        public void AddPersonalCard(string sender, string content)
        {
            runtimeCards.Add(new CardSwipeMiniGame.CardData(sender, content, false));
            Debug.Log($"Добавлена личная карточка: {sender} - {content}");
        }

        /// <summary>
        /// Очищает карточки добавленные во время выполнения
        /// </summary>
        public void ClearRuntimeCards()
        {
            runtimeCards.Clear();
            Debug.Log("Карточки времени выполнения очищены");
        }

        /// <summary>
        /// Применяет текущую конфигурацию к мини-игре
        /// </summary>
        public void ApplyConfigurationToGame(CardSwipeMiniGame cardGame)
        {
            if (cardGame == null)
            {
                Debug.LogError("CardSwipeMiniGame не задана!");
                return;
            }

            // Очищаем старые карточки
           

            // Добавляем новые карточки
            List<CardSwipeMiniGame.CardData> cards = GetAllCards();
            var lvl = MiniGameCoordinator.DayLevel + 1;

           var enumForCurrentDay =  cards.Where(t => t.sender.Contains($"Day{lvl}"));

            if(enumForCurrentDay.Count() > 0)
                cardGame.ClearCards();

            cardGame.SetCustomCards(enumForCurrentDay.ToList());

            // Применяем настройки если есть
            if (currentConfig != null)
            {
                cardGame.SetTargetScore(currentConfig.targetScore);
                Debug.Log($"Применена конфигурация '{currentConfig.configName}' с {cards.Count} карточками, цель: {currentConfig.targetScore} очков");
            }
        }

        // Методы для быстрого добавления карточек
        public void AddWorkCards(params (string sender, string content)[] tasks)
        {
            foreach (var task in tasks)
            {
                AddWorkCard(task.sender, task.content);
            }
        }

        public void AddPersonalCards(params (string sender, string content)[] activities)
        {
            foreach (var activity in activities)
            {
                AddPersonalCard(activity.sender, activity.content);
            }
        }

        // Предустановленные конфигурации для разных ситуаций
        public void SetupMorningWork()
        {
            ClearRuntimeCards();
            AddWorkCards(
                ("Начальник", "Планирование рабочего дня"),
                ("Коллега", "Обсуждение проекта на сегодня"),
                ("Клиент", "Уточнение деталей по договору"),
                ("HR", "Заполнить отчет о работе")
            );
            AddPersonalCards(
                ("Друг", "Планы на выходные"),
                ("Интернет-магазин", "Новые скидки!"),
                ("Соцсеть", "Посмотри новые фото"),
                ("YouTube", "Новое видео от блогера")
            );
        }

        public void SetupAfternoonWork()
        {
            ClearRuntimeCards();
            AddWorkCards(
                ("Начальник", "Завершить отчет до конца дня"),
                ("Техподдержка", "Обновить ПО"),
                ("Коллега", "Помочь с багом в коде"),
                ("Клиент", "Финальное согласование")
            );
            AddPersonalCards(
                ("Мама", "Купить продукты после работы"),
                ("Друг", "Встретимся вечером?"),
                ("Доставка еды", "Ваш заказ готов"),
                ("Игра", "Энергия восстановлена!")
            );
        }

        public void SetupDeadlineMode()
        {
            ClearRuntimeCards();
            AddWorkCards(
                ("Начальник", "СРОЧНО: доделать проект!"),
                ("Клиент", "ВАЖНО: нужен ответ сегодня"),
                ("Коллега", "Помогите срочно исправить баг"),
                ("HR", "Последний день подачи документов")
            );
            AddPersonalCards(
                ("Любой контакт", "Любое отвлечение сейчас"),
                ("Соцсеть", "Новости могут подождать"),
                ("YouTube", "Видео можно посмотреть потом"),
                ("Друг", "Личные разговоры после дедлайна")
            );
        }

        private void Start()
        {
            // Устанавливаем первую доступную конфигурацию по умолчанию
            if (currentConfig == null && availableConfigs.Count > 0)
            {
                currentConfig = availableConfigs[0];
                Debug.Log($"Установлена конфигурация по умолчанию: {currentConfig.configName}");
            }
        }

        // Для отладки в инспекторе
        [ContextMenu("Log Current Config")]
        public void LogCurrentConfig()
        {
            if (currentConfig != null)
            {
                Debug.Log($"Текущая конфигурация: {currentConfig.configName}");
                Debug.Log($"Карточек в конфигурации: {currentConfig.cards?.Count ?? 0}");
                Debug.Log($"Карточек времени выполнения: {runtimeCards.Count}");
                Debug.Log($"Всего карточек: {GetAllCards().Count}");
                Debug.Log($"Цель: {currentConfig.targetScore} очков");
            }
            else
            {
                Debug.Log("Конфигурация не установлена");
            }
        }

        [ContextMenu("Test Add Sample Cards")]
        public void TestAddSampleCards()
        {
            ClearRuntimeCards();

            AddWorkCards(
                ("Босс", "Подготовить презентацию к завтрашней встрече"),
                ("Коллега", "Код-ревью нового модуля"),
                ("Клиент", "Обсудить изменения в проекте"),
                ("HR", "Заполнить анкету сотрудника")
            );

            AddPersonalCards(
                ("Мама", "Позвонить бабушке"),
                ("Друг", "Новая игра вышла!"),
                ("Магазин", "Распродажа до конца дня"),
                ("Соцсеть", "5 новых лайков на фото")
            );

            Debug.Log("Добавлены тестовые карточки");
        }
    }
}
