using System;
using Game.Quests;

namespace Game.MiniGames
{
    public interface IMiniGame : IDisposable
    {
        public QuestType QType { get; }
        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;
        void OnActionButtonClick();
        void StartGame();
    }
}