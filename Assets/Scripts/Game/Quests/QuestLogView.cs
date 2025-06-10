using System.Collections.Generic;
using UnityEngine;

namespace Game.Quests
{
    public class QuestLogView : MonoBehaviour
    {
        [SerializeField] Transform questListContainer;
        [SerializeField] QuestView _questViewPrefab;
        
        private List<Quest> _quests = new List<Quest>();
        public void UpdateQuestList(List<Quest> quests)
        {
            foreach (var quest in quests)
            {
                if(_quests.Contains(quest))continue;
                
                var questItem = Instantiate(_questViewPrefab, questListContainer);
                questItem.name = quest.Type + "Quest";
                questItem.Initialize($"{quest.Type}: {quest.Description}",quest. IsCompleted );
                _quests.Add(quest);
            }
        }
    }
}