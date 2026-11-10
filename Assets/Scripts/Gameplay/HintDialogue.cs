using UnityEngine;
using UnityEngine.UI;

namespace ColorShiftKnight.Gameplay
{
    // The hint helper. She slides in from the left, the first speech plate shows up,
    // and the second speech plate follows a moment later. Then everything slides away again.
    // While she talks her mouth flaps by swapping between the two painted frames.
    // Art positions come from the painted sheets (1980 x 1125), so the plates sit where they were drawn.
    public class HintDialogue : MonoBehaviour
    {
        private const float SheetToCanvas = 0.96f;   // canvas height 1080 / sheet height 1125
        private const float SlideSeconds = 0.4f;
        private const float SecondPlateDelay = 1.5f;
        private const float HoldSeconds = 4f;
        private const float FrameSeconds = 0.15f;

        private static readonly Color CharacterFallback = new Color(0.45f, 0.45f, 0.5f);
        private static readonly Color PlateFallback = new Color(0.09f, 0.12f, 0.18f);

        private RectTransform _character;
        private Image _characterImage;
        private Sprite[] _frames;
        private GameObject _firstPlate;
        private GameObject _secondPlate;
        private Text _firstText;
        private Text _secondText;
        private float _time;
        private bool _active;

        // True from the moment the hint starts until she has slid away again.
        public bool IsActive
        {
            get { return _active; }
        }

        public void Build(Sprite[] characterFrames, Sprite firstPlateSprite, Sprite secondPlateSprite, Font font)
        {
            RectTransform self = (RectTransform)transform;
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = Vector2.zero;
            self.offsetMax = Vector2.zero;

            _frames = characterFrames;
            Sprite firstFrame = null;
            if (_frames != null && _frames.Length > 0)
            {
                firstFrame = _frames[0];
            }
            _character = NewPart("HintCharacter", firstFrame, new Vector2(404f, 702f), CharacterFallback);
            _characterImage = _character.GetComponent<Image>();

            RectTransform first = NewPart("HintFirstPlate", firstPlateSprite, new Vector2(569f, 142f), PlateFallback);
            _firstPlate = first.gameObject;
            _firstText = NewText(first, font, 34);

            RectTransform second = NewPart("HintSecondPlate", secondPlateSprite, new Vector2(405f, 243f), PlateFallback);
            _secondPlate = second.gameObject;
            _secondText = NewText(second, font, 30);

            Hide();
        }

        // Starts the little scene with the two lines of speech.
        public void Show(string firstLine, string secondLine)
        {
            _firstText.text = firstLine;
            _secondText.text = secondLine;
            _time = 0f;
            _active = true;
            ShowFrame(0);
            _character.gameObject.SetActive(true);
            _firstPlate.SetActive(false);
            _secondPlate.SetActive(false);
        }

        public void Hide()
        {
            _active = false;
            _character.gameObject.SetActive(false);
            _firstPlate.SetActive(false);
            _secondPlate.SetActive(false);
        }

        private void Update()
        {
            if (_active == false)
            {
                return;
            }

            _time += Time.deltaTime;

            float talkEnd = SlideSeconds + SecondPlateDelay + HoldSeconds;
            if (_time >= talkEnd + SlideSeconds)
            {
                Hide();
                return;
            }

            // p goes 0 -> 1 while sliding in, stays 1 while talking, then goes 1 -> 0 while sliding out.
            float p = 1f;
            if (_time < SlideSeconds)
            {
                p = _time / SlideSeconds;
            }
            else if (_time >= talkEnd)
            {
                p = 1f - (_time - talkEnd) / SlideSeconds;
            }
            float eased = 1f - (1f - p) * (1f - p);

            float width = _character.sizeDelta.x;
            _character.anchoredPosition = new Vector2(-(1f - eased) * width, _character.anchoredPosition.y);

            bool talking = (_time >= SlideSeconds && _time < talkEnd);
            _firstPlate.SetActive(talking);
            _secondPlate.SetActive(talking && _time >= SlideSeconds + SecondPlateDelay);

            // Mouth: closed while sliding, then open and closed again and again while she talks.
            int frame = 0;
            if (talking)
            {
                int step = (int)((_time - SlideSeconds) / FrameSeconds);
                if (step % 2 == 0)
                {
                    frame = 1;
                }
            }
            ShowFrame(frame);
        }

        private void ShowFrame(int index)
        {
            if (_frames == null || index >= _frames.Length || _frames[index] == null)
            {
                return;
            }
            _characterImage.sprite = _frames[index];
            _character.sizeDelta = new Vector2(_frames[index].rect.width, _frames[index].rect.height) * SheetToCanvas;
        }

        // One painted piece, placed at the spot where it sits on its sheet (bottom-left of the screen as origin).
        private RectTransform NewPart(string name, Sprite sprite, Vector2 fallbackSize, Color fallbackColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;

            if (sprite != null)
            {
                img.sprite = sprite;
                rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height) * SheetToCanvas;
                rt.anchoredPosition = new Vector2(sprite.rect.x, sprite.rect.y) * SheetToCanvas;
            }
            else
            {
                img.color = fallbackColor;
                rt.sizeDelta = fallbackSize * SheetToCanvas;
                rt.anchoredPosition = Vector2.zero;
            }
            return rt;
        }

        private static Text NewText(RectTransform plate, Font font, int maxSize)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(plate, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(40f, 22f);
            rt.offsetMax = new Vector2(-40f, -22f);

            Text t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = maxSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 12;
            t.resizeTextMaxSize = maxSize;
            return t;
        }
    }
}
