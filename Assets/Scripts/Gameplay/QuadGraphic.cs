using UnityEngine;
using UnityEngine.UI;

namespace ColorShiftKnight.Gameplay
{
    // A UI shape drawn as a four-corner quad, used for the slanted squares of the painted board.
    // The corners are offsets from the centre of the object: top-left, top-right, bottom-right, bottom-left.
    [RequireComponent(typeof(CanvasRenderer))]
    public class QuadGraphic : MaskableGraphic, ICanvasRaycastFilter
    {
        private Vector2[] corners = new Vector2[4];

        public void SetCorners(Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
        {
            corners[0] = topLeft;
            corners[1] = topRight;
            corners[2] = bottomRight;
            corners[3] = bottomLeft;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 center = rectTransform.rect.center;
            for (int i = 0; i < 4; i++)
            {
                vh.AddVert(center + corners[i], color, Vector2.zero);
            }
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }

        // Only clicks inside the quad count (not the whole rectangle around it).
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            Vector2 local;
            bool hit = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);
            if (hit == false)
            {
                return false;
            }
            return Inside(local - rectTransform.rect.center);
        }

        private bool Inside(Vector2 p)
        {
            bool positive = false;
            bool negative = false;
            for (int i = 0; i < 4; i++)
            {
                Vector2 a = corners[i];
                Vector2 b = corners[(i + 1) % 4];
                float cross = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
                if (cross > 0f)
                {
                    positive = true;
                }
                if (cross < 0f)
                {
                    negative = true;
                }
            }
            return !(positive && negative);
        }
    }
}
