using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    //hitbox object, also add checks for crazy meter and attack rate
    [Header("MetaData")]
    public int attackDamage;
    public float attackRange;
    public float attackRadius;
    public float fireRate;
    public int crazyThreshold;
    public float pierce; //possible idea, reduction to armor
    public string weaponName;
    private double nextFire = 0.0f;
    public UnityEvent attackEvent;
    private CoroutineTask animationTask;
    [Header("HitBoxes")]
    public GameObject LMBBoxPrefab; //prefabs
    public GameObject LMBChargeBoxPrefab;
    public GameObject RMBBoxPrefab;
    public GameObject RMBChargeBoxPrefab;

    private GameObject LMBBox;
    private GameObject LMBChargeBox;
    private GameObject RMBBox;
    private GameObject RMBChargeBox;


    // Start is called before the first frame update
    void Start()
    {
        if (attackEvent == null)
            attackEvent = new UnityEvent();
        InstantiateBoxes();
        InitializeHitboxes();
    }

    private void InstantiateBoxes()
    {
        LMBBox = Instantiate(LMBBoxPrefab, this.transform, false);
        LMBChargeBox = Instantiate(LMBChargeBoxPrefab, this.transform, false);
        RMBBox = Instantiate(RMBBoxPrefab, this.transform, false);
        RMBChargeBox = Instantiate(RMBChargeBoxPrefab, this.transform, false);
    }
    private void InitializeHitboxes()
    {
        LMBBox.SetActive(false);
        LMBChargeBox.SetActive(false);
        RMBBox.SetActive(false);
        RMBChargeBox.SetActive(false);

        HitBox LMBHb = LMBBox.GetComponent<HitBox>();
        HitBox LMBChargeHb = LMBChargeBox.GetComponent<HitBox>();
        HitBox RMBHb = RMBBox.GetComponent<HitBox>();
        HitBox RMBChargeHb = RMBChargeBox.GetComponent<HitBox>();

        LMBHb.attack = attackDamage; //Change Later to be different
        LMBChargeHb.attack = attackDamage;
        RMBHb.attack = attackDamage;
        RMBChargeHb.attack = attackDamage;
    }

    public void WeaponAttack(bool charge, string type)
    {
        if (CheckConstraints())
        {
            attackEvent.Invoke();
            switch (type)
            {
                case "LMB":
                    EnableHitbox(LMBBox);
                    break;
                case "RMB":
                    EnableHitbox(RMBBox);
                    break;
                case "LMBCharge":
                    if (CheckCrazy())
                    {
                        EnableHitbox(LMBChargeBox);
                    }
                    break;
                case "RMBCharge":
                    if (CheckCrazy())
                    {
                        EnableHitbox(RMBChargeBox);
                    }
                    break;
            }
        }
    }


    private bool CheckCrazy()
    {
        return PlayerStats.GetIntAttribute("curr craziness") > crazyThreshold;
    }
        
    private bool CheckConstraints()
    {
        if (Time.timeAsDouble > nextFire) 
        {
            nextFire = Time.timeAsDouble + fireRate;
            return true;
        }
        return false;
    }

    private void EnableHitbox(GameObject hb) 
    {
        hb.SetActive(true);
        animationTask = new CoroutineTask(this);
        animationTask.StartCoroutine(ExeAttackAnim(hb));
    }

    private IEnumerator ExeAttackAnim(GameObject hb)
    {
        float attackAnimDuration = 1.0f;
        float startTime = Time.time;
        Vector3 startPosition = transform.localPosition;
        Quaternion startRotation = transform.localRotation;
        Vector3 targetPosition = Vector3.forward;
        Vector3 targetRotation = new Vector3(0,90,90);
        while (true)
        {
            float currTime = Time.time;
            float animProgress = (currTime - startTime) / attackAnimDuration;
            transform.localPosition = (Vector3.Lerp(transform.localPosition, targetPosition, animProgress));
            transform.localRotation = (Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetRotation), animProgress));
            if (animProgress > fireRate - 0.01f)
            {
                hb.SetActive(false);
                transform.localPosition = startPosition;
                transform.localRotation = startRotation;
                yield break;// exit
            }
            yield return CoroutineTask.WaitForNextFrame;
        }
    }
}
