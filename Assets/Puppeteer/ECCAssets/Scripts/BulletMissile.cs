using EndlessCarChase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMissile : BulletBase
{
    private Vector3 fireDir;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 unPoweredPos;
    private float unpoweredTime = 2f;
    private float unpoweredTimer = 0f;
    public float verticalSpeed = 0f; // 导弹的向下速度
    private float distanceToTarget; // 导弹与目标的水平距离
    private float verticalOffset; // 目标与导弹的高度偏差
    private float gravity = 9.8f;
    float time = 0f;
    private float timeToReachTarget; // 导弹到达目标落点所需的时间

    private bool isStartUp  = true;

    //private bool isMustInView = true;

    public void SetParam(float _maximumRange,GameObject _shooter, int _hurt = 1, int _hurtRange = 0, float _speed = 5)
    {
        hurt = _hurt;
        hurtRange = _hurtRange;
        speed = _speed;
        maximumRange = _maximumRange;

        shooter = _shooter;
        startPoint = shooter.transform.position;
    }

    public void Go(Transform targetTrs)
    {
        endPoint = targetTrs.position;
        //endPoint = startPoint + (holderTrs.forward * 20);
        //fireDir = (endPoint - holderTrs.position).normalized;
    }

    public override void BulletUpdate()
    {
        base.BulletUpdate();
        if (gameObject.activeSelf)
        {
            if(isStartUp)
            {
                Quaternion currentRotation = transform.rotation;
                transform.LookAt(transform.position + Vector3.up, Vector3.back);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentRotation.eulerAngles.z);
                transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
                unpoweredTimer += Time.deltaTime;
                if(unpoweredTimer >= unpoweredTime)
                {
                    isStartUp = false;
                    unpoweredTimer = 0f;
                    unPoweredPos = transform.position;


                    distanceToTarget = Vector3.Distance(new Vector3(unPoweredPos.x, 0f, unPoweredPos.z), new Vector3(endPoint.x, 0f, endPoint.z));
                    verticalOffset = endPoint.y - unPoweredPos.y;
                    timeToReachTarget = distanceToTarget / speed;
                    //verticalSpeed = verticalOffset / timeToReachTarget;
                    verticalSpeed = -2;
                }
            }
            else
            {
                if (time <= timeToReachTarget)
                {
                    float x = unPoweredPos.x + (endPoint.x - unPoweredPos.x) * (time / timeToReachTarget);
                    float y = unPoweredPos.y + verticalSpeed * time - 0.5f * gravity * time * time;
                    float z = unPoweredPos.z + (endPoint.z - unPoweredPos.z) * (time / timeToReachTarget);

                    Vector3 oldTrs = transform.position;
                    transform.position = new Vector3(x, y, z);

                    time += Time.deltaTime;
                    Vector3 rotationDir = (transform.position - oldTrs).normalized;
                    if(rotationDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(rotationDir);
                    }                
                }
                else
                {
                    DestroyMyself();
                    //transform.position = endPoint;
                    CalHurtRange();
                }
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
        isStartUp = true;
        time = 0;
        unPoweredPos = Vector3.zero;


        gameController.dictObjectPool["FX_Missile"].ReturnObject(gameObject);
        gameController.RemoveBulletFromList(this);
    }

    public void CalHurtRange()
    {
        ECCCar[] allCars = GameObject.FindObjectsOfType<ECCCar>();
        for (int i = 0; i < allCars.Length; ++i)
        {
            float distance = Vector3.Distance(allCars[i].gameObject.transform.position, endPoint);
            if(distance <= hurtRange)
            {
                ProcessDamage(allCars[i]);
            }
        }
          
    }

    private void ProcessDamage(ECCCar car)
    {
        int damage = GetHurt();
        //Debug.Log("damage:" + damage.ToString());
        //Debug.Log("carHp:" + car.health.ToString());
        car.ChangeHealth(-damage);

        GameObject hitEffect = gameController.dictObjectPool["FX_Missile_Hit"].GetObject();
        hitEffect.transform.position = car.transform.position;
        hitEffect.transform.rotation = car.transform.rotation;
        ParticleControl particleControl = hitEffect.GetComponent<ParticleControl>();
        particleControl.Play();
    }

   
}
