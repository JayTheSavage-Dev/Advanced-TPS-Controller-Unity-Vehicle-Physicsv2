using UnityEngine;
using UnityEngine.UIElements;

public static class UIToolkitHUDInstaller
{
    private const string DefaultHudResourcePath = "AdvancedTPSUI/TPSHUD";
    private const string DefaultPanelSettingsResourcePath = "AdvancedTPSUI/HUDPanelSettings";

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
        var document = hudRoot.AddComponent<UIDocument>();
        document.panelSettings = LoadOrCreatePanelSettings();
        document.visualTreeAsset = hudTree;

        var bridge = hudRoot.AddComponent<UIToolkitUIBridge>();
        bridge.Initialize(document);
    }

    private static PanelSettings LoadOrCreatePanelSettings()
    {
        PanelSettings panelSettings = Resources.Load<PanelSettings>(DefaultPanelSettingsResourcePath);
        if (panelSettings != null)
        {
            return panelSettings;
        }

        panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = "Runtime HUD PanelSettings";
        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = new Vector2Int(1920, 1080);
        panelSettings.sortingOrder = 100;

        Debug.LogWarning(
            "UIToolkit HUD installer could not find HUDPanelSettings.asset in Resources/AdvancedTPSUI. " +
            "Using a runtime fallback PanelSettings instance.");

        return panelSettings;
    }
}
