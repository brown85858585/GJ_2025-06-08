using System;
using Game.Quests;

namespace Game.MiniGames
{
    public interface IMiniGame : IDisposable
    {
        public int Level { get; set; }
        public QuestType QType { get; }
        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;
        void OnActionButtonClick();
        void StartGame();
        bool IsCompleted { get; set; }
        bool IsWin { get;}
    }
}