using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class SetupGameLocalization
{
    [InitializeOnLoadMethod]
    private static void ScheduleInitialSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                (!File.Exists("Assets/Localization/GameLocalizationSettings.asset") ||
                 LocalizationEditorSettings.GetStringTableCollection("GameUI")?.SharedData.GetEntry("Level") == null)) Run();
        };
    }

    [Serializable] private class Entry { public string key; public string en; public string th; }
    [Serializable] private class Entries { public Entry[] entries; }

    [MenuItem("Color-Shift Knight/Setup Thai and English")]
    public static void Run()
    {
        Directory.CreateDirectory("Assets/Localization");
        AssetDatabase.Refresh();
        const string settingsPath = "Assets/Localization/GameLocalizationSettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(settingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, settingsPath);
        }
        LocalizationEditorSettings.ActiveLocalizationSettings = settings;
        foreach (string code in new[] { "en", "th" })
        {
            string path = "Assets/Localization/" + code + ".asset";
            var locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (locale == null)
            {
                locale = Locale.CreateLocale(code);
                AssetDatabase.CreateAsset(locale, path);
            }
            LocalizationEditorSettings.AddLocale(locale);
        }
        settings.GetStartupLocaleSelectors().Clear();
        settings.GetStartupLocaleSelectors().Add(new PlayerPrefLocaleSelector { PlayerPreferenceKey = "Game.Language" });
        settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId = new LocaleIdentifier("en") });
        LocalizationSettings.InitializeSynchronously = false;
        EditorUtility.SetDirty(settings);
        var strings = LocalizationEditorSettings.GetStringTableCollection("GameUI")
            ?? LocalizationEditorSettings.CreateStringTableCollection("GameUI", "Assets/Localization");
        var data = JsonUtility.FromJson<Entries>(File.ReadAllText("Assets/Localization/translations.json"));
        foreach (var table in strings.StringTables)
        {
            foreach (var entry in data.entries)
                table.AddEntry(entry.key, table.LocaleIdentifier.Code == "th" ? entry.th : entry.en);
            LocalizationEditorSettings.SetPreloadTableFlag(table, true);
            EditorUtility.SetDirty(table);
        }
        EditorUtility.SetDirty(strings.SharedData);
        EditorUtility.SetDirty(strings);
        var assets = LocalizationEditorSettings.GetAssetTableCollection("GameUIAssets")
            ?? LocalizationEditorSettings.CreateAssetTableCollection("GameUIAssets", "Assets/Localization");
        string fontGuid = AssetDatabase.AssetPathToGUID("Assets/Fonts/Sarabun-Regular.ttf");
        var addressables = AddressableAssetSettingsDefaultObject.GetSettings(true);
        addressables.CreateOrMoveEntry(fontGuid, addressables.DefaultGroup);
        foreach (var table in assets.AssetTables)
        {
            table.AddEntry("UIFont", fontGuid);
            LocalizationEditorSettings.SetPreloadTableFlag(table, true);
            EditorUtility.SetDirty(table);
        }
        EditorUtility.SetDirty(assets.SharedData);
        EditorUtility.SetDirty(assets);
        LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, strings);
        LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, assets);
        AssetDatabase.SaveAssets();
        AddressableAssetSettings.BuildPlayerContent(out var result);
        if (!string.IsNullOrEmpty(result.Error)) throw new Exception(result.Error);
        Debug.Log("Thai/English localization tables and font assets created and built.");
    }
}
