using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {

        MToggle GuidedRocketToggle;
        MSlider GuidedRocketTorqueSlider;
        MKey LockTargetKey;

        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //public bool rocketPodIsActivated = false;
        public bool guidedRocketIsActivated = false;
        //public int noOfRocketsInPod = 18;
        public bool hasFired = false;
        public float torque = 100f;
        public float previousAngleDiff = 0;
        public float angleDiffCumulative = 0;
        public Transform target;
        public TimedRocket rocket;
        public bool exploding = false;
        //public int explosionType = 0;

        protected override void SafeStart()
        {

            GuidedRocketToggle = AddToggle("追踪目标", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketIsActivated = GuidedRocketToggle.IsActive; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 1000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketIsActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketIsActivated;
        }

        protected override void OnSimulateStart()
        {
            rocket = gameObject.GetComponent<TimedRocket>();
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (LockTargetKey.IsReleased)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.collider.transform;
                }
            }
        }
        protected override void LateUpdate()
        {
            if (StatMaster.levelSimulating)
            {
                if (guidedRocketIsActivated && rocket.hasFired)
                {
                    if (target != null)
                    {
                        // Calculating the rotating axis
                        Vector3 velocityNormarlised = GetComponent<Rigidbody>().velocity.normalized;
                        Vector3 positionDiff = target.position - transform.position;
                        float angleDiff = Vector3.Angle(positionDiff, velocityNormarlised);
                        Vector3 rotatingAxis = -Vector3.Cross(positionDiff, velocityNormarlised);
                        float angularSpeed = (angleDiff - previousAngleDiff) / Time.fixedDeltaTime;
                        // if the velocity is more than 90 degree apart from the target direction, use maximum torque
                        // otherwise use proportional torque.
                        if (angleDiff > 90)
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(torque * rotatingAxis);
                        }
                        else
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(torque * (angleDiff / 90f) * rotatingAxis);
                        }
                        //Trying to implement a PID controller
                        //BesiegeConsoleController.ShowMessage("PID working");
                        //transform.GetComponent<Rigidbody>().AddTorque(Mathf.Clamp(torque * (PIDControl(angleDiff)), 0, torque) * rotatingAxis);
                    }
                }
            }
        }

        float PIDControl(float angleDiff)
        {
            float controlOutput = 0;

            float p = 0.001f;
            float i = 0.00001f;
            float d = 0.0001f;

            //P
            controlOutput += p * angleDiff;

            //I
            angleDiffCumulative += angleDiff * Time.fixedDeltaTime;
            controlOutput += i * angleDiffCumulative;

            //D
            float angularSpeed = (angleDiff - previousAngleDiff) / Time.fixedDeltaTime;
            controlOutput += d * angularSpeed;

            previousAngleDiff = angleDiff;

            return controlOutput;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (rocket.hasFired && collision.impulse.magnitude > 1)
            {
                rocket.OnExplode();
            }
        }
        void OnCollisionStay(Collision collision)
        {
            if (rocket.hasFired && collision.impulse.magnitude > 1)
            {
                rocket.OnExplode();
            }
        }
    }
}


