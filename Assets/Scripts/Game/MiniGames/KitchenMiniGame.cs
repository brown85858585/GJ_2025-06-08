using System;
using Game.Quests;
using UnityEngine;

namespace Game.MiniGames
{
    public class KitchenMiniGame : IMiniGame
    {
        public QuestType QType { get; } = QuestType.Kitchen;
        public event Action<QuestType> OnMiniGameComplete;
        public event Action<QuestType> OnMiniGameStart;

        public void OnActionButtonClick()
        {
            throw new NotImplementedException();
        }

        public void StartGame()
        {
            Debug.Log("Kitchen Mini Game Started");
            OnMiniGameComplete?.Invoke(QType);
        }
    }
}