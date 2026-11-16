using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace ColorShiftKnight.Gameplay
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public sealed class LocalizedGameText : MonoBehaviour
    {
        private UnityEngine.UI.Text text;
        private LocalizeStringEvent stringEvent;
        private readonly LocalizedAsset<Font> localizedFont = new LocalizedAsset<Font>
        {
            TableReference = GameLocalization.AssetTable,
            TableEntryReference = "UIFont"
        };

        private void Awake()
        {
            if (text != null) return;
            text = GetComponent<UnityEngine.UI.Text>();
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = text.fontSize;
            stringEvent = gameObject.AddComponent<LocalizeStringEvent>();
            stringEvent.OnUpdateString.AddListener(ApplyText);
        }

        private void OnEnable() { localizedFont.AssetChanged += ApplyFont; }
        private void OnDisable() { localizedFont.AssetChanged -= ApplyFont; }

        public void SetKey(string key, object[] arguments)
        {
            if (text == null) Awake();
            var reference = stringEvent.StringReference;
            reference.Arguments = arguments;
            reference.SetReference(GameLocalization.StringTable, key);
            reference.RefreshString();
        }

        private void ApplyText(string value) { text.text = value; }
        private void ApplyFont(Font font)
        {
            if (font != null) text.font = font;
        }
    }
}
