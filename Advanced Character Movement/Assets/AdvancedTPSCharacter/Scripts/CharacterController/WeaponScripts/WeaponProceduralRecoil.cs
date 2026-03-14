using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CM = Cinemachine;

public class WeaponProceduralRecoil : MonoBehaviour
{
    [HideInInspector] public CM.CinemachineFreeLook playerCamera;
    [HideInInspector] public CM.CinemachineImpulseSource cameraShake;
    [HideInInspector] public Animator RigController;
    [HideInInspector] public ReloadWeapon reloadWeapon;
    public float duration;
    public Vector2[] recoilPattern;
    float time;
    int index;
    float verticalRecoil;
    float HorizontalRecoil;
    public float RecoilModifier = 1f;

    private void Awake()
    {
        cameraShake = GetComponent<CM.CinemachineImpulseSource>();
        reloadWeapon = GetComponentInParent<ReloadWeapon>();
    }

    int NextIndex(int index)
    {
        return (index + 1) % recoilPattern.Length;
    }

    public void Reset()
    {
        index = 0;
    }

    public void GenerateRecoil(string WeaponName)
    {
        if (recoilPattern == null || recoilPattern.Length == 0)
        {
            return;
        }

        if (IsReloadBlockingRecoil(WeaponName))
        {
            return;
        }

        time = duration;

        if (cameraShake != null)
        {
            Camera currentCamera = FindFirstObjectByType<Camera>();
            if (currentCamera != null)
            {
                cameraShake.GenerateImpulse(currentCamera.transform.forward);
            }
        }

        HorizontalRecoil = recoilPattern[index].x;
        verticalRecoil = recoilPattern[index].y;
        index = NextIndex(index);

        if (RigController != null)
        {
            string stateName = "Weapon_Recoil_" + WeaponName;
            int stateHash = Animator.StringToHash(stateName);
            for (int layer = 0; layer < RigController.layerCount; layer++)
            {
                if (RigController.HasState(layer, stateHash))
                {
                    RigController.Play(stateHash, layer, 0.0f);
                    break;
                }
            }
        }
    }

    private bool IsReloadBlockingRecoil(string weaponName)
    {
        if (reloadWeapon != null && reloadWeapon.IsReloading)
        {
            return true;
        }

        if (RigController == null)
        {
            return false;
        }

        string reloadStateName = "Weapon_Reload_" + weaponName;
        for (int layer = 0; layer < RigController.layerCount; layer++)
        {
            AnimatorStateInfo currentState = RigController.GetCurrentAnimatorStateInfo(layer);
            if (currentState.IsTag("Reload") || currentState.IsName(reloadStateName))
            {
                return true;
            }

            if (RigController.IsInTransition(layer))
            {
                AnimatorStateInfo nextState = RigController.GetNextAnimatorStateInfo(layer);
                if (nextState.IsTag("Reload") || nextState.IsName(reloadStateName))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 0 && playerCamera != null)
        {
            playerCamera.m_YAxis.Value -= ((verticalRecoil / 1000 * Time.deltaTime) / duration) * RecoilModifier;
            playerCamera.m_XAxis.Value -= ((HorizontalRecoil / 10 * Time.deltaTime) / duration) * RecoilModifier;
            time -= Time.deltaTime;
        }
    }
}
