using RiskOfOptionsExtender.Resilience;
using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal static class ComponentExtensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        return component ? component : gameObject.AddComponent<T>();
    }

    public static T GetRequiredComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (!component)
            throw new IncompatibilityException($"\"{gameObject.name}\" has no {typeof(T).Name}.");

        return component;
    }

    public static T GetRequiredComponent<T>(this Component owner) where T : Component
    {
        return owner.gameObject.GetRequiredComponent<T>();
    }

    public static T GetRequiredComponentInChildren<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponentInChildren<T>(true);
        if (!component)
            throw new IncompatibilityException($"\"{gameObject.name}\" has no {typeof(T).Name} in its children.");

        return component;
    }

    public static Transform FindRequired(this Transform parent, string path)
    {
        var child = parent.Find(path);
        if (!child)
            throw new IncompatibilityException($"\"{parent.name}\" has no \"{path}\" child.");

        return child;
    }
}
