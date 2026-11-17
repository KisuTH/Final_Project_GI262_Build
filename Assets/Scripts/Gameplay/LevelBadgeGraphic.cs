using UnityEngine;

namespace ColorShiftKnight.Gameplay
{
    // A faceted stone rim drawn at UI resolution, so the hexagonal badge stays crisp.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LevelBadgeGraphic : UnityEngine.UI.Graphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2 center = rect.center;
            float radius = Mathf.Min(rect.width, rect.height) * .5f;
            const int segments = 6;
            mesh.AddVert(center, new Color(.105f, .105f, .105f), Vector2.zero);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                const float edge = 1f;
                float light = .35f + .18f * Mathf.Max(0, direction.y) + (i % 3) * .025f;
                mesh.AddVert(center + direction * radius * .76f, new Color(.105f, .105f, .105f), Vector2.zero);
                mesh.AddVert(center + direction * radius * .81f, new Color(.23f, .23f, .23f), Vector2.zero);
                mesh.AddVert(center + direction * radius * .93f, new Color(light, light, light), Vector2.zero);
                mesh.AddVert(center + direction * radius * edge, new Color(.20f, .20f, .20f), Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
            {
                int a = 1 + i * 4;
                int b = 1 + (i + 1) % segments * 4;
                mesh.AddTriangle(0, a, b);
                for (int ring = 0; ring < 3; ring++)
                {
                    mesh.AddTriangle(a + ring, a + ring + 1, b + ring + 1);
                    mesh.AddTriangle(a + ring, b + ring + 1, b + ring);
                }
            }
        }
    }
}
