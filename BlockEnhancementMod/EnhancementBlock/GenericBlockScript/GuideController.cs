﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Modding;
using Modding.Common;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class GuideController : MonoBehaviour
    {

        //Guide Setting
        private Rigidbody parentRigidbody;
        private BlockBehaviour parentBlock;
        private RadarScript blockRadar;
        private float sourceSpeedPower;
        public float searchAngle = 0f;
        public float torque = 0f;
        public float maxTorque = 1500f;
        private Vector3 previousPosition = Vector3.zero;
        private Vector3 ForwardDirection { get { return parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward; } }

        private Transform preTargetTransform = null;
        public bool Switch { get; set; } = false;

        public static float pFactor = BlockEnhancementMod.ModSetting.GuideControl_P_Factor, iFactor = BlockEnhancementMod.ModSetting.GuideControl_I_Factor, dFactor = BlockEnhancementMod.ModSetting.GuideControl_D_Factor;
        public float integral = 0;
        public float lastError = 0;

        //Aerodynamics setting
        private readonly float aeroEffectMultiplier = 5f;
        private Vector3 aeroEffectPosition = Vector3.zero;
        public bool enableAerodynamicEffect = false;
        public bool constantForce = false;

        public void Setup(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar, float sourceSpeedPower,
            float sourceSearchAngle, float sourceTorque, bool constantForce)
        {
            parentBlock = sourceBlock;
            parentRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            this.sourceSpeedPower = sourceSpeedPower;
            enableAerodynamicEffect = false;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
            this.constantForce = constantForce;
            //preTargetTransform = new BlockBehaviour();
            //pFactor = BlockEnhancementMod.ModSetting.GuideControl_P_Factor;
            //iFactor = BlockEnhancementMod.ModSetting.GuideControl_I_Factor;
            //dFactor = BlockEnhancementMod.ModSetting.GuideControl_D_Factor;
        }

        void FixedUpdate()
        {
            if (StatMaster.isClient) return;
            if (parentBlock == null || parentRigidbody == null) return;

            //if (blockRadar == null) return;
            //if (blockRadar.target == null) return;
            if (!Switch) return;

            if (blockRadar != null)
            {
                if (blockRadar.RadarTarget != null && Switch)
                {
                    //if (blockRadar.target.transform != null)
                    //{
                    //    if (blockRadar.target.block != null)
                    //    {
                    //        if (blockRadar.target.block != parentBlock)
                    //        {
                    //            StartCoroutine(AddGuideForce());
                    //        }
                    //    }
                    //    else
                    //    {

                    //        StartCoroutine(AddGuideForce());
                    //    }
                    //}
                    if (blockRadar.RadarTarget.Enable)
                    {
                        StartCoroutine(AddGuideForce());
                    }
                    else
                    {
                        StopCoroutine(AddGuideForce());
                    }
                }
            }
            if (enableAerodynamicEffect) StartCoroutine(AddAerodynamicsToRocketVelocity());
        }

        private IEnumerator AddGuideForce()
        {
            //if (blockRadar.target.transform != preTargetTransform)
            //{
                previousPosition = Vector3.zero;
                //preTargetTransform = blockRadar.target.transform;
                integral = 0;
                lastError = 0;
            //}

            Vector3 addedForce;

            // Calculating the rotating axis
            Vector3 positionDiff = blockRadar.RadarTarget.Position - parentBlock.transform.position;
            //Vector3 targetVelocity = blockRadar.target.rigidbody == null ?
            //    (blockRadar.target.Position - previousPosition) / Time.fixedDeltaTime : blockRadar.target.Velocity;
            Vector3 targetVelocity = blockRadar.RadarTarget.Velocity;
            previousPosition = blockRadar.RadarTarget.Position;
            Vector3 relVelocity = targetVelocity - parentBlock.Rigidbody.velocity;

            //float speed;
            bool turretMode;
            if (blockRadar.RadarType == RadarScript.RadarTypes.ActiveRadar)
            {
                turretMode = blockRadar.ShowBulletLanding;
                //speed = turretMode ? blockRadar.cannonBallSpeed : parentRigidbody.velocity.magnitude;
            }
            else
            {
                turretMode = blockRadar.passiveSourceRadar == null ? false : blockRadar.passiveSourceRadar.ShowBulletLanding && sourceSpeedPower < 0.1f;
                //speed = turretMode ? blockRadar.passiveSourceRadar.cannonBallSpeed : parentRigidbody.velocity.magnitude;
            }

            // Get the predicted point
            float time;
            Vector3 positionDiffPredicted;
            if (turretMode)
            {
                Vector3 aimDir;
                if (blockRadar.RadarType == RadarScript.RadarTypes.ActiveRadar)
                {
                    aimDir = blockRadar.aimDir;
                }
                else
                {
                    aimDir = blockRadar.passiveSourceRadar == null ? Vector3.zero : blockRadar.passiveSourceRadar.aimDir;
                }
                positionDiffPredicted = parentBlock.transform.position + aimDir * parentBlock.transform.position.magnitude * 10000;
            }
            else
            {
                time = InterceptionCalculation.FirstOrderInterceptTime(parentRigidbody.velocity.magnitude, positionDiff, relVelocity);
                positionDiffPredicted = positionDiff + relVelocity * time;
            }
            positionDiffPredicted = positionDiffPredicted.normalized;

            // Get the angle difference
            float dotProduct = Vector3.Dot(ForwardDirection, positionDiffPredicted);
            Vector3 towardsPositionDiff = (dotProduct * positionDiffPredicted - ForwardDirection) * Mathf.Sign(dotProduct);
            towardsPositionDiff = towardsPositionDiff.normalized;

            if (constantForce)
            {
                addedForce = torque * maxTorque * towardsPositionDiff * 2000f;
            }
            else
            {
                float angleDiff = Vector3.Angle(ForwardDirection, positionDiff) + Vector3.Angle(positionDiff, positionDiffPredicted);
                integral += angleDiff * Time.fixedDeltaTime;
                float derivitive = (angleDiff - lastError) / Time.fixedDeltaTime;
                lastError = angleDiff;
                float coefficient = angleDiff * pFactor + integral * iFactor + derivitive * dFactor;
                addedForce = torque * maxTorque * coefficient * towardsPositionDiff;
            }

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
