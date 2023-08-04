using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Reflection;

namespace EndlessCarChase
{
    /// <summary>
    /// This script defines a car, which has health, speed, rotation speed, damage, and other attributes related to the car's behaviour. It also defines AI controls when the car is not player-controlled.
    /// </summary>
    public struct WeaponXml
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
    }

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

        [Tooltip("The health of the player. If this reaches 0, the player dies")]
        public float health = 10;
        internal float healthMax;

        internal Transform healthBar;
        internal Image healthBarFill;

        [Tooltip("When the car gets hit and hurt, there is a delay during which it cannot be hit again")]
        public float hurtDelay = 2;
        internal float hurtDelayCount = 0;

        [Tooltip("The color in which the car flashes when hurt")]
        public Color hurtFlashColor = new Color(0.5f, 0.5f, 0.5f, 1);

        [Tooltip("The speed of the player, how fast it moves player. The player moves forward constantly")]
        public float speed = 10;
        internal float speedMultiplier = 1;

        [Tooltip("How quickly the player car rotates, in both directions")]
        public float rotateSpeed = 200;
        internal float currentRotation = 0;

        [Tooltip("The damage this car causes when hitting other cars. Damage is reduced from Health.")]
        public int damage = 1;

        [Tooltip("The effect that appears when this car is hit by another car")]
        public Transform hitEffect;

        [Tooltip("The effect that appears when this car dies")]
        public Transform deathEffect;

        [Tooltip("The slight extra rotation that happens to the car as it turns, giving a drifting effect")]
        public float driftAngle = 50;

        [Tooltip("The slight side tilt that happens to the car chassis as the car turns, making it lean inwards or outwards from the center of rotation")]
        public float leanAngle = 10;

        [Tooltip("How quickly the chassis returns to its straight angle after we stop turning")]
        public float leanReturnSpeed = 5;

        [Tooltip("The chassis object of the car which leans when the car rotates")]
        public Transform chassis;

        [Tooltip("The wheels of the car which rotate based on the speed of the car. The front wheels also rotate in the direction the car is turning")]
        public Transform[] wheels;

        [Tooltip("The front wheels of the car also rotate in the direction the car is turning")]
        public int frontWheels = 2;

        internal int index;

        [Header("AI Car Attributes")]
        [Tooltip("A random value that is added to the base speed of the AI car, to make their movements more varied")]
        public float speedVariation = 2;

        // The angle range that AI cars try to chase the player at. So for example if 0 they will target the player exactly, while at 30 angle they stop rotating when they are at a 30 angle relative to the player
        internal float chaseAngle;

        [Tooltip("A random value that is to the chase angle to make the AI cars more varied in how to chase the player")]
        public Vector2 chaseAngleRange = new Vector2(0, 30);

        [Tooltip("Make AI cars try to avoid obstacles. Obstacle are objects that have the ECCObstacle component attached to them")]
        public bool avoidObstacles = true;

        [Tooltip("The width of the obstacle detection area for this AI car")]
        public float detectAngle = 2;

        [Tooltip("The forward distance of the obstacle detection area for this AI car")]
        public float detectDistance = 3;

        [Tooltip("Allow this AI car to chase oher cars ( not just he player)")]
        public bool chaseOtherCars = true;

        [Tooltip("The chance for this AI car to chanage a target to chase other cars ( if chaseOtherCars is true )")]
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
                    XmlDocument xmlDoc = new XmlDocument();
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
                    }
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

        /// <summary>
        /// Is executed when this obstacle touches another object with a trigger collider
        /// </summary>
        /// <param name="other"><see cref="Collider"/></param>
        void OnTriggerStay(Collider other)
        {
            // If the hurt delay is over, and this car was hit by another car, damage it
            if ( hurtDelayCount <= 0  && other.GetComponent<ECCCar>() )
            {
                // Reset the hurt delay
                hurtDelayCount = hurtDelay;

                // Damage the car
                other.GetComponent<ECCCar>().ChangeHealth(-damage);

                // If there is a hit effect, create it
                if (health - damage > 0 && hitEffect) Instantiate(hitEffect, transform.position, transform.rotation);
            }
        }

        /// <summary>
        /// Changes the lives of the player. If lives reach 0, it's game over
        /// </summary>
        /// <param name="changeValue"></param>
        public void ChangeHealth(float changeValue)
        {
            // Change the health value
            health += changeValue;

            // Limit the value of the health to the maximum allowed value
            if (health > healthMax) health = healthMax;

            // Update the value in the health bar, if it exists
            if ( healthBar )
            {
                healthBarFill.fillAmount = health / healthMax;
            }

            // If this is the player car, play the shake animation
            if (changeValue < 0 && gameController.playerObject == this) Camera.main.transform.root.GetComponent<Animation>().Play();

            // If health reaches 0, the car dies
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

                // If this is the player car, trigger the GameOver event
                if (gameController.playerObject && gameController.playerObject == this)
                {
                    gameController.SendMessage("GameOver", 1.2f);

                    // Play a slowmotion effect
                    Time.timeScale = 0.5f;
                }
            }

            // Update the health bar 
            if ( gameController.playerObject && gameController.playerObject == this && gameController.healthCanvas)
            {
                // Update the health bar based on the health we have
                if (gameController.healthCanvas.Find("Full")) gameController.healthCanvas.Find("Full").GetComponent<Image>().fillAmount = health / healthMax;

                if (gameController.healthCanvas.Find("Text")) gameController.healthCanvas.Find("Text").GetComponent<Text>().text = health.ToString();

                // Play the animation of the health icon
                if (gameController.healthCanvas.GetComponent<Animation>()) gameController.healthCanvas.GetComponent<Animation>().Play();
            }
        }

        /// <summary>
        /// Kill the car and create a death effect
        /// </summary>
        public void Die()
        {
            // Create a death effect at the position of the player
            if (deathEffect) Instantiate(deathEffect, transform.position, transform.rotation);

            // Remove the player from the game
            Destroy(gameObject);
        }

        /// <summary>
        /// Make the car lose control for a second, and then kill it
        /// </summary>
        public void DelayedDie()
        {
            //chaseTarget = null;

            // Add all wheels as part of the chasis to make sure they flip over with it
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

        /// <summary>
        /// Detects the terrain under the car and aligns it to it
        /// </summary>
        public void DetectGround()
        {
            // Cast a ray to the ground below
            Ray carToGround = new Ray(thisTransform.position + Vector3.up * 10, -Vector3.up * 20);

            // Detect terrain under the car
            if (Physics.Raycast(carToGround, out groundHitInfo, 20, gameController.groundLayer))
            {
                //transform.position = new Vector3(transform.position.x, groundHitInfo.point.y, transform.position.z);
            }
            
            // Set the position of the car along the terrain
            thisTransform.position = new Vector3(thisTransform.position.x, groundHitInfo.point.y + 0.1f, thisTransform.position.z);

            RaycastHit hit;

            // Detect a point along the terrain in front of the car, so that we can make the car rotate accordingly
            if (Physics.Raycast(thisTransform.position + Vector3.up * 5 + thisTransform.forward * 1.0f, -10 * Vector3.up, out hit, 100, gameController.groundLayer))
            {
                forwardPoint = hit.point;
            }
            else if ( gameController.groundObject && gameController.groundObject.gameObject.activeSelf == true )
            {
                forwardPoint = new Vector3(thisTransform.position.x, gameController.groundObject.position.y, thisTransform.position.z);
            }

            // Make the car look at the point in front of it along the terrain
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
