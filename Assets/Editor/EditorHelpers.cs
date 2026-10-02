using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorHelpers
{
    [MenuItem("Tools/Start game")]
    public static void LoadBootstrapperScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/BootstrapperScene.unity");
        EditorApplication.isPlaying = true;
    }
}
