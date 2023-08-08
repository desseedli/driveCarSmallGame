using EndlessCarChase;
using UnityEngine;

public class WeaponBase
{
  /*  protected float attackRange;    //���Թ�����Բ�η�Χ
    protected int magazineSize;      //��������
    protected float reloadTime;      //װ��ʱ��
    protected float fireRate;        //������*/
    protected float fireTimer = 0f;
    private float reloadTimer = 0f;
    protected int bulletCount;
    public Transform holderTrs;
    public bool isManualMode = false;

    //bullet param
/*    protected int hurt;
    protected int hurtRange;
    protected float speed;
    protected float maximumRange;    //������*/

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
