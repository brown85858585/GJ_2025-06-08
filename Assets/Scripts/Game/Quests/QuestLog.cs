using System;
using System.Collections.Generic;
using System.Linq;
using Game.MiniGames;
using Player.Interfaces;
using UnityEngine.PlayerLoop;

namespace Game.Quests
{
    public class QuestLog : IDisposable
    {
        public List<Quest> Quests { get; private set; }
        
        private QuestLogView _view;
        private IInputAdapter _inputAdapter;
        private readonly QuestsData _data;
        private MiniGameCoordinator _coordinator;
        public event Action AllQuestsCompleted;
        public bool IsAllQuestsCompleted { get; private set; }

        public QuestLog(IInputAdapter inputAdapter, QuestsData questsData)
        {
            Quests = new List<Quest>();
            
            _inputAdapter = inputAdapter;
            _data = questsData;
        }

        public void Initialization(QuestLogView view, MiniGameCoordinator coordinator)
        {
            _view = view;
            AddQuests(_data.GetQuests());
            _inputAdapter.OnQuests += HandleOpenQuestLog;
            
            _coordinator = coordinator;
            foreach (var game in coordinator.Games)
            {
                game.OnMiniGameComplete += CompleteQuest;
            }
        }
        
        public void Dispose()
        {
            _inputAdapter.OnQuests -= HandleOpenQuestLog;
            _view = null;
            _inputAdapter = null;
            
            foreach (var game in _coordinator.Games)
            {
                game.OnMiniGameComplete -= CompleteQuest;
            }
            _coordinator = null;
        }
        private void HandleOpenQuestLog(bool press)
        {
            if (press)
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
            var quest = Quests.FirstOrDefault(q => q.Type == questCategory);
            if (quest == null || quest.IsCompleted)
                return;

            quest.IsCompleted = true;

            _view.CompleteQuest(questCategory);

            // Check if all quests are completed
            if (Quests.Any(q => !q.IsCompleted))
                return;

            AllQuestsCompleted?.Invoke();
            IsAllQuestsCompleted = true;
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

        public void ResetQuests()
        {
            foreach (var quest in Quests)
            {
                quest.IsCompleted = false;
            }
            IsAllQuestsCompleted = false;
            _view?.UpdateQuestList(Quests);
        }
    }
}