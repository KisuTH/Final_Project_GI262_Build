using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace ColorShiftKnight.Gameplay
{
    public sealed class LanguageChoiceButton : MonoBehaviour
    {
        private string localeCode;
        private UnityEngine.UI.Button button;
        private UnityEngine.UI.Text label;
        private GameObject check;
        private readonly LocalizedAsset<Font> font = new LocalizedAsset<Font>
        {
            TableReference = GameLocalization.AssetTable,
            TableEntryReference = "UIFont"
        };

        public static void Configure(UnityEngine.UI.Button target, string code)
        {
            var choice = target.gameObject.AddComponent<LanguageChoiceButton>();
            choice.localeCode = code;
            choice.button = target;
            choice.label = target.GetComponentInChildren<UnityEngine.UI.Text>(true);
            choice.label.text = code == "th" ? "ไทย" : "English";
            choice.label.color = Color.white;
            choice.label.resizeTextForBestFit = true;
            choice.label.resizeTextMinSize = 12;
            var labelRect = (RectTransform)choice.label.transform;
            labelRect.anchorMin = new Vector2(.34f, .08f);
            labelRect.anchorMax = new Vector2(.88f, .92f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var box = Rect("Checkbox", target.transform);
            box.anchorMin = new Vector2(.15f, .27f);
            box.anchorMax = new Vector2(.28f, .73f);
            box.offsetMin = box.offsetMax = Vector2.zero;
            Border(box, "Top", new Vector2(0, .9f), Vector2.one);
            Border(box, "Bottom", Vector2.zero, new Vector2(1, .1f));
            Border(box, "Left", Vector2.zero, new Vector2(.1f, 1));
            Border(box, "Right", new Vector2(.9f, 0), Vector2.one);
            var tick = Rect("Checkmark", box);
            tick.anchorMin = Vector2.zero;
            tick.anchorMax = Vector2.one;
            tick.offsetMin = tick.offsetMax = Vector2.zero;
            choice.check = tick.gameObject;
            Stroke(tick, new Vector2(.32f, .40f), new Vector2(.35f, .14f), -45);
            Stroke(tick, new Vector2(.62f, .52f), new Vector2(.64f, .14f), 50);
            target.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            target.onClick.AddListener(choice.Select);
            target.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            choice.Refresh();
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Border(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var rect = Rect(name, parent);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }

        private static void Stroke(Transform parent, Vector2 center, Vector2 size, float angle)
        {
            var rect = Rect("TickStroke", parent);
            rect.anchorMin = center - size * .5f;
            rect.anchorMax = center + size * .5f;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            rect.gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
            font.AssetChanged += ApplyFont;
            StartCoroutine(RefreshWhenReady());
        }

        private IEnumerator RefreshWhenReady()
        {
            yield return LocalizationSettings.InitializationOperation;
            Refresh();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
            font.AssetChanged -= ApplyFont;
        }

        private void ApplyFont(Font value)
        {
            // Configure can run after OnEnable when added to an active object.
            if (label == null) label = GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (value != null && label != null) label.font = value;
        }

        private void LocaleChanged(Locale _) { Refresh(); }
        private void Select() { GameLocalization.SelectLanguage(localeCode); }

        private void Refresh()
        {
            if (button == null || check == null) return;
            var operation = LocalizationSettings.InitializationOperation;
            bool ready = operation.IsDone && LocalizationSettings.SelectedLocale != null;
            button.interactable = ready;
            bool selected = ready && LocalizationSettings.SelectedLocale.Identifier.Code == localeCode;
            check.SetActive(selected);
            var colors = button.colors;
            colors.normalColor = selected ? Color.white : new Color(.68f, .68f, .68f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = selected ? Color.white : new Color(.82f, .82f, .82f);
            button.colors = colors;
        }
    }
}
