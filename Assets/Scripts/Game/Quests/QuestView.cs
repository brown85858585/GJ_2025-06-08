using Knot.Localization.Components;
using TMPro;
using UI;
using UnityEngine;

namespace Game.Quests
{
    public class QuestView : MonoBehaviour
    {
        [SerializeField] private TMP_Text questTMPText;
        [SerializeField] private KnotLocalizedTextMeshProUGUI quesTranslator;
        [SerializeField] private CheckCrossToggle checkmark;

        public void Initialize(string key, bool isCompleted = true)
        {
            quesTranslator.KeyReference.Key = key;
            checkmark.IsOn = isCompleted;
            checkmark.gameObject.SetActive(isCompleted);
            questTMPText.fontStyle = FontStyles.Normal;
        }
        
        public void SetActiveCheckmark(bool isWin)
        {
            questTMPText.fontStyle = FontStyles.Strikethrough;
            
            checkmark.gameObject.SetActive(true);
            checkmark.IsOn = isWin;
        }
    }
}