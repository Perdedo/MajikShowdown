using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FontManager : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Font Manager")]
    public static void Open()
    {
        GetWindow<FontManager>("Font Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("Apply Font To Entire Project", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font", newFont, typeof(TMP_FontAsset),false);
        GUILayout.Space(10);
        if (GUILayout.Button("Apply"))
        {
            if (newFont == null)
            {
                Debug.LogError("Choose a font first.");
                return;
            }
            ApplyToPrefabs();
            ApplyToScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Done!");
        }
    }

    private void ApplyToPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            bool changed = false;
            foreach (TMP_Text text in texts)
            {
                if (text.font != newFont)
                {
                    text.font = newFont;
                    EditorUtility.SetDirty(text);
                    changed = true;
                }
            }

            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log("Finished Prefabs");
    }

    private void ApplyToScenes()
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        foreach (string guid in sceneGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path);
            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool changed = false;
            foreach (TMP_Text text in texts)
            {
                if (text.font != newFont)
                {
                    Undo.RecordObject(text, "Change Font");
                    text.font = newFont;
                    EditorUtility.SetDirty(text);
                    changed = true;
                }
            }

            if (changed) EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("Finished Scenes");
    }
}