using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private GunController weaponFab;
    [SerializeField] private bool destroyPickupAfterCollect = true;
    [SerializeField] private GameObject pickupPrompt;

    private ActiveWeapon activeWeaponInRange;

    private void Awake()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (activeWeaponInRange == null || weaponFab == null)
        {
            return;
        }

        if (InputManager.inputActions == null)
        {
            return;
        }

        if (InputManager.inputActions.Keyboard.Equip.WasPressedThisFrame())
        {
            GunController newWeapon = Instantiate(weaponFab);
            activeWeaponInRange.Equip(newWeapon, true);

            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(false);
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
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(true);
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
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }
}
