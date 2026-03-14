using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CM = Cinemachine;

public class WeaponAiming : MonoBehaviour
{
    private CM.CinemachineFreeLook cam;
    private PlayerControls controls;
    bool Aiming = false;
    float currentFOV;
    float standardFOV;
    public float AimSpeed;
    [HideInInspector] public string currentWeapon;
    ActiveWeapon activeWeapon;
    private void Awake()
    {
        Aiming = false;
        cam = FindFirstObjectByType<CM.CinemachineFreeLook>();
        if (cam != null)
        {
            currentFOV = cam.m_Lens.FieldOfView;
            standardFOV = cam.m_Lens.FieldOfView;
            currentFOV = 50;
            standardFOV = 51;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        controls = InputManager.Actions;
        activeWeapon = GetComponent<ActiveWeapon>();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Keyboard.Aim.started += OnAimStarted;
        controls.Keyboard.Aim.canceled += OnAimCanceled;
    }

    private void OnDisable()
    {
        if (controls == null)
        {
            return;
        }

        controls.Keyboard.Aim.started -= OnAimStarted;
        controls.Keyboard.Aim.canceled -= OnAimCanceled;
    }

    private void OnAimStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Aiming = true;
    }

    private void OnAimCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Aiming = false;
    }
    private void Update()
    {
        var weapon = activeWeapon.GetActiveWeapon();
        if (weapon && weapon.recoil != null && (currentWeapon != "Axe") && (currentWeapon != "Knife"))
        {
            weapon.recoil.RecoilModifier = Aiming ? 0.3f : 1.0f;
        }
        if (string.IsNullOrEmpty(currentWeapon) || currentWeapon == "Axe" || currentWeapon == "Knife")
        {
            return;
        }
        if(currentWeapon == "Sniper") { SniperAiming(); }
        else { OtherWeaponAiming(); }
    }

    void SniperAiming()
    {
        if (cam == null) { return; }
        if (Aiming && currentFOV > 21)
        {
            currentFOV -= Time.deltaTime * AimSpeed * 1.5f;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        else if (Aiming && currentFOV < 21)
        {
            currentFOV = 20;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        else if (!Aiming && currentFOV < standardFOV)
        {
            currentFOV += Time.deltaTime * AimSpeed * 1.5f;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        else if (!Aiming && currentFOV >= standardFOV)
        {
            currentFOV = standardFOV;
            cam.m_Lens.FieldOfView = currentFOV;
        }
    }
    void OtherWeaponAiming()
    {
        if (cam == null) { return; }
        if (Aiming && currentFOV > 35)
        {
            currentFOV -= Time.deltaTime * AimSpeed;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        else if (Aiming && currentFOV <= 35f)
        {
            currentFOV = 35f;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        else if (!Aiming && currentFOV < standardFOV)
        {
            currentFOV += Time.deltaTime * AimSpeed * 1.5f;
            cam.m_Lens.FieldOfView = currentFOV;
        }
        else if (!Aiming && currentFOV >= standardFOV)
        {
            currentFOV = standardFOV;
            cam.m_Lens.FieldOfView = currentFOV;
        }
    }
    public void StopAiming()
    {
        Aiming = false;
    }
}
