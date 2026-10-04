using UnityEngine;
using UnityEngine.EventSystems;

namespace RiskOfOptionsExtender.UI;

internal abstract class PointerTarget : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected bool IsHovered { get; private set; }

    // Unity only completes a click on the object that also received the press; without this a parent button takes the press.
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
        Refresh();
    }

    protected abstract void OnLeftClick();

    protected abstract void Refresh();
}
