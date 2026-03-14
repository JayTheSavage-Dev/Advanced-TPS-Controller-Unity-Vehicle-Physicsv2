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
        else if (weaponName == "Axe")
        {
            displayText = "Axe";
        }
        else if (weaponName == "Knife")
        {
            displayText = "Knife";
        }
        else
        {
            displayText = weaponName + " " + ammoCount + "/" + maxammo;
        }

        if (ammoText != null)
        {
            ammoText.text = displayText;
        }

        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetAmmoText(displayText);
        }
    }
}
