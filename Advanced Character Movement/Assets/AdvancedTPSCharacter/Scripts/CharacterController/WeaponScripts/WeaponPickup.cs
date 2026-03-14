using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private GunController weaponFab;
    [SerializeField] private bool destroyPickupAfterCollect = true;
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private string pickupPromptText = "Press E to pick up";

    private ActiveWeapon activeWeaponInRange;

    private void Awake()
    {
        bool useUIToolkit = UIToolkitUIBridge.Instance != null;
        if (!useUIToolkit && pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }

        if (useUIToolkit)
        {
            UIToolkitUIBridge.Instance.SetPickupPromptVisible(false);
            UIToolkitUIBridge.Instance.SetInteractionPromptVisible(false);
        }
    }

    private void Update()
    {
        if (activeWeaponInRange == null || weaponFab == null)
        {
            return;
        }

        if (InputManager.Actions.Keyboard.Equip.WasPressedThisFrame())
        {
            GunController newWeapon = Instantiate(weaponFab);
            activeWeaponInRange.Equip(newWeapon, true);

            bool useUIToolkit = UIToolkitUIBridge.Instance != null;
            if (!useUIToolkit && pickupPrompt != null)
            {
                pickupPrompt.SetActive(false);
            }

            if (useUIToolkit)
            {
                UIToolkitUIBridge.Instance.SetPickupPromptVisible(false);
                UIToolkitUIBridge.Instance.SetInteractionPromptVisible(false);
            }

            if (destroyPickupAfterCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon == null)
        {
            return;
        }

        activeWeaponInRange = activeWeapon;
        bool useUIToolkit = UIToolkitUIBridge.Instance != null;
        if (!useUIToolkit && pickupPrompt != null)
        {
            pickupPrompt.SetActive(true);
        }

        if (useUIToolkit)
        {
            UIToolkitUIBridge.Instance.SetPickupPromptText(pickupPromptText);
            UIToolkitUIBridge.Instance.SetPickupPromptVisible(true);
            UIToolkitUIBridge.Instance.SetInteractionPromptText(pickupPromptText);
            UIToolkitUIBridge.Instance.SetInteractionPromptVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon == null || activeWeapon != activeWeaponInRange)
        {
            return;
        }

        activeWeaponInRange = null;
        bool useUIToolkit = UIToolkitUIBridge.Instance != null;
        if (!useUIToolkit && pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }

        if (useUIToolkit)
        {
            UIToolkitUIBridge.Instance.SetPickupPromptVisible(false);
            UIToolkitUIBridge.Instance.SetInteractionPromptVisible(false);
        }
    }
}
