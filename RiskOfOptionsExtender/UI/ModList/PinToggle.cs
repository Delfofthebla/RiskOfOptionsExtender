using RiskOfOptions.Components.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class PinToggle : PointerTarget
{
    private const float Size = 28f;
    private const float RightInset = 12f;

    private static readonly Color PinnedColor = new(1f, 0.78f, 0.25f, 1f);
    private static readonly Color UnpinnedColor = new(1f, 1f, 1f, 0.2f);
    private static readonly Color HoveredColor = new(1f, 1f, 1f, 0.6f);

    private SuppressedRaycasts _rowRaycasts;
    private RectTransform _label;
    private Image _image;
    private string _modGuid;
    private ModListOrganizer _organizer;

    public static PinToggle AddTo(ModListButton button, ModListOrganizer organizer)
    {
        var label = (RectTransform)button.transform.FindRequired("ButtonText");

        var pinObject = new GameObject("Pin Toggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pinObject.layer = button.gameObject.layer;

        var rect = (RectTransform)pinObject.transform;
        rect.SetParent(button.transform, false);
        rect.AnchorAt(new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        rect.sizeDelta = new Vector2(Size, Size);
        rect.anchoredPosition = new Vector2(-RightInset, 0);

        var toggle = pinObject.AddComponent<PinToggle>();
        toggle._image = pinObject.GetComponent<Image>();
        toggle._image.sprite = Sprites.PinStar;
        toggle._image.preserveAspect = true;
        toggle._modGuid = button.modGuid;
        toggle._organizer = organizer;
        toggle._label = label;
        toggle._rowRaycasts = SuppressedRaycasts.Under(button.gameObject, button.gameObject, pinObject);
        toggle.Refresh();

        label.offsetMax -= new Vector2(Size + RightInset, 0);
        return toggle;
    }

    public void Remove()
    {
        if (_label)
            _label.offsetMax += new Vector2(Size + RightInset, 0);

        _rowRaycasts?.Restore();
        Destroy(gameObject);
    }

    protected override void OnLeftClick()
    {
        _organizer.TogglePin(_modGuid);
        Refresh();
    }

    protected override void Refresh()
    {
        if (_organizer.IsPinned(_modGuid))
            _image.color = PinnedColor;
        else
            _image.color = IsHovered ? HoveredColor : UnpinnedColor;
    }
}
