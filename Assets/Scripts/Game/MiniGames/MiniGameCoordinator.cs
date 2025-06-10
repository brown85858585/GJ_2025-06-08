using System;
using System.Collections.Generic;
using Game.Interactions;

namespace Game.MiniGames
{
    public class MiniGameCoordinator
    {
        private readonly IInteractionSystem _interactionSystem;
        
        private readonly Dictionary<ItemCategory, Func<IMiniGame>> _factories = new();
        
        public MiniGameCoordinator(IInteractionSystem interactionSystem)
        {
            _interactionSystem = interactionSystem;
            RegisterGames();
            _interactionSystem.OnInteraction += HandleInteraction;
        }
        
        private void RegisterGames()
        {
            _factories[ItemCategory.Flower] = () => new FlowerMiniGame();
            _factories[ItemCategory.Kitchen] = () => new KitchenMiniGame();
        }

        private void HandleInteraction(ItemCategory category)
        {
            if (!_factories.TryGetValue(category, out var factory)) return;
            
            var game = factory();
            game.StartGame();
        }
    }
}