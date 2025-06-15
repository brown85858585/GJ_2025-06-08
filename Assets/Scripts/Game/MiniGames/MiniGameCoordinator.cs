using System;
using System.Collections.Generic;
using Game.Interactions;
using Game.Models;
using UnityEngine;

namespace Game.MiniGames
{
    public class MiniGameCoordinator
    {
        private readonly IInteractionSystem _interactionSystem;
        private readonly PlayerModel _playerModel;
        
        private readonly Dictionary<ItemCategory, IMiniGame> _factories = new();
        
        public MiniGameCoordinator(IInteractionSystem interactionSystem, PlayerModel playerModel)
        {
            _interactionSystem = interactionSystem;
            _playerModel = playerModel;
            RegisterGames();
            _interactionSystem.OnInteraction += HandleInteraction;
        }
        
        private void RegisterGames()
        {
            _factories[ItemCategory.Flower] = new FlowerMiniGame();
            _factories[ItemCategory.Kitchen] = new KitchenMiniGame();
        }

        private void HandleInteraction(ItemCategory category)
        {
            if (!_factories.TryGetValue(category, out var game)) return;

            switch (_playerModel.ItemInHand)
            {
                
                case ItemCategory.WateringCan:
                    if (category == ItemCategory.Flower)
                    {
                        game.StartGame();
                    }
                    else
                    {
                        Debug.Log("You cannot start game without watering can.");
                    }
                    break;
                case ItemCategory.None:
                case ItemCategory.Bed:
                case ItemCategory.Door:
                case ItemCategory.Flower:
                case ItemCategory.Kitchen:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}