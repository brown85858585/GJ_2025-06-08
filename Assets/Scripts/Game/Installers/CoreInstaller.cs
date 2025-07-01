using Game.Installers;
using Game.Interactions;
using Game.Intertitles;
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
        public IntertitleSystem IntertitleSystem{ get; }

        public CoreInstaller(PlayerInput playerInput, IntertitleConfig intertitleConfig)
        {
            QuestsData = new QuestsData();
            PlayerModel = new PlayerModel();
            InputAdapter = new InputAdapter(playerInput);
            InteractionSystem = new InteractionSystem(InputAdapter);
            IntertitleSystem = new IntertitleSystem(intertitleConfig, InputAdapter);
        }
    }
}