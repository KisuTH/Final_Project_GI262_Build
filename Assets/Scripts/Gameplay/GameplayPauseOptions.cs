using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorShiftKnight.Gameplay
{
    public partial class GameplayBootstrap
    {
        private GameObject _pauseOverlay;
        private GameObject _pauseMenu;
        private GameObject _optionsMenu;
        private UnityEngine.UI.Button _pauseButton;
        private UnityEngine.UI.Button _resumeButton;
        private UnityEngine.UI.Button _optionsButton;
        private UnityEngine.UI.Slider _mainSlider;
        private bool _paused;
        private float _timeBeforePause;

        private void BuildPauseUi(Transform root)
        {
            _pauseButton = MakeButton("PauseButton", root, Vector2.zero, "PAUSE");
            RectTransform pauseRect = (RectTransform)_pauseButton.transform;
            pauseRect.anchorMin = pauseRect.anchorMax = Vector2.one;
            pauseRect.pivot = Vector2.one;
            pauseRect.anchoredPosition = new Vector2(-32, -32);
            pauseRect.sizeDelta = new Vector2(220, 64);
            Inset((RectTransform)_pauseButton.GetComponentInChildren<UnityEngine.UI.Text>().transform, 0);
            _pauseButton.onClick.AddListener(PauseGame);

            UnityEngine.UI.Image shade = NewImage("PauseOverlay", root, Vector2.zero, Vector2.zero);
            Inset(shade.rectTransform, 0);
            shade.color = new Color(0, 0, 0, .78f);
            _pauseOverlay = shade.gameObject;

            UnityEngine.UI.Image menu = MakeImage("PauseMenu", shade.transform, Vector2.zero,
                new Vector2(700, 480), PanelSprite, new Color(.12f, .12f, .12f));
            _pauseMenu = menu.gameObject;
            MakeText("Title", menu.transform, new Vector2(0, 145), new Vector2(500, 60), 42, TextAnchor.MiddleCenter).text = "PAUSED";
            _resumeButton = MakeButton("ResumeButton", menu.transform, new Vector2(0, 45), "RESUME");
            _resumeButton.onClick.AddListener(ResumeGame);
            _optionsButton = MakeButton("OptionsButton", menu.transform, new Vector2(0, -45), "OPTIONS");
            _optionsButton.onClick.AddListener(OpenPauseOptions);
            MakeButton("BackToMenuButton", menu.transform, new Vector2(0, -135), "BACK TO MENU")
                .onClick.AddListener(BackToMenu);

            UnityEngine.UI.Image options = MakeImage("OptionsMenu", shade.transform, Vector2.zero,
                new Vector2(800, 720), PanelSprite, new Color(.12f, .12f, .12f));
            _optionsMenu = options.gameObject;
            MakeText("Title", options.transform, new Vector2(0, 285), new Vector2(540, 60), 40, TextAnchor.MiddleCenter).text = "OPTIONS";
            _mainSlider = MakeVolumeSlider(options.transform, "MAIN SOUND", 175, GameAudioSettings.Channel.Main);
            MakeVolumeSlider(options.transform, "MUSIC", 55, GameAudioSettings.Channel.Music);
            MakeVolumeSlider(options.transform, "EFFECT", -65, GameAudioSettings.Channel.Effect);
            UnityEngine.UI.Button back = MakeButton("BackButton", options.transform, new Vector2(0, -285), "BACK");
            if (_damageSprites != null)
            {
                var image = back.GetComponent<UnityEngine.UI.Image>();
                image.sprite = _damageSprites[0];
                image.color = Color.white;
                image.preserveAspect = true;
            }
            back.onClick.AddListener(ClosePauseOptions);
            var languageLabel = MakeText("LanguageLabel", options.transform, new Vector2(0, -155), new Vector2(520, 42), 26, TextAnchor.MiddleLeft);
            GameLocalization.Set(languageLabel, "Language");
            for (int i = 0; i < 2; i++)
            {
                string code = i == 0 ? "en" : "th";
                var choice = MakeButton("Language_" + code, options.transform, new Vector2(i == 0 ? -135 : 135, -215), "");
                ((RectTransform)choice.transform).sizeDelta = new Vector2(250, 64);
                if (_damageSprites != null) choice.GetComponent<UnityEngine.UI.Image>().sprite = _damageSprites[0];
                LanguageChoiceButton.Configure(choice, code);
            }
            _optionsMenu.SetActive(false);
            _pauseOverlay.SetActive(false);
        }

        private UnityEngine.UI.Slider MakeVolumeSlider(Transform parent, string label, float y, GameAudioSettings.Channel channel)
        {
            MakeText(label + "Label", parent, new Vector2(-65, y), new Vector2(390, 40), 26, TextAnchor.MiddleLeft).text = label;
            UnityEngine.UI.Text value = MakeText(label + "Value", parent, new Vector2(215, y), new Vector2(90, 40), 24, TextAnchor.MiddleRight);
            GameObject sliderGo = NewRect(label + "Slider", parent, new Vector2(0, y - 43), new Vector2(520, 42));
            UnityEngine.UI.Slider slider = sliderGo.AddComponent<UnityEngine.UI.Slider>();
            UnityEngine.UI.Image track = NewImage("Track", sliderGo.transform, Vector2.zero, new Vector2(520, 10));
            track.color = new Color(.27f, .27f, .27f);
            GameObject fillArea = NewRect("FillArea", sliderGo.transform, Vector2.zero, new Vector2(496, 10));
            UnityEngine.UI.Image fill = NewImage("Fill", fillArea.transform, Vector2.zero, Vector2.zero);
            Inset(fill.rectTransform, 0);
            fill.color = new Color(.8f, .8f, .8f);
            GameObject handleArea = NewRect("HandleArea", sliderGo.transform, Vector2.zero, new Vector2(496, 42));
            UnityEngine.UI.Image handle = NewImage("Handle", handleArea.transform, Vector2.zero, new Vector2(24, 0));
            handle.color = new Color(.95f, .95f, .95f);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.SetValueWithoutNotify(GameAudioSettings.Get(channel));
            value.text = Mathf.RoundToInt(slider.value * 100) + "%";
            slider.onValueChanged.AddListener(amount =>
            {
                GameAudioSettings.Set(channel, amount);
                value.text = Mathf.RoundToInt(amount * 100) + "%";
            });
            return slider;
        }

        private void PauseGame()
        {
            if (_paused || !Application.isPlaying) return;
            _paused = true;
            _timeBeforePause = Time.timeScale;
            Time.timeScale = 0;
            _tooltip.Hide();
            _pauseButton.interactable = false;
            _pauseMenu.SetActive(true);
            _optionsMenu.SetActive(false);
            _pauseOverlay.SetActive(true);
            SelectPauseControl(_resumeButton.gameObject);
        }

        private void ResumeGame()
        {
            if (!_paused) return;
            Time.timeScale = _timeBeforePause;
            _paused = false;
            _pauseOverlay.SetActive(false);
            _pauseButton.interactable = true;
            GameAudioSettings.Save();
            SelectPauseControl(_pauseButton.gameObject);
        }

        private void BackToMenu()
        {
            ReleasePause();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        private void OpenPauseOptions()
        {
            _pauseMenu.SetActive(false);
            _optionsMenu.SetActive(true);
            SelectPauseControl(_mainSlider.gameObject);
        }

        private void ClosePauseOptions()
        {
            GameAudioSettings.Save();
            _optionsMenu.SetActive(false);
            _pauseMenu.SetActive(true);
            SelectPauseControl(_optionsButton.gameObject);
        }

        private static void SelectPauseControl(GameObject target)
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(target);
        }

        private void UpdatePauseInput()
        {
            bool escape = false;
#if ENABLE_INPUT_SYSTEM
            escape = UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (!escape) return;
            if (!_paused) PauseGame();
            else if (_optionsMenu.activeSelf) ClosePauseOptions();
            else ResumeGame();
        }

        private void ReleasePause()
        {
            if (!_paused) return;
            Time.timeScale = _timeBeforePause;
            _paused = false;
            GameAudioSettings.Save();
        }
    }
}
