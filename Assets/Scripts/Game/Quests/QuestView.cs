using TMPro;
using UnityEngine;

namespace Game.Quests
{
    public class QuestView : MonoBehaviour
    {
        [SerializeField] private TMP_Text questDescription;
        [SerializeField] private GameObject checkmark;

        public string QuestDescription => questDescription.text;

        public void Initialize(string description, bool isCompleted = true)
        {
            questDescription.text = description;
            checkmark.SetActive(isCompleted);
        }
    }
}