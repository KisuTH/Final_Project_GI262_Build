using UnityEngine;
using UnityEngine.EventSystems;
using ColorShiftKnight.Gameplay;

public sealed class MenuOptionsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private UnityEngine.UI.Button optionButton;
    [SerializeField] private UnityEngine.UI.Button startButton;
    [SerializeField] private UnityEngine.UI.Button backButton;
    [SerializeField] private UnityEngine.UI.Slider volumeSlider;
    [SerializeField] private UnityEngine.UI.Text volumeValue;
    private bool isOpen;
    private bool startWasInteractable;
    private bool optionWasInteractable;
    private UnityEngine.UI.Slider musicSlider;
    private UnityEngine.UI.Slider effectSlider;
    private UnityEngine.UI.Text musicValue;
    private UnityEngine.UI.Text effectValue;

    private void Awake()
    {
        panel.SetActive(false);
        BuildAudioRows();
        StyleBackButton();
        SyncVolume();
        BuildLanguageRow();
        foreach (var label in GetComponentsInChildren<UnityEngine.UI.Text>(true))
            GameLocalization.Label(label, label.text);
    }

    public void Open()
    {
        if (isOpen) return;
        startWasInteractable = startButton.interactable;
        optionWasInteractable = optionButton.interactable;
        isOpen = true;
        startButton.interactable = false;
        optionButton.interactable = false;
        SyncVolume();
        panel.SetActive(true);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }

    public void Close()
    {
        if (!isOpen) return;
        RestoreMenu();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(optionButton.gameObject);
    }

    public void SetVolume(float value)
    {
        GameAudioSettings.Set(GameAudioSettings.Channel.Main, value);
        volumeValue.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }

    private void SyncVolume()
    {
        SyncRow(volumeSlider, volumeValue, GameAudioSettings.Channel.Main);
        SyncRow(musicSlider, musicValue, GameAudioSettings.Channel.Music);
        SyncRow(effectSlider, effectValue, GameAudioSettings.Channel.Effect);
    }

    private void BuildAudioRows()
    {
        Transform parent = volumeSlider.transform.parent;
        UnityEngine.UI.Text label = parent.Find("VolumeLabel").GetComponent<UnityEngine.UI.Text>();
        label.text = "MAIN SOUND";
        PlaceRow(label, volumeValue, volumeSlider, .67f);
        CloneRow(parent, label, "MUSIC", .52f, GameAudioSettings.Channel.Music, out musicSlider, out musicValue);
        CloneRow(parent, label, "EFFECT", .37f, GameAudioSettings.Channel.Effect, out effectSlider, out effectValue);
        Place((RectTransform)backButton.transform, .37f, .08f, .63f, .17f);
    }

    private void BuildLanguageRow()
    {
        Transform parent = volumeSlider.transform.parent;
        var label = Instantiate(parent.Find("VolumeLabel").GetComponent<UnityEngine.UI.Text>(), parent);
        label.name = "LanguageLabel";
        label.text = "";
        Place((RectTransform)label.transform, .23f, .255f, .78f, .30f);
        GameLocalization.Set(label, "Language");
        for (int i = 0; i < 2; i++)
        {
            string code = i == 0 ? "en" : "th";
            var language = Instantiate(backButton, parent);
            language.name = "Language_" + code;
            float left = i == 0 ? .23f : .515f;
            Place((RectTransform)language.transform, left, .165f, left + .265f, .255f);
            LanguageChoiceButton.Configure(language, code);
        }
        Place((RectTransform)backButton.transform, .37f, .055f, .63f, .145f);
    }

    private void StyleBackButton()
    {
        Sprite[] states = Resources.LoadAll<Sprite>("UI/StoneButtonStates");
        Sprite intact = System.Array.Find(states, sprite => sprite.name == "StoneButtonState0");
        if (intact == null) return;
        var image = backButton.GetComponent<UnityEngine.UI.Image>();
        image.sprite = intact;
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        backButton.GetComponentInChildren<UnityEngine.UI.Text>(true).color = Color.white;
    }

    private void CloneRow(Transform parent, UnityEngine.UI.Text template, string title, float y,
        GameAudioSettings.Channel channel, out UnityEngine.UI.Slider slider, out UnityEngine.UI.Text value)
    {
        var label = Instantiate(template, parent);
        label.name = title + "Label";
        label.text = title;
        value = Instantiate(volumeValue, parent);
        value.name = title + "Value";
        slider = Instantiate(volumeSlider, parent);
        slider.name = title + "Slider";
        // Cloning also copies the serialized master callback; replace the event entirely.
        slider.onValueChanged = new UnityEngine.UI.Slider.SliderEvent();
        UnityEngine.UI.Text display = value;
        slider.onValueChanged.AddListener(amount =>
        {
            GameAudioSettings.Set(channel, amount);
            display.text = Mathf.RoundToInt(amount * 100) + "%";
        });
        PlaceRow(label, value, slider, y);
    }

    private static void SyncRow(UnityEngine.UI.Slider slider, UnityEngine.UI.Text value, GameAudioSettings.Channel channel)
    {
        float amount = GameAudioSettings.Get(channel);
        slider.SetValueWithoutNotify(amount);
        value.text = Mathf.RoundToInt(amount * 100) + "%";
    }

    private static void PlaceRow(UnityEngine.UI.Text label, UnityEngine.UI.Text value, UnityEngine.UI.Slider slider, float y)
    {
        Place((RectTransform)label.transform, .23f, y, .64f, y + .07f);
        Place((RectTransform)value.transform, .65f, y, .78f, y + .07f);
        Place((RectTransform)slider.transform, .23f, y - .08f, .78f, y - .01f);
    }

    private static void Place(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(left, bottom);
        rect.anchorMax = new Vector2(right, top);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void RestoreMenu()
    {
        GameAudioSettings.Save();
        panel.SetActive(false);
        startButton.interactable = startWasInteractable;
        optionButton.interactable = optionWasInteractable;
        isOpen = false;
    }

    private void OnDisable()
    {
        if (isOpen) RestoreMenu();
    }
}
