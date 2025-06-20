#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using Knot.Localization;
using Knot.Localization.Data;

public class KnotAddEmptyKeyWindow : EditorWindow
{
    private const string WINDOW_TITLE = "KNOT – Add Key";
    private string _newKey = "";

    [MenuItem("Tools/KNOT/Add Empty Text Key…", priority = 100)]
    private static void ShowWindow() => GetWindow<KnotAddEmptyKeyWindow>(true, WINDOW_TITLE);

    private void OnGUI()
    {
        GUILayout.Label("Новый ключ локализации", EditorStyles.boldLabel);
        _newKey = EditorGUILayout.TextField("Key", _newKey);

        GUILayout.Space(8);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newKey)))
        {
            if (GUILayout.Button("Добавить"))
                TryAddKey(_newKey.Trim());
        }
    }

    // ──────────────────────────────────────────────────────────────

    private static void TryAddKey(string key)
    {
        // 1) Проверяем, что база подключена
        KnotDatabase db = KnotLocalization.Manager.Database;
        if (db == null)
        {
            EditorUtility.DisplayDialog(
                WINDOW_TITLE,
                "Default Database не назначена в Project Settings → KNOT → Localization.",
                "OK");
            return;
        }

        // 2) Добавляем сам ключ в Key Collection (если настроена)
        bool keyExists = false;
        var keyCollection = db.TextKeyCollections[0];
        {
            keyExists = keyCollection.Any(k => k.Key == key);
            if (!keyExists)
            {
                keyCollection.Add(new KnotKeyData(key));
                EditorUtility.SetDirty(keyCollection);
            }
        }

        // 3) Добавляем пустые значения во все Text Collections
        foreach (KnotLanguageData lang in db.Languages)
        {
            foreach (KnotAssetCollectionProvider provider in lang.CollectionProviders
                     .OfType<KnotAssetCollectionProvider>())
            {
                if (provider.Collection is KnotTextCollection texts &&
                    !texts.Any(t => t.Key == key))
                {
                    // Пустая строка-заглушка
                    texts.Add(new KnotTextData(key, string.Empty));
                    EditorUtility.SetDirty(texts);
                }
            }
        }

        AssetDatabase.SaveAssets();
        // string msg = keyExists
        //     ? $"Ключ «{key}» уже был в базе — добавлены только пустые значения."
        //     : $"Ключ «{key}» добавлен во все языки.";
        // Debug.Log($"[KNOT] {msg}");
    }
}
#endif
