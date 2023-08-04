using EndlessCarChase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase:MonoBehaviour
{
    protected int hurt;
    protected int hurtRange;
    protected float speed;
    protected float maximumRange;    //×î´óÉä³Ì
    protected GameObject shooter;
    protected string prefabName;
    protected string hitEffect;

    protected static ECCGameController gameController;

/*    public BulletBase()
    {
        if (gameController == null) gameController = GameObject.FindObjectOfType<ECCGameController>();
    }*/

    public void Awake()
    {
        if (gameController == null) gameController = GameObject.FindObjectOfType<ECCGameController>();
    }
    public void Start()
    {
        //if (gameController == null) gameController = GameObject.FindObjectOfType<ECCGameController>();
    }

    public virtual void BulletUpdate()
    {
        
    }

    public virtual void DestroyMyself()
    {
 
    }

    public int GetHurt()
    {
        return hurt;
    }
}
