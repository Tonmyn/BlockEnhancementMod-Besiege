using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        private Vector3 previousPosition = Vector3.zero;
        private Vector3 ForwardDirection { get { return parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward; } }

        private BlockBehaviour preTargetBlock = null;
        public bool Switch { get; set; } = false;

        public float pFactor, iFactor, dFactor;
        public float integral = 0;
        public float lastError = 0;

        //Aerodynamics setting
        private readonly float aeroEffectMultiplier = 5f;
        private Vector3 aeroEffectPosition = Vector3.zero;
        public bool enableAerodynamicEffect = false;

        public void Setup(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar, float sourceSearchAngle, float sourceTorque, float sourcePrediction)
        {
            parentBlock = sourceBlock;
            parentRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = false;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
            prediction = sourcePrediction;
            preTargetBlock = new BlockBehaviour();
            pFactor =/* 1.25f*/BlockEnhancementMod.Configuration.GetValue<float>("GuideControl P Factor");
            iFactor = /*10f*/BlockEnhancementMod.Configuration.GetValue<float>(" GuideControl I Factor");
            dFactor =/*5f*/BlockEnhancementMod.Configuration.GetValue<float>(" GuideControl D Factor");
        }

        void FixedUpdate()
        {
            if (StatMaster.isClient) return;
            if (parentBlock == null || parentRigidbody == null) return;
            if (enableAerodynamicEffect) StartCoroutine(AddAerodynamicsToRocketVelocity());

            if (blockRadar == null) return;
            if (blockRadar.target == null) return;
            if (Switch == false) return;
            StartCoroutine(AddGuideForce());
        }

        private IEnumerator AddGuideForce()
        {
            if (blockRadar.target.block != preTargetBlock)
            {
                previousPosition = Vector3.zero;
                preTargetBlock = blockRadar.target.block;
                integral = 0;
                lastError = 0;
            }

            // Calculating the rotating axis
            Vector3 velocity = (blockRadar.target.transform.position - previousPosition) / Time.fixedDeltaTime - parentBlock.Rigidbody.velocity;
            previousPosition = blockRadar.target.transform.position;

            // Get the predicted point
            float factor_Distance = Mathf.Clamp01(blockRadar.TargetDistance / blockRadar.target.initialDistance);
            float pathPredictionTime = Time.fixedDeltaTime * prediction * factor_Distance;
            Vector3 positionDiff = blockRadar.target.transform.position - parentBlock.transform.position;
            Vector3 positionDiffPredicted = positionDiff + velocity * pathPredictionTime;

            // Get the angle difference
            float dotProduct = Vector3.Dot(ForwardDirection, positionDiffPredicted.normalized);
            float angleDiff = Vector3.Angle(ForwardDirection, positionDiff) + Vector3.Angle(positionDiff, positionDiffPredicted);

            integral += angleDiff * Time.fixedDeltaTime;
            float derivitive = (angleDiff - lastError) / Time.fixedDeltaTime;
            lastError = angleDiff;
            float coefficient = angleDiff * pFactor + integral * iFactor + derivitive * dFactor;

            Vector3 towardsPositionDiff = (dotProduct * positionDiffPredicted.normalized - ForwardDirection) * Mathf.Sign(dotProduct);
            Vector3 addedForce = torque * maxTorque * coefficient * towardsPositionDiff;

            // Add force to rotate rocket
            parentRigidbody.AddForceAtPosition(addedForce, parentBlock.transform.position + ForwardDirection);
            parentRigidbody.AddForceAtPosition(-addedForce, parentBlock.transform.position - ForwardDirection);
            yield break;
        }

        private IEnumerator AddAerodynamicsToRocketVelocity()
        {
            aeroEffectPosition = ForwardDirection * 5f;

            Vector3 locVel = transform.InverseTransformDirection(parentRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * aeroEffectMultiplier;
            float velocitySqr = parentRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            Vector3 force = transform.localToWorldMatrix * Vector3.Scale(dir, -locVel) * currentVelocitySqr;

            parentRigidbody.AddForceAtPosition(force, parentBlock.CenterOfBounds - aeroEffectPosition);
            parentRigidbody.AddForceAtPosition(-0.1f * force, parentBlock.CenterOfBounds + aeroEffectPosition);

            yield break;
        }
    }
}
