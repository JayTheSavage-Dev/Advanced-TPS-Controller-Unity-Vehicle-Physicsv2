using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadWeapon : MonoBehaviour
{
    [Serializable]
    private class ReloadPoseOverride
    {
        public ActiveWeapon.WeaponSlot slot;
        public Vector3 detachedMagazineLocalPosition;
        public Vector3 detachedMagazineLocalEuler;
        public Vector3 droppedMagazinePositionOffset;
        public Vector3 droppedMagazineEulerOffset;
    }
    private const string ReloadLockLayersParam = "reload_lock_layers";

    [SerializeField] private Animator rigController;
    private PlayerControls Controls;
    public WeaponAnimationEvents animationEvents;
    private ActiveWeapon activeWeapon;
    public Transform lefthand;
    public AmmoWidget ammoWidget;
    [Header("Reload pose tuning")]
    [SerializeField] private List<ReloadPoseOverride> reloadPoseOverrides = new List<ReloadPoseOverride>();
    [SerializeField] private bool useSlotSpecificMagazinePose = true;
    GameObject magazineHand;
    private bool isReloading;
    public bool IsReloading => isReloading;
    public event Action<bool> ReloadStateChanged;
    private void Start()
    {
        InitializeDefaultReloadPoseOverrides();
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
        ApplyDropPoseOverride(droppedMagazine.transform);
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
        ApplyDetachedPoseOverride(weapon, magazineHand.transform);
        weapon.Magazine.SetActive(false);
    }

    private void ApplyDetachedPoseOverride(GunController weapon, Transform detachedMagazine)
    {
        if (!useSlotSpecificMagazinePose || weapon == null || detachedMagazine == null)
        {
            return;
        }

        ReloadPoseOverride slotPose = GetPoseOverride(weapon.WeaponSlotType);
        if (slotPose == null)
        {
            return;
        }

        detachedMagazine.localPosition = slotPose.detachedMagazineLocalPosition;
        detachedMagazine.localRotation = Quaternion.Euler(slotPose.detachedMagazineLocalEuler);
    }

    private void ApplyDropPoseOverride(Transform droppedMagazine)
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        if (!useSlotSpecificMagazinePose || droppedMagazine == null || weapon == null)
        {
            return;
        }

        ReloadPoseOverride slotPose = GetPoseOverride(weapon.WeaponSlotType);
        if (slotPose == null)
        {
            return;
        }

        droppedMagazine.position += droppedMagazine.TransformDirection(slotPose.droppedMagazinePositionOffset);
        droppedMagazine.rotation *= Quaternion.Euler(slotPose.droppedMagazineEulerOffset);
    }

    private ReloadPoseOverride GetPoseOverride(ActiveWeapon.WeaponSlot slot)
    {
        for (int i = 0; i < reloadPoseOverrides.Count; i++)
        {
            if (reloadPoseOverrides[i] != null && reloadPoseOverrides[i].slot == slot)
            {
                return reloadPoseOverrides[i];
            }
        }

        return null;
    }

    private void InitializeDefaultReloadPoseOverrides()
    {
        if (reloadPoseOverrides.Count > 0)
        {
            return;
        }

        reloadPoseOverrides = new List<ReloadPoseOverride>
        {
            CreatePose(ActiveWeapon.WeaponSlot.Pistol, new Vector3(0.015f, -0.03f, 0.01f), new Vector3(10f, -15f, 95f), new Vector3(0.01f, -0.025f, 0f), new Vector3(0f, 0f, 20f)),
            CreatePose(ActiveWeapon.WeaponSlot.Shotgun, new Vector3(0.02f, -0.035f, -0.005f), new Vector3(5f, -10f, 90f), new Vector3(0f, -0.02f, 0f), new Vector3(5f, 0f, 0f)),
            CreatePose(ActiveWeapon.WeaponSlot.Rifle, new Vector3(0.018f, -0.03f, 0f), new Vector3(8f, -12f, 92f), new Vector3(0.005f, -0.02f, 0f), new Vector3(0f, 0f, 10f)),
            CreatePose(ActiveWeapon.WeaponSlot.SMG, new Vector3(0.012f, -0.028f, 0.005f), new Vector3(12f, -14f, 96f), new Vector3(0.01f, -0.025f, 0f), new Vector3(0f, 0f, 25f)),
            CreatePose(ActiveWeapon.WeaponSlot.Sniper, new Vector3(0.016f, -0.032f, 0.003f), new Vector3(6f, -9f, 88f), new Vector3(0f, -0.02f, -0.005f), new Vector3(0f, 0f, 15f))
        };
    }

    private ReloadPoseOverride CreatePose(ActiveWeapon.WeaponSlot slot, Vector3 detachedPos, Vector3 detachedEuler, Vector3 dropPos, Vector3 dropEuler)
    {
        return new ReloadPoseOverride
        {
            slot = slot,
            detachedMagazineLocalPosition = detachedPos,
            detachedMagazineLocalEuler = detachedEuler,
            droppedMagazinePositionOffset = dropPos,
            droppedMagazineEulerOffset = dropEuler
        };
    }
    IEnumerator DestroyClip(GameObject clip)
    {
        yield return new WaitForSeconds(7f);
        Destroy(clip.gameObject);
    }
}
