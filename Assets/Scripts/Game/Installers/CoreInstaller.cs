using Game.Interactions;
using Game.Quests;
using Player;
using UnityEngine.InputSystem;

namespace Game
{
    public class CoreInstaller
    {
        public QuestsData QuestsData { get; }
        public PlayerModel PlayerModel { get; }
        public InputAdapter InputAdapter { get; }
        public InteractionSystem InteractionSystem { get; }

        public CoreInstaller(PlayerInput playerInput)
        {
            QuestsData = new QuestsData();
            PlayerModel = new PlayerModel();
            InputAdapter = new InputAdapter(playerInput);
            InteractionSystem = new InteractionSystem(InputAdapter);
        }
    }
}