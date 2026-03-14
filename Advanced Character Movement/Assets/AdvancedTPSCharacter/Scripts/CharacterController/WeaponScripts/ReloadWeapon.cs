using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadWeapon : MonoBehaviour
{
    private const string ReloadLockLayersParam = "reload_lock_layers";

    [SerializeField] private Animator rigController;
    private PlayerControls Controls;
    public WeaponAnimationEvents animationEvents;
    private ActiveWeapon activeWeapon;
    public Transform lefthand;
    public AmmoWidget ammoWidget;
    GameObject magazineHand;
    private bool isReloading;
    public bool IsReloading => isReloading;
    public event Action<bool> ReloadStateChanged;
    private void Start()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        animationEvents.WeaponAnimationEvent.AddListener(OnAnimationEvent);
        Controls = InputManager.inputActions ?? new PlayerControls();
        Controls.Enable();
        Controls.Keyboard.Reload.performed += ctx =>
        {
            GunController weapon = activeWeapon.GetActiveWeapon();
            if (CanTriggerReload(weapon))
            {
                weapon.StopFiring();
                SetReloading(true);
                rigController.SetTrigger("reload_weapon");
            }
        };
    }
    private void Update()
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        if (CanTriggerReload(weapon) && weapon.ammoCount <= 0)
        {
            weapon.StopFiring();
            SetReloading(true);
            rigController.SetTrigger("reload_weapon");
        }
        if (weapon)
        {
            ammoWidget.Refresh(weapon.ammoCount, weapon.ClipSize, weapon.WeaponSlotType.ToString());
        }
    }
    void OnAnimationEvent(string eventName)
    {
        switch (eventName)
        {
            case "detach_magazine":
                DetachMagazine();
                break;
            case "drop_magazine":
                DropMagazine();
                break;
            case "refill_magazine":
                Refillagazine();
                break;
            case "attach_magazine":
                AttachMagazine();
                break;
        }
    }

    private void AttachMagazine()
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        if (weapon == null || weapon.Magazine == null)
        {
            return;
        }

        weapon.Magazine.SetActive(true);
        if (magazineHand != null)
        {
            Destroy(magazineHand);
        }
        weapon.ammoCount = weapon.ClipSize;
        SetReloading(false);
        rigController.ResetTrigger("reload_weapon");
        ammoWidget.Refresh(weapon.ammoCount, weapon.ClipSize, weapon.WeaponSlotType.ToString());
    }

    private bool CanTriggerReload(GunController weapon)
    {
        if (weapon == null || weapon.WeaponSlotType == ActiveWeapon.WeaponSlot.Axe || weapon.WeaponSlotType == ActiveWeapon.WeaponSlot.Knife)
        {
            return false;
        }

        if (weapon.ammoCount >= weapon.ClipSize || isReloading)
        {
            return false;
        }

        bool animatorReloading = IsAnimatorInReloadState();

        return !animatorReloading;
    }

    public bool IsAnimatorInReloadState()
    {
        if (rigController == null)
        {
            return false;
        }

        for (int layer = 0; layer < rigController.layerCount; layer++)
        {
            AnimatorStateInfo currentState = rigController.GetCurrentAnimatorStateInfo(layer);
            if (currentState.IsTag("Reload") || currentState.IsName("Weapon_Reload_" + activeWeapon.GetActiveWeapon()?.WeaponSlotType))
            {
                return true;
            }

            if (rigController.IsInTransition(layer))
            {
                AnimatorStateInfo nextState = rigController.GetNextAnimatorStateInfo(layer);
                if (nextState.IsTag("Reload") || nextState.IsName("Weapon_Reload_" + activeWeapon.GetActiveWeapon()?.WeaponSlotType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void SetReloading(bool value)
    {
        if (isReloading == value)
        {
            return;
        }

        isReloading = value;
        SyncReloadAnimatorLocks(value);
        ReloadStateChanged?.Invoke(isReloading);
    }

    private void SyncReloadAnimatorLocks(bool reloading)
    {
        if (rigController == null)
        {
            return;
        }

        if (HasAnimatorParameter(rigController, ReloadLockLayersParam, AnimatorControllerParameterType.Bool))
        {
            rigController.SetBool(ReloadLockLayersParam, reloading);
        }

        if (reloading)
        {
            if (HasAnimatorParameter(rigController, "ConstrainAxe", AnimatorControllerParameterType.Bool))
            {
                rigController.SetBool("ConstrainAxe", false);
            }

            if (HasAnimatorParameter(rigController, "ConstrainKnife", AnimatorControllerParameterType.Bool))
            {
                rigController.SetBool("ConstrainKnife", false);
            }
        }
    }

    private bool HasAnimatorParameter(Animator animator, string parameterName, AnimatorControllerParameterType expectedType)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == expectedType)
            {
                return true;
            }
        }

        return false;
    }

    private void Refillagazine()
    {
        if (magazineHand != null)
        {
            magazineHand.SetActive(true);
        }
    }

    private void DropMagazine()
    {
        if (magazineHand == null)
        {
            return;
        }

        GameObject droppedMagazine = Instantiate(magazineHand, magazineHand.transform.position, magazineHand.transform.rotation);
        droppedMagazine.AddComponent<Rigidbody>();
        droppedMagazine.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        droppedMagazine.AddComponent<BoxCollider>();
        magazineHand.SetActive(false);
        StartCoroutine(DestroyClip(droppedMagazine));
    }

    private void DetachMagazine()
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        if (weapon == null || weapon.Magazine == null || lefthand == null)
        {
            return;
        }

        magazineHand = Instantiate(weapon.Magazine, lefthand, true);
        weapon.Magazine.SetActive(false);
    }
    IEnumerator DestroyClip(GameObject clip)
    {
        yield return new WaitForSeconds(7f);
        Destroy(clip.gameObject);
    }
}
