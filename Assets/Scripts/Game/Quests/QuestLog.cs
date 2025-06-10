using System.Collections.Generic;

namespace Game.Quests
{
    public class QuestLog
    {
        public List<Quest> Quests { get; private set; }
        
        private QuestLogView _view;

        public QuestLog(QuestLogView view)
        {
            Quests = new List<Quest>();
            _view = view;
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