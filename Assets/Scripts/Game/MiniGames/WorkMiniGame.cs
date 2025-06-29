using System;
using Game.Quests;
using UnityEngine;

namespace Game.MiniGames
{
    public class WorkMiniGame: IMiniGame
    {
        public QuestType QType { get; } = QuestType.Work;
        public int Level { get ; set; }

        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;
        public WorkMiniGame()
        {
            Level = MiniGameCoordinator.DayLevel;
        }

        public void Dispose()
        {
            // TODO
            //throw new NotImplementedException();
        }

        public void OnActionButtonClick()
        {
            // TODO
           // throw new NotImplementedException();
        }

        public void StartGame()
        {
            Debug.Log("Work Mini Game Started");
            OnMiniGameComplete?.Invoke(QType);
        }
    }
}