using System;
using Game.Quests;
using UnityEngine;

namespace Game.MiniGames
{
    public class WorkMiniGame: IMiniGame
    {
        public QuestType QType { get; } = QuestType.Work;
        public event Action<QuestType> OnMiniGameComplete;

        public void StartGame()
        {
            Debug.Log("Kitchen Mini Game Started");
            OnMiniGameComplete?.Invoke(QType);
        }
    }
}