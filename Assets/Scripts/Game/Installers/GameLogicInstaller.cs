using Game.MiniGames;
using Game.MiniGames.Flower;
using Game.Quests;
using Player;

namespace Game.Installers
{
    public class GameLogicInstaller
    {
        public PlayerController PlayerController { get; }
        public MiniGameCoordinator MiniGameCoordinator { get; }
        public QuestLog QuestLog { get; }

        public GameLogicInstaller(CoreInstaller core, MiniGamePrefabAccumulator prefabAccumulator)
        {
            PlayerController = new PlayerController(core.PlayerModel, core.InputAdapter);
            MiniGameCoordinator = new MiniGameCoordinator(core.InteractionSystem, core.PlayerModel, PlayerController, prefabAccumulator);
            QuestLog = new QuestLog(core.InputAdapter, core.QuestsData);
        }
    }
}