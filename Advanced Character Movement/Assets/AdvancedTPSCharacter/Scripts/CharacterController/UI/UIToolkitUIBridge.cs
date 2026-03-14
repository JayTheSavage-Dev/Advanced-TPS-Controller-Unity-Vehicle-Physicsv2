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
    [SerializeField] private string interactionPromptName = "interaction-prompt";
    [SerializeField] private string interactionPromptLabelName = "interaction-prompt-label";
    [SerializeField] private string healthContainerName = "health-container";
    [SerializeField] private string healthBarFillName = "health-bar-fill";
    [SerializeField] private string healthLabelName = "health-label";

    private Label ammoLabel;
    private VisualElement vehiclePrompt;
    private VisualElement crosshair;
    private VisualElement settingsPanel;
    private VisualElement pickupPrompt;
    private Label pickupPromptLabel;
    private VisualElement interactionPrompt;
    private Label interactionPromptLabel;
    private VisualElement healthContainer;
    private VisualElement healthBarFill;
    private Label healthLabel;

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
        SetInteractionPromptVisible(false);
        SetHealth(100f, 100f);
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
        interactionPrompt = root.Q<VisualElement>(interactionPromptName);
        interactionPromptLabel = root.Q<Label>(interactionPromptLabelName);
        healthContainer = root.Q<VisualElement>(healthContainerName);
        healthBarFill = root.Q<VisualElement>(healthBarFillName);
        healthLabel = root.Q<Label>(healthLabelName);
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

    public void SetInteractionPromptVisible(bool visible)
    {
        if (interactionPrompt == null)
        {
            CacheElements();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetInteractionPromptText(string text)
    {
        if (interactionPromptLabel == null)
        {
            CacheElements();
        }

        if (interactionPromptLabel != null)
        {
            interactionPromptLabel.text = text ?? string.Empty;
        }
    }

    public void SetHealth(float current, float max)
    {
        if (healthContainer == null || healthBarFill == null || healthLabel == null)
        {
            CacheElements();
        }

        if (healthContainer == null || healthBarFill == null || healthLabel == null)
        {
            return;
        }

        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        healthBarFill.style.width = Length.Percent(normalized * 100f);
        healthLabel.text = $"HP {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

}
