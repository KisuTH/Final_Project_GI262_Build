using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnityEngine.UI.Image leftHorse;
    [SerializeField] private UnityEngine.UI.Image rightHorse;
    private UnityEngine.UI.Button button;

    private void Awake()
    {
        button = GetComponent<UnityEngine.UI.Button>();
    }

    private void OnEnable() => SetVisible(false);
    private void OnDisable() => SetVisible(false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetVisible(button != null && button.IsInteractable());
    }

    public void OnPointerExit(PointerEventData eventData) => SetVisible(false);

    private void SetVisible(bool visible)
    {
        if (leftHorse != null) leftHorse.enabled = visible;
        if (rightHorse != null) rightHorse.enabled = visible;
    }
}
