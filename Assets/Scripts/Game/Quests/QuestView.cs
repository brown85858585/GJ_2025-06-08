using Knot.Localization.Components;
using TMPro;
using UnityEngine;

namespace Game.Quests
{
    public class QuestView : MonoBehaviour
    {
        [SerializeField] private TMP_Text questTMPText;
        [SerializeField] private KnotLocalizedTextMeshProUGUI quesTranslator;
        [SerializeField] private GameObject checkmark;

        public void Initialize(string key, bool isCompleted = true)
        {
            quesTranslator.KeyReference.Key = key;
            checkmark.SetActive(isCompleted);
            questTMPText.fontStyle = FontStyles.Normal;
        }
        
        public void SetActiveCheckmark()
        {
            
            questTMPText.fontStyle = FontStyles.Strikethrough;
            checkmark.SetActive(true);
        }
    }
}