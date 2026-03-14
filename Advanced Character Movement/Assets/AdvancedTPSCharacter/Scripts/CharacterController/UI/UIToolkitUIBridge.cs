using UnityEngine;
using UnityEngine.UIElements;

[AddComponentMenu("Advanced TPS/UI/UIToolkit UI Bridge")]
public class UIToolkitUIBridge : MonoBehaviour
{
    public static UIToolkitUIBridge Instance { get; private set; }

    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private StyleSheet styleSheet;
    [SerializeField] private string resourcesStyleSheetPath = "AdvancedTPSUI/TPSHUD";
    [SerializeField] private string ammoLabelName = "ammo-label";
    [SerializeField] private string vehiclePromptName = "vehicle-prompt";
    [SerializeField] private string crosshairName = "crosshair";
    [SerializeField] private string settingsPanelName = "settings-panel";
    [SerializeField] private string pickupPromptName = "pickup-prompt";
    [SerializeField] private string pickupPromptLabelName = "pickup-prompt-label";

    private Label ammoLabel;
    private VisualElement vehiclePrompt;
    private VisualElement crosshair;
    private VisualElement settingsPanel;
    private VisualElement pickupPrompt;
    private Label pickupPromptLabel;

    public void Initialize(UIDocument document)
    {
        uiDocument = document;
        CacheElements();
    }

    private void Awake()
    {
        Instance = this;
        CacheElements();
        SetVehiclePromptVisible(false);
        SetPickupPromptVisible(false);
        SetSettingsVisible(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void CacheElements()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (styleSheet == null)
        {
            styleSheet = Resources.Load<StyleSheet>(resourcesStyleSheetPath);
        }

        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
        {
            root.styleSheets.Add(styleSheet);
        }

        ammoLabel = root.Q<Label>(ammoLabelName);
        vehiclePrompt = root.Q<VisualElement>(vehiclePromptName);
        crosshair = root.Q<VisualElement>(crosshairName);
        settingsPanel = root.Q<VisualElement>(settingsPanelName);
        pickupPrompt = root.Q<VisualElement>(pickupPromptName);
        pickupPromptLabel = root.Q<Label>(pickupPromptLabelName);
    }

    public void SetAmmoText(string text)
    {
        if (ammoLabel == null)
        {
            CacheElements();
        }

        if (ammoLabel != null)
        {
            ammoLabel.text = text ?? string.Empty;
        }
    }

    public void SetVehiclePromptVisible(bool visible)
    {
        if (vehiclePrompt == null)
        {
            CacheElements();
        }

        if (vehiclePrompt != null)
        {
            vehiclePrompt.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetCrosshairVisible(bool visible)
    {
        if (crosshair == null)
        {
            CacheElements();
        }

        if (crosshair != null)
        {
            crosshair.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetSettingsVisible(bool visible)
    {
        if (settingsPanel == null)
        {
            CacheElements();
        }

        if (settingsPanel != null)
        {
            settingsPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetPickupPromptVisible(bool visible)
    {
        if (pickupPrompt == null)
        {
            CacheElements();
        }

        if (pickupPrompt != null)
        {
            pickupPrompt.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetPickupPromptText(string text)
    {
        if (pickupPromptLabel == null)
        {
            CacheElements();
        }

        if (pickupPromptLabel != null)
        {
            pickupPromptLabel.text = text ?? string.Empty;
        }
    }
}
