using UnityEngine;

[AddComponentMenu("Advanced TPS/UI/Player Health")]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private bool autoInitializeToMax = true;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Start()
    {
        if (autoInitializeToMax)
        {
            currentHealth = maxHealth;
        }

        PushToUI();
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        PushToUI();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetHealth(currentHealth + amount);
    }

    private void Update()
    {
        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetHealth(currentHealth, maxHealth);
        }
    }

    private void PushToUI()
    {
        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetHealth(currentHealth, maxHealth);
        }
    }
}
