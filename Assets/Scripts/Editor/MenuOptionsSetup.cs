using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MenuOptionsSetup
{
    [MenuItem("Color-Shift Knight/Setup Stone Options Panel")]
    public static void Setup()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/Menu.unity")
            throw new InvalidOperationException("Open Menu in Edit mode first.");
        var buttons = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<UnityEngine.UI.Button>(true)).ToArray();
        var option = buttons.Single(b => b.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text.Trim() == "Option");
        var start = buttons.Single(b => b.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text.Trim() == "Start");
        var canvas = option.GetComponentInParent<Canvas>();
        if (canvas.transform.Find("OptionsOverlay") != null)
            throw new InvalidOperationException("OptionsOverlay already exists; edit the existing panel.");
        const string artPath = "Assets/Arts/Game Option Stone Plaque.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(artPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
        var font = option.GetComponentInChildren<UnityEngine.UI.Text>(true).font;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Create stone options panel");
        var overlay = Rect("OptionsOverlay", canvas.transform, Vector2.zero, Vector2.one);
        var shade = overlay.gameObject.AddComponent<UnityEngine.UI.Image>();
        shade.color = new Color(0, 0, 0, .65f);
        var bounds = Rect("PanelBounds", overlay, new Vector2(.08f, .04f), new Vector2(.92f, .96f));
        var plaque = Rect("StonePlaque", bounds, Vector2.zero, Vector2.one);
        var art = plaque.gameObject.AddComponent<UnityEngine.UI.Image>();
        art.sprite = sprite;
        art.preserveAspect = true;
        var fit = plaque.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio = sprite.rect.width / sprite.rect.height;
        Label("VolumeLabel", plaque, font, "MASTER VOLUME", new Vector2(.23f,.59f), new Vector2(.64f,.69f), 22, TextAnchor.MiddleLeft);
        var value = Label("VolumeValue", plaque, font, "100%", new Vector2(.65f,.59f), new Vector2(.78f,.69f), 22, TextAnchor.MiddleRight);
        var sliderRect = Rect("VolumeSlider", plaque, new Vector2(.23f,.43f), new Vector2(.78f,.55f));
        var slider = sliderRect.gameObject.AddComponent<UnityEngine.UI.Slider>();
        var track = Rect("Track", sliderRect, new Vector2(0,.4f), new Vector2(1,.6f));
        track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.12f,.11f,.1f,1);
        var fillArea = Rect("FillArea", sliderRect, new Vector2(0,.4f), new Vector2(1,.6f));
        var fill = Rect("Fill", fillArea, Vector2.zero, Vector2.one);
        fill.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.78f,.71f,.6f,1);
        var handleArea = Rect("HandleArea", sliderRect, Vector2.zero, Vector2.one);
        handleArea.offsetMin = new Vector2(10,0);
        handleArea.offsetMax = new Vector2(-10,0);
        var handle = Rect("Handle", handleArea, new Vector2(0,.18f), new Vector2(0,.82f));
        handle.sizeDelta = new Vector2(20,0);
        var handleImage = handle.gameObject.AddComponent<UnityEngine.UI.Image>();
        handleImage.color = new Color(.94f,.89f,.8f,1);
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        var backRect = Rect("BackButton", plaque, new Vector2(.37f,.21f), new Vector2(.63f,.32f));
        var backImage = backRect.gameObject.AddComponent<UnityEngine.UI.Image>();
        backImage.color = new Color(.75f,.69f,.62f,1);
        var back = backRect.gameObject.AddComponent<UnityEngine.UI.Button>();
        back.targetGraphic = backImage;
        var backLabel = Label("Label", backRect, font, "BACK", Vector2.zero, Vector2.one, 20, TextAnchor.MiddleCenter);
        backLabel.color = new Color(.13f,.12f,.11f,1);
        var controller = Undo.AddComponent<MenuOptionsPanel>(canvas.gameObject);
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("panel").objectReferenceValue = overlay.gameObject;
        serialized.FindProperty("optionButton").objectReferenceValue = option;
        serialized.FindProperty("startButton").objectReferenceValue = start;
        serialized.FindProperty("backButton").objectReferenceValue = back;
        serialized.FindProperty("volumeSlider").objectReferenceValue = slider;
        serialized.FindProperty("volumeValue").objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
        Undo.RecordObject(option, "Wire Option to panel");
        // Option owns only the opening action; Start's existing callback is preserved.
        for (int i = option.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(option.onClick, i);
        UnityEventTools.AddPersistentListener(option.onClick, controller.Open);
        UnityEventTools.AddPersistentListener(back.onClick, controller.Close);
        UnityEventTools.AddPersistentListener(slider.onValueChanged, controller.SetVolume);
        overlay.gameObject.SetActive(false);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Stone Options panel saved: Option opens overlay, Back closes, slider controls master volume; no scene transition.");
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create options UI");
        go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static UnityEngine.UI.Text Label(string name, Transform parent, Font font, string value,
        Vector2 min, Vector2 max, int size, TextAnchor alignment)
    {
        var rect = Rect(name, parent, min, max);
        var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = size;
        text.color = new Color(.95f,.92f,.86f,1);
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }
}
