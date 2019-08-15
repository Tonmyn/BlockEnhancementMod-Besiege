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
        private Rigidbody parentBlockRigidbody;
        private BlockBehaviour parentBlock;
        private RadarScript blockRadar;
        public float prediction = 10f;
        public float searchAngle = 0f;
        public float initialDistance = 1f;
        public float torque = 0f;
        public float maxTorque = 1500f;
        private Vector3 previousVelocity = Vector3.zero;
        private Vector3 acceleration = Vector3.zero;


        //Aerodynamics setting
        private readonly float aeroEffectMultiplier = 5f;
        private Vector3 aeroEffectPosition = Vector3.up * 5f;
        public bool enableAerodynamicEffect = false;
        public void SetupGuideController(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar, float sourceSearchAngle, float sourceTorque)
        {
            parentBlock = sourceBlock;
            parentBlockRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = false;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
        }

        public void SetupGuideController(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar,
            bool sourceEnableAerodynamicEffect, float sourceSearchAngle, float sourceTorque)
        {
            parentBlock = sourceBlock;
            parentBlockRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = sourceEnableAerodynamicEffect;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
        }

        void FixedUpdate()
        {
            if (parentBlock == null || parentBlockRigidbody == null) return;
            if (enableAerodynamicEffect)
            {
                AddAerodynamicsToRocketVelocity();
            }
        }

        void LateUpdate()
        {
            if (parentBlock == null || parentBlockRigidbody == null) return;
            //if (enableAerodynamicEffect)
            //{
            //    AddAerodynamicsToRocketVelocity();
            //}
            if (blockRadar == null) return;
            if (!StatMaster.isClient)
            {
                if (blockRadar.target == null) return;
                // Calculating the rotating axis
                Vector3 velocity = Vector3.zero;
                try
                {
                    velocity = blockRadar.target.collider.attachedRigidbody.velocity - parentBlock.Rigidbody.velocity;
                    if (previousVelocity != Vector3.zero)
                    {
                        acceleration = (velocity - previousVelocity) / Time.deltaTime;
                    }
                    previousVelocity = velocity;
                }
                catch { }
                //Add position prediction
                float ratio = (blockRadar.target.collider.bounds.center - parentBlock.transform.position).magnitude / initialDistance;
                float actualPrediction = prediction * Mathf.Clamp(Mathf.Pow(ratio, 2), 0f, 1.5f);
                float pathPredictionTime = Time.fixedDeltaTime * actualPrediction;

                Vector3 forwardDirection = parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward;
                Vector3 positionDiffPredicted = blockRadar.target.collider.bounds.center + velocity * pathPredictionTime + 0.5f * acceleration * pathPredictionTime * pathPredictionTime - parentBlock.transform.position;
                float dotProduct = Vector3.Dot(forwardDirection, positionDiffPredicted.normalized);

                Vector3 towardsPositionDiff = dotProduct * positionDiffPredicted.normalized - forwardDirection;
                parentBlockRigidbody.AddForceAtPosition(Mathf.Clamp(torque, 0, 100) * maxTorque * towardsPositionDiff, transform.position + forwardDirection);
                parentBlockRigidbody.AddForceAtPosition(Mathf.Clamp(torque, 0, 100) * maxTorque * (-towardsPositionDiff), transform.position - forwardDirection);

                //Vector3 rotatingAxis = -Vector3.Cross(blockRadar.target.positionDiff.normalized, forwardDirection);
                //blockRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / searchAngle - 1f, 2) + 1)) * rotatingAxis);
            }
        }

        private void AddAerodynamicsToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(parentBlockRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * aeroEffectMultiplier;
            float velocitySqr = parentBlockRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);

            Vector3 force = transform.localToWorldMatrix * Vector3.Scale(dir, -locVel) * currentVelocitySqr;
            parentBlockRigidbody.AddForceAtPosition(force, parentBlock.transform.position - aeroEffectPosition);
        }
    }
}
