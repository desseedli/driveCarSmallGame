using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Reflection;

namespace EndlessCarChase
{
/*    public struct WeaponXml
    {
        public float attackRange;    //可以攻击的圆形范围
        public int magazineSize;      //弹夹容量
        public float reloadTime;      //装弹时间
        public float fireRate;        //射击间隔
        //bullet param
        public int hurt;
        public int hurtRange;
        public float speed;
        public float maximumRange;    //最大射程
        public string bulletPrefab;
        public string bulletHitEffect;
    }*/

    public class ECCCar : MonoBehaviour
    {
        // Various varialbes for quicker access
        internal Transform thisTransform;
        static ECCGameController gameController;
        internal Transform chaseTarget;
        internal Vector3 targetPosition;

        internal RaycastHit groundHitInfo;
        internal Vector3 groundPoint;
        internal Vector3 forwardPoint;
        internal float forwardAngle;
        internal float rightAngle;
        public float health = 10;
        internal float healthMax;
        internal Transform healthBar;
        internal Image healthBarFill;
        public float hurtDelay = 2;
        internal float hurtDelayCount = 0;
        public Color hurtFlashColor = new Color(0.5f, 0.5f, 0.5f, 1);
        public float speed = 10;
        internal float speedMultiplier = 1;
        public float rotateSpeed = 200;
        internal float currentRotation = 0;
        public int damage = 1;
        public Transform hitEffect;
        public Transform deathEffect;
        public float driftAngle = 50;
        public float leanAngle = 10;
        public float leanReturnSpeed = 5;
        public Transform chassis;
        public Transform[] wheels;
        public int frontWheels = 2;
        internal int index;

        [Header("AI Car Attributes")]
        public float speedVariation = 2;
        internal float chaseAngle;
        public Vector2 chaseAngleRange = new Vector2(0, 30);
        public bool avoidObstacles = true;
        public float detectAngle = 2;
        public float detectDistance = 3;
        public bool chaseOtherCars = true;
        public float changeTargetChance = 0.01f;


        [Header("DEMO Feature")]
        public float NOSModifySpeedMultiplier = 1.3f;
        public float NOSDuration = 2f;
        private float NOSTimer = 0f;
        [HideInInspector] public bool isStartNOS = false;
        public float speedChangeValue = 1;
        public int maxSpeed = 50;
        public int minSpeed = -10;
        private ECCAI eccAI;

        [Header("DEMO Weapon And Bullet")]
        public string weaponNumber;
        private List<WeaponBase> listWeapon = new List<WeaponBase>();

        private void Start()
        {
            thisTransform = this.transform;
            if ( gameController == null )    gameController = GameObject.FindObjectOfType<ECCGameController>();

            if (chaseTarget == null && gameController.gameStarted == true && gameController.playerObject)
            {
                chaseTarget = gameController.playerObject.transform;
            }

            RaycastHit hit;
            if (Physics.Raycast(thisTransform.position + Vector3.up * 5 + thisTransform.forward * 1.0f, -10 * Vector3.up, out hit, 100, gameController.groundLayer))
            { 
                forwardPoint = hit.point;
            }

            thisTransform.Find("Base").LookAt(forwardPoint);

            if ( thisTransform.Find("HealthBar") )
            {
                healthBar = thisTransform.Find("HealthBar");

                healthBarFill = thisTransform.Find("HealthBar/Empty/Full").GetComponent<Image>();
            }

            healthMax = health;
            ChangeHealth(0);

            string[] numbers = weaponNumber.Split(';');
            foreach(string number in numbers)
            {
                if(int.TryParse(number,out int res))
                {
                    WeaponCfg weaponCfg = WeaponCfgMgr.Instance.GetTemplateByID(res);
                    if (res == 1 || res == 2)
                    {
                        WeaponGun weaponGun = new WeaponGun(weaponCfg);
                        listWeapon.Add(weaponGun);
                    }
                    else if(res == 3)
                    {
                        WeaponMissile weaponMissile = new WeaponMissile(weaponCfg);
                        listWeapon.Add(weaponMissile);
                    }


/*                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load("Assets/Puppeteer/ECCAssets/weapon.xml");
                    XmlNodeList structNodes = xmlDoc.SelectNodes("/config/struct");
                    foreach (XmlNode structNode in structNodes)
                    {
                        string structId = structNode.Attributes["id"].Value;
                        if (int.TryParse(structId,out int weaponId))
                        {
                            XmlNodeList variableNodes = structNode.SelectNodes("variable");
                            Dictionary<string, string> variableValues = new Dictionary<string, string>();
                            foreach (XmlNode variableNode in variableNodes)
                            {
                                string variableName = variableNode.Attributes["name"].Value;
                                string variableValue = variableNode.InnerText;
                                variableValues[variableName] = variableValue;
                            }
                            Type structType = typeof(WeaponXml);
                            object structInstance = Activator.CreateInstance(structType);

                            foreach (KeyValuePair<string, string> kvp in variableValues)
                            {
                                string variableName = kvp.Key;
                                string variableValue = kvp.Value;

                                FieldInfo field = structType.GetField(variableName);
                                if (field != null)
                                {
                                    object parsedValue = Convert.ChangeType(variableValue, field.FieldType);
                                    field.SetValue(structInstance, parsedValue);
                                }
                            }
                            WeaponXml weaponData = (WeaponXml)structInstance;
                            if (res == weaponId)
                            {
                                if (weaponId == 1 || weaponId == 2)
                                {
                                    WeaponGun weaponGun = new WeaponGun(weaponData);
                                    listWeapon.Add(weaponGun);
                                }
                                else if(weaponId == 3)
                                {
                                    WeaponMissile weaponMissile = new WeaponMissile(weaponData);
                                    listWeapon.Add(weaponMissile);
                                }
                            }                       
                        }                 
                    }*/
                }
            }
        }

        public void SetAIMode(ECCAI ai)
        {
            eccAI = ai;
        }

        // This function runs whenever we change a value in the component
        private void OnValidate()
        {
            // Limit the maximum number of front wheels to the actual front wheels we have
            frontWheels = Mathf.Clamp(frontWheels, 0, wheels.Length);
        }

        public void AddSpeed()
        {
            speed += speedChangeValue;
            if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
            Debug.LogWarning("目前速度:" + speed.ToString());
        }

        public void SubSpeed()
        {
            speed -= speedChangeValue;
            if (speed <= minSpeed)
            {
                speed = minSpeed;
            }
            Debug.LogWarning("目前速度:" + speed.ToString());
        }

        // Update is called once per frame
        void Update()
        {
            if (isStartNOS)
            {
                NOSTimer += Time.deltaTime;
                if(NOSTimer >= NOSDuration)
                {
                    isStartNOS = false;
                    NOSTimer = 0;
                    speedMultiplier = 1;
                    Debug.LogWarning("氮气加速时间结束");
                }
                else
                {
                    speedMultiplier = NOSModifySpeedMultiplier;
                }
            }


            // Directional controls and acceleration/stopping for player car
            if (gameController.playerObject == this)
            {
                /* (UNUSED CODE)
                // calculate the accelration of the car, as long as there is user input
                float acceleration = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));

                // Smoothly change the speed multiplier to match the current accelration value
                speedMultiplier = Mathf.Lerp(speedMultiplier, acceleration, Time.deltaTime);
                */

                // Rotate the car until it reaches the desired chase angle from either side of the player
                if (Vector3.Angle(thisTransform.forward, targetPosition - thisTransform.position) > 0)
                {
                    Rotate(ChaseAngle(thisTransform.forward, targetPosition - thisTransform.position, Vector3.up));
                }

            }

            // If the game hasn't started yet, nothing happens
            if (gameController && gameController.gameStarted == false) return;

            // If we have no target, choose a random target to chase
/*            if ( chaseOtherCars == true && Random.value < changeTargetChance )
            { 
                ChooseTarget();
            }*/

            // Move the player forward
            thisTransform.Translate(Vector3.forward * Time.deltaTime * speed * speedMultiplier, Space.Self);

            // Get the current position of the target player
            if ( health > 0 )
            {
                if (chaseTarget)
                {
                    targetPosition = chaseTarget.transform.position;
                }

                if (healthBar)
                { 
                    healthBar.LookAt(Camera.main.transform);
                }
            }
            else
            {
                if (healthBar && healthBar.gameObject.activeSelf == true )
                { 
                    healthBar.gameObject.SetActive(false);
                }
            }

            if (gameController.playerObject != this)
            {
                eccAI.AIUpdate();
            }

            for(int i = 0; i < listWeapon.Count;++i)
            {
                listWeapon[i].WeaponUpdate(thisTransform);
            }

            // If we have no ground object assigned, or it is turned off, then cars will use raycast to move along terrain surfaces
            if ( gameController.groundObject == null || gameController.groundObject.gameObject.activeSelf == false )
            { 
                DetectGround();
            }

            // Count down the hurt delay, during which the car can't be hurt again
            if (hurtDelayCount > 0 && health > 0)
            {
                hurtDelayCount -= Time.deltaTime;

                // Change the emission color of the car to indicate that the car is hurt
                if ( GetComponentInChildren<MeshRenderer>() )
                {
                    foreach ( Material part in GetComponentInChildren<MeshRenderer>().materials )
                    {
                        if (Mathf.Round(hurtDelayCount * 10) % 2 == 0) part.SetColor("_EmissionColor", Color.black);
                        else part.SetColor("_EmissionColor", hurtFlashColor);

                        //hurtFlashObject.material.SetColor("_EmissionColor", hurtFlashColor);
                    }
                }

            }
        }
        
        public float ChaseAngle(Vector3 forward, Vector3 targetDirection, Vector3 up)
        {
            // Calculate the approach angle
            float approachAngle = Vector3.Dot(Vector3.Cross(up, forward), targetDirection);
            
            // If the angle is higher than 0, we approach from the left ( so we must rotate right )
            if (approachAngle > 0f)
            {
                return 1f;
            }
            else if (approachAngle < 0f) //Otherwise, if the angle is lower than 0, we approach from the right ( so we must rotate left )
            {
                return -1f;
            }
            else // Otherwise, we are within the angle range so we don't need to rotate
            {
                return 0f;
            }
        }

        public void Rotate( float rotateDirection )
        {
            //thisTransform.localEulerAngles = new Vector3(Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal).eulerAngles.x, thisTransform.localEulerAngles.y, Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal).eulerAngles.z);

            //thisTransform.rotation = Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal);


            // If the car is rotating either left or right, make it drift and lean in the direction its rotating
            if ( rotateDirection != 0 )
            {
                //thisTransform.localEulerAngles = Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal).eulerAngles + Vector3.up * currentRotation;

                // Rotate the car based on the control direction
                thisTransform.localEulerAngles += Vector3.up * rotateDirection * rotateSpeed * Time.deltaTime;

                thisTransform.eulerAngles = new Vector3(thisTransform.eulerAngles.x, thisTransform.eulerAngles.y, thisTransform.eulerAngles.z);

                //thisTransform.eulerAngles = new Vector3(rightAngle, thisTransform.eulerAngles.y, forwardAngle);

                currentRotation += rotateDirection * rotateSpeed * Time.deltaTime;

                if (currentRotation > 360)
                { 
                    currentRotation -= 360;
                }
                //print(forwardAngle);
                // Make the base of the car drift based on the rotation angle
                thisTransform.Find("Base").localEulerAngles = new Vector3(rightAngle, Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle + Mathf.Sin(Time.time * 50) * hurtDelayCount * 50, Time.deltaTime), 0);
                //  Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime);

                // Make the chassis lean to the sides based on the rotation angle
                if (chassis) chassis.localEulerAngles = Vector3.forward * Mathf.LerpAngle(chassis.localEulerAngles.z, rotateDirection * leanAngle, Time.deltaTime);
                //  Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime);
                
                // Play the skidding animation. In this animation you can trigger all kinds of effects such as dust, skid marks, etc
                GetComponent<Animator>().Play("Skid");

                // Go through all the wheels making them spin, and make the front wheels turn sideways based on rotation
                for (index = 0; index < wheels.Length; index++)
                {
                    // Turn the front wheels sideways based on rotation
                    if (index < frontWheels)
                    {
                        wheels[index].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[index].localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime * 10);
                    }

                    // Spin the wheel
                    wheels[index].Find("WheelObject").Rotate(Vector3.right * Time.deltaTime * speed * 20, Space.Self);
                }
            }
            else // Otherwise, if we are no longer rotating, straighten up the car and front wheels
            {
                // Return the base of the car to its 0 angle
                thisTransform.Find("Base").localEulerAngles = Vector3.up * Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, 0, Time.deltaTime * 5);

                // Return the chassis to its 0 angle
                if (chassis) chassis.localEulerAngles = Vector3.forward * Mathf.LerpAngle(chassis.localEulerAngles.z, 0, Time.deltaTime * leanReturnSpeed);//  Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime);

                // Play the move animation. In this animation we stop any previously triggered effects such as dust, skid marks, etc
                GetComponent<Animator>().Play("Move");

                // Go through all the wheels making them spin faster than when turning, and return the front wheels to their 0 angle
                for (index = 0; index < wheels.Length; index++)
                {
                    // Return the front wheels to their 0 angle
                    if (index < frontWheels) wheels[index].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[index].localEulerAngles.y, 0, Time.deltaTime * 5);

                    // Spin the wheel faster
                    wheels[index].Find("WheelObject").Rotate(Vector3.right * Time.deltaTime * speed * 30, Space.Self);
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            if ( hurtDelayCount <= 0  && other.GetComponent<ECCCar>() )
            {
                hurtDelayCount = hurtDelay;

                other.GetComponent<ECCCar>().ChangeHealth(-damage);

                if (health - damage > 0 && hitEffect) Instantiate(hitEffect, transform.position, transform.rotation);
            }
        }

        public void ChangeHealth(float changeValue)
        {
            health += changeValue;

            if (health > healthMax) health = healthMax;

            if ( healthBar )
            {
                healthBarFill.fillAmount = health / healthMax;
            }

            if (changeValue < 0 && gameController.playerObject == this) Camera.main.transform.root.GetComponent<Animation>().Play();

            if (health <= 0)
            {
                if (gameController.playerObject && gameController.playerObject != this)
                {
                    DelayedDie();
                }
                else
                {
                    Die();
                }

                if (gameController.playerObject && gameController.playerObject == this)
                {
                    gameController.SendMessage("GameOver", 1.2f);

                    Time.timeScale = 0.5f;
                }
            }

            if ( gameController.playerObject && gameController.playerObject == this && gameController.healthCanvas)
            {
                if (gameController.healthCanvas.Find("Full")) gameController.healthCanvas.Find("Full").GetComponent<Image>().fillAmount = health / healthMax;

                if (gameController.healthCanvas.Find("Text")) gameController.healthCanvas.Find("Text").GetComponent<Text>().text = health.ToString();

                if (gameController.healthCanvas.GetComponent<Animation>()) gameController.healthCanvas.GetComponent<Animation>().Play();
            }
        }

        public void Die()
        {
            if (deathEffect) Instantiate(deathEffect, transform.position, transform.rotation);

            Destroy(gameObject);
        }

        public void DelayedDie()
        {
            //chaseTarget = null;

            for (index = 0; index < wheels.Length; index++)
            {
                wheels[index].transform.SetParent(chassis);
            }

            targetPosition = thisTransform.forward * -10;

            leanAngle = UnityEngine.Random.Range(100,300);

            driftAngle = UnityEngine.Random.Range(100, 150); ;

            //rotateSpeed *= 2;

            Invoke("Die", UnityEngine.Random.Range(0,0.8f));
        }

        public void DetectGround()
        {
            Ray carToGround = new Ray(thisTransform.position + Vector3.up * 10, -Vector3.up * 20);

            if (Physics.Raycast(carToGround, out groundHitInfo, 20, gameController.groundLayer))
            {
                //transform.position = new Vector3(transform.position.x, groundHitInfo.point.y, transform.position.z);
            }
            
            thisTransform.position = new Vector3(thisTransform.position.x, groundHitInfo.point.y + 0.1f, thisTransform.position.z);

            RaycastHit hit;

            if (Physics.Raycast(thisTransform.position + Vector3.up * 5 + thisTransform.forward * 1.0f, -10 * Vector3.up, out hit, 100, gameController.groundLayer))
            {
                forwardPoint = hit.point;
            }
            else if ( gameController.groundObject && gameController.groundObject.gameObject.activeSelf == true )
            {
                forwardPoint = new Vector3(thisTransform.position.x, gameController.groundObject.position.y, thisTransform.position.z);
            }

            thisTransform.Find("Base").LookAt(forwardPoint);
        }

        public bool IsPlayer()
        {
            if(this == gameController.playerObject)
            {
                return true;
            }
            return false;
        }
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawRay(transform.position + Vector3.up * 0.2f + transform.right * detectAngle * 0.5f + transform.right * detectAngle * 0.0f * Mathf.Sin(Time.time * 50), transform.TransformDirection(Vector3.forward) * detectDistance);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.2f + transform.right * -detectAngle * 0.5f - transform.right * detectAngle * 0.0f * Mathf.Sin(Time.time * 50), transform.TransformDirection(Vector3.forward) * detectDistance);

            Gizmos.DrawSphere(forwardPoint, 0.5f);

            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(targetPosition, 0.3f);

            Gizmos.color = Color.green;
            if(eccAI != null)
            {
                Gizmos.DrawSphere(eccAI.targetPosition,1f);
            }
        }
    }
}
