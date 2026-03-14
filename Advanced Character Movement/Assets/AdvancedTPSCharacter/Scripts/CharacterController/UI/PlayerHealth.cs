using UnityEngine;

[AddComponentMenu("Advanced TPS/UI/Player Health")]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Armor")]
    [SerializeField] private float maxArmor = 100f;
    [SerializeField] private float currentArmor;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaRegenPerSecond = 12f;

    [SerializeField] private bool autoInitializeToMax = true;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Start()
    {
        if (autoInitializeToMax)
        {
            currentHealth = maxHealth;
            currentArmor = Mathf.Min(currentArmor, maxArmor);
            currentStamina = maxStamina;
        }

        PushToUI();
    }

    private void Update()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenPerSecond * Time.deltaTime);
        }

        PushToUI();
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        PushToUI();
    }

    public void SetArmor(float value)
    {
        currentArmor = Mathf.Clamp(value, 0f, maxArmor);
        PushToUI();
    }

    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        PushToUI();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        float remainingDamage = amount;
        if (currentArmor > 0f)
        {
            float absorbed = Mathf.Min(currentArmor, remainingDamage);
            currentArmor -= absorbed;
            remainingDamage -= absorbed;
        }

        if (remainingDamage > 0f)
        {
            currentHealth = Mathf.Clamp(currentHealth - remainingDamage, 0f, maxHealth);
        }

        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetCrosshairHitFeedback();
        }

        PushToUI();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetHealth(currentHealth + amount);
    }

    private void PushToUI()
    {
        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetHealth(currentHealth, maxHealth);
            UIToolkitUIBridge.Instance.SetArmor(currentArmor, maxArmor);
            UIToolkitUIBridge.Instance.SetStamina(currentStamina, maxStamina);
        }
    }
}
