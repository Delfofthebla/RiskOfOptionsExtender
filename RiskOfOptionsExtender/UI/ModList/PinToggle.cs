using RiskOfOptions.Components.Panel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class PinToggle : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private const float Size = 28f;
    private const float RightInset = 12f;

    private static readonly Color PinnedColor = new(1f, 0.78f, 0.25f, 1f);
    private static readonly Color UnpinnedColor = new(1f, 1f, 1f, 0.2f);
    private static readonly Color HoveredColor = new(1f, 1f, 1f, 0.6f);

    private Image _image;
    private string _modGuid;
    private ModListOrganizer _organizer;
    private bool _hovered;

    private static ExtenderSettings Settings => RiskOfOptionsExtenderPlugin.Settings;

    public static void AddTo(ModListButton button, ModListOrganizer organizer)
    {
        StopChildrenCatchingClicks(button);

        var label = (RectTransform)button.transform.Find("ButtonText");
        label.offsetMax -= new Vector2(Size + RightInset, 0);

        var pinObject = new GameObject("Pin Toggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pinObject.layer = button.gameObject.layer;

        var rect = (RectTransform)pinObject.transform;
        rect.SetParent(button.transform, false);
        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(1, 0.5f);
        rect.sizeDelta = new Vector2(Size, Size);
        rect.anchoredPosition = new Vector2(-RightInset, 0);

        var toggle = pinObject.AddComponent<PinToggle>();
        toggle._image = pinObject.GetComponent<Image>();
        toggle._image.sprite = PinSprite.Get();
        toggle._image.preserveAspect = true;
        toggle._modGuid = button.modGuid;
        toggle._organizer = organizer;
        toggle.Refresh();
    }

    // Unity only completes a click on the object that also received the press; without this the mod row takes the press.
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Settings.TogglePin(_modGuid);
        Refresh();
        _organizer.Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        Refresh();
    }

    private void Refresh()
    {
        if (Settings.IsPinned(_modGuid))
            _image.color = PinnedColor;
        else
            _image.color = _hovered ? HoveredColor : UnpinnedColor;
    }

    private static void StopChildrenCatchingClicks(ModListButton button)
    {
        foreach (var graphic in button.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.gameObject != button.gameObject)
                graphic.raycastTarget = false;
        }
    }
}
