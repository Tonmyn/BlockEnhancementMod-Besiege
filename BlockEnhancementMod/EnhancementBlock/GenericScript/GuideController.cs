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
        private Rigidbody parentRigidbody;
        private BlockBehaviour parentBlock;
        private RadarScript blockRadar;
        public float prediction = 10f;
        public float searchAngle = 0f;
        public float torque = 0f;
        public float maxTorque = 1500f;
        private Vector3 previousVelocity = Vector3.zero;
        private Vector3 previousPosition = Vector3.zero;
        private Vector3 acceleration = Vector3.zero;
        private Vector3 forwardDirection = Vector3.zero;
        private Target preTarget = null;

        public float pFactor, iFactor, dFactor;
        public float integral = 0;
        public float lastError = 0;

        //Aerodynamics setting
        private readonly float aeroEffectMultiplier = 5f;
        private Vector3 aeroEffectPosition = Vector3.zero;
        public bool enableAerodynamicEffect = false;

        public void SetupGuideController(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar, float sourceSearchAngle, float sourceTorque, float sourcePrediction)
        {
            parentBlock = sourceBlock;
            parentRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = false;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
            prediction = sourcePrediction;
            preTarget = new Target();
            pFactor = 10f;
            iFactor = 15f;
            dFactor = 1.25f;
        }

        void FixedUpdate()
        {
            if (parentBlock == null || parentRigidbody == null) return;

            if (enableAerodynamicEffect)
            {
                AddAerodynamicsToRocketVelocity();
            }

            //if (blockRadar == null) return;

            //if (!StatMaster.isClient)
            //{
            //    if (blockRadar.target == null) return;
            //    if (blockRadar.target != preTarget)
            //    {
            //        previousVelocity = Vector3.zero;
            //        previousPosition = Vector3.zero;
            //        preTarget = blockRadar.target;
            //        integral = 0;
            //        lastError = 0;
            //    }
            //    if (blockRadar.target.positionDiff.magnitude <= 3) return;

            //    // Calculating the rotating axis
            //    Vector3 velocity = (blockRadar.target.transform.position - previousPosition) / Time.deltaTime - parentBlock.Rigidbody.velocity;
            //    previousVelocity = velocity;
            //    previousPosition = blockRadar.target.transform.position;

            //    // Get the set point
            //    float pathPredictionTime = Time.fixedDeltaTime * prediction;
            //    Vector3 positionDiff = blockRadar.target.transform.position - parentBlock.transform.position;
            //    Vector3 positionDiffPredicted = positionDiff + velocity * pathPredictionTime;

            //    // Get the angle difference
            //    forwardDirection = parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward;

            //    float dotProduct = Vector3.Dot(forwardDirection, positionDiffPredicted.normalized);
            //    float angleDiff = Vector3.Angle(forwardDirection, positionDiff) + Vector3.Angle(positionDiff, positionDiffPredicted);
            //    integral += angleDiff * Time.fixedDeltaTime;
            //    float derivitive = (angleDiff - lastError) / Time.fixedDeltaTime;
            //    lastError = angleDiff;
            //    float coefficient = angleDiff * pFactor + integral * iFactor + derivitive * dFactor;

            //    Vector3 towardsPositionDiff = dotProduct * positionDiffPredicted.normalized - forwardDirection;
            //    parentRigidbody.AddForceAtPosition(torque * maxTorque * coefficient * towardsPositionDiff, parentBlock.transform.position + forwardDirection);
            //    parentRigidbody.AddForceAtPosition(torque * maxTorque * coefficient * (-towardsPositionDiff), parentBlock.transform.position - forwardDirection);
            //}
        }

        void LateUpdate()
        {
            if (parentBlock == null || parentRigidbody == null || blockRadar == null) return;

            if (!StatMaster.isClient)
            {
                if (blockRadar.target == null) return;
                if (blockRadar.target != preTarget)
                {
                    previousVelocity = Vector3.zero;
                    previousPosition = Vector3.zero;
                    preTarget = blockRadar.target;
                    integral = 0;
                    lastError = 0;
                }
                if (blockRadar.target.positionDiff.magnitude <= 3) return;

                // Calculating the rotating axis
                Vector3 velocity = (blockRadar.target.transform.position - previousPosition) / Time.deltaTime - parentBlock.Rigidbody.velocity;
                //velocity = blockRadar.target.block.Rigidbody.velocity - parentBlock.Rigidbody.velocity;
                //acceleration = (velocity - previousVelocity) / Time.deltaTime;
                previousVelocity = velocity;
                previousPosition = blockRadar.target.transform.position;

                //Add position prediction
                //float ratio = (blockRadar.target.collider.bounds.center - parentBlock.CenterOfBounds).magnitude / blockRadar.target.initialDistance;
                //float actualPrediction = prediction * Mathf.Clamp(Mathf.Pow(ratio, 2), 0f, 1.5f);
                //float pathPredictionTime = Time.fixedDeltaTime * actualPrediction;
                //Vector3 positionDiffPredicted = blockRadar.target.collider.bounds.center + velocity * pathPredictionTime + 0.5f * acceleration * pathPredictionTime * pathPredictionTime - parentBlock.CenterOfBounds;

                //forwardDirection = parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward;
                //float dotProduct = Vector3.Dot(forwardDirection, positionDiffPredicted.normalized);
                //Vector3 towardsPositionDiff = dotProduct * positionDiffPredicted.normalized - forwardDirection;
                //parentRigidbody.AddForceAtPosition(torque * maxTorque * ((-Mathf.Pow(blockRadar.target.angleDiff * 2 / searchAngle - 1f, 2) + 1))
                //    * towardsPositionDiff, parentBlock.CenterOfBounds + forwardDirection);
                //parentRigidbody.AddForceAtPosition(torque * maxTorque * ((-Mathf.Pow(blockRadar.target.angleDiff * 2 / searchAngle - 1f, 2) + 1))
                //    * (-towardsPositionDiff), parentBlock.CenterOfBounds - forwardDirection);

                // Get the set point
                //float ratio = (blockRadar.target.collider.bounds.center - parentBlock.CenterOfBounds).magnitude / blockRadar.target.initialDistance;
                //float actualPrediction = prediction * Mathf.Clamp(Mathf.Pow(ratio, 2), 0f, 1.5f);
                //float pathPredictionTime = Time.fixedDeltaTime * actualPrediction;
                float pathPredictionTime = Time.fixedDeltaTime * prediction;
                Vector3 positionDiff = blockRadar.target.transform.position - parentBlock.transform.position;
                Vector3 positionDiffPredicted = positionDiff + velocity * pathPredictionTime;
                //Vector3 positionDiffPredicted = positionDiff + velocity * pathPredictionTime + 0.5f * acceleration * pathPredictionTime * pathPredictionTime;

                // Get the angle difference
                forwardDirection = parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward;

                float dotProduct = Vector3.Dot(forwardDirection, positionDiffPredicted.normalized);
                float angleDiff = Vector3.Angle(forwardDirection, positionDiff) + Vector3.Angle(positionDiff, positionDiffPredicted);
                integral += angleDiff * Time.deltaTime;
                float derivitive = (angleDiff - lastError) / Time.deltaTime;
                lastError = angleDiff;
                float coefficient = angleDiff * pFactor + integral * iFactor + derivitive * dFactor;

                Vector3 towardsPositionDiff = dotProduct * positionDiffPredicted.normalized - forwardDirection;
                parentRigidbody.AddForceAtPosition(torque * maxTorque * coefficient * towardsPositionDiff, parentBlock.transform.position + forwardDirection);
                parentRigidbody.AddForceAtPosition(torque * maxTorque * coefficient * (-towardsPositionDiff), parentBlock.transform.position - forwardDirection);

                //    //Vector3 rotatingAxis = -Vector3.Cross(blockRadar.target.positionDiff.normalized, forwardDirection);
                //    //blockRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / searchAngle - 1f, 2) + 1)) * rotatingAxis);
            }
        }

        private void AddAerodynamicsToRocketVelocity()
        {
            forwardDirection = parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward;
            aeroEffectPosition = forwardDirection * 5f;

            Vector3 locVel = transform.InverseTransformDirection(parentRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * aeroEffectMultiplier;
            float velocitySqr = parentRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            Vector3 force = transform.localToWorldMatrix * Vector3.Scale(dir, -locVel) * currentVelocitySqr;

            parentRigidbody.AddForceAtPosition(force, parentBlock.CenterOfBounds - aeroEffectPosition);
            parentRigidbody.AddForceAtPosition(-0.1f * force, parentBlock.CenterOfBounds + aeroEffectPosition);
        }
    }
}
