using UnityEngine;
using UnityEngine.UIElements;

public static class UIToolkitHUDInstaller
{
    private const string DefaultHudResourcePath = "AdvancedTPSUI/TPSHUD";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureHudExists()
    {
        if (Object.FindFirstObjectByType<UIToolkitUIBridge>() != null)
        {
            return;
        }

        VisualTreeAsset hudTree = Resources.Load<VisualTreeAsset>(DefaultHudResourcePath);
        if (hudTree == null)
        {
            Debug.LogWarning("UIToolkit HUD installer could not find TPSHUD.uxml in Resources/AdvancedTPSUI.");
            return;
        }

        var hudRoot = new GameObject("UIToolkit HUD");
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = new Vector2Int(1920, 1080);
        panelSettings.sortingOrder = 100;

        var document = hudRoot.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;
        document.visualTreeAsset = hudTree;

        var bridge = hudRoot.AddComponent<UIToolkitUIBridge>();
        bridge.Initialize(document);
    }
}
