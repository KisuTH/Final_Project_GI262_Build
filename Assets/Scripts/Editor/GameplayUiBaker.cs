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

            AssignSheet(bootstrap, "DelightedSheet", "Assets/Arts/Desire Fullfilled.png");
            AssignSheet(bootstrap, "FrustratedSheet", "Assets/Arts/Desired Unfullfilled.png");
            bootstrap.BuildPreview();
            EditorSceneManager.MarkSceneDirty(bootstrap.gameObject.scene);
        }

        // Fills an empty art slot of the GameplayBootstrap with the picture at the given path.
        private static void AssignSheet(GameplayBootstrap bootstrap, string fieldName, string path)
        {
            SerializedObject so = new SerializedObject(bootstrap);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null || property.objectReferenceValue != null)
            {
                return;
            }
            property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            so.ApplyModifiedProperties();
        }
    }
}
