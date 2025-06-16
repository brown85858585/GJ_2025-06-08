using System;
using Game.Quests;

namespace Game.MiniGames
{
    public interface IMiniGame
    {
        public QuestType QType { get; }
        public event Action<QuestType> OnMiniGameComplete;
        void StartGame();
    }
}