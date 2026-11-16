using UnityEngine;
using UnityEngine.UI;

namespace ColorShiftKnight.Gameplay
{
    // The reaction character on the left side of the screen. She has two moods, and each mood has 4 stages.
    //   Delighted  ("Desire Fullfilled" sheet)    : the player is close to running out of moves without collecting everything.
    //   Frustrated ("Desired Unfullfilled" sheet) : the player already has the Relic and the King and still has moves to spare.
    // She is always on screen. The higher the stage, the stronger the face. Stage 1 and 2 stand still, the movement starts at stage 3.
    // Delighted bobs up and down slowly, Frustrated trembles from side to side (same as the reference videos).
    public class ReactionFace : MonoBehaviour
    {
        public const int KindNone = 0;
        public const int KindDelighted = 1;
        public const int KindFrustrated = 2;

        private const int StageCount = 4;
        private const int MoveFromStage = 3;       // she only starts to move from this stage
        private const float SheetToCanvas = 0.96f;   // canvas height 1080 / sheet height 1125
        private const float CropHeight = 790f;       // the painted faces and their little effects fit in the bottom 790 pixels
        private const float SheetWidth = 2610f;      // size of the painted sheets; Unity may import them smaller, so every cut is scaled

        // Where each stage sits in its sheet (pixels from the left, and width). The crop starts at the body, so every stage touches the screen edge.
        private static readonly float[] DelightedX = { 0f, 493f, 1069f, 1788f };
        private static readonly float[] DelightedWidth = { 460f, 547f, 631f, 632f };
        private static readonly float[] FrustratedX = { 0f, 519f, 1051f, 1576f };
        private static readonly float[] FrustratedWidth = { 460f, 481f, 419f, 584f };

        // Movement per stage (canvas pixels, seconds)
        private static readonly float[] BobAmount = { 10f, 20f, 30f, 40f };
        private static readonly float[] BobPeriod = { 2.6f, 2.3f, 2.0f, 1.7f };
        private static readonly float[] ShakeAmount = { 6f, 14f, 20f, 26f };

        private RectTransform _rect;
        private Image _image;
        private Sprite[] _delighted = new Sprite[StageCount];
        private Sprite[] _frustrated = new Sprite[StageCount];
        private Vector2[] _delightedSize = new Vector2[StageCount];
        private Vector2[] _frustratedSize = new Vector2[StageCount];
        private int _kind;
        private int _stage;
        private float _time;
        private bool _hidden;

        // The two sheets are whole pictures; each stage is cut out of them here.
        public void Build(Texture2D delightedSheet, Texture2D frustratedSheet)
        {
            _rect = (RectTransform)transform;
            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.zero;
            _rect.pivot = Vector2.zero;

            _image = gameObject.AddComponent<Image>();
            _image.raycastTarget = false;

            CutStages(delightedSheet, DelightedX, DelightedWidth, _delighted, _delightedSize);
            CutStages(frustratedSheet, FrustratedX, FrustratedWidth, _frustrated, _frustratedSize);

            _kind = KindNone;
            gameObject.SetActive(false);
        }

        // Hides her while the hint helper is on screen, and brings her back afterwards.
        public void SetHidden(bool hidden)
        {
            if (hidden == _hidden)
            {
                return;
            }
            _hidden = hidden;
            ApplyVisible();
        }

        private void ApplyVisible()
        {
            gameObject.SetActive(_kind != KindNone && _hidden == false);
        }

        // kind = KindNone hides her. stage is 1 to 4.
        public void Show(int kind, int stage)
        {
            if (stage < 1)
            {
                stage = 1;
            }
            if (stage > StageCount)
            {
                stage = StageCount;
            }
            if (kind == _kind && stage == _stage)
            {
                return;
            }

            if (kind != _kind)
            {
                _time = 0f;
            }
            _kind = kind;
            _stage = stage;

            Sprite sprite = null;
            Vector2 size = Vector2.zero;
            if (kind == KindDelighted)
            {
                sprite = _delighted[stage - 1];
                size = _delightedSize[stage - 1];
            }
            else if (kind == KindFrustrated)
            {
                sprite = _frustrated[stage - 1];
                size = _frustratedSize[stage - 1];
            }

            if (sprite == null)
            {
                _kind = KindNone;
                ApplyVisible();
                return;
            }

            _image.sprite = sprite;
            _rect.sizeDelta = size * SheetToCanvas;
            ApplyVisible();
            Move();
        }

        private void Update()
        {
            if (_kind == KindNone)
            {
                return;
            }
            _time += Time.deltaTime;
            Move();
        }

        // The picture is placed a little past the screen edge (left and bottom), so moving never shows a gap.
        private void Move()
        {
            int i = _stage - 1;
            float baseX = -ShakeAmount[StageCount - 1];
            float baseY = -BobAmount[StageCount - 1];
            float x = baseX;
            float y = baseY;

            if (_stage < MoveFromStage)
            {
                // Stage 1 and 2 stand still
            }
            else if (_kind == KindDelighted)
            {
                y += BobAmount[i] * Mathf.Sin(_time * 2f * Mathf.PI / BobPeriod[i]);
            }
            else
            {
                float shake = 0.6f * Mathf.Sin(_time * 2f * Mathf.PI * 9f) + 0.4f * Mathf.Sin(_time * 2f * Mathf.PI * 14.3f + 1.3f);
                x += ShakeAmount[i] * shake;
            }
            _rect.anchoredPosition = new Vector2(x, y);
        }

        // sizes receives the size of each cut in the original sheet pixels (used to show it at the painted size).
        private static void CutStages(Texture2D sheet, float[] xs, float[] widths, Sprite[] result, Vector2[] sizes)
        {
            if (sheet == null)
            {
                return;
            }
            float scale = sheet.width / SheetWidth;
            for (int i = 0; i < StageCount; i++)
            {
                float height = Mathf.Min(CropHeight * scale, sheet.height);
                float width = Mathf.Min(widths[i] * scale, sheet.width - xs[i] * scale);
                Rect area = new Rect(xs[i] * scale, 0f, width, height);
                result[i] = Sprite.Create(sheet, area, Vector2.zero, 100f, 0, SpriteMeshType.FullRect);
                sizes[i] = new Vector2(width / scale, height / scale);
            }
        }
    }
}
