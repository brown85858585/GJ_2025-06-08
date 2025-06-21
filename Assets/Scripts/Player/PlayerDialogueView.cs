using Knot.Localization.Components;
using TMPro;
using UnityEngine;

namespace Player
{
    public class PlayerDialogueView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        public void SetText(string text)
        {
            if (_text != null)
            {
                _text.GetComponent<KnotLocalizedTextMeshProUGUI>().KeyReference.Key = text;
            }
            else
            {
                Debug.LogWarning("Text component is not assigned in PlayerDialogueView.");
            }
        }
    }
}