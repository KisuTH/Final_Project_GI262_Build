using System.Linq;
using ColorShiftKnight.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorShiftKnight.EditorTools
{
    public static class GameplaySceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Color-Shift Knight/Create Gameplay Scene")]
        public static void CreateGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            new GameObject("GameplayBootstrap").AddComponent<GameplayBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"Gameplay scene created at {ScenePath}");
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path))
                return;

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
