using System.IO;
using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal static class PinSprite
{
    private const string ResourceName = "RiskOfOptionsExtender.PinStar.png";

    private static Sprite _sprite;

    public static Sprite Get()
    {
        if (_sprite)
            return _sprite;

        using var stream = typeof(PinSprite).Assembly.GetManifestResourceStream(ResourceName);
        using var memory = new MemoryStream();
        stream!.CopyTo(memory);

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(memory.ToArray());
        _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return _sprite;
    }
}
