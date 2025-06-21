using System.Collections.Generic;
using Game.Interactions;
using Game.Levels;
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

       

        public MonologSystem(IInteractionSystem interactionSystem, IPlayerController playerController, LevelManager levelManager)
        {
            _interactionSystem = interactionSystem;
            _playerController = playerController;
            _levelManager = levelManager;
            _keyCollection = KnotLocalization.Manager.Database.TextKeyCollections[0];

            _interactionSystem.OnInteraction += HandleInteraction;
            _interactionSystem.ExitInteraction += () =>
            {
                _playerController.Dialogue.CloseDialogue();
            };
        }

        private void HandleInteraction(ItemCategory item)
        {
            var itemKeySuffix = $"Day{_levelManager.CurrentLevelIndex+1}_{item}";
            
            if (_monologIndexesBySuffix.TryGetValue(itemKeySuffix, out var indexPostfix))
            {
                indexPostfix++;
                if(_keyCollection.ContainsKey(itemKeySuffix+indexPostfix.ToString()))
                {
                    _monologIndexesBySuffix[itemKeySuffix] = indexPostfix;
                    
                    _playerController.Dialogue.OpenDialogue(itemKeySuffix + indexPostfix);
                }
                else
                {
                    _playerController.Dialogue.OpenDialogue(itemKeySuffix + _monologIndexesBySuffix[itemKeySuffix]);
                }

            }
            else
            {
                const int zeroPostfix = 0;
                if (_keyCollection.ContainsKey(itemKeySuffix + zeroPostfix.ToString()))
                {
                    _monologIndexesBySuffix[itemKeySuffix] = zeroPostfix;
                    _playerController.Dialogue.OpenDialogue(itemKeySuffix + zeroPostfix.ToString());
                } 
                else
                {
                    // Handle case where the key does not exist
                    // For example, you might log a message or show a default message
                    Debug.LogWarning($"Monolog key '{itemKeySuffix + zeroPostfix}' not found.");
                }
            }
        }
    }

    internal class Phrases
    {
        public List<string> PhrasesList { get; private set; } = new List<string>();

        public Phrases(string[] phrases = null)
        {
            if (phrases != null)
            {
                PhrasesList.AddRange(phrases);
            }
        } 
        private Dictionary<ItemCategory, Phrases> _monologPhrases = new Dictionary<ItemCategory, Phrases>
                 {
                     {
                         ItemCategory.Bed, new Phrases(new[]
                         {
                             "I should get some rest.",
                             "This bed looks comfortable.",
                             "I could use a nap."
                         })
                     },
                     {
                         ItemCategory.Door, new Phrases(new[]
                         {
                             "I wonder where this door leads.",
                             "I should check if it's locked.",
                             "Maybe I can find something interesting behind this door."
                         })
                     },
                     {
                         ItemCategory.Flower, new Phrases(new[]
                         {
                             "These flowers are beautiful.",
                             "I love the smell of these flowers.",
                             "They remind me of spring."
                         })
                     },
                     {
                         ItemCategory.Kitchen, new Phrases(new[]
                         {
                             "I could use a snack from the kitchen.",
                             "The kitchen is well stocked.",
                             "I wonder if I can cook something here."
                         })
                     },
                     {
                         ItemCategory.WateringCan, new Phrases(new[]
                         {
                             "I should water the plants with this can.",
                             "This watering can is handy.",
                             "I love taking care of the plants."
                         })
                     },
                     {
                         ItemCategory.Computer, new Phrases(new[]
                         {
                             "I should check my emails on this computer.",
                             "Maybe I can find some useful information online.",
                             "This computer looks like it has a lot of potential."
                         })
                     }
                 };

    }
}