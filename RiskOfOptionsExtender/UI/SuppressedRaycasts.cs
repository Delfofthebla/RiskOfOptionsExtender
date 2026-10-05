using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI;

internal sealed class SuppressedRaycasts
{
    private readonly List<Graphic> _graphics;

    private SuppressedRaycasts(List<Graphic> graphics)
    {
        _graphics = graphics;
    }

    public static SuppressedRaycasts Under(GameObject root, params GameObject[] except)
    {
        var graphics = root.GetComponentsInChildren<Graphic>(true)
            .Where(graphic => graphic.raycastTarget && !except.Contains(graphic.gameObject))
            .ToList();

        foreach (var graphic in graphics)
            graphic.raycastTarget = false;

        return new SuppressedRaycasts(graphics);
    }

    public void Restore()
    {
        foreach (var graphic in _graphics)
        {
            if (graphic)
                graphic.raycastTarget = true;
        }

        _graphics.Clear();
    }
}
