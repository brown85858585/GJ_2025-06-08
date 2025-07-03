using TMPro;
using UnityEngine;
using Utilities;

namespace Game.Intertitles
{
    public class ScoreIntertitleCardView : MonoBehaviour
    {
        [SerializeField] private int requiredScore;
        [SerializeField] private UIElementTweener tweener;
        
        public int RequiredScore => requiredScore;
        public UIElementTweener Tweener => tweener;

        public void SetScore(int score)
        {
        }
    }
}