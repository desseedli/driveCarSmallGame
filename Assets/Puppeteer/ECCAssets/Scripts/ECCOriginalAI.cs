using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace EndlessCarChase
{
    public class ECCOriginalAI : ECCAI
    {
        public ECCOriginalAI(GameObject gameObject, ECCCar eccCar):base(gameObject,eccCar)
        {
            if (chaseTarget == null && gameController.gameStarted == true && gameController.playerObject)
            {
                chaseTarget = gameController.playerObject.transform;
            }
        }

        public override void AIUpdate()
        {
            if (gameController && gameController.gameStarted == false) return;

            if (m_eccCar.chaseOtherCars == true && Random.value < m_eccCar.changeTargetChance)
            {
                ChooseTarget();
            }

            if (chaseTarget)
            {
                targetPosition = chaseTarget.transform.position;
            }

            // Make the AI controlled car rotate towards the player
            Ray rayRight = new Ray(thisTransform.position + Vector3.up * 0.2f + thisTransform.right * m_eccCar.detectAngle * 0.5f + thisTransform.right * m_eccCar.detectAngle * 0.0f * Mathf.Sin(Time.time * 50), thisTransform.TransformDirection(Vector3.forward) * m_eccCar.detectDistance);
            Ray rayLeft = new Ray(thisTransform.position + Vector3.up * 0.2f + thisTransform.right * -m_eccCar.detectAngle * 0.5f - thisTransform.right * m_eccCar.detectAngle * 0.0f * Mathf.Sin(Time.time * 50), thisTransform.TransformDirection(Vector3.forward) * m_eccCar.detectDistance);

            RaycastHit hit;

            // If we detect an obstacle on our right side, swerve to the left
            if (m_eccCar.avoidObstacles == true && Physics.Raycast(rayRight, out hit, m_eccCar.detectDistance) && (hit.transform.GetComponent<ECCObstacle>() || (hit.transform.GetComponent<ECCCar>() && gameController.playerObject != hit.transform.GetComponent<ECCCar>())))
            {
                // Change the emission color of the obstacle to indicate that the car detected it
                //if (hit.transform.GetComponent<MeshRenderer>() ) hit.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red);

                m_eccCar.Rotate(-1);

                //obstacleDetected = 0.1f;
            }
            else if (m_eccCar.avoidObstacles == true && Physics.Raycast(rayLeft, out hit, m_eccCar.detectDistance) && (hit.transform.GetComponent<ECCObstacle>() || (hit.transform.GetComponent<ECCCar>() && gameController.playerObject != hit.transform.GetComponent<ECCCar>()))) // Otherwise, if we detect an obstacle on our left side, swerve to the right
            {
                // Change the emission color of the obstacle to indicate that the car detected it
                //if (hit.transform.GetComponent<MeshRenderer>()) hit.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red);

                m_eccCar.Rotate(1);

                //obstacleDetected = 0.1f;
            }
            else// if (obstacleDetected <= 0) // Otherwise, if no obstacle is detected, keep chasing the player normally
            {
                // Rotate the car until it reaches the desired chase angle from either side of the player
                if (Vector3.Angle(thisTransform.forward, targetPosition - thisTransform.position) > m_eccCar.chaseAngle)
                {
                    m_eccCar.Rotate(m_eccCar.ChaseAngle(thisTransform.forward, targetPosition - thisTransform.position, Vector3.up));
                }
                else
                {
                    m_eccCar.Rotate(0);
                }
            }
        }       
    }
}
