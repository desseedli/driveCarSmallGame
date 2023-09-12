using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UIFramework;

namespace EndlessCarChase
{
	public class ECCGameController : MonoBehaviour 
	{
        public Transform cameraHolder;
        internal Transform miniMap;
        public float cameraRotate = 0;
        public ECCCar playerObject;
        internal float playerDirection;
        public bool directionalControls = false;
        public Transform groundObject;
        public LayerMask groundLayer;
        public float groundTextureSpeed = -0.4f;
        public float startDelay = 1;
        internal bool gameStarted = false;
        public Transform readyGoEffect;
        public int score = 0;
        public int scorePerSecond = 1;
        public Transform scoreText;
        public string moneyPlayerPrefs = "Money";

        internal int highScore = 0;
		internal int scoreMultiplier = 1;
        public ECCShop shopMenu;

        public Slider steeringWheel;

        // Various canvases for the UI
        public Transform gameCanvas;
        public Transform healthCanvas;
        public Transform pauseCanvas;
		public Transform gameOverCanvas;

		internal bool  isGameOver = false;
		
		public string mainMenuLevelName = "ECCGame";
		
		public AudioClip soundGameOver;
		public string soundSourceTag = "Sound";
		internal GameObject soundSource;
		
		public string confirmButton = "Submit";
		
		public string pauseButton = "Cancel";
		internal bool  isPaused = false;

		internal int index = 0;

        internal Vector2 gameArea = new Vector2(10, 10);
        internal bool wrapAroundGameArea = false;

        private bool isManualSpeed = false;
        public float speedChangeTime = 0.5f;
        private float speedChangeTimer = 0f;

        private bool isUseMouseControlRotate = false;
        public float mouseControlRotateTime = 0.1f;
        public float mouseControlRotateTimer = 0;

        public Dictionary<string, ObjectPool> dictObjectPool = new Dictionary<string, ObjectPool>();
        public List<BulletBase> listBullet = new List<BulletBase>();

        void Awake()
		{
            Time.timeScale = 1;

            if (shopMenu)
            {
                shopMenu.currentItem = PlayerPrefs.GetInt(shopMenu.currentPlayerprefs, shopMenu.currentItem);
                playerObject = shopMenu.items[shopMenu.currentItem].itemIcon.GetComponent<ECCCar>();
            }
            string configpath = Application.dataPath + "/BinaryData.data";
            ConfigManager.LoadConfig(configpath);
        }

		void Start()
		{
            //Application.targetFrameRate = 30;

            // Update the score at 0
            ChangeScore(0);

            //Hide the cavases
            if ( shopMenu )    shopMenu.gameObject.SetActive(false);
            if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);
            if ( gameCanvas)    gameCanvas.gameObject.SetActive(false);

            //Get the highscore for the player
            highScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "HighScore", 0);

            //Assign the sound source for easier access
            if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

            if (steeringWheel) steeringWheel.gameObject.SetActive(false);


            ShowSpeedModeLog();
            ShowControlRotateLog();
            speedChangeTimer = speedChangeTime;
            //mouseControlRotateTime = 5f;

            //CreateObjectPool();  //temp remove

            UIManager.GetInstance().ShowUI(UIPage.GameMenu);
        }

        private void CreateObjectPool()
        {
            ObjectPool objectPool = new ObjectPool("FX_Bullet", 20);
            dictObjectPool["FX_Bullet"] = objectPool;
            objectPool = new ObjectPool("FX_Bullet_Enemy", 20);
            dictObjectPool["FX_Bullet_Enemy"] = objectPool;
            objectPool = new ObjectPool("FX_Bullet_Hit", 3);
            dictObjectPool["FX_Bullet_Hit"] = objectPool;
            objectPool = new ObjectPool("FX_Missile_Hit", 3);
            dictObjectPool["FX_Missile_Hit"] = objectPool;
            objectPool = new ObjectPool("FX_Missile", 3);
            dictObjectPool["FX_Missile"] = objectPool;
            objectPool = new ObjectPool("FX_Missile_Range", 5);
            dictObjectPool["FX_Missile_Range"] = objectPool;
            objectPool = new ObjectPool("CarExplode", 5);
            dictObjectPool["CarExplode"] = objectPool;
        }

        private void ShowSpeedModeLog()
        {
            if (isManualSpeed)
            {
                Debug.LogWarning("手动加速减速模式,按键盘W加速，按键盘S减速");
            }
            else
            {
                Debug.LogWarning("自动模式");
            }
        }

        private void ShowControlRotateLog()
        {
            if(isUseMouseControlRotate)
            {
                Debug.LogWarning("使用鼠标来控制车辆的行进方向");
            }
            else
            {
                Debug.LogWarning("使用键盘来控制车辆的行进方向");
            }
        }

        public void StartGame()
        {
            if (playerObject)
            {
                playerObject = Instantiate(playerObject);
                playerObject.tag = "Player";
            }

            if (GetComponent<ECCSpawnAroundObject>()) GetComponent<ECCSpawnAroundObject>().isSpawning = true;
            if (GetComponent<ECCSpawnOnPoints>()) GetComponent<ECCSpawnOnPoints>().isSpawning = true;

            if (gameCanvas) gameCanvas.gameObject.SetActive(true);

            if (readyGoEffect) Instantiate(readyGoEffect);

            gameStarted = true;

            if (scorePerSecond > 0)    
                InvokeRepeating("ScorePerSecond", startDelay, 1);

            if (steeringWheel && Application.isMobilePlatform) steeringWheel.gameObject.SetActive(true);
        }

        float Remap(float inputValue, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * (inputValue - inputMin) / (inputMax - inputMin);
        }

        void Update()
		{
            if ( gameStarted == false) return;

			if ( startDelay > 0 )
			{
				startDelay -= Time.deltaTime;
            }
			else
			{
				if ( isGameOver == true )
				{
					if ( Input.GetButtonDown(confirmButton) )
					{
						Restart();
					}
					
					if ( Input.GetButtonDown(pauseButton) )
					{
						MainMenu();
					}
				}
				else
				{
                    if (playerObject)
                    {
                        if (Application.isMobilePlatform)
                        {
                            if (steeringWheel && steeringWheel.gameObject.activeInHierarchy)
                            {
                                if (Input.GetMouseButton(0))
                                {
                                    playerDirection = steeringWheel.value;
                                }
                                else
                                {
                                    steeringWheel.value = playerDirection = 0;
                                }

                                steeringWheel.transform.Find("Wheel").eulerAngles = Vector3.forward * playerDirection * -100;
                            }
                            else if (Input.GetMouseButton(0)) 
                            {
                                if (Input.mousePosition.x > Screen.width * 0.5f)
                                {
                                    playerDirection = 1;
                                }
                                else 
                                {
                                    playerDirection = -1;
                                }
                            }
                            else 
                            {
                                playerDirection = 0;
                            }
                        }
                        else 
                        {                     
                            if ( directionalControls)
                            { 
                                playerObject.targetPosition = playerObject.transform.position + new Vector3(Input.GetAxis("Horizontal") * 5, playerObject.transform.position.y, Input.GetAxis("Vertical") * 5);
                            }
                            else
                            {                                                     
                                if (Input.GetKeyDown(KeyCode.Home))
                                {
                                    isManualSpeed = !isManualSpeed;
                                    ShowSpeedModeLog();
                                }
                                if (Input.GetKeyDown(KeyCode.End))
                                {
                                    isUseMouseControlRotate = !isUseMouseControlRotate;
                                    ShowControlRotateLog();
                                }

                                if (isUseMouseControlRotate)
                                {                   
                                    mouseControlRotateTimer += Time.deltaTime;
                                    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                                    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                                    float rayDistance;
                                    float angle = 0;
                                    if (groundPlane.Raycast(mouseRay, out rayDistance))
                                    {
                                        Vector3 intersectionPoint = mouseRay.GetPoint(rayDistance);
                                        intersectionPoint.y = playerObject.transform.position.y;
                                        //playerObject.targetPosition = intersectionPoint;

                                        Vector3 targetVector = intersectionPoint - playerObject.targetPosition;
                                        Quaternion rotation = Quaternion.LookRotation(targetVector, Vector3.up);
                                        angle = Quaternion.Angle(rotation, Quaternion.LookRotation(playerObject.transform.forward, Vector3.up));
                                        if (Vector3.Cross(playerObject.transform.forward, targetVector).y < 0f)
                                        {
                                            angle = -angle;
                                            angle = Mathf.Clamp(angle, -90f, 90f);
                                            //Debug.Log("angle: " + angle);
                                        }
                                    }

                                    if (mouseControlRotateTimer >= mouseControlRotateTime)
                                    {
                                        //playerDirection = (angle + 180f) / 360f * 2f - 1f;
                                        playerDirection = Remap(angle, -90f, 90f, -1f, 1f);
                                        mouseControlRotateTimer = 0;
                                    }
                                   
                                    if (Mathf.Abs(angle) <= 10)
                                    {
                                        playerDirection = 0f;
                                    }
                                    //Debug.Log("playerDirection in UseMouseControlRotate: " + playerDirection);
                                }
                                else
                                {
                                    playerDirection = Input.GetAxis("Horizontal");
                                    //Debug.Log("playerDirection:" + playerDirection.ToString()); 
                                }

                                if (isManualSpeed)
                                {
                                    if(Input.GetKey(KeyCode.W))
                                    {
                                        speedChangeTimer += Time.deltaTime;
                                        if (speedChangeTimer - speedChangeTime > 0)
                                        {
                                            playerObject.AddSpeed();
                                            speedChangeTimer = 0;
                                        }         
                                    }
                                    else if(Input.GetKey(KeyCode.S))
                                    {
                                        speedChangeTimer += Time.deltaTime;
                                        if (speedChangeTimer - speedChangeTime > 0)
                                        {
                                            playerObject.SubSpeed();
                                            speedChangeTimer = 0;
                                        }
                                    }
                                    if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
                                    {
                                        speedChangeTimer = speedChangeTime;
                                    }
                                }
                                if(Input.GetKeyDown(KeyCode.Space))
                                {
                                    if(!playerObject.isStartNOS)
                                    {
                                        Debug.LogWarning("氮气加速开始");
                                        playerObject.isStartNOS = true;
                                    }                                  
                                }
                            }
                        }

                        // Calculate the rotation direction
                        playerObject.Rotate(playerDirection);

                        // Unused code that makes the player wrap around the edge of the game area
                        if (wrapAroundGameArea == true )
                        {
                            if (playerObject.transform.position.x > gameArea.x * 0.5f) playerObject.transform.position -= Vector3.right * gameArea.x;
                            if (playerObject.transform.position.x < gameArea.x * -0.5f) playerObject.transform.position += Vector3.right * gameArea.x;
                            if (playerObject.transform.position.z > gameArea.y * 0.5f) playerObject.transform.position -= Vector3.forward * gameArea.y;
                            if (playerObject.transform.position.z < gameArea.y * -0.5f) playerObject.transform.position += Vector3.forward * gameArea.y;
                        }
                    }
                    
                    //Toggle pause/unpause in the game
                    if ( Input.GetButtonDown(pauseButton) )
					{
						if ( isPaused == true )    Unpause();
						else    Pause(true);
					}

                    for(int i = 0; i < listBullet.Count;++i)
                    {
                        listBullet[i].BulletUpdate();
                    }
				}
			}
		}

        void LateUpdate()
        {
            if (playerObject)
            {
                if (cameraHolder)
                {
                    cameraHolder.position = playerObject.transform.position;

                    if (cameraRotate > 0) cameraHolder.eulerAngles = Vector3.up * Mathf.LerpAngle(cameraHolder.eulerAngles.y, playerObject.transform.eulerAngles.y, Time.deltaTime * cameraRotate);
                }

                if ( miniMap ) miniMap.position = playerObject.transform.position;

                if (groundObject)
                {
                    groundObject.position = playerObject.transform.position;

                    groundObject.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(playerObject.transform.position.x, playerObject.transform.position.z) * groundTextureSpeed);
                }
            }
        }

        public void ChangeHealth(float changeValue)
        {
            if (playerObject) playerObject.ChangeHealth(changeValue);
        }

        public void  ChangeScore( int changeValue )
		{
			score += changeValue;

            if (scoreText)
            {
                scoreText.GetComponent<Text>().text = score.ToString();

                if (scoreText.GetComponent<Animation>()) scoreText.GetComponent<Animation>().Play();
            }
        }
        
        void SetScoreMultiplier( int setValue )
		{
			scoreMultiplier = setValue;
		}

        public void ScorePerSecond()
        {
            ChangeScore(scorePerSecond);
        }
        public void Pause(bool showMenu)
        {
            isPaused = true;

            Time.timeScale = 0;

            if (showMenu == true)
            {
                if (pauseCanvas) pauseCanvas.gameObject.SetActive(true);
                if (gameCanvas) gameCanvas.gameObject.SetActive(false);
            }
        }

        public void Unpause()
        {
            isPaused = false;

            Time.timeScale = 1;

            if (pauseCanvas) pauseCanvas.gameObject.SetActive(false);
            if (gameCanvas) gameCanvas.gameObject.SetActive(true);
        }

        IEnumerator GameOver(float delay)
		{
			isGameOver = true;

			yield return new WaitForSeconds(delay);
			
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);
            if ( gameCanvas )    gameCanvas.gameObject.SetActive(false);

            int totalMoney = PlayerPrefs.GetInt(moneyPlayerPrefs, 0);

            totalMoney += score;

            PlayerPrefs.SetInt(moneyPlayerPrefs, totalMoney);

            if ( gameOverCanvas )    
			{
				gameOverCanvas.gameObject.SetActive(true);
				
				gameOverCanvas.Find("Base/TextScore").GetComponent<Text>().text = "SCORE " + score.ToString();
				
				if ( score > highScore )    
				{
					highScore = score;
					
					PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "HighScore", score);
				}
				
				gameOverCanvas.Find("Base/TextHighScore").GetComponent<Text>().text = "HIGH SCORE " + highScore.ToString();

				if ( soundSource && soundGameOver )    
				{
					soundSource.GetComponent<AudioSource>().pitch = 1;
					
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
				}
			}
		}
		
		void  Restart()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		
		void  MainMenu()
		{
			SceneManager.LoadScene(mainMenuLevelName);
		}

        public void AddBulletToList(BulletBase bullet)
        {
            listBullet.Add(bullet);
        }

        public void RemoveBulletFromList(BulletBase bullet)
        {
            listBullet.Remove(bullet);
        }

        public bool IsObjectInViewPort(GameObject obj)
        {
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(obj.transform.position);
            bool isIn = viewportPosition.x > 0 && viewportPosition.x < 1 &&
                         viewportPosition.y > 0 && viewportPosition.y < 1 &&
                         viewportPosition.z > 0;
            return isIn;
        }

        void OnDrawGizmos()
        {
            //Gizmos.color = Color.blue;

            // Draw two lines to show the edges of the street
            //Gizmos.DrawWireCube(Vector3.zero, new Vector3(gameArea.x, 1, gameArea.y));
        }
    }
}