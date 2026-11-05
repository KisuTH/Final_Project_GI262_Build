using UnityEngine;
using UnityEngine.UI;

namespace ColorShiftKnight.Gameplay
{
    // A small plate that shows the name of the square under the mouse (for example "B4").
    public class CellTooltip
    {
        private const float PlateWidth = 120f;
        private const float PlateHeight = 46f;

        private GameObject _root;
        private RectTransform _rect;
        private Text _text;

        public void Build(Transform parent, Sprite plateSprite, Font font)
        {
            _root = new GameObject("CellTooltip", typeof(RectTransform), typeof(Image));
            _rect = (RectTransform)_root.transform;
            _rect.SetParent(parent, false);
            _rect.anchorMin = new Vector2(0.5f, 0.5f);
            _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.sizeDelta = new Vector2(PlateWidth, PlateHeight);

            // The plate must not catch the mouse, or the square under it would lose the hover.
            Image plate = _root.GetComponent<Image>();
            plate.raycastTarget = false;
            if (plateSprite != null)
            {
                plate.sprite = plateSprite;
            }
            else
            {
                plate.color = new Color(0.09f, 0.12f, 0.18f);
            }

            GameObject textGo = new GameObject("Text", typeof(RectTransform));
            RectTransform textRect = (RectTransform)textGo.transform;
            textRect.SetParent(_rect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _text = textGo.AddComponent<Text>();
            _text.font = font;
            _text.fontSize = 28;
            _text.fontStyle = FontStyle.Bold;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.color = Color.white;
            _text.raycastTarget = false;

            Hide();
        }

        public void Show(string cellName, Vector2 position)
        {
            _text.text = cellName;
            _rect.anchoredPosition = position;
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }
    }
}
