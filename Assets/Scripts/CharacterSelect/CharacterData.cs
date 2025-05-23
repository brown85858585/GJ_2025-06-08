using UnityEngine;

namespace CharacterSelect
{
    [CreateAssetMenu(menuName = "Game/Character Data")]
    public class CharacterData : ScriptableObject
    {
        public string id;
        public Sprite icon;
        public int maxLevel;
        public int baseXp;
        public int level;
    }
}