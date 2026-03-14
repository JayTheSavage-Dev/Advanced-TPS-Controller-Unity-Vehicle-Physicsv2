using UnityEngine;

public class AmmoWidget : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text ammoText;

    private void Start()
    {
        Refresh(0, 0, null);
    }

    public void Refresh(int ammoCount, int maxammo, string weaponName)
    {
        string displayText;
        if (string.IsNullOrEmpty(weaponName))
        {
            displayText = string.Empty;
        }
        else if (weaponName == "Axe" || weaponName == "Knife")
        {
            displayText = "MELEE";
        }
        else
        {
            displayText = ammoCount + " / " + maxammo;
        }

        if (ammoText != null)
        {
            ammoText.text = string.IsNullOrEmpty(weaponName) ? string.Empty : weaponName + " " + displayText;
        }

        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetWeaponName(weaponName);
            UIToolkitUIBridge.Instance.SetAmmoText(displayText);
        }
    }
}
