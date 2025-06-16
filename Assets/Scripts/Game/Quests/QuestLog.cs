using System.Collections.Generic;
using Player.Interfaces;

namespace Game.Quests
{
    public class QuestLog
    {
        public List<Quest> Quests { get; private set; }
        
        private QuestLogView _view;
        private IInputAdapter _inputAdapter;
        private readonly QuestsModel _model;

        public QuestLog(QuestLogView view, IInputAdapter inputAdapter, QuestsModel questsModel)
        {
            Quests = new List<Quest>();
            _inputAdapter = inputAdapter;
            _view = view;
            _model = questsModel;
            AddQuests(_model.GetQuests());
            
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

        private void AddQuest(Quest quest)
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

        public void CompleteQuest(QuestType questCategory)
        {
            _view.CompleteQuest(questCategory);
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