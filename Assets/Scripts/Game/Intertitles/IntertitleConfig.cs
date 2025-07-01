using UnityEngine;

namespace Game.Intertitles
{
    
    [CreateAssetMenu(fileName = "IntertitleConfig", menuName = "Game/Intertitle Config")]
    public class IntertitleConfig : ScriptableObject
    {
        public IntertitleCardView[] intertitles;
        
        public ScoreIntertitleCardView[] scoreIntertitles;
    }
}