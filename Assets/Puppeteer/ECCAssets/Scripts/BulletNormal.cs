using EndlessCarChase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletNormal : BulletBase
{
    private Vector3 fireDir;
    private Vector3 startPoint;


    private bool isMustInView = true;
    
    public void SetParam(WeaponXml weaponXml, GameObject _shooter)
    {
        hurt = weaponXml.hurt;
        hurtRange = weaponXml.hurtRange;
        speed = weaponXml.speed;
        maximumRange = weaponXml.maximumRange;

        shooter = _shooter;
        startPoint = shooter.transform.position;
        prefabName = weaponXml.bulletPrefab;
        hitEffect = weaponXml.bulletHitEffect;
    }

    public void Go(Vector3 _fireDir)
    {
        fireDir = _fireDir;
    }

    public override void BulletUpdate()
    {
        base.BulletUpdate();
        if (gameObject.activeSelf)
        {
            gameObject.transform.Translate(fireDir.normalized * Time.deltaTime * speed);
            float distance = Vector3.Distance(gameObject.transform.position, startPoint);
            if(distance >= maximumRange)
            {
                DestroyMyself();
            }
        }
    }

    public override void DestroyMyself()
    {
        startPoint = Vector3.zero;
        fireDir = Vector3.zero;
        hurt = 0;
        hurtRange = 0;
        speed = 0f;
        maximumRange = 0f;
        shooter = null;
        gameController.dictObjectPool[prefabName].ReturnObject(gameObject);
        gameController.RemoveBulletFromList(this);
    }

    private void ProcessDamage(ECCCar car)
    {
        int damage = GetHurt();
        //Debug.Log("damage:" + damage.ToString());
        //Debug.Log("carHp:" + car.health.ToString());
        car.ChangeHealth(-damage);

        GameObject objHitEffect = gameController.dictObjectPool[hitEffect].GetObject();
        objHitEffect.transform.position = car.transform.position;
        objHitEffect.transform.rotation = car.transform.rotation;
        ParticleControl particleControl = objHitEffect.GetComponent<ParticleControl>();
        particleControl.Play();
        DestroyMyself();
    }

    void OnTriggerEnter(Collider other)
    {
        ECCCar car = other.GetComponent<ECCCar>();
        if(shooter != null)
        {
            ECCCar selfCar = shooter.GetComponent<ECCCar>();

            if (car != null && car.gameObject != shooter)
            {
                bool selfCarIsPlayer = selfCar.IsPlayer();
                bool otherCarIsPlayer = car.IsPlayer();
                if (selfCarIsPlayer != otherCarIsPlayer)
                {
                    if (isMustInView)
                    {
                        if (gameController.IsObjectInViewPort(car.gameObject))
                        {
                            ProcessDamage(car);
                        }
                    }
                    else
                    {
                        ProcessDamage(car);
                    }
                }
            }
        }
      
    }
}
