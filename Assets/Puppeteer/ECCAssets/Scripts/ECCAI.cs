using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace EndlessCarChase
{
    /// <summary>
    /// This script defines a car, which has health, speed, rotation speed, damage, and other attributes related to the car's behaviour. It also defines AI controls when the car is not player-controlled.
    /// </summary>
    public class ECCAI
    {
        // Various varialbes for quicker access
        protected Transform thisTransform;
        protected static ECCGameController gameController;
        protected ECCCar m_eccCar;
        protected Transform chaseTarget;
        public Vector3 targetPosition;

        public ECCAI(GameObject gameObject,ECCCar eccCar)
        {
            thisTransform = gameObject.transform;
            m_eccCar = eccCar;

            // Hold the gamecontroller for easier access
            if (gameController == null)
            { 
                gameController = GameObject.FindObjectOfType<ECCGameController>();
            }
            if (m_eccCar == null)
            {
                m_eccCar = gameObject.transform.GetComponent<ECCCar>();
            }

            // If this AI car can chase other cars, choose one randomly from the scene
            if (m_eccCar.chaseOtherCars == true)
            {
                ChooseTarget();
            }

            // If this is not the player, then it is an AI controlled car, so we set some attribute variations for the AI such as speed and chase angle variations
            m_eccCar.chaseAngle = Random.Range(m_eccCar.chaseAngleRange.x, m_eccCar.chaseAngleRange.y);

            m_eccCar.speed += Random.Range(0, m_eccCar.speedVariation);
        }

        public void ChooseTarget()
        {
            // Get all the cars in the scene
            ECCCar[] allCars = GameObject.FindObjectsOfType<ECCCar>();

            // Choose a random car
            int randomCar = Random.Range(0, allCars.Length);

            // Set the random car as the current chase target
            if ( allCars[randomCar] )    chaseTarget = allCars[randomCar].transform;
        }

        public virtual void AIUpdate()
        {
           
        }       
    }
}
