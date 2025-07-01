using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(fileName = "LevelsConfig", menuName = "Game/Levels Config")]
    public class LevelsConfig : ScriptableObject
    {
        [System.Serializable]
        public struct LevelEntry
        {
            [Tooltip("Имя уровня для удобства в инспекторе")] public string levelName;
            [Tooltip("Префаб сцены уровня")] public GameObject levelPrefab;
        }

        [Tooltip("Список всех уровней в порядке загрузки")] public LevelEntry[] levels;
    }
}