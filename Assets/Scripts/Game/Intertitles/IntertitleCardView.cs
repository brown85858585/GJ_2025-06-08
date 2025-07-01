using UI;
using UnityEngine;

namespace Game.Intertitles
{
    public class IntertitleCardView : MonoBehaviour
    {
        [SerializeField] private SequentialTextFader sequentialTextFader;
        
        public SequentialTextFader SequentialTextFader => sequentialTextFader;
    }
}