using System.Collections.Generic;
using System.Linq;
using RiskOfOptions.Components.Options;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Resilience;
using UnityEngine;
using UnityEngine.UI;
using RooAssets = RiskOfOptions.Resources.Assets;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal sealed class RestartIcon : MonoBehaviour
{
    private static readonly float Size = 26f;
    private static readonly float Gap = 6f;
    private static readonly string SpritePath = "assets/RiskOfOptions/ror2RestartSymbol.png";

    private static readonly Color Tint = new(1f, 0.5f, 0.5f, 0.85f);
    private static readonly HashSet<RestartIcon> _liveIcons = [];
    private static Sprite _sprite;

    private RectTransform _label;

    public static void AddToRows(ModOptionPanelController panelController)
    {
        foreach (var row in panelController._modSettings)
        {
            if (row && row.IsRestartRequired() && !row.GetComponentInChildren<RestartIcon>(true))
                AddTo(row);
        }
    }

    public static void RemoveAll()
    {
        foreach (var icon in _liveIcons.ToList())
            icon.Remove();
    }

    private static void AddTo(ModSetting row)
    {
        var text = row.NameText();
        if (!text)
            return;

        var sprite = LoadSprite();
        var label = text.rectTransform;

        var rect = UiObjects.Create("Restart Required Icon", label, typeof(CanvasRenderer), typeof(Image));
        rect.AnchorAt(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        rect.sizeDelta = new Vector2(Size, Size);
        rect.anchoredPosition = new Vector2(-(Size + Gap), 0);

        var image = rect.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.color = Tint;
        image.raycastTarget = false;

        var icon = rect.gameObject.AddComponent<RestartIcon>();
        icon._label = label;
        _liveIcons.Add(icon);

        label.offsetMin += new Vector2(Size + Gap, 0);
    }

    private static Sprite LoadSprite()
    {
        if (!_sprite)
            _sprite = RooAssets.Load<Sprite>(SpritePath);

        if (!_sprite)
            throw new IncompatibilityException($"Risk Of Options has no \"{SpritePath}\" sprite.");

        return _sprite;
    }

    private void Remove()
    {
        if (_label)
            _label.offsetMin -= new Vector2(Size + Gap, 0);

        Destroy(gameObject);
    }

    private void OnDestroy() => _liveIcons.Remove(this);
}
