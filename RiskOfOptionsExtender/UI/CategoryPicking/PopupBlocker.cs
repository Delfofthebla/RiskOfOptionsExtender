using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal sealed class PopupBlocker : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    private const float CoverSize = 20000f;

    private Canvas _canvas;
    private Action _onPressed;

    public static PopupBlocker Create(Transform popup, Action onPressed)
    {
        var blockerObject = new GameObject("Click Blocker", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
        blockerObject.layer = popup.gameObject.layer;

        var rect = (RectTransform)blockerObject.transform;
        rect.SetParent(popup, false);
        rect.SetAsFirstSibling();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(CoverSize, CoverSize);

        blockerObject.GetComponent<Image>().color = Color.clear;

        var blocker = blockerObject.AddComponent<PopupBlocker>();
        blocker._canvas = blockerObject.GetComponent<Canvas>();
        blocker._onPressed = onPressed;
        return blocker;
    }

    public void DrawBelow(int popupSortingOrder)
    {
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = popupSortingOrder - 1;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _onPressed();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }
}
