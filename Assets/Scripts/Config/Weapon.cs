
//-----------------------------------------------
//              生成代码不要修改
//-----------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class WeaponCfg
{
    public readonly int ID;    //		主键
    public readonly string name;    //		名字
    public readonly float attackRange;    //		索敌距离
    public readonly int magazineSize;    //		弹夹容量
    public readonly float reloadTime;    //		冷却时间
    public readonly float fireRate;    //		发射间隔
    public readonly int hurt;    //		伤害
    public readonly float hurtRange;    //		伤害范围
    public readonly float speed;    //		速度
    public readonly float maximumRange;    //		子弹最远飞多远
    public readonly string bulletPrefab;    //		子弹prefab
    public readonly string bulletHitEffect;    //		子弹击中prefab

    public WeaponCfg(DynamicPacket packet)
    {
        ID = packet.PackReadInt32();
        name = packet.PackReadString();
        attackRange = packet.PackReadFloat();
        magazineSize = packet.PackReadInt32();
        reloadTime = packet.PackReadFloat();
        fireRate = packet.PackReadFloat();
        hurt = packet.PackReadInt32();
        hurtRange = packet.PackReadFloat();
        speed = packet.PackReadFloat();
        maximumRange = packet.PackReadFloat();
        bulletPrefab = packet.PackReadString();
        bulletHitEffect = packet.PackReadString();
    }
}

public class WeaponCfgMgr
{
    private static WeaponCfgMgr mInstance;
    
    public static WeaponCfgMgr Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new WeaponCfgMgr();
            }
            
            return mInstance;
        }
    }

    private Dictionary<int, WeaponCfg> mDict = new Dictionary<int, WeaponCfg>();
    
    public Dictionary<int, WeaponCfg> Dict
    {
        get {return mDict;}
    }

    public void Deserialize (DynamicPacket packet)
    {
        int num = (int)packet.PackReadInt32();
        for (int i = 0; i < num; i++)
        {
            WeaponCfg item = new WeaponCfg(packet);
            if (mDict.ContainsKey(item.ID))
            {
                mDict[item.ID] = item;
            }
            else
            {
                mDict.Add(item.ID, item);
            }
        }
    }
    
    public WeaponCfg GetTemplateByID(int id)
    {
        if(mDict.ContainsKey(id))
        {
            return mDict[id];
        }
        
        return null;
    }
}
