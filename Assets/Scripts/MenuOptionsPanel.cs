using UnityEngine;
using UnityEngine.EventSystems;

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

    private void Awake()
    {
        panel.SetActive(false);
        SyncVolume();
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
        AudioListener.volume = Mathf.Clamp01(value);
        volumeValue.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }

    private void SyncVolume()
    {
        volumeSlider.SetValueWithoutNotify(AudioListener.volume);
        volumeValue.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }

    private void RestoreMenu()
    {
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
