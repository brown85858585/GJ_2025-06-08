using System.Collections.Generic;
using Game.MiniGames;
using Game.Quests;
using Player;

namespace Game.Monolog
{
    public class MonologMiniGameHandler
    {
        private List<ItemCategory> _interactableItemsThatIsMiniGame = new()
        {
            ItemCategory.Kitchen,
            ItemCategory.Computer,
            ItemCategory.Flower,
            ItemCategory.Door,
        };

        private readonly MiniGameCoordinator _miniGameCoordinator;
        private readonly IPlayerController _playerController;

        public MonologMiniGameHandler(MiniGameCoordinator miniGameCoordinator, IPlayerController playerController)
        {
            _miniGameCoordinator = miniGameCoordinator;
            _playerController = playerController;
        }
        public InteractionOutcome HandleMinigameDialogue(ItemCategory itemCategory)
        {
            if (!_interactableItemsThatIsMiniGame.Contains(itemCategory)) return InteractionOutcome.Continue;
            
            var miniGame = _miniGameCoordinator.GetMiniGame(itemCategory);
            
            if (miniGame == null)
            {
                return InteractionOutcome.Continue;
            }
            switch (miniGame)
            {
                case { IsCompleted: true }:
                    return InteractionOutcome.NeedFindMiniGameKey;
                case { IsCompleted: false }:
                    return miniGame.QType != QuestType.Flower ? InteractionOutcome.Interrupt : FlowerGameHandler();
                default:
                    return InteractionOutcome.Continue;
            }
        }

        private InteractionOutcome FlowerGameHandler()
        {
            return _playerController.Model.ItemInHand != ItemCategory.WateringCan ? InteractionOutcome.Continue : InteractionOutcome.Interrupt;
        }

        public string FindMiniGameKey(ItemCategory item)
        {
            return $"Finding...{item}";
        }
    }
}