using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MenuHorseSetup
{
    [MenuItem("Color-Shift Knight/Setup Menu Horse Hover")]
    public static void Setup()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/Menu.unity")
            throw new InvalidOperationException("Open Menu in Edit mode first.");

        var sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Arts/ChessGame_Pieces.png")
            .OfType<Sprite>().Single(s => s.name == "ChessGame_Pieces_0");
        var buttons = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            .Where(b => {
                var text = b.GetComponentInChildren<UnityEngine.UI.Text>(true);
                return text != null && (text.text.Trim() == "Start" || text.text.Trim() == "Option");
            }).ToArray();
        if (buttons.Length != 2)
            throw new InvalidOperationException("Expected the existing Start and Option buttons.");

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add menu horse hover");
        foreach (var button in buttons)
        {
            var hover = button.GetComponent<MenuButtonHover>();
            if (hover == null) hover = Undo.AddComponent<MenuButtonHover>(button.gameObject);
            var left = Horse(button, sprite, "HoverHorseLeft", true);
            var right = Horse(button, sprite, "HoverHorseRight", false);
            var serialized = new SerializedObject(hover);
            serialized.FindProperty("leftHorse").objectReferenceValue = left;
            serialized.FindProperty("rightHorse").objectReferenceValue = right;
            serialized.ApplyModifiedProperties();
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Menu horse hover configured: Start and Option, 4 sprite references, saved Menu scene.");
    }

    private static UnityEngine.UI.Image Horse(UnityEngine.UI.Button button, Sprite sprite, string name, bool left)
    {
        var child = button.transform.Find(name);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            Undo.RegisterCreatedObjectUndo(go, "Create hover horse");
            go.transform.SetParent(button.transform, false);
        }
        else go = child.gameObject;
        var rect = (RectTransform)go.transform;
        var image = go.GetComponent<UnityEngine.UI.Image>();
        Undo.RecordObject(rect, "Position hover horse");
        Undo.RecordObject(image, "Configure hover horse");
        float height = Mathf.Clamp(((RectTransform)button.transform).rect.height * 1.2f, 32f, 48f);
        float width = height * sprite.rect.width / sprite.rect.height;
        rect.anchorMin = rect.anchorMax = new Vector2(left ? 0f : 1f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2((left ? -1f : 1f) * (width / 2f + 10f), 0f);
        rect.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.enabled = false;
        return image;
    }
}
