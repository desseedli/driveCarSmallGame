﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace EndlessCarChase
{
    public class ECCRandomPointInCircleAI : ECCAI
    {
        public float radius = 0f;
        public float recalculateTime = 0f;
        private float recalculateTimer = 0f;

        public ECCRandomPointInCircleAI(GameObject gameObject, ECCCar eccCar,float _radius = 20f,float _recalculateTime = 2f) :base(gameObject,eccCar)
        {
            if (chaseTarget == null && gameController.gameStarted == true && gameController.playerObject)
            {
                CalTargetPosition();
            }
            radius = _radius;
            recalculateTime = _recalculateTime;
        }

        public void CalTargetPosition()
        {

            Vector2 center = new Vector2(gameController.playerObject.transform.position.x, gameController.playerObject.transform.position.z);
            Vector2 randomPoint = Random.insideUnitCircle * radius + center;
            targetPosition = new Vector3(randomPoint.x, 0, randomPoint.y);
        }

        public override void AIUpdate()
        {
            // If the game hasn't started yet, nothing happens
            if (gameController && gameController.gameStarted == false) return;

            // If we have no target, choose a random target to chase
            if (m_eccCar.chaseOtherCars == true && Random.value < m_eccCar.changeTargetChance)
            {
                ChooseTarget();
            }

            if (chaseTarget)
            {
                targetPosition = chaseTarget.transform.position;
            }
            else
            {
                if(gameController.playerObject != null)
                {
                    recalculateTimer += Time.deltaTime;
                    if (recalculateTimer >= recalculateTime)
                    {
                        CalTargetPosition();
                        recalculateTimer = 0f;
                    }
                }
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

                // Rotate left to avoid obstacle
                m_eccCar.Rotate(-1);

                //obstacleDetected = 0.1f;
            }
            else if (m_eccCar.avoidObstacles == true && Physics.Raycast(rayLeft, out hit, m_eccCar.detectDistance) && (hit.transform.GetComponent<ECCObstacle>() || (hit.transform.GetComponent<ECCCar>() && gameController.playerObject != hit.transform.GetComponent<ECCCar>()))) // Otherwise, if we detect an obstacle on our left side, swerve to the right
            {
                // Change the emission color of the obstacle to indicate that the car detected it
                //if (hit.transform.GetComponent<MeshRenderer>()) hit.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red);

                // Rotate right to avoid obstacle
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
                else // Otherwise, stop rotating
                {
                    m_eccCar.Rotate(0);
                }
            }
        }
    }
}
