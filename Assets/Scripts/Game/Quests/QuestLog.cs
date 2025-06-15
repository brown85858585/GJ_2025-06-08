using System;
using System.Collections.Generic;
using Player.Interfaces;

namespace Game.Quests
{
    public class QuestLog
    {
        public List<Quest> Quests { get; private set; }
        
        private QuestLogView _view;
        private IInputAdapter _inputAdapter;

        public QuestLog(QuestLogView view, IInputAdapter inputAdapter)
        {
            Quests = new List<Quest>();
            _inputAdapter = inputAdapter;
            _view = view;
            
            _inputAdapter.OnQuests += HandleOpenQuestLog;
        }

        private void HandleOpenQuestLog(bool obj)
        {
            if (obj)
            {
                _view?.OpenQuestLog();
            }
            else
            {
                _view?.CloseQuestLog();
            }
        }

        public void AddQuest(Quest quest)
        {
            if (quest != null && !Quests.Contains(quest))
            {
                Quests.Add(quest);
            }
            
            _view?.UpdateQuestList(Quests);
        }
        
        public void AddQuests(IEnumerable<Quest> quests)
        {
            if (quests == null) return;

            foreach (var quest in quests)
            {
                AddQuest(quest);
            }
        }

        public void CompleteQuest(ItemCategory questCategory)
        {
            switch (questCategory)
            {
                case ItemCategory.None:
                    break;
                case ItemCategory.Bed:
                    _view.CompleteQuest(QuestType.Work);
                    break;
                case ItemCategory.Door:
                    _view.CompleteQuest(QuestType.Sprint);
                    break;
                case ItemCategory.Flower:
                    _view.CompleteQuest(QuestType.Flower);
                    break;
                case ItemCategory.Kitchen:
                    _view.CompleteQuest(QuestType.Kitchen);
                    break;
                case ItemCategory.WateringCan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(questCategory), questCategory, null);
            }
        }
        

        public void RemoveQuest(Quest quest)
        {
            if (quest != null && Quests.Contains(quest))
            {
                Quests.Remove(quest);
                _view?.UpdateQuestList(Quests);
            }
        }

        public void ClearQuests()
        {
            Quests.Clear();
        }
    }
}