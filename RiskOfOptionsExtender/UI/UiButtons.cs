using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;

namespace RiskOfOptionsExtender.UI;

internal static class UiButtons
{
    public static GameObject Clone(GameObject template, Transform parent, string name, UnityAction onClick)
    {
        template.GetRequiredComponentInChildren<HGButton>();

        var buttonObject = Object.Instantiate(template, parent);
        buttonObject.name = name;
        buttonObject.SetActive(true);

        foreach (var languageController in buttonObject.GetComponentsInChildren<LanguageTextMeshController>(true))
            Object.DestroyImmediate(languageController);

        var button = buttonObject.GetComponentInChildren<HGButton>(true);
        button.disablePointerClick = false;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
        return buttonObject;
    }
}
