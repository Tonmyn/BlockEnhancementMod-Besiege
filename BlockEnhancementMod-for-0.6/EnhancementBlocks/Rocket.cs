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
        //MToggle PID;
        MSlider GuidedRocketTorqueSlider;
        //MSlider PSlider;
        //MSlider ISlider;
        //MSlider DSlider;
        MKey LockTargetKey;

        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        public bool guidedRocketIsActivated = false;
        public bool hasFired = false;
        //public bool pid = false;
        public float torque = 100f;
        public float previousAngleDiff = 0;
        public float angleDiffCumulative = 0;
        public Transform target;
        public TimedRocket rocket;

        protected override void SafeAwake()
        {

            GuidedRocketToggle = AddToggle("追踪目标", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketIsActivated = GuidedRocketToggle.IsActive; };


            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);
            LockTargetKey.KeysChanged += ChangedProperties;

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 10000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();

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

        protected override void OnSimulateFixedUpdate()
        {
            if (guidedRocketIsActivated && LockTargetKey.IsReleased)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    target = hit.collider.transform;
                }
            }
        }

        protected override void LateUpdate()
        {
            if (StatMaster.levelSimulating)
            {
                try
                {
                    if (guidedRocketIsActivated && rocket.hasFired && target != null)
                    {
                        // Calculating the rotating axis
                        Vector3 velocityNormarlized = GetComponent<Rigidbody>().velocity.normalized;
                        Vector3 positionDiff = target.position - transform.position;
                        float angleDiff = Vector3.Angle(positionDiff.normalized, velocityNormarlized);
                        Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, velocityNormarlized);
                        float angularSpeed = (angleDiff - previousAngleDiff) / Time.fixedDeltaTime;
                        if (angleDiff > 90)
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(torque * rotatingAxis);
                        }
                        else
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(torque * (angleDiff / 90f) * rotatingAxis);
                        }
                    }
                }
                catch (Exception)
                {
                    //Rocket will destroy itself upon explosion hence cause Null Reference Exception
                }

            }
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


