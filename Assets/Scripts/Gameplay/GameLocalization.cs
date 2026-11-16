using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace ColorShiftKnight.Gameplay
{
    public static class GameLocalization
    {
        public const string StringTable = "GameUI";
        public const string AssetTable = "GameUIAssets";

        public static void Set(UnityEngine.UI.Text text, string key, params object[] arguments)
        {
            var binding = text.GetComponent<LocalizedGameText>();
            if (binding == null) binding = text.gameObject.AddComponent<LocalizedGameText>();
            binding.SetKey(key, arguments);
        }

        public static void Label(UnityEngine.UI.Text text, string english)
        {
            string key;
            switch (english)
            {
                case "Start": key = "Start"; break;
                case "OPTIONS": case "Option": key = "Options"; break;
                case "PAUSE": key = "Pause"; break;
                case "PAUSED": key = "Paused"; break;
                case "RESUME": key = "Resume"; break;
                case "BACK": key = "Back"; break;
                case "BACK TO MENU": key = "BackToMenu"; break;
                case "New Level": key = "NewLevel"; break;
                case "Undo": key = "Undo"; break;
                case "Hint": key = "Hint"; break;
                case "MAIN SOUND": key = "MainSound"; break;
                case "MUSIC": key = "Music"; break;
                case "EFFECT": key = "Effect"; break;
                default: return;
            }
            Set(text, key);
        }

        public static void SelectLanguage(string code)
        {
            var operation = LocalizationSettings.InitializationOperation;
            if (!operation.IsDone) return;
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale == null) return;
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString("Game.Language", code);
            PlayerPrefs.Save();
        }
    }
}
