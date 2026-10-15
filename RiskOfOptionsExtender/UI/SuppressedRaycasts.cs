using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI;

internal sealed class SuppressedRaycasts(List<Graphic> graphics)
{
    public static SuppressedRaycasts Under(GameObject root, params GameObject[] except)
    {
        var graphics = root.GetComponentsInChildren<Graphic>(true).Where(graphic => graphic.raycastTarget && !except.Contains(graphic.gameObject)).ToList();

        foreach (var graphic in graphics)
            graphic.raycastTarget = false;

        return new(graphics);
    }

    public void Restore()
    {
        foreach (var graphic in graphics)
        {
            if (graphic)
                graphic.raycastTarget = true;
        }

        graphics.Clear();
    }
}
