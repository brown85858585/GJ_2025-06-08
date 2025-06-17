using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Quests
{
    public class QuestLogView : MonoBehaviour
    {
        [SerializeField] Transform questListContainer;
        [SerializeField] QuestView _questViewPrefab;
        
        private Dictionary<QuestType, QuestView> _quests = new ();

        private void Start()
        {
            
            gameObject.SetActive(false);
        }

        public void UpdateQuestList(List<Quest> quests)
        {
            foreach (var quest in quests)
            {
                if (_quests.ContainsKey(quest.Type))
                {
                    _quests[quest.Type].Initialize($"{quest.Type}: {quest.Description}", quest.IsCompleted);
                    continue;
                }
        
                var questItem = Instantiate(_questViewPrefab, questListContainer);
                questItem.name = quest.Type + "Quest";
                questItem.Initialize($"{quest.Type}: {quest.Description}", quest.IsCompleted);
                _quests.Add(quest.Type, questItem);
            }
        }

        public void CompleteQuest(QuestType quest)
        {
            if (!_quests.TryGetValue(quest, value: out var questItem)) return;
            
            if (questItem != null)
            {
                questItem.SetActiveCheckmark();
            }
        }

        public void OpenQuestLog()
        {
            gameObject.SetActive(true);
        }

        public void CloseQuestLog()
        {
            gameObject.SetActive(false);
        }
    }
}