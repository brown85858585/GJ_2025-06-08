using UnityEngine;

namespace Game.Intertitles
{
    
    [CreateAssetMenu(fileName = "IntertitleConfig", menuName = "Game/Intertitle Config")]
    public class IntertitleConfig : ScriptableObject
    {
        [Tooltip("Список всех интерактивных титулов в порядке загрузки")]
        public IntertitleCardView[] intertitles;
    }
}