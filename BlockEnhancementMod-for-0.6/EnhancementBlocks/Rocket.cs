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
        //public bool exploding = false;
        //float p = 0.1f;
        //float i = 0.0f;
        //float d = 0.0f;

        protected override void SafeAwake()
        {

            GuidedRocketToggle = AddToggle("追踪目标", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketIsActivated = GuidedRocketToggle.IsActive; };

            //PID = AddToggle("PID", "pid", pid);
            //PID.Toggled += (bool value) =>
            //{
            //    pid = PSlider.DisplayInMapper = ISlider.DisplayInMapper = DSlider.DisplayInMapper = value;
            //    ChangedProperties();
            //};
            //BlockDataLoadEvent += (XDataHolder BlockData) => { pid = PID.IsActive; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 10000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            //PSlider = AddSlider("Tau P", "taup", p, -100, 100, false);
            //PSlider.ValueChanged += (float value) => { p = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { p = PSlider.Value; };

            //ISlider = AddSlider("Tau I", "taui", i, -100, 100, false);
            //ISlider.ValueChanged += (float value) => { i = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { i = ISlider.Value; };

            //DSlider = AddSlider("Tau D", "taud", d, -100, 100, false);
            //DSlider.ValueChanged += (float value) => { d = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { d = DSlider.Value; };

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
            //PID.DisplayInMapper = value && guidedRocketIsActivated;
            //PSlider.DisplayInMapper = value && guidedRocketIsActivated && pid;
            //ISlider.DisplayInMapper = value && guidedRocketIsActivated && pid;
            //DSlider.DisplayInMapper = value && guidedRocketIsActivated && pid;
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
                        //if (pid)
                        //{
                        //    //Trying to implement a PID controller
                        //    transform.GetComponent<Rigidbody>().AddTorque(Mathf.Clamp(torque * (PIDControl(angleDiff / 90f)), 0, torque) * rotatingAxis);
                        //}
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
                    }
                }
                catch (Exception)
                {
                    //Rocket will destroy itself upon explosion hence cause Null Reference Exception
                }

            }
        }

        //float PIDControl(float angleDiff)
        //{
        //    float controlOutput = 0;

        //    //P
        //    controlOutput += p * angleDiff;

        //    //I
        //    angleDiffCumulative += angleDiff * Time.fixedDeltaTime;
        //    controlOutput += i * angleDiffCumulative;

        //    //D
        //    float angularSpeed = (angleDiff - previousAngleDiff) / Time.fixedDeltaTime;
        //    controlOutput += d * angularSpeed;

        //    previousAngleDiff = angleDiff;

        //    return controlOutput;
        //}

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


