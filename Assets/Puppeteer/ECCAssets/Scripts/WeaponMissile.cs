using EndlessCarChase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMissile : WeaponBase
{
    public Transform targetTrs;

/*    public WeaponMissile(WeaponXml _weaponXml)
    {
        weaponXml = _weaponXml;
    }*/

    public WeaponMissile(WeaponCfg _weaponCfg)
    {
        weaponCfg = _weaponCfg;
    }

    public override void Shoot()
    {
        base.Shoot();

        GameObject obj = gameController.dictObjectPool[weaponCfg.bulletPrefab].GetObject();
        obj.transform.position = new Vector3(holderTrs.position.x, holderTrs.position.y + 0.25f, holderTrs.position.z);
        BulletMissileImprove bulletMissile = obj.GetComponent<BulletMissileImprove>();
        if(bulletMissile == null)
        {
            bulletMissile = obj.AddComponent<BulletMissileImprove>();
        }
        bulletMissile.SetParam(weaponCfg, holderTrs.gameObject);  //holderTrs must have gameObj
        bulletMissile.Go(targetTrs);

        gameController.AddBulletToList(bulletMissile);
    }

    public override void Reload()
    {
        base.Reload();
    }

    public override bool CanFire()
    {
        float minDis = 9999;
        Transform tempTrs = null;
        ECCCar[] allCars = GameObject.FindObjectsOfType<ECCCar>();
        for (int i = 0; i < allCars.Length;++i)
        {
            if(allCars[i].gameObject.transform != holderTrs)
            {
                ECCCar selfCar = holderTrs.gameObject.GetComponent<ECCCar>();
                if(selfCar != null)
                {
                    bool selfCarIsPlayer = selfCar.IsPlayer();
                    bool otherCarIsPlayer = allCars[i].IsPlayer();
                    if(selfCarIsPlayer != otherCarIsPlayer)
                    {
                        float distance = Vector3.Distance(allCars[i].gameObject.transform.position, holderTrs.position);
                        if (distance < minDis && weaponCfg.attackRange > distance)
                        {
                            minDis = distance;
                            tempTrs = allCars[i].gameObject.transform;
                        }
                    }
                }              
            }
        }
        if(tempTrs != null)
        {
            targetTrs = tempTrs;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void WeaponUpdate(Transform carTrs)
    {
        holderTrs = carTrs;
        fireTimer += Time.deltaTime;
        if (fireTimer >= weaponCfg.fireRate && bulletCount > 0 && CanFire())
        {
            fireTimer = 0;
            Shoot();
        }

        if (bulletCount == 0)
        {
            Reload();
        }
    }
}
