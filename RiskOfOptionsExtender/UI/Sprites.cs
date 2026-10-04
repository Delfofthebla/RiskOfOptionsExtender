using System.IO;
using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal static class Sprites
{
    private const string PinStarResource = "RiskOfOptionsExtender.PinStar.png";

    private static Sprite _pinStar;

    public static Sprite PinStar
    {
        get
        {
            if (!_pinStar)
                _pinStar = FromPng(ReadResource(PinStarResource));

            return _pinStar;
        }
    }

    public static Sprite FromPng(byte[] png)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(png))
            return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static byte[] ReadResource(string name)
    {
        using var stream = typeof(Sprites).Assembly.GetManifestResourceStream(name);
        using var memory = new MemoryStream();
        stream!.CopyTo(memory);
        return memory.ToArray();
    }
}
