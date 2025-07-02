using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Quests
{
    public class QuestLogView : MonoBehaviour
    {
        [SerializeField] private Transform questListContainer;
        [FormerlySerializedAs("_questViewPrefab")] 
        [SerializeField] private QuestView questViewPrefab;
        
        private readonly Dictionary<QuestType, QuestView> _quests = new ();

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
                    _quests[quest.Type].Initialize(quest.Key, quest.IsCompleted);
                    continue;
                }
        
                var questView = Instantiate(questViewPrefab, questListContainer);
                questView.name = quest.Type + "Quest";
                questView.Initialize(quest.Key, quest.IsCompleted);
                _quests.Add(quest.Type, questView);
            }
        }

        public void CompleteQuest(QuestType quest, bool isWin)
        {
            if (!_quests.TryGetValue(quest, value: out var questItem)) return;
            
            if (questItem != null)
            {
                questItem.SetActiveCheckmark(isWin);
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