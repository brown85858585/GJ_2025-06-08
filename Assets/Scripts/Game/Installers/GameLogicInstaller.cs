using Game.MiniGames;
using Game.Quests;
using Player;

namespace Game
{
    public class GameLogicInstaller
    {
        public PlayerController PlayerController { get; }
        public MiniGameCoordinator MiniGameCoordinator { get; }
        public QuestLog QuestLog { get; }

        public GameLogicInstaller(CoreInstaller core)
        {
            PlayerController = new PlayerController(core.PlayerModel, core.InputAdapter);
            MiniGameCoordinator = new MiniGameCoordinator(core.InteractionSystem, core.PlayerModel, PlayerController);
            QuestLog = new QuestLog(core.InputAdapter, core.QuestsData);
        }
    }
}