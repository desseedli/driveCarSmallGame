using DG.Tweening;
using EndlessCarChase;
using UnityEngine;

public class BulletMissileImprove : BulletBase
{
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint = Vector3.zero;
    private float _pointCount = 50f;
    private Vector3[] pathVect3;
    private float controlPointHeight = 10f;
    private float angelRange = 3f;
    private float minDis = 5;
    private float maxDis = 10;
    private Transform targetTrs;

    //for OnDrawGizmos
    private Vector3 drawClockwiseRotatedVector = Vector3.zero;
    private Vector3 drawCounterclockwiseRotatedVector = Vector3.zero;
    private Vector3 drawResVector = Vector3.zero;

    //private bool isMustInView = true;

    public void SetParam(WeaponXml weaponXml, GameObject _shooter)
    {
        hurt = weaponXml.hurt;
        hurtRange = weaponXml.hurtRange;
        speed = weaponXml.speed;
        maximumRange = weaponXml.maximumRange;

        shooter = _shooter;
        startPoint = shooter.transform.position;
        if (maxDis >= maximumRange)
        {
            maxDis = maximumRange;
        }

        prefabName = weaponXml.bulletPrefab;
        hitEffect = weaponXml.bulletHitEffect;
    }

    public void Go(Transform targetTrs)
    {
        this.targetTrs = targetTrs;

        ChooseTargetPiont();  //get end piont;

        ObjectPool objPool = gameController.dictObjectPool["FX_Missile_Range"];
        GameObject attackWrarning = objPool.GetObject();
        attackWrarning.transform.position = new Vector3(endPoint.x, endPoint.y + 0.25f, endPoint.z);

        controlPoint = new Vector3(startPoint.x, startPoint.y + controlPointHeight, startPoint.z);
        transform.position = startPoint;
        transform.LookAt(transform.position + Vector3.up, Vector3.back);

        pathVect3 = Bezier2Path(startPoint, controlPoint, endPoint);

        //float totalDis = CalBezierLength();
        //float times = totalDis / speed;
        //Debug.Log("need times:" + times.ToString());
        transform.DOPath(pathVect3, speed).OnWaypointChange(WayPointChange).OnComplete(DestroyMyself).SetSpeedBased().SetEase(Ease.InSine);

    }

    private Vector3 RotateVector(Vector3 vector,float angelRange)
    {
        Quaternion rotation = Quaternion.AngleAxis(angelRange,Vector3.up);
        return rotation * vector;
    }
    public void ChooseTargetPiont()
    {
        Vector3 clockwiseRotatedVector = RotateVector(this.targetTrs.position, angelRange);
        Vector3 counterclockwiseRotatedVector = RotateVector(this.targetTrs.position, -angelRange);
        float angleBetweenVectors = Vector3.Angle(clockwiseRotatedVector, counterclockwiseRotatedVector);
        float randomAngle = Random.Range(0f, angleBetweenVectors);
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
        Vector3 resVect3 = rotation * counterclockwiseRotatedVector.normalized;
        float randomDistance = Random.Range(this.minDis, this.maxDis);
        Vector3 randomPoint = resVect3 * randomDistance + this.targetTrs.position;
        endPoint = randomPoint;

        //drawLine
        drawClockwiseRotatedVector = clockwiseRotatedVector;
        drawCounterclockwiseRotatedVector = counterclockwiseRotatedVector;
        drawResVector = resVect3;
    }

/*    float CalBezierLength()
    {
        float res = 0;
        for(int i = 0; i < pathvec.Length - 1;++i)
        {
            float tempDist = Vector3.Distance(pathvec[i], pathvec[i + 1]);
            res += tempDist;
        }
        return res;
    }*/


    public override void BulletUpdate()
    {
        base.BulletUpdate();
    }


    public void WayPointChange(int index)
    {
        Vector3 oldTrs = pathVect3[index - 1];
        Vector3 currentTrs = pathVect3[index];
        Vector3 rotationDir = (currentTrs - oldTrs).normalized;
        if (rotationDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rotationDir);
        }
    }


    private Vector3[] Bezier2Path(Vector3 startPos, Vector3 controlPos, Vector3 endPos)
    {
        Vector3[] path = new Vector3[(int)_pointCount];
        for (int i = 1; i <= _pointCount; i++)
        {
            float t = i / _pointCount;
            path[i - 1] = Bezier2(startPos, controlPos, endPos, t);
        }
        return path;
    }

    public static Vector3 Bezier2(Vector3 startPos, Vector3 controlPos, Vector3 endPos, float t)
    {
        return (1 - t) * (1 - t) * startPos + 2 * t * (1 - t) * controlPos + t * t * endPos;
    }

    public override void DestroyMyself()
    {
        gameController.dictObjectPool[prefabName].ReturnObject(gameObject);
        gameController.RemoveBulletFromList(this);

        ObjectPool objPool = gameController.dictObjectPool["CarExplode"];
        GameObject CarExplodeEffect = objPool.GetObject();
        CarExplodeEffect.transform.position = endPoint;

        CalHurtRange();
        hurtRange = 0;
        startPoint = Vector3.zero;
        hurt = 0;

        speed = 0f;
        maximumRange = 0f;
        shooter = null;
    }

    public void CalHurtRange()
    {
        ECCCar[] allCars = GameObject.FindObjectsOfType<ECCCar>();
        for (int i = 0; i < allCars.Length; ++i)
        {
            float distance = Vector3.Distance(allCars[i].gameObject.transform.position, endPoint);
            if(distance <= hurtRange)
            {
                //Debug.Log(allCars[i].name);
                ProcessDamage(allCars[i]);
            }
        }
    }

    private void ProcessDamage(ECCCar car)
    {
        int damage = GetHurt();

        car.ChangeHealth(-damage);

        GameObject objHitEffect = gameController.dictObjectPool[hitEffect].GetObject();
        objHitEffect.transform.position = car.transform.position;
        objHitEffect.transform.rotation = car.transform.rotation;
        ParticleControl particleControl = objHitEffect.GetComponent<ParticleControl>();
        particleControl.Play();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(targetTrs.position, drawClockwiseRotatedVector.normalized * maxDis + targetTrs.position);
        Gizmos.DrawLine(targetTrs.position, drawCounterclockwiseRotatedVector.normalized * maxDis + targetTrs.position);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(targetTrs.position, drawResVector.normalized * maxDis + targetTrs.position);       
    }

}
