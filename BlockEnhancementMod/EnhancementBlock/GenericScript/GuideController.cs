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
        public bool constantForce = false;

        public void Setup(BlockBehaviour sourceBlock, Rigidbody sourceRigidbody, RadarScript sourceRadar, float sourceSearchAngle, float sourceTorque, float sourcePrediction, bool constantForce)
        {
            parentBlock = sourceBlock;
            parentRigidbody = sourceRigidbody;
            blockRadar = sourceRadar;
            enableAerodynamicEffect = false;
            searchAngle = sourceSearchAngle;
            torque = sourceTorque;
            prediction = sourcePrediction;
            this.constantForce = constantForce;
            preTargetBlock = new BlockBehaviour();
            pFactor =/* 1.25f*/BlockEnhancementMod.Configuration.GetValue<float>("GuideControl P Factor");
            iFactor = /*10f*/BlockEnhancementMod.Configuration.GetValue<float>(" GuideControl I Factor");
            dFactor =/*5f*/BlockEnhancementMod.Configuration.GetValue<float>(" GuideControl D Factor");
        }

        void FixedUpdate()
        {
            if (StatMaster.isClient) return;
            if (parentBlock == null || parentRigidbody == null) return;

            //if (blockRadar == null) return;
            //if (blockRadar.target == null) return;
            if (Switch == false) return;

            if (blockRadar != null)
            {
                if (blockRadar.target != null && Switch != false)
                {
                    StartCoroutine(AddGuideForce());
                }
            }
            if (enableAerodynamicEffect) StartCoroutine(AddAerodynamicsToRocketVelocity());
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

            Vector3 addedForce;

            //RadarScript.SolveBallisticArc(parentBlock.transform.position, 1000f, blockRadar.target.transform.position, blockRadar.target.rigidbody.velocity, Physics.gravity.magnitude, out Vector3 dir, out float time);



            // Calculating the rotating axis
            Vector3 positionDiff = blockRadar.target.transform.position - parentBlock.transform.position;
            Vector3 velocity = (blockRadar.target.transform.position - previousPosition) / Time.fixedDeltaTime - parentBlock.Rigidbody.velocity;
            previousPosition = blockRadar.target.transform.position;

            float speed;
            bool turretMode;
            if (blockRadar.RadarType == RadarScript.RadarTypes.ActiveRadar)
            {
                turretMode = blockRadar.ShowBulletLanding;
                speed = turretMode ? blockRadar.cannonBallSpeed : parentRigidbody.velocity.magnitude;
            }
            else
            {
                if (blockRadar.passiveSourceRadar == null)
                {
                    turretMode = false;
                }
                else
                {
                    turretMode = blockRadar.passiveSourceRadar.ShowBulletLanding;
                }
                speed = turretMode ? blockRadar.passiveSourceRadar.cannonBallSpeed : parentRigidbody.velocity.magnitude;
            }

            float time;
            if (turretMode)
            {
                RadarScript.SolveBallisticArc(parentBlock.transform.position, speed, blockRadar.target.transform.position, blockRadar.target.rigidbody.velocity, Physics.gravity.magnitude, out Vector3 dir, out time);
            }
            else
            {
                time = FirstOrderInterceptTime(speed, positionDiff, velocity);
            }

            // Get the predicted point
            //float factor_Distance = Mathf.Clamp01(blockRadar.TargetDistance / blockRadar.target.initialDistance);
            //float pathPredictionTime = Time.fixedDeltaTime * prediction * factor_Distance;
            //Vector3 positionDiffPredicted = positionDiff + velocity * pathPredictionTime;

            Vector3 positionDiffPredicted = positionDiff + velocity * time;

            // Get the angle difference
            float dotProduct = Vector3.Dot(ForwardDirection, positionDiffPredicted.normalized);
            Vector3 towardsPositionDiff = (dotProduct * positionDiffPredicted.normalized - ForwardDirection) * Mathf.Sign(dotProduct);
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

        public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
        {
            float velocitySquared = targetRelativeVelocity.sqrMagnitude;
            if (velocitySquared < 0.001f)
                return 0f;

            float a = velocitySquared - shotSpeed * shotSpeed;

            //handle similar velocities
            if (Mathf.Abs(a) < 0.001f)
            {
                float t = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
                return Mathf.Max(t, 0f); //don't shoot back in time
            }

            float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
            float c = targetRelativePosition.sqrMagnitude;
            float determinant = b * b - 4f * a * c;

            if (determinant > 0f)
            { //determinant > 0; two intercept paths (most common)
                float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                        t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
                if (t1 > 0f)
                {
                    if (t2 > 0f)
                        return Mathf.Min(t1, t2); //both are positive
                    else
                        return t1; //only t1 is positive
                }
                else
                    return Mathf.Max(t2, 0f); //don't shoot back in time
            }
            else if (determinant < 0f) //determinant < 0; no intercept path
                return 0f;
            else //determinant = 0; one intercept path, pretty much never happens
                return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
        }
    }
}
