using System;
using System.Collections.Generic;
using System.Linq;
using Effects;
using Game.Interactions;
using Game.Quests;
using Player;
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
        private Transform _level;

        private int CurrentLevelIndex { get; set; } = 0;
        public static int DayLevel { get; set; } = 0; // Уровень дня, который будет использоваться в мини-играх

        // Словари для хранения делегатов
        private readonly Dictionary<IMiniGame, Action<Quests.QuestType>> _startHandlers = new();
        private readonly Dictionary<IMiniGame, Action<Quests.QuestType>> _completeHandlers = new();
        IMiniGame currentMiniGame;

        public List<IMiniGame> Games => _factories.Values.ToList();

        public MiniGameCoordinator(IInteractionSystem interactionSystem, PlayerModel playerModel,
            IPlayerController playerController)
        {
            _interactionSystem = interactionSystem;
            _playerModel = playerModel;
            _playerController = playerController;
        }

        public void SetLevel(int level)
        {
            CurrentLevelIndex = level;
            DayLevel = level; // Устанавливаем уровень дня для мини-игр
        }

        public void RegisterGames(Transform level, EffectAccumulatorView effectAccumulatorView)
        {
            _interactionSystem.OnInteraction += HandleInteraction;
         
            _level = level;

            _factories[ItemCategory.Flower] = new FlowerMiniGame();
            _factories[ItemCategory.Kitchen] = new KitchenMiniGame();
            _factories[ItemCategory.Computer] = new WorkPapersMiniGame();
            
            var parkLevel = Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/ParkLevel"));
            _factories[ItemCategory.Door] = new ParkMiniGame(_playerController);
            (_factories[ItemCategory.Door] as ParkMiniGame)?.Initialization(parkLevel, effectAccumulatorView, level);
        }

        private void SwitchOnInputSystem(Quests.QuestType questType)
        {
            if (questType != Quests.QuestType.Sprint)
            {
                var playerController = (_playerController as PlayerController);
                playerController.InputAdaptep.SwitchAdapterToMiniGameMode();
                playerController.InputAdaptep.OnGameInteract += InputAdaptep_OnGameInteract;
            }


            //Unsubscribe(currentGame);

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

        private void HandleInteraction(ItemCategory category)
        {
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

            // _interactionSystem.OnInteraction += game.OnActionButtonClick;
            // Подписываемся на события


            game.OnMiniGameStart += startHandler;
            game.OnMiniGameComplete += completeHandler;
            currentMiniGame = game;
            

            if (category == ItemCategory.Door)
            {
                game.OnMiniGameComplete += OnMiniGameComplete;
            }
            game.Level = CurrentLevelIndex;
            game.StartGame();
        }

        private void OnMiniGameComplete(QuestType type)
        {
            _level.gameObject.SetActive(true);
            _playerController.SetPosition(Vector3.zero * 6);
        }

        public void UnregisterAll()
        {
            _interactionSystem.OnInteraction -= HandleInteraction;
            foreach (var game in _factories.Values)
            {
                game.OnMiniGameComplete -= OnMiniGameComplete;
                game.Dispose();
            }

            _factories.Clear();

            _level = null;
        }

        // Метод для отписки, если потребуется
        private void Unsubscribe(IMiniGame game)
        {
            if (_startHandlers.TryGetValue(game, out var startHandler))
                game.OnMiniGameStart -= startHandler;
            if (_completeHandlers.TryGetValue(game, out var completeHandler))
                game.OnMiniGameComplete -= completeHandler;
            _startHandlers.Remove(game);
            _completeHandlers.Remove(game);
        }
    }
}