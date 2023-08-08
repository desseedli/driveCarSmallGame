using EndlessCarChase;
using UnityEngine;

public class WeaponBase
{
  /*  protected float attackRange;    //可以攻击的圆形范围
    protected int magazineSize;      //弹夹容量
    protected float reloadTime;      //装弹时间
    protected float fireRate;        //射击间隔*/
    protected float fireTimer = 0f;
    private float reloadTimer = 0f;
    protected int bulletCount;
    public Transform holderTrs;
    public bool isManualMode = false;

    //bullet param
/*    protected int hurt;
    protected int hurtRange;
    protected float speed;
    protected float maximumRange;    //最大射程*/

    //protected WeaponXml weaponXml;
    protected WeaponCfg weaponCfg;

    protected static ECCGameController gameController;

    public WeaponBase()
    {
        if (gameController == null) gameController = GameObject.FindObjectOfType<ECCGameController>();
    }

    public virtual void Shoot()
    {
        bulletCount -= 1;
    }

    public virtual void Reload()
    {
        reloadTimer += Time.deltaTime;
        if(reloadTimer >= weaponCfg.reloadTime)
        {
            reloadTimer = 0;
            bulletCount = weaponCfg.magazineSize;
            fireTimer = weaponCfg.fireRate;
        }
    }

    public virtual bool CanFire()
    {
        return false;
    }

    public virtual bool CanManualFire()
    {
        return false;
    }

    public virtual void WeaponUpdate(Transform carTrs)
    {
        
    }
}
