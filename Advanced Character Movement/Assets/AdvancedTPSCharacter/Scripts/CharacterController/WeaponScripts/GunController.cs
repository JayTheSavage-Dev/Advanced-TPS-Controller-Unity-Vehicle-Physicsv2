using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    private CrossHairTarget target;
    [Header("Shooting Weapons")]
    public int fireRate = 25;
    public ActiveWeapon.WeaponSlot WeaponSlotType;
    [SerializeField] private ParticleSystem[] muzzleFlash;
    [SerializeField] private ParticleSystem hiteffect;
    [SerializeField] private Transform RaycastOrigin;
    [SerializeField] private Transform RaycastDestination;
    [SerializeField] private ActiveWeapon activeWeaponSource;
    private TrailRenderer tracerEffect;
    public float Damage = 10f;
    Ray ray;
    RaycastHit hitInfo;
    public bool isFiring;
    public GameObject Magazine;
    public int ammoCount;
    public int ClipSize;

    [Header("Combat Weapons")]
    public bool AxeAttack;
    public bool KnifeAttacking;
    float accumalatedTime;
    public WeaponProceduralRecoil recoil { get; set; }
    [Range(0f, 1f)]
    public float ricochetChance;
    public float minRicochetAngle;
    [SerializeField] private GameObject hitArea;
    private void Awake()
    {
        recoil = GetComponent<WeaponProceduralRecoil>();
    }
    private void Start()
    {
        hitArea = GameObject.FindGameObjectWithTag("HitArea");
        target = FindFirstObjectByType<CrossHairTarget>();
        if (target != null)
        {
            RaycastDestination = target.gameObject.transform;
        }

        if (activeWeaponSource == null)
        {
            activeWeaponSource = GetComponentInParent<ActiveWeapon>();
        }

        if (activeWeaponSource != null)
        {
            tracerEffect = activeWeaponSource.tracerRenderer;
        }
    }
    public void StartAxeAttack(Animator Main, Animator Rig)
    {
        AxeAttack = true;
        Main.SetBool("AttackAxe", true);
        Rig.Play("Weapon_Constrain_Axe", 1);
        StartCoroutine(AxeStopSequence(Main, Rig));
        Invoke("CalculateAxeHit", 0.75f);
    }
    IEnumerator AxeStopSequence(Animator Main, Animator Rig)
    {
        yield return new WaitForSeconds(((2.267f/1.25f)/Main.GetCurrentAnimatorStateInfo(0).speed - 0.3f) - 0.25f);
        Main.SetBool("AttackAxe", false);
        Rig.SetBool("ConstrainAxe", false);
        yield return new WaitForSeconds(0.25f);
        AxeAttack = false;
    }
    public void StartFiring()
    {
        isFiring = true;
        recoil.Reset();
        accumalatedTime = 0.0f;
        FireBullet();
    }
    public void UpdateFiring(float deltaTime)
    {
        accumalatedTime += deltaTime;
        float fireInterval = 1.0f / fireRate;
        while (accumalatedTime >= fireInterval)
        {
            FireBullet();
            accumalatedTime -= fireInterval;
        }
    }
    private void FireBullet()
    {
        if(ammoCount <= 0)
        {
            return;
        }
        ammoCount--;
        if (recoil != null)
        {
            recoil.GenerateRecoil(WeaponSlotType.ToString());
        }

        if (muzzleFlash != null)
        {
            for (int i = 0; i < muzzleFlash.Length; i++)
            {
                if (muzzleFlash[i] != null)
                {
                    muzzleFlash[i].Emit(1);
                }
            }
        }

        if (RaycastOrigin == null || RaycastDestination == null)
        {
            return;
        }

        ray.origin = RaycastOrigin.position;
        ray.direction = RaycastDestination.position - RaycastOrigin.position;

        TrailRenderer tracer = null;
        if (tracerEffect != null)
        {
            tracer = Instantiate(tracerEffect, ray.origin, Quaternion.identity);
            tracer.AddPosition(ray.origin);
        }

        if (Physics.Raycast(ray, out hitInfo, 1000f) && hitInfo.collider.tag != "Player")
        {
            Debug.Log(hitInfo.collider.gameObject.name);
            if (hiteffect != null)
            {
                hiteffect.transform.position = hitInfo.point;
                hiteffect.transform.forward = hitInfo.normal;
                hiteffect.Emit(1);
            }

            if (tracer != null)
            {
                tracer.transform.position = hitInfo.point;
            }
            var rb2d = hitInfo.collider.GetComponent<Rigidbody>();
            if (hitInfo.collider.gameObject.GetComponent<Destructable>() != null)
            {
                hitInfo.collider.gameObject.GetComponent<Destructable>().DestroyAndReplace();
            }
            if (rb2d)
            {
                rb2d.AddForceAtPosition(ray.direction * 20, hitInfo.point, ForceMode.Impulse);
            }
        }
    }
    public void StopFiring()
    {
        isFiring = false;
    }
  
    public void KnifeAttack(Animator Main, Animator Rig)
    {
        KnifeAttacking = true;
        Main.SetBool("AttackKnife", true);
        Rig.Play("Weapon_Constrain_Knife", 1);
        StartCoroutine(KnifeStopSequence(Main, Rig));
    }
    IEnumerator KnifeStopSequence(Animator Main, Animator Rig)
    {
        yield return new WaitForSeconds(((2.133f / 1.25f) / Main.GetCurrentAnimatorStateInfo(0).speed - 0.3f) - 0.25f);
        Main.SetBool("AttackKnife", false);
        Rig.SetBool("ConstrainKnife", false);
        yield return new WaitForSeconds(0.25f);
        KnifeAttacking = false;
    }

    private void CalculateAxeHit()
    {
        Collider[] hitcolliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(var hitCollider in hitcolliders)
        {
            if(hitCollider.tag == "Tree")
            {
                hitCollider.gameObject.GetComponent<TreeDamageable>().Break();
            }
        }
    }
}
