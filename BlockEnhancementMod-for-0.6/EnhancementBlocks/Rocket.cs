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
        MSlider GuideDelaySlider;
        MKey LockTargetKey;

        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        public bool guidedRocketIsActivated = false;
        public bool hasFired = false;
        public float fireTime = 0f;
        public bool fireTimeRecorded = false;
        public float torque = 100f;
        public float guideDelay = 0f;
        public float previousAngleDiff = 0;
        public float angleDiffCumulative = 0;
        public Transform target;
        public TimedRocket rocket;

        protected override void SafeAwake()
        {

            GuidedRocketToggle = AddToggle("追踪目标", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = GuideDelaySlider.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketIsActivated = GuidedRocketToggle.IsActive; };

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 10000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            GuideDelaySlider = AddSlider("延迟追踪", "guideDelay", guideDelay, 0, 100, false);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guideDelay = GuideDelaySlider.Value; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);
            //LockTargetKey.KeysChanged += ChangedProperties;

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
            GuideDelaySlider.DisplayInMapper = value && guidedRocketIsActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketIsActivated;
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (guidedRocketIsActivated && LockTargetKey.IsReleased)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                ConsoleController.ShowMessage("Ray casted");
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    target = hit.collider.transform;
                    //fireTime = Time.time;
                }
            }
        }

        protected override void OnSimulateLateUpdate()
        {
            try
            {
                if (guidedRocketIsActivated && rocket.hasFired && target != null)
                {
                    if (!fireTimeRecorded)
                    {
                        ConsoleController.ShowMessage("Fire time recorded");
                        fireTimeRecorded = true;
                        fireTime = Time.time;
                    }
                    if (Time.time - fireTime > guideDelay)
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
            }
            catch (Exception)
            {
                //Rocket will destroy itself upon explosion hence cause Null Reference Exception
                //ConsoleController.ShowMessage(e.ToString());
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


