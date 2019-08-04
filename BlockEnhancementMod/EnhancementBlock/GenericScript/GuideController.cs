using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Common;

namespace BlockEnhancementMod
{
    class GuideController : MonoBehaviour
    {
        //Guide Setting
        private Rigidbody blockRigidbody;
        private BlockBehaviour block;
        private RadarScript blockRadar;
        public float prediction = 10f;
        public float searchAngle = 0f;
        public float initialDistance = 1f;
        public float torque = 0f;
        public float maxTorque = 10000f;
        private Vector3 previousVelocity = Vector3.zero;
        private Vector3 acceleration = Vector3.zero;


        //Aerodynamics setting
        private readonly float aeroEffectMultiplier = 5f;
        private Vector3 aeroEffectPosition = Vector3.zero;
        public bool enableAerodynamicEffect = false;
        public void SetupGuideController(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar, float sourceSearchAngle, float sourceTorque)
        {
            block = sourceBlock;
            blockRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = false;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
        }

        public void SetupGuideController(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar,
            bool sourceEnableAerodynamicEffect, float sourceSearchAngle, float sourceTorque)
        {
            block = sourceBlock;
            blockRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = sourceEnableAerodynamicEffect;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
        }

        void LateUpdate()
        {
            if (block == null || blockRigidbody == null) return;
            if (enableAerodynamicEffect)
            {
                AddAerodynamicsToRocketVelocity();
            }
            if (blockRadar == null) return;
            if (!StatMaster.isClient)
            {
                if (blockRadar.target == null) return;
                // Calculating the rotating axis
                Vector3 velocity = Vector3.zero;
                try
                {
                    velocity = blockRadar.target.collider.attachedRigidbody.velocity - block.Rigidbody.velocity;
                    if (previousVelocity != Vector3.zero)
                    {
                        acceleration = (velocity - previousVelocity) / Time.deltaTime;
                    }
                    previousVelocity = velocity;
                }
                catch { }
                //Add position prediction
                float ratio = (blockRadar.target.collider.bounds.center - block.transform.position).magnitude / initialDistance;
                float actualPrediction = prediction * Mathf.Clamp(Mathf.Pow(ratio, 2), 0f, 1.5f);
                float pathPredictionTime = Time.fixedDeltaTime * actualPrediction;
                Vector3 forwardDirection = block.BlockID == (int)BlockType.Rocket ? block.transform.up : block.transform.forward;
                Vector3 positionDiff = blockRadar.target.collider.bounds.center + velocity * pathPredictionTime + 0.5f * acceleration * pathPredictionTime * pathPredictionTime - block.transform.position;
                float angleDiff = Vector3.Angle(positionDiff, forwardDirection);
                bool forward = Vector3.Dot(forwardDirection, positionDiff) > 0;
                Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, forwardDirection);
#if DEBUG
                //Debug.Log("forward is " + forward);
                //Debug.Log("angle diff is " + angleDiff);
#endif
                //Add torque to the rocket based on the angle difference
                //If in auto guide mode, the rocket will restart searching when target is out of sight
                //else, apply maximum torque to the rocket
                if (forward && angleDiff <= searchAngle)
                {
#if DEBUG
                    //Debug.Log("target in range");
#endif
                    blockRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / searchAngle - 1f, 2) + 1)) * rotatingAxis);
                }
                else
                {
#if DEBUG
                    Debug.Log("target out of range");
#endif
                    blockRadar.SendClientTargetNull();
                }
            }
        }

        private void AddAerodynamicsToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(blockRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * aeroEffectMultiplier;
            float velocitySqr = blockRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);

            Vector3 force = transform.localToWorldMatrix * Vector3.Scale(dir, -locVel) * currentVelocitySqr;
            blockRigidbody.AddForceAtPosition(force, block.transform.position - aeroEffectPosition);
        }
    }
}
