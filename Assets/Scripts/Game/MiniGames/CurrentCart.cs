using Knot.Localization.Components;
using UnityEngine;

namespace Game.MiniGames
{
    public class CurrentCart : MonoBehaviour
    {

        [SerializeField] private KnotLocalizedTextMeshProUGUI content;
        [SerializeField] private KnotLocalizedTextMeshProUGUI senderName;

        public KnotLocalizedTextMeshProUGUI ContentText => content;
        public KnotLocalizedTextMeshProUGUI SanderText => senderName;



    }
}
