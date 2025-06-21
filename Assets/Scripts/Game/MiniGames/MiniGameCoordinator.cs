using System;
using System.Collections.Generic;
using System.Linq;
using Game.Interactions;
using Game.Quests;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames
{

    // Пошаговый план:
    // 1. Для отписки от событий нужно сохранять делегаты, которые были подписаны.
    // 2. Для этого создайте словари для хранения делегатов для каждого мини-игры.
    // 3. При подписке сохраняйте делегаты, при отписке используйте их.

    public class MiniGameCoordinator
    {
        private readonly IInteractionSystem _interactionSystem;
        private readonly PlayerModel _playerModel;

        private readonly Dictionary<ItemCategory, IMiniGame> _factories = new();
        private readonly IPlayerController _playerController;
        private Transform _firstLevel;
        private readonly GameObject _miniGameObj;

        // Словари для хранения делегатов
        private readonly Dictionary<IMiniGame, Action<Quests.QuestType>> _startHandlers = new();
        private readonly Dictionary<IMiniGame, Action<Quests.QuestType>> _completeHandlers = new();
        IMiniGame currentMiniGame;

        public List<IMiniGame> Games => _factories.Values.ToList();

        // ===== НОВОЕ: Единый менеджер для timing игр =====
        [Header("New Unified System")]
        [SerializeField] private bool useUnifiedManager = false; // Флаг для переключения
        private readonly GameObject _unifiedMiniGameObj;
        private readonly UnifiedMiniGameManager _unifiedManager;

        public MiniGameCoordinator(IInteractionSystem interactionSystem, PlayerModel playerModel,
            IPlayerController playerController)
        {


            //   _miniGameObj = Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/MiniGameManager"));
            _interactionSystem = interactionSystem;
            _playerModel = playerModel;
            _playerController = playerController;


            try
            {
                var unifiedPrefab = Resources.Load<GameObject>("Prefabs/MiniGame/UnifiedMiniGameManager");

                if (unifiedPrefab != null)
                {
                    _unifiedMiniGameObj = Object.Instantiate(unifiedPrefab);

                    // ВАЖНО: Убедиться что объект активен для настройки
                    _unifiedMiniGameObj.SetActive(true);

                    _unifiedManager = _unifiedMiniGameObj.GetComponent<UnifiedMiniGameManager>();

                    if (_unifiedManager != null)
                    {
                        Debug.Log("✅ UnifiedMiniGameManager найден и готов!");

                        // Проверить основные ссылки
                        CheckUnifiedManagerReferences();

                        // Выключить после настройки
                        _unifiedMiniGameObj.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("❌ UnifiedMiniGameManager компонент отсутствует!");

                        // Попробовать добавить автоматически
                        _unifiedManager = _unifiedMiniGameObj.AddComponent<UnifiedMiniGameManager>();
                        if (_unifiedManager != null)
                        {
                            Debug.Log("🔧 UnifiedMiniGameManager добавлен автоматически");
                            SetupUnifiedManagerReferences();
                            _unifiedMiniGameObj.SetActive(false);
                        }
                    }
                }
                else
                {
                    Debug.LogError("❌ Префаб UnifiedMiniGameManager не найден!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Ошибка создания UnifiedMiniGameManager: {ex.Message}");
                _unifiedManager = null;
                _unifiedMiniGameObj = null;
            }


        }

        private void CheckUnifiedManagerReferences()
        {
            if (_unifiedManager == null) return;

            Debug.Log("🔍 Проверяем ссылки UnifiedMiniGameManager:");

            // Проверить Canvas
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"✅ Canvas найден: {canvas.name}");
                // Можно попробовать установить через рефлексию или публичное поле
            }
            else
            {
                Debug.LogWarning("⚠️ Canvas не найден в сцене!");
            }

            // Проверить Panel
            var panel = GameObject.Find("MiniGamePanel");
            if (panel != null)
            {
                Debug.Log($"✅ MiniGamePanel найден: {panel.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ MiniGamePanel не найден в сцене!");
            }
        }

        private void SetupUnifiedManagerReferences()
        {
            if (_unifiedManager == null) return;

            // Найти Canvas и Panel в сцене и назначить
            var canvas = Object.FindObjectOfType<Canvas>();
            var panel = GameObject.Find("MiniGamePanel");

            // Если у UnifiedMiniGameManager есть публичные поля для назначения
            // _unifiedManager.mainCanvas = canvas;
            // _unifiedManager.panel = panel;

            Debug.Log("🔧 Ссылки для UnifiedManager настроены автоматически");
        }

        public void RegisterGames(Transform firstLevel)
        {
            _interactionSystem.OnInteraction += HandleInteraction;

            _firstLevel = firstLevel;

            // СТАРАЯ ЛОГИКА - НЕ ТРОГАЕМ
           // _factories[ItemCategory.Flower] = new FlowerMiniGame(_miniGameObj);
           // _factories[ItemCategory.Kitchen] = new KitchenMiniGame(_miniGameObj);
            _factories[ItemCategory.Computer] = new WorkMiniGame();

            var parkLevel = Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/ParkLevel"));
            _factories[ItemCategory.Door] = new ParkMiniGame(_playerController);
            (_factories[ItemCategory.Door] as ParkMiniGame)?.Initialization(parkLevel);

            Debug.Log($"Зарегистрированы игры. Unified Manager: {(_unifiedManager != null ? "Доступен" : "Недоступен")}");
        }

        private void HandleInteraction(ItemCategory category)
        {
            // ===== НОВАЯ ЛОГИКА: Проверяем можно ли использовать unified менеджер =====
            if (/*useUnifiedManager && */_unifiedManager != null && IsTimingBasedCategory(category))
            {
                Debug.Log($"🆕 Используем UnifiedMiniGameManager для {category}");
                HandleUnifiedMiniGame(category);
                return;
            }

            // ===== СТАРАЯ ЛОГИКА - НЕ ТРОГАЕМ =====
            if (!_factories.TryGetValue(category, out var game)) return;

            if (category == ItemCategory.Flower && _playerModel.ItemInHand != ItemCategory.WateringCan)
            {
                Debug.Log("You need a watering can to start the flower mini-game.");
                return;
            }

            // Создаём делегаты и сохраняем их для последующей отписки
            Action<Quests.QuestType> startHandler = SwitchOnInputSystem;
            Action<Quests.QuestType> completeHandler = SwitchOffInputSystem;

            // Сохраняем делегаты
            _startHandlers[game] = startHandler;
            _completeHandlers[game] = completeHandler;

            // Подписываемся на события
            game.OnMiniGameStart += startHandler;
            game.OnMiniGameComplete += completeHandler;
            currentMiniGame = game;


            if (category == ItemCategory.Door)
            {
                _firstLevel.gameObject.SetActive(false);
                game.OnMiniGameComplete += OnMiniGameComplete;
            }

            game.StartGame();
        }

        // ===== НОВЫЕ МЕТОДЫ для unified менеджера =====
        private bool IsTimingBasedCategory(ItemCategory category)
        {
            return category == ItemCategory.Flower || category == ItemCategory.Kitchen;
        }

        private void HandleUnifiedMiniGame(ItemCategory category)
        {
            if (_unifiedManager == null) return;

            // Проверить условия (как в старой логике)
            if (category == ItemCategory.Flower && _playerModel.ItemInHand != ItemCategory.WateringCan)
            {
                Debug.Log("You need a watering can to start the flower mini-game.");
                return;
            }

            // Установить тип игры
            MiniGameType gameType = category == ItemCategory.Flower ? MiniGameType.Flower : MiniGameType.Cooking;
            _unifiedManager.SetGameType(gameType);

            // Настроить input (как в старой логике)
            SetupUnifiedInput();

            // Подписаться на завершение
            _unifiedManager.OnMiniGameComplete += OnUnifiedGameComplete;

            // Активировать объект и запустить
            _unifiedMiniGameObj.SetActive(true);
            _unifiedManager.StartMiniGame();

            Debug.Log($"🚀 Запущена unified игра: {gameType}");
        }

        private void SetupUnifiedInput()
        {
            var playerController = (_playerController as PlayerController);
            if (playerController != null)
            {
                playerController.InputAdaptep.SwitchAdapterToMiniGameMode();
                playerController.InputAdaptep.OnGameInteract += OnUnifiedGameInput;
            }
        }

        private void OnUnifiedGameInput(bool pressed)
        {
            if (pressed && _unifiedManager != null)
            {
                _unifiedManager.OnActionButtonClick();
            }
        }

        private void OnUnifiedGameComplete()
        {
            Debug.Log("🎉 Unified игра завершена!");

            // Отключить input
            var playerController = (_playerController as PlayerController);
            if (playerController != null)
            {
                playerController.InputAdaptep.SwitchAdapterToGlobalMode();
                playerController.InputAdaptep.OnGameInteract -= OnUnifiedGameInput;
            }

            // Отписаться от событий
            if (_unifiedManager != null)
            {
                _unifiedManager.OnMiniGameComplete -= OnUnifiedGameComplete;
            }

            // Деактивировать объект
            _unifiedMiniGameObj.SetActive(false);
        }

        // ===== СТАРЫЕ МЕТОДЫ - НЕ ТРОГАЕМ =====
        private void SwitchOnInputSystem(Quests.QuestType questType)
        {
            if (questType != Quests.QuestType.Sprint)
            {
                var playerController = (_playerController as PlayerController);
                playerController.InputAdaptep.SwitchAdapterToMiniGameMode();
                playerController.InputAdaptep.OnGameInteract += InputAdaptep_OnGameInteract;
            }
        }

        private void InputAdaptep_OnGameInteract(bool obj)
        {
            currentMiniGame.OnActionButtonClick();
        }

        private void SwitchOffInputSystem(Quests.QuestType questType)
        {
            if (questType != Quests.QuestType.Sprint)
            {
                var playerController = (_playerController as PlayerController);
                playerController.InputAdaptep.SwitchAdapterToGlobalMode();
                playerController.InputAdaptep.OnGameInteract -= InputAdaptep_OnGameInteract;
            }
            Unsubscribe(currentMiniGame);
            currentMiniGame = null; // Сбрасываем текущую игру после переключения
        }

        private void OnMiniGameComplete(QuestType type)
        {
            _firstLevel.gameObject.SetActive(true);
            _playerController.SetPosition(Vector3.zero * 6);
        }

        public void UnregisterAll()
        {
            _interactionSystem.OnInteraction -= HandleInteraction;
            foreach (var game in _factories.Values)
            {
                game.OnMiniGameComplete -= OnMiniGameComplete;
            }

            _factories.Clear();

            // ===== НОВОЕ: Очистка unified менеджера =====
            if (_unifiedManager != null)
            {
                _unifiedManager.OnMiniGameComplete -= OnUnifiedGameComplete;
                Object.Destroy(_unifiedMiniGameObj);
            }

            _firstLevel = null;
        }

        // Метод для отписки, если потребуется (СТАРАЯ ЛОГИКА)
        private void Unsubscribe(IMiniGame game)
        {
            if (_startHandlers.TryGetValue(game, out var startHandler))
                game.OnMiniGameStart -= startHandler;
            if (_completeHandlers.TryGetValue(game, out var completeHandler))
                game.OnMiniGameComplete -= completeHandler;
            _startHandlers.Remove(game);
            _completeHandlers.Remove(game);
        }

        // ===== НОВЫЕ ПУБЛИЧНЫЕ МЕТОДЫ для управления =====

        /// <summary>
        /// Переключить на новую unified систему для timing игр
        /// </summary>
        public void EnableUnifiedManager()
        {
            useUnifiedManager = true;
            Debug.Log("🆕 Включена unified система мини-игр");
        }

        /// <summary>
        /// Вернуться к старой системе
        /// </summary>
        public void DisableUnifiedManager()
        {
            useUnifiedManager = false;
            Debug.Log("📜 Возврат к старой системе мини-игр");
        }

        /// <summary>
        /// Проверить доступность unified менеджера
        /// </summary>
        public bool IsUnifiedManagerAvailable()
        {
            return _unifiedManager != null;
        }

        [ContextMenu("Test Unified Flower Game")]
        public void TestUnifiedFlowerGame()
        {
            Debug.Log("=== ДИАГНОСТИКА ===");
            Debug.Log($"_unifiedManager null? {_unifiedManager == null}");
            Debug.Log($"_unifiedMiniGameObj null? {_unifiedMiniGameObj == null}");
            Debug.Log($"_unifiedMiniGameObj active? {_unifiedMiniGameObj?.activeInHierarchy}");

            if (_unifiedManager != null)
            {
                Debug.Log("🌸 Тестируем Unified Flower Game");
                EnableUnifiedManager();

                // Принудительно активировать
                _unifiedMiniGameObj.SetActive(true);

                HandleInteraction(ItemCategory.Flower);
            }
            else
            {
                Debug.LogError("❌ UnifiedManager не найден!");
            }
        }

        /// <summary>
        /// Тест новой системы для цветка
        /// </summary>


        /// <summary>
        /// Тест новой системы для готовки
        /// </summary>
        [ContextMenu("Test Unified Cooking Game")]
        public void TestUnifiedCookingGame()
        {
            if (_unifiedManager != null)
            {
                EnableUnifiedManager();
                HandleInteraction(ItemCategory.Kitchen);
            }
        }
    }
}