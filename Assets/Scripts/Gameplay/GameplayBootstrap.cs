using System;
using ColorShiftKnight.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ColorShiftKnight.Gameplay
{
    // Builds the gameplay UI from code and connects it to GameCore.
    // The art fields are assigned in the Inspector (or by the "Create Gameplay Scene" menu command).
    public class GameplayBootstrap : MonoBehaviour
    {
        private const int Size = BoardConfig.Size;
        private const int CellCount = BoardConfig.CellCount;
        private const int LowMovesToShow = 3;     // the delighted face can only appear when this many moves (or fewer) are left
        private const float PanelX = 710f;   // centre of the side panel, to the right of the centred board

        private static readonly Color ButtonFallback = new Color(0.15f, 0.20f, 0.28f);
        private static readonly Color PanelFallback = new Color(0.09f, 0.12f, 0.18f);

        private static readonly Color Clear = new Color(1f, 1f, 1f, 0.004f);
        private static readonly Color LegalOverlay = new Color(0.20f, 0.85f, 1f, 0.40f);
        private static readonly Color KnightOverlay = new Color(0.95f, 0.80f, 0.35f, 0.45f);
        private static readonly Color ShiftOverLight = new Color(0.02f, 0.02f, 0.05f, 0.82f);
        private static readonly Color ShiftOverDark = new Color(0.82f, 0.84f, 0.90f, 0.55f);

        [Header("Art (assigned by Create Gameplay Scene)")]
        public Sprite BoardBackground;
        public Sprite KnightSprite;
        public Sprite KingSprite;
        public Sprite RelicSprite;
        public Sprite GoalSprite;
        public Sprite[] ObstacleSprites = new Sprite[2];
        public Sprite ButtonSprite;
        public Sprite PanelSprite;

        [Header("Hint helper art")]
        public Sprite[] HintCharacterFrames = new Sprite[2];
        public Sprite HintFirstPlateSprite;
        public Sprite HintSecondPlateSprite;

        [Header("Reaction character art (the two Desire sheets)")]
        public Texture2D DelightedSheet;
        public Texture2D FrustratedSheet;

        private QuadGraphic[] _cellImages = new QuadGraphic[CellCount];
        private Image[] _cellPieces = new Image[CellCount];
        private Button[] _cellButtons = new Button[CellCount];
        private Vector2[] _cellSize = new Vector2[CellCount];

        private GameCore _game;
        private Font _font;
        private Text _hud;
        private Text _message;
        private Text _overlayTitle;
        private Text _overlayBody;
        private HintDialogue _hintDialogue;
        private CellTooltip _tooltip;
        private GameObject _overlay;
        private GameObject _canvasGo;
        private bool _keepInScene;
        private int _levelNumber = 1;
        private Button _restartButton;
        private Button _nextButton;
        private Image _overlayDim;
        private Text _levelText;
        private ReactionFace _reaction;

        private void OnEnable()
        {
            Teardown();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (Application.isPlaying)
            {
                EnsureEventSystem();
            }
            BuildUi();
            _game = new GameCore(Environment.TickCount);
            NewLevel();
        }

        private void OnDisable()
        {
            Teardown();
        }

        // When Play starts, the copy of the UI that is saved in the scene is still there, because the scene is not
        // fully loaded yet inside OnEnable. Start runs after loading, so the old copy is removed here.
        private void Start()
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == "GameplayCanvas" && roots[i] != _canvasGo)
                {
                    DestroyObject(roots[i]);
                }
            }
        }

        // Called from the editor menu "Color-Shift Knight/Show UI In Scene".
        // Builds the same UI inside the scene itself, so it shows in the Hierarchy and is saved with the scene.
        // When Play starts, the game removes this copy and builds a fresh one.
        public void BuildPreview()
        {
            Teardown();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _keepInScene = true;
            BuildUi();
            _keepInScene = false;
            _game = new GameCore(Environment.TickCount);
            NewLevel();
        }

        // The UI is built from code every time the game starts, so it is removed here first.
        private void Teardown()
        {
            DestroyObject(_canvasGo);
            _canvasGo = null;

            if (gameObject.scene.IsValid() == false || gameObject.scene.isLoaded == false)
            {
                return;
            }
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == "GameplayCanvas")
                {
                    DestroyObject(roots[i]);
                }
            }
        }

        private static void DestroyObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Destroy(go);
            }
            else
            {
                DestroyImmediate(go);
            }
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        private void BuildUi()
        {
            GameObject canvasGo = new GameObject("GameplayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _canvasGo = canvasGo;
            Transform root = canvasGo.transform;

            // Dark backdrop, then the painted board art.
            Image backdrop = NewImage("Backdrop", root, Vector2.zero, Vector2.zero);
            Inset(backdrop.rectTransform, 0f);
            backdrop.color = new Color(0.07f, 0.07f, 0.09f);
            backdrop.raycastTarget = false;

            if (BoardBackground != null)
            {
                Image art = NewImage("BoardArt", root, BoardLayout.BoardCenterOnCanvas, new Vector2(BoardLayout.ArtWidth, BoardLayout.ArtHeight) * BoardLayout.ArtScale);
                art.sprite = BoardBackground;
                art.raycastTarget = false;
            }

            // 36 clickable squares placed on the painted squares. Row 0 is the top row (Goal F1 is top-right).
            Vector2[] centers = new Vector2[CellCount];
            for (int i = 0; i < CellCount; i++)
            {
                int r = Position.RowOf(i);
                int c = Position.ColOf(i);

                Vector2 tl = BoardLayout.ToCanvas(BoardLayout.BoardPoint(r, c));
                Vector2 tr = BoardLayout.ToCanvas(BoardLayout.BoardPoint(r, c + 1));
                Vector2 bl = BoardLayout.ToCanvas(BoardLayout.BoardPoint(r + 1, c));
                Vector2 br = BoardLayout.ToCanvas(BoardLayout.BoardPoint(r + 1, c + 1));
                centers[i] = (tl + tr + bl + br) * 0.25f;
                float w = ((tr.x - tl.x) + (br.x - bl.x)) * 0.5f;
                float h = ((tl.y + tr.y) - (bl.y + br.y)) * 0.5f;
                _cellSize[i] = new Vector2(w, h);

                float minX = Mathf.Min(Mathf.Min(tl.x, tr.x), Mathf.Min(bl.x, br.x));
                float maxX = Mathf.Max(Mathf.Max(tl.x, tr.x), Mathf.Max(bl.x, br.x));
                float minY = Mathf.Min(Mathf.Min(tl.y, tr.y), Mathf.Min(bl.y, br.y));
                float maxY = Mathf.Max(Mathf.Max(tl.y, tr.y), Mathf.Max(bl.y, br.y));
                Vector2 boxCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);

                QuadGraphic cell = NewQuad("Cell" + i, root, boxCenter, new Vector2(maxX - minX, maxY - minY));
                cell.SetCorners(tl - boxCenter, tr - boxCenter, br - boxCenter, bl - boxCenter);
                cell.color = Clear;

                Button button = cell.gameObject.AddComponent<Button>();
                button.targetGraphic = cell;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(OnCellClicked);

                _cellImages[i] = cell;
                _cellButtons[i] = button;
            }

            // The pieces sit in their own layer above all squares.
            for (int i = 0; i < CellCount; i++)
            {
                Image pieceImg = NewImage("Piece" + i, root, Vector2.zero, Vector2.zero);
                RectTransform rt = pieceImg.rectTransform;
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(centers[i].x, centers[i].y - _cellSize[i].y * 0.5f + _cellSize[i].y * 0.08f);
                pieceImg.raycastTarget = false;
                pieceImg.enabled = false;
                _cellPieces[i] = pieceImg;
            }

            // Column letters and row numbers around the board
            BoardLabels.Build(root, _font);

            // Name of the square under the mouse
            _tooltip = new CellTooltip();
            _tooltip.Build(root, ButtonSprite, _font);
            for (int i = 0; i < CellCount; i++)
            {
                CellHover hover = _cellImages[i].gameObject.AddComponent<CellHover>();
                Vector2 tipPosition = new Vector2(centers[i].x, centers[i].y + _cellSize[i].y * 0.5f + 34f);
                hover.Setup(_tooltip, Position.NameOf(i), tipPosition);
            }

            // The reaction character (drawn above the board and below the panels)
            GameObject reactionGo = NewRect("ReactionFace", root, Vector2.zero, Vector2.zero);
            _reaction = reactionGo.AddComponent<ReactionFace>();
            _reaction.Build(DelightedSheet, FrustratedSheet);

            // Side panel
            Image hudPlate = MakeImage("HudPlate", root, new Vector2(-PanelX, 330), new Vector2(440, 240), PanelSprite, PanelFallback);
            _hud = MakeText("HUD", hudPlate.transform, new Vector2(0, -4), new Vector2(320, 160), 24, TextAnchor.MiddleLeft);
            FitText(_hud, 12, 24);

            Image messagePlate = MakeImage("MessagePlate", root, new Vector2(PanelX, 250), new Vector2(440, 130), PanelSprite, PanelFallback);
            _message = MakeText("Message", messagePlate.transform, new Vector2(0, -2), new Vector2(320, 76), 20, TextAnchor.MiddleLeft);
            FitText(_message, 11, 20);

            _levelText = MakeText("LevelText", root, new Vector2(PanelX, 400), new Vector2(440, 60), 44, TextAnchor.MiddleCenter);
            _levelText.color = new Color(0.72f, 0.72f, 0.76f);
            Button undoButton = MakeButton("UndoButton", root, new Vector2(-780, -440), "Undo");
            undoButton.onClick.AddListener(OnUndo);
            Button hintButton = MakeButton("HintButton", root, new Vector2(-480, -440), "Hint");
            hintButton.onClick.AddListener(OnHint);

            // The hint helper (her sheets are painted at screen size, so she gets the whole canvas)
            GameObject dialogueGo = NewRect("HintDialogue", root, Vector2.zero, Vector2.zero);
            _hintDialogue = dialogueGo.AddComponent<HintDialogue>();
            _hintDialogue.Build(HintCharacterFrames, HintFirstPlateSprite, HintSecondPlateSprite, _font);

            // Pop-up for winning (Next) and for running out of moves (Restart), created last so it is drawn on top
            Image overlayImg = NewImage("Overlay", root, Vector2.zero, Vector2.zero);
            _overlay = overlayImg.gameObject;
            Inset(overlayImg.rectTransform, 0f);
            overlayImg.color = new Color(0f, 0f, 0f, 0.75f);
            _overlayDim = overlayImg;

            Image modal = MakeImage("Modal", _overlay.transform, Vector2.zero, new Vector2(560, 320), PanelSprite, PanelFallback);
            _overlayTitle = MakeText("Title", modal.transform, new Vector2(0, 90), new Vector2(520, 60), 40, TextAnchor.MiddleCenter);
            _overlayBody = MakeText("Body", modal.transform, new Vector2(0, 10), new Vector2(500, 120), 26, TextAnchor.MiddleCenter);
            _nextButton = MakeButton("NextButton", modal.transform, new Vector2(0, -110), "Next");
            _nextButton.onClick.AddListener(OnNextClicked);
            _restartButton = MakeButton("RestartButton", modal.transform, new Vector2(0, -110), "Restart");
            _restartButton.onClick.AddListener(OnRestartClicked);
            _overlay.SetActive(false);

            if (_keepInScene == false)
            {
                SetNotSaved(canvasGo.transform);
            }
        }

        // Marks an object and all its children so they are not saved into the scene file.
        private static void SetNotSaved(Transform t)
        {
            t.gameObject.hideFlags = HideFlags.DontSave;
            for (int i = 0; i < t.childCount; i++)
            {
                SetNotSaved(t.GetChild(i));
            }
        }

        private static float PieceHeightFactor(bool obstacle, bool knight, bool king, bool relic)
        {
            if (obstacle)
            {
                return 1.9f;
            }
            if (knight || king)
            {
                return 1.7f;
            }
            if (relic)
            {
                return 0.95f;
            }
            return 0.85f;
        }

        private void NewLevel()
        {
            _overlay.SetActive(false);
            _hintDialogue.Hide();
            if (_game.StartNewLevel())
            {
                _message.text = "New level generated. Reach the Relic, capture the King, then reach Goal.";
            }
            else
            {
                _message.text = "Could not generate a level. Try again.";
            }
            Refresh();
        }

        // Button of the win pop-up: goes to the next level.
        private void OnNextClicked()
        {
            _levelNumber++;
            NewLevel();
        }

        // Button of the move-limit pop-up: puts the same level back to its start.
        private void OnRestartClicked()
        {
            _overlay.SetActive(false);
            _hintDialogue.Hide();
            _game.Restart();
            _message.text = "Level restarted. Reach the Relic, capture the King, then reach Goal.";
            Refresh();
        }

        // All 36 squares call this method. The clicked square is the one the EventSystem has selected.
        private void OnCellClicked()
        {
            GameObject clicked = EventSystem.current.currentSelectedGameObject;
            int index = -1;
            for (int i = 0; i < CellCount; i++)
            {
                if (_cellButtons[i].gameObject == clicked)
                {
                    index = i;
                }
            }
            if (index < 0)
            {
                return;
            }

            _hintDialogue.Hide();
            int result = _game.TryMove(index);
            _message.text = Describe(result);

            if (result == MoveResult.Won)
            {
                ShowOverlay("LEVEL " + _levelNumber + " COMPLETE", "Finished in " + _game.State.Moves + " moves (limit " + _game.State.MoveLimit + ").", true);
            }
            else if (result == MoveResult.Lost)
            {
                ShowOverlay("MOVE LIMIT REACHED", "Undo a move, or restart this level.", false);
            }
            Refresh();
        }

        private void OnUndo()
        {
            _hintDialogue.Hide();
            if (_game.UndosLeft <= 0)
            {
                _message.text = "No undo left. Each level allows " + BoardConfig.MaxUndos + ".";
            }
            else if (_game.Undo())
            {
                _message.text = "Undid one move. " + _game.UndosLeft + " undo left.";
            }
            else
            {
                _message.text = "Nothing to undo.";
            }
            Refresh();
        }

        private void OnHint()
        {
            HintResult hint = _game.GetHint();
            if (hint != null)
            {
                _message.text = "Hint: go to " + Position.NameOf(hint.NextSquare) + ". Minimum " + hint.RemainingMinMoves + " moves left.";
                _hintDialogue.Show("Psst... try " + Position.NameOf(hint.NextSquare) + ".",
                                   "That is the shortest way. " + hint.RemainingMinMoves + " moves left at the least.");
            }
            else
            {
                _message.text = "No hint available.";
                _hintDialogue.Show("Hmm...", "I have no hint for you right now.");
            }
            Refresh();
        }

        private void ShowOverlay(string title, string body, bool won)
        {
            _overlayTitle.text = title;
            _overlayBody.text = body;
            _nextButton.gameObject.SetActive(won);
            _restartButton.gameObject.SetActive(won == false);
            // Winning dims the whole screen; the move-limit pop-up leaves the board and Undo usable
            _overlayDim.color = new Color(0f, 0f, 0f, won ? 0.75f : 0f);
            _overlayDim.raycastTarget = won;
            _overlay.SetActive(true);
        }

        private void Refresh()
        {
            LevelState s = _game.State;
            if (s == null)
            {
                return;
            }

            _levelText.text = "Level " + _levelNumber;
            UpdateReaction(s);
            if (_game.IsLost == false && _game.IsWon == false)
            {
                _overlay.SetActive(false);
            }

            bool[] legal = _game.LegalMoves();

            for (int i = 0; i < CellCount; i++)
            {
                int r = Position.RowOf(i);
                int c = Position.ColOf(i);
                bool isObstacle = s.Obstacles[i];
                bool isKnight = (i == s.Knight);
                bool isLegal = legal[i];

                // Colour layer on top of the painted square. In the art the top-left square is dark.
                bool lightSquare = (r + c) % 2 == 1;
                Color tint = Clear;
                if (s.BoardShifted)
                {
                    if (lightSquare)
                    {
                        tint = ShiftOverLight;
                    }
                    else
                    {
                        tint = ShiftOverDark;
                    }
                }
                if (isLegal)
                {
                    tint = LegalOverlay;
                }
                if (isKnight)
                {
                    tint = KnightOverlay;
                }
                _cellImages[i].color = tint;

                // Which piece stands on this square
                Sprite piece = null;
                bool isRelic = false;
                bool isKing = false;
                if (isObstacle)
                {
                    piece = ObstacleSprite(i);
                }
                else if (isKnight)
                {
                    piece = KnightSprite;
                }
                else if (s.RelicCollected == false && i == s.Relic)
                {
                    piece = RelicSprite;
                    isRelic = true;
                }
                else if (s.KingCaptured == false && i == s.King)
                {
                    piece = KingSprite;
                    isKing = true;
                }
                else if (i == BoardConfig.Goal)
                {
                    piece = GoalSprite;
                }

                _cellPieces[i].sprite = piece;
                _cellPieces[i].enabled = (piece != null);
                if (piece != null)
                {
                    float h = _cellSize[i].y * PieceHeightFactor(isObstacle, isKnight, isKing, isRelic);
                    float w = h * piece.rect.width / piece.rect.height;
                    _cellPieces[i].rectTransform.sizeDelta = new Vector2(w, h);
                }
                _cellButtons[i].interactable = isLegal;
            }

            string boardColor = "WHITE";
            if (s.BoardShifted)
            {
                boardColor = "BLACK";
            }
            string relicText = "0";
            if (s.RelicCollected)
            {
                relicText = "1";
            }
            string kingText = "NOT YET";
            if (s.KingCaptured)
            {
                kingText = "CAPTURED";
            }

            _hud.text = "MOVES    " + s.Moves + "\n" +
                        "LIMIT    " + s.MoveLimit + "\n" +
                        "BOARD    " + boardColor + "\n" +
                        "RELIC    " + relicText + " / 1\n" +
                        "KING     " + kingText + "\n" +
                        "GOAL     " + Position.NameOf(BoardConfig.Goal);
        }

        // Chooses the mood and stage of the reaction character.
        //   Stage : follows the moves already made, so she starts calm (stage 1) and changes a little with every step.
        //           The moves are split in 4 equal parts of the Move Limit, stage 4 is the last part.
        //   Mood  : Delighted ("Desire Fullfilled") when the limit is about to run out and Game Over is possible,
        //           otherwise Frustrated ("Desired Unfullfilled").
        private void UpdateReaction(LevelState s)
        {
            int left = s.MoveLimit - s.Moves;
            int stage = 1;
            if (s.MoveLimit > 0)
            {
                stage = 1 + (s.Moves * 4) / s.MoveLimit;
            }

            int kind = ReactionFace.KindFrustrated;
            if (_game.IsWon == false && left <= LowMovesToShow && CanGameOver(s, left))
            {
                kind = ReactionFace.KindDelighted;
            }
            _reaction.Show(kind, stage);
        }

        // Game Over is possible when something is still missing, or when the moves that are left
        // are not more than the shortest way to finish.
        private bool CanGameOver(LevelState s, int left)
        {
            if (s.HasAllObjectives() == false)
            {
                return true;
            }
            HintResult hint = _game.GetHint();
            if (hint == null)
            {
                return true;
            }
            return hint.RemainingMinMoves >= left;
        }

        // The reaction character steps aside while the hint helper is on screen.
        private void Update()
        {
            if (_reaction != null && _hintDialogue != null)
            {
                _reaction.SetHidden(_hintDialogue.IsActive);
            }
        }

        private Sprite ObstacleSprite(int cell)
        {
            if (ObstacleSprites == null || ObstacleSprites.Length < 2)
            {
                return null;
            }
            return ObstacleSprites[(Position.RowOf(cell) + Position.ColOf(cell)) % 2];
        }

        private static string Describe(int result)
        {
            if (result == MoveResult.Moved)
            {
                return "Moved.";
            }
            if (result == MoveResult.RelicCollected)
            {
                return "Shadow Relic collected! The board flipped and obstacles were reshuffled.";
            }
            if (result == MoveResult.KingCaptured)
            {
                return "King captured!";
            }
            if (result == MoveResult.GoalLocked)
            {
                return "Goal is locked. Collect the Relic and capture the King first.";
            }
            if (result == MoveResult.Won)
            {
                return "Mission complete!";
            }
            if (result == MoveResult.Lost)
            {
                return "Move limit reached. Undo or press Restart.";
            }
            if (result == MoveResult.Rejected)
            {
                return "No solvable layout after the Relic, so that move is blocked.";
            }
            return "That move is not allowed.";
        }

        private Button MakeButton(string name, Transform parent, Vector2 pos, string label)
        {
            Image img = MakeImage(name, parent, pos, new Vector2(320, 64), ButtonSprite, ButtonFallback);
            Button button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            Text text = MakeText("Label", img.transform, Vector2.zero, new Vector2(320, 64), 26, TextAnchor.MiddleCenter);
            text.text = label;
            return button;
        }

        private Image MakeImage(string name, Transform parent, Vector2 pos, Vector2 size, Sprite sprite, Color fallback)
        {
            Image img = NewImage(name, parent, pos, size);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = fallback;
            }
            return img;
        }

        private Text MakeText(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor)
        {
            GameObject go = NewRect(name, parent, pos, size);
            Text t = go.AddComponent<Text>();
            t.font = _font;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        // Makes the text shrink by itself when it is too long for its box.
        private static void FitText(Text text, int minSize, int maxSize)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
        }

        private static void Inset(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        private static Image NewImage(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject go = NewRect(name, parent, pos, size);
            return go.AddComponent<Image>();
        }

        private static QuadGraphic NewQuad(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject go = NewRect(name, parent, pos, size);
            if (go.GetComponent<CanvasRenderer>() == null)
            {
                go.AddComponent<CanvasRenderer>();
            }
            return go.AddComponent<QuadGraphic>();
        }

        // Creates an empty UI object with a RectTransform at the given position and size.
        private static GameObject NewRect(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }
    }
}
