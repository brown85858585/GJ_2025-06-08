using System;
using System.Collections.Generic;
using System.Linq;
using Effects.PostProcess;
using Game.Interactions;
using Game.MiniGames.Flower;
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
        private readonly MiniGamePrefabAccumulator _prefabAccumulator;

        private readonly Dictionary<ItemCategory, IMiniGame> _factories = new();
        private readonly IPlayerController _playerController;
        private Transform _level;

        private Canvas _miniGameCanvas;
        private int CurrentLevelIndex { get; set; } = 0;
        public static int DayLevel { get; set; } = 0; // Уровень дня, который будет использоваться в мини-играх

        // Словари для хранения делегатов
        private readonly Dictionary<IMiniGame, Action<Quests.QuestType>> _startHandlers = new();
        private readonly Dictionary<IMiniGame, Action<Quests.QuestType, bool>> _completeHandlers = new();
        private IMiniGame _currentMiniGame;
        private EffectAccumulatorView _effectAccumulatorView;

        public List<IMiniGame> Games => _factories.Values.ToList();

        public MiniGameCoordinator(IInteractionSystem interactionSystem, PlayerModel playerModel,
            IPlayerController playerController, MiniGamePrefabAccumulator prefabAccumulator)
        {
            _interactionSystem = interactionSystem;
            _playerModel = playerModel;
            _playerController = playerController;
            _prefabAccumulator = prefabAccumulator;

            _miniGameCanvas = Object.Instantiate(_prefabAccumulator.MiniGameCanvas);
        }

        public IMiniGame GetMiniGame(ItemCategory category)
        {
            return _factories.GetValueOrDefault(category);
        }
        
        public void SetLevel(int level)
        {
            CurrentLevelIndex = level;
            DayLevel = level; // Устанавливаем уровень дня для мини-игр
        }

        public void RegisterGames(Transform level, EffectAccumulatorView effectAccumulatorView)
        {
            _effectAccumulatorView = effectAccumulatorView;
            
            _interactionSystem.OnInteraction += HandleInteraction;
         
            _level = level;

            _factories[ItemCategory.Flower] = new FlowerMiniGame(_playerController, _prefabAccumulator, _miniGameCanvas);
            _factories[ItemCategory.Kitchen] = new KitchenMiniGame(_playerController);
            _factories[ItemCategory.Computer] = new WorkPapersMiniGame(_playerController);
            
            var parkLevel = Object.Instantiate(_prefabAccumulator.ParkLevelViews[CurrentLevelIndex]);
            _factories[ItemCategory.Door] = new ParkMiniGame(_playerController);
            (_factories[ItemCategory.Door] as ParkMiniGame)?.Initialization(parkLevel,
                effectAccumulatorView, level);
        }

        public void SetWateringCanView(GameObject wateringCanView)
        {
            var flowerGame = _factories[ItemCategory.Flower] as FlowerMiniGame;
                flowerGame.SetWateringCanView(wateringCanView);
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
            _currentMiniGame.OnActionButtonClick();
        }

        private void SwitchOffInputSystem(Quests.QuestType questType, bool isWin)
        {
            var game = _factories.Where(k => k.Key.ToString() == questType.ToString()).ToList();
           
            if (game.Count > 0)
                game.First().Value.IsCompleted = true;

            if (questType != Quests.QuestType.Sprint)
            {
                var playerController = (_playerController as PlayerController);
                playerController.InputAdaptep.SwitchAdapterToGlobalMode();
                playerController.InputAdaptep.OnGameInteract -= InputAdaptep_OnGameInteract;
            }
            Unsubscribe(_currentMiniGame);
            _currentMiniGame = null; // Сбрасываем текущую игру после переключения
        }

        private void HandleInteraction(ItemCategory category)
        {
            //КОСТЫЛИЩЕ
            if (CurrentLevelIndex == 4 &&
                category == ItemCategory.WMGKitchen)
            {
                SwitchOffInputSystem(QuestType.Kitchen, false);
                OnMiniGameComplete(QuestType.Kitchen, false);
                if (_factories.TryGetValue(ItemCategory.Kitchen, out var gameX))
                {
                    gameX.OnMiniGameComplete?.Invoke(QuestType.Kitchen, false);
                }
                return;
            }
            if(CurrentLevelIndex == 5)
            {
                if (category == ItemCategory.WMGDoor)
                {
                    
                    SwitchOffInputSystem(QuestType.Sprint, false);
                    OnMiniGameComplete(QuestType.Sprint, false); 
                    if (_factories.TryGetValue(ItemCategory.Door, out var gameX))
                    {
                        gameX.OnMiniGameComplete?.Invoke(QuestType.Sprint, false);
                    }
                    return;
                }
                if (category == ItemCategory.WMGKitchen)
                {
                    SwitchOffInputSystem(QuestType.Kitchen, false);
                    OnMiniGameComplete(QuestType.Kitchen, false);
                    if (_factories.TryGetValue(ItemCategory.Kitchen, out var gameX))
                    {
                        gameX.OnMiniGameComplete?.Invoke(QuestType.Kitchen, false);
                    }
                    return;
                }
                if (category == ItemCategory.WMGComputer)
                {
                    SwitchOffInputSystem(QuestType.Work, false);
                    OnMiniGameComplete(QuestType.Work, false);
                    if (_factories.TryGetValue(ItemCategory.Computer, out var gameX))
                    {
                        gameX.OnMiniGameComplete?.Invoke(QuestType.Work, false);
                    }
                    return;
                }
            }

            
            
            
            if (!_factories.TryGetValue(category, out var game)) return;

            if(game.IsCompleted) return;
            
            if (category == ItemCategory.Flower && _playerModel.ItemInHand != ItemCategory.WateringCan)
            {
                Debug.Log("You need a watering can to start the flower mini-game.");
                return;
            }
            
            // Создаём делегаты и сохраняем их для последующей отписки
            Action<Quests.QuestType> startHandler = SwitchOnInputSystem;
            Action<Quests.QuestType, bool> completeHandler = SwitchOffInputSystem;

            // Сохраняем делегаты
            _startHandlers[game] = startHandler;
            _completeHandlers[game] = completeHandler;

            // _interactionSystem.OnInteraction += game.OnActionButtonClick;
            // Подписываемся на события


            game.OnMiniGameStart += startHandler;
            game.OnMiniGameComplete += completeHandler;
            _currentMiniGame = game;

            game.OnMiniGameComplete += OnMiniGameComplete;
            
           if (game.QType == QuestType.Sprint)
           {
                          
           }
           else
           {
               _effectAccumulatorView.Blur();
           }
           
            game.Level = CurrentLevelIndex;

           
            // Запускаем мини-игру
            game.StartGame();
        }

        private void OnMiniGameComplete(QuestType questType, bool isWin)
        {
            _currentMiniGame = TypeMatchingForCurrentGame(questType);

            if (questType == QuestType.Sprint)
            {
                OnRunMiniGameComplete();
            }
            
            _currentMiniGame.IsCompleted = true;
            _currentMiniGame.OnMiniGameComplete -= OnMiniGameComplete;
            
            _effectAccumulatorView.Unblur();
        }

       
        private void OnRunMiniGameComplete()
        {
            _level.gameObject.SetActive(true);
            Debug.Log(DayLevel);
            if(DayLevel == 5) return;
            _playerController.SetPosition(new Vector3(11.005374f,0.0271916389f,4.8061552f));
            _playerController.SetRotation(new Quaternion(0f,0.984181404f,0f,-0.177163616f));
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
            if(game == null) return;

            if (_startHandlers.TryGetValue(game, out var startHandler))
                game.OnMiniGameStart -= startHandler;
            if (_completeHandlers.TryGetValue(game, out var completeHandler))
                game.OnMiniGameComplete -= completeHandler;
            _startHandlers.Remove(game);
            _completeHandlers.Remove(game);
        }
        
        private IMiniGame TypeMatchingForCurrentGame(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.Sprint:
                {
                    _factories.TryGetValue(ItemCategory.Door, out var game);
                    return game;
                }
                case QuestType.Kitchen:
                {
                    _factories.TryGetValue(ItemCategory.Kitchen, out var game);
                    return game;
                }
                case QuestType.Flower:
                {
                    _factories.TryGetValue(ItemCategory.Flower, out var game);
                    return game;
                }
                case QuestType.Work:
                {
                    _factories.TryGetValue(ItemCategory.Computer, out var game);
                    return game;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(questType), questType, null);
            }
        }

    }
}