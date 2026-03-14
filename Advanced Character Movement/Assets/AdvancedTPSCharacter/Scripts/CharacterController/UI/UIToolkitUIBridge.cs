using System.Collections;
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
    [SerializeField] private string weaponNameLabelName = "weapon-name-label";
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
    [SerializeField] private string armorBarFillName = "armor-bar-fill";
    [SerializeField] private string armorLabelName = "armor-label";
    [SerializeField] private string staminaBarFillName = "stamina-bar-fill";
    [SerializeField] private string staminaLabelName = "stamina-label";
    [SerializeField] private string statusNotificationName = "status-notification";
    [SerializeField] private string statusNotificationLabelName = "status-notification-label";

    private Label ammoLabel;
    private Label weaponNameLabel;
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
    private VisualElement armorBarFill;
    private Label armorLabel;
    private VisualElement staminaBarFill;
    private Label staminaLabel;
    private VisualElement statusNotification;
    private Label statusNotificationLabel;

    private Coroutine hitFeedbackRoutine;
    private Coroutine statusNotificationRoutine;

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
        SetArmor(0f, 100f);
        SetStamina(100f, 100f);
        SetWeaponName("UNARMED");
        SetStatusNotification(string.Empty, 0f);
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
        weaponNameLabel = root.Q<Label>(weaponNameLabelName);
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
        armorBarFill = root.Q<VisualElement>(armorBarFillName);
        armorLabel = root.Q<Label>(armorLabelName);
        staminaBarFill = root.Q<VisualElement>(staminaBarFillName);
        staminaLabel = root.Q<Label>(staminaLabelName);
        statusNotification = root.Q<VisualElement>(statusNotificationName);
        statusNotificationLabel = root.Q<Label>(statusNotificationLabelName);
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

    public void SetWeaponName(string weaponName)
    {
        if (weaponNameLabel == null)
        {
            CacheElements();
        }

        if (weaponNameLabel != null)
        {
            weaponNameLabel.text = string.IsNullOrWhiteSpace(weaponName) ? "UNARMED" : weaponName.ToUpperInvariant();
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

    public void SetCrosshairHitFeedback(float duration = 0.08f)
    {
        if (crosshair == null)
        {
            CacheElements();
        }

        if (crosshair == null)
        {
            return;
        }

        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
        }

        hitFeedbackRoutine = StartCoroutine(CrosshairHitFeedbackRoutine(duration));
    }

    private IEnumerator CrosshairHitFeedbackRoutine(float duration)
    {
        crosshair.AddToClassList("crosshair-hit");
        yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
        crosshair.RemoveFromClassList("crosshair-hit");
        hitFeedbackRoutine = null;
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
        healthLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    public void SetArmor(float current, float max)
    {
        if (armorBarFill == null || armorLabel == null)
        {
            CacheElements();
        }

        if (armorBarFill == null || armorLabel == null)
        {
            return;
        }

        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        armorBarFill.style.width = Length.Percent(normalized * 100f);
        armorLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    public void SetStamina(float current, float max)
    {
        if (staminaBarFill == null || staminaLabel == null)
        {
            CacheElements();
        }

        if (staminaBarFill == null || staminaLabel == null)
        {
            return;
        }

        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        staminaBarFill.style.width = Length.Percent(normalized * 100f);
        staminaLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    public void SetStatusNotification(string message, float duration = 2.2f)
    {
        if (statusNotification == null || statusNotificationLabel == null)
        {
            CacheElements();
        }

        if (statusNotification == null || statusNotificationLabel == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            statusNotification.style.display = DisplayStyle.None;
            statusNotificationLabel.text = string.Empty;
            return;
        }

        statusNotificationLabel.text = message.ToUpperInvariant();
        statusNotification.style.display = DisplayStyle.Flex;

        if (statusNotificationRoutine != null)
        {
            StopCoroutine(statusNotificationRoutine);
        }

        if (duration > 0f)
        {
            statusNotificationRoutine = StartCoroutine(HideStatusNotificationRoutine(duration));
        }
    }

    private IEnumerator HideStatusNotificationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (statusNotification != null)
        {
            statusNotification.style.display = DisplayStyle.None;
        }

        statusNotificationRoutine = null;
    }
}
