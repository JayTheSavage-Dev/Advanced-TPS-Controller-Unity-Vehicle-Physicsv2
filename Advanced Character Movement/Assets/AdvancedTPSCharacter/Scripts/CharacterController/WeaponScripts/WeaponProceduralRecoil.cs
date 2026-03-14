using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProceduralRecoil : MonoBehaviour
{
    [HideInInspector] public Cinemachine.CinemachineFreeLook playerCamera;
    [HideInInspector] public Cinemachine.CinemachineImpulseSource cameraShake;
    [HideInInspector] public Animator RigController;
    public float duration;
    public Vector2[] recoilPattern;
    float time;
    int index;
    float verticalRecoil;
    float HorizontalRecoil;
    public float RecoilModifier = 1f;
    private void Awake()
    {
        cameraShake = GetComponent<Cinemachine.CinemachineImpulseSource>();
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

        time = duration;

        if (cameraShake != null)
        {
            Camera currentCamera = FindObjectOfType<Camera>();
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
    // Update is called once per frame
    void Update()
    {
        if(time > 0)
        {
            playerCamera.m_YAxis.Value -= ((verticalRecoil / 1000 * Time.deltaTime) / duration) * RecoilModifier;
            playerCamera.m_XAxis.Value -= ((HorizontalRecoil / 10 * Time.deltaTime) / duration) * RecoilModifier;
            time -= Time.deltaTime;
        }
    }
}
