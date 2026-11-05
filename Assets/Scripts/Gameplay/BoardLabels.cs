using ColorShiftKnight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorShiftKnight.Gameplay
{
    // Letters A-F above the columns and numbers 1-6 beside the rows, so every square has a name.
    // The numbers follow the square names used by the hint: 1 is the top row and 6 is the bottom row (Goal F1 is top-right).
    public static class BoardLabels
    {
        private const int FontSize = 34;
        private const float LetterLift = 34f;    // how far above the top edge the letters sit
        private const float NumberShift = 42f;   // how far right of the board edge the numbers sit

        private static readonly Color LabelColor = new Color(0.90f, 0.90f, 0.92f);   // light grey like the plate borders

        public static void Build(Transform parent, Font font)
        {
            string letters = "ABCDEF";
            for (int c = 0; c < BoardConfig.Size; c++)
            {
                Vector2 left = BoardLayout.ToCanvas(BoardLayout.BoardPoint(0, c));
                Vector2 right = BoardLayout.ToCanvas(BoardLayout.BoardPoint(0, c + 1));
                Vector2 pos = (left + right) * 0.5f + new Vector2(0f, LetterLift);
                string letter = letters.Substring(c, 1);
                MakeLabel("Letter" + letter, parent, font, letter, pos);
            }

            for (int r = 0; r < BoardConfig.Size; r++)
            {
                Vector2 top = BoardLayout.ToCanvas(BoardLayout.BoardPoint(r, BoardConfig.Size));
                Vector2 bottom = BoardLayout.ToCanvas(BoardLayout.BoardPoint(r + 1, BoardConfig.Size));
                Vector2 pos = (top + bottom) * 0.5f + new Vector2(NumberShift, 0f);
                string number = (r + 1).ToString();
                MakeLabel("Number" + number, parent, font, number, pos);
            }
        }

        private static void MakeLabel(string name, Transform parent, Font font, string content, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(60f, 50f);

            Text t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = FontSize;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = LabelColor;
            t.text = content;
            t.raycastTarget = false;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
        }
    }
}
