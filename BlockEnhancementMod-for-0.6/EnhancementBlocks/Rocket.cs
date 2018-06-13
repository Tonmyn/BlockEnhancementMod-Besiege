using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {

        //MMenu ExplosionTypeMenu;
        //MToggle RocketPodToggle;
        MToggle GuidedRocketToggle;
        MSlider GuidedRocketTorqueSlider;
        MKey LockTargetKey;

        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //List<string> explosionTypes;

        public bool rocketPodIsActivated = false;
        public bool guidedRocketIsActivated = false;
        public int noOfRocketsInPod = 18;
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
            //explosionTypes = new List<string> { "Rocket", "Bomb", "Grenade" };
            //ExplosionTypeMenu = new MMenu("ExplosionType", (int)RocketScript.ExplosionType.rocket, explosionTypes, false);
            //CurrentMapperTypes.Add(ExplosionTypeMenu);
            //ExplosionTypeMenu.ValueChanged += (int value) =>
            //{
            //    ExplosionTypeMenu.Value = explosionType = value;
            //    ChangedPropertise();
            //    BesiegeConsoleController.ShowMessage(value.ToString());
            //};

            //RocketPodToggle = new MToggle("火箭巢", "RocketPod", rocketPodIsActivated);
            //RocketPodToggle.Toggled += (bool value) => { rocketPodIsActivated = value; ChangedPropertise(); };
            //CurrentMapperTypes.Add(RocketPodToggle);

            GuidedRocketToggle = new MToggle("追踪目标", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = value;
                ChangedProperties();
            };
            CurrentMapperTypes.Add(GuidedRocketToggle);

            LockTargetKey = new MKey("锁定目标", "lockTarget", lockKeys[0]);
            LockTargetKey.KeysChanged += ChangedProperties;
            CurrentMapperTypes.Add(LockTargetKey);

            GuidedRocketTorqueSlider = new MSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 1000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            CurrentMapperTypes.Add(GuidedRocketTorqueSlider);

#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void ChangedProperties()
        {
            lockKeys.Clear();
            for (int i = 0; i < LockTargetKey.KeysCount; i++)
            {
                lockKeys.Add(LockTargetKey.GetKey(i));
                //ConsoleController.ShowMessage(LockTargetKey.GetKey(i).ToString());
            }
        }

        public override void DisplayInMapper(bool value)
        {
            //base.DisplayInMapper(value);
            //RocketPodToggle.DisplayInMapper = value;
            //ExplosionTypeMenu.DisplayInMapper = value;
            GuidedRocketToggle.DisplayInMapper = value;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketIsActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketIsActivated;
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + GuidedRocketToggle.Key))
                    {
                        GuidedRocketToggle.IsActive = guidedRocketIsActivated = bd.ReadBool("bmt-" + GuidedRocketToggle.Key);
                    }
                    if (bd.HasKey("bmt-" + GuidedRocketTorqueSlider.Key))
                    {
                        GuidedRocketTorqueSlider.Value = torque = bd.ReadFloat("bmt-" + GuidedRocketTorqueSlider.Key);
                    }
                    if (bd.HasKey("bmt-" + LockTargetKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + LockTargetKey.Key))
                        {
                            LockTargetKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }
                    break;
                }

            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    blockinfo.BlockData.Write("bmt-" + GuidedRocketToggle.Key, GuidedRocketToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + GuidedRocketTorqueSlider.Key, GuidedRocketTorqueSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + LockTargetKey.Key, LockTargetKey.Serialize().RawValue);
                    break;
                }

            }
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


