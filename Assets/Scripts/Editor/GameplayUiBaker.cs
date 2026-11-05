using ColorShiftKnight.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorShiftKnight.EditorTools
{
    // Menu command that puts the gameplay UI into the open scene, so it can be seen in the Hierarchy.
    public static class GameplayUiBaker
    {
        [MenuItem("Color-Shift Knight/Show UI In Scene")]
        public static void ShowUi()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Color-Shift Knight", "Stop Play mode first.", "OK");
                return;
            }

            GameplayBootstrap bootstrap = Object.FindAnyObjectByType<GameplayBootstrap>();
            if (bootstrap == null)
            {
                EditorUtility.DisplayDialog("Color-Shift Knight", "There is no GameplayBootstrap in the open scene.", "OK");
                return;
            }

            bootstrap.BuildPreview();
            EditorSceneManager.MarkSceneDirty(bootstrap.gameObject.scene);
        }
    }
}
