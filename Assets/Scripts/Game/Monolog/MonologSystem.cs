using System;
using System.Collections.Generic;
using Game.Interactions;
using Game.Levels;
using Game.MiniGames;
using Game.Quests;
using Knot.Localization;
using Knot.Localization.Data;
using Player;
using UnityEngine;

namespace Game.Monolog
{
    public class MonologSystem
    {
        private readonly IInteractionSystem _interactionSystem;
        private readonly IPlayerController _playerController;
        
        private Dictionary<string, int> _monologIndexesBySuffix = new ();
        private readonly KnotKeyCollection _keyCollection;
        private readonly LevelManager _levelManager;
        private readonly MonologMiniGameHandler _miniGameHandler;
        private readonly QuestLog _questLog;

        public MonologSystem(IInteractionSystem interactionSystem,
            IPlayerController playerController,
            LevelManager levelManager,
            MiniGameCoordinator miniGameCoordinator, 
            QuestLog logicQuestLog)
        {
            _interactionSystem = interactionSystem;
            _playerController = playerController;
            _levelManager = levelManager;
            _questLog = logicQuestLog;
            _keyCollection = KnotLocalization.Manager.Database.TextKeyCollections[0];

            _interactionSystem.OnInteraction += HandleInteraction;
            _interactionSystem.ExitInteraction += () =>
            {
                _playerController.Dialogue.CloseDialogue();
            };
            
            _miniGameHandler = new MonologMiniGameHandler(miniGameCoordinator, playerController);
        }

        private void HandleInteraction(ItemCategory item)
        {
            //todo ! for debug
            if(!_questLog.IsAllQuestsCompleted && item == ItemCategory.Bed) return;
            
            var outcome = _miniGameHandler.HandleMinigameDialogue(item);
            switch (outcome)
            {
                case InteractionOutcome.Interrupt:
                    return;
                case InteractionOutcome.NeedFindMiniGameKey:
                    OpenDialogue(_miniGameHandler.FindMiniGameKey(item, _levelManager.CurrentLevelIndex));
                    break;
                case InteractionOutcome.Continue:
                {
                    var suffix = GetItemKeySuffix(item);

                    if (_monologIndexesBySuffix.TryGetValue(suffix, out var currentIndex))
                        HandleExistingMonolog(suffix, currentIndex);
                    else
                        HandleNewMonolog(suffix);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetItemKeySuffix(ItemCategory item) => $"Day{_levelManager.CurrentLevelIndex + 1}_{item}";

        private void HandleExistingMonolog(string suffix, int currentIndex)
        {
            var nextIndex = currentIndex + 1;
            var nextKey = ComposeKey(suffix, nextIndex);

            if (_keyCollection.ContainsKey(nextKey))
            {
                // Сохраняем и показываем следующий диалог
                _monologIndexesBySuffix[suffix] = nextIndex;
                OpenDialogue(nextKey);

                // Если за ним больше нет ключа — дизейблим интерактив
                var afterNext = ComposeKey(suffix, nextIndex + 1);
                if (!_keyCollection.ContainsKey(afterNext))
                    _interactionSystem.DisableCurrentMultiplyInteractable();
            }
            else
            {
                // Больше нет новых диалогов — дизейблим и показываем текущий
                _interactionSystem.DisableCurrentMultiplyInteractable();
                OpenDialogue(ComposeKey(suffix, currentIndex));
            }
        }

        private void HandleNewMonolog(string suffix)
        {
            const int initialIndex = 0;
            var key = ComposeKey(suffix, initialIndex);

            if (_keyCollection.ContainsKey(key))
            {
                _monologIndexesBySuffix[suffix] = initialIndex;
                OpenDialogue(key);
            }
            else
            {
                Debug.LogWarning($"Monolog key '{key}' not found.");
            }
        }

        private string ComposeKey(string suffix, int index) => $"{suffix}{index}";

        public void OpenDialogue(string key) => _playerController.Dialogue.OpenDialogue(key);
        public void CloseDialogue() => _playerController.Dialogue.CloseDialogue();
    }
}