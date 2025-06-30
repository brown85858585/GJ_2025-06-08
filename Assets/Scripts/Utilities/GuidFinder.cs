#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    public static class GuidFinder
    {
        [MenuItem("Tools/Find Asset By GUID")]
        public static void FindByGuid()
        {
            const string guid = "2c0727325dc555844a98a33d796c3597";
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"GUID {guid} не найден в папке Assets.");
                
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            Debug.Log($"GUID {guid} → {path}", asset);
            Selection.activeObject = asset;   // подсветить в Project View
        }
    }
}
#endif

