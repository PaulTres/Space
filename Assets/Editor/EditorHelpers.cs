using AudioSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class EditorHelpers
{
    [MenuItem("Tools/Add Sound Hook To All Buttons")]
    public static void AddHookToAllButtons()
    {
        int prefabCount = 0;
        int sceneCount = 0;

        // 1. All Button prefabs in the project
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Button[] buttons = prefab.GetComponentsInChildren<Button>(true);
            bool changed = false;

            foreach (Button btn in buttons)
            {
                if (btn.GetComponent<ButtonSoundHook>() == null)
                {
                    Undo.AddComponent<ButtonSoundHook>(btn.gameObject);
                    changed = true;
                    prefabCount++;
                }
            }

            if (changed)
                EditorUtility.SetDirty(prefab);
        }

        // 2. All Buttons currently in open scenes (catches non-prefab instances)
        Button[] sceneButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in sceneButtons)
        {
            if (btn.GetComponent<ButtonSoundHook>() == null)
            {
                Undo.AddComponent<ButtonSoundHook>(btn.gameObject);
                sceneCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Added ButtonSoundHook to {prefabCount} prefab buttons and {sceneCount} scene buttons.");
    }


    [MenuItem("Tools/Start game")]
    public static void LoadBootstrapperScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/BootstrapperScene.unity");
        EditorApplication.isPlaying = true;
    }
}
