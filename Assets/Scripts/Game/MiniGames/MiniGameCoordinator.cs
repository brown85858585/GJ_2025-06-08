using System;
using System.Collections.Generic;
using System.Linq;
using Game.Interactions;
using Game.Models;
using Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames
{
    public class MiniGameCoordinator
    {
        private readonly IInteractionSystem _interactionSystem;
        private readonly PlayerModel _playerModel;
        
        private readonly Dictionary<ItemCategory, IMiniGame> _factories = new();
        private readonly IPlayerController _playerController;
        private readonly GameObject _firstLevel;

        public List<IMiniGame> Games => _factories.Values.ToList();
        
        public MiniGameCoordinator(IInteractionSystem interactionSystem, PlayerModel playerModel,
            IPlayerController playerController, GameObject firstLevel)
        {
            _interactionSystem = interactionSystem;
            _playerModel = playerModel;
            _playerController = playerController;
            _firstLevel = firstLevel;
            RegisterGames();
            _interactionSystem.OnInteraction += HandleInteraction;
        }
        
        private void RegisterGames()
        {
            _factories[ItemCategory.Flower] = new FlowerMiniGame();
            _factories[ItemCategory.Kitchen] = new KitchenMiniGame();
            
            var parkLevel = Object.Instantiate(Resources.Load<GameObject>("Prefabs/MiniGame/ParkLevel"));
            _factories[ItemCategory.Door] = new ParkMiniGame(_playerController);
            (_factories[ItemCategory.Door] as ParkMiniGame)?.Initialization(parkLevel);
        }

        private void HandleInteraction(ItemCategory category)
        {
            if (!_factories.TryGetValue(category, out var game)) return;

            if (category == ItemCategory.Flower && _playerModel.ItemInHand != ItemCategory.WateringCan)
            {
                Debug.Log("You need a watering can to start the flower mini-game.");
                return;
            }

            if (category == ItemCategory.Door)
            {
                _firstLevel.SetActive(false);
                game.OnMiniGameComplete += _ =>
                {
                    //todo утечка
                    _firstLevel.SetActive(true);
                    _playerController.SetPosition(Vector3.zero * 6);
                };
            }
            
            game.StartGame();
        }
    }
}