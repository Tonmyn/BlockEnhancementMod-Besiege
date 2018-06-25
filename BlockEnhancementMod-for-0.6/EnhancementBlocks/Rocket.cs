using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {
        //General setting
        MToggle GuidedRocketToggle;
        MKey LockTargetKey;
        public Transform target;
        public TimedRocket rocket;
        public int selfIndex;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //Firing record related setting
        public bool hasFired = false;
        public float fireTime = 0f;
        public bool fireTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        public bool guidedRocketActivated = false;
        public float torque = 100f;

        //proximity fuze related setting
        MToggle ProximityFuzeToggle;
        MSlider ProximityFuzeRangeSlider;
        MSlider ProximityFuzeAngleSlider;
        public bool proximityFuzeActivated = false;
        public float proximityRange = 0f;
        public float proximityAngle = 0f;

        //Guide delay related setting
        MSlider GuideDelaySlider;
        public float guideDelay = 0f;

        //High power explosion related setting
        MToggle HighExploToggle;
        public bool highExploActivated = false;
        public bool hasExploded = false;
        public float e = Mathf.Exp(1);
        public float explosiveCharge = 0f;
        public float radius = 7f;
        public float power = 3600f;
        public float torquePower = 100000f;
        public float upPower = 0.25f;

        protected override void SafeAwake()
        {
            GuidedRocketToggle = AddToggle("追踪目标", "TrackingRocket", guidedRocketActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketActivated = GuidedRocketTorqueSlider.DisplayInMapper = ProximityFuzeToggle.DisplayInMapper = LockTargetKey.DisplayInMapper = GuideDelaySlider.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketActivated = GuidedRocketToggle.IsActive; };

            ProximityFuzeToggle = AddToggle("近炸", "ProximityFuze", proximityFuzeActivated);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated = ProximityFuzeRangeSlider.DisplayInMapper = ProximityFuzeAngleSlider.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { proximityFuzeActivated = ProximityFuzeToggle.IsActive; };

            HighExploToggle = AddToggle("高爆", "HighExplo", highExploActivated);
            HighExploToggle.Toggled += (bool value) =>
            {
                highExploActivated = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { highExploActivated = HighExploToggle.IsActive; };

            ProximityFuzeRangeSlider = AddSlider("近炸距离", "closeRange", proximityRange, 0, 10, false);
            ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { proximityRange = ProximityFuzeRangeSlider.Value; };

            ProximityFuzeAngleSlider = AddSlider("近炸角度", "closeAngle", proximityAngle, 0, 90, false);
            ProximityFuzeAngleSlider.ValueChanged += (float value) => { proximityAngle = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { proximityAngle = ProximityFuzeAngleSlider.Value; };

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 10000, true);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            GuideDelaySlider = AddSlider("延迟追踪", "guideDelay", guideDelay, 0, 100, false);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guideDelay = GuideDelaySlider.Value; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);
            LockTargetKey.InvokeKeysChanged();

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            selfIndex = transform.GetComponent<BlockBehaviour>().BuildIndex;

#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            HighExploToggle.DisplayInMapper = value;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated;
        }

        public override void LoadConfiguration(XDataHolder BlockData)
        {
            if (BlockData.HasKey("bmt-" + "RocketTarget"))
            {
                SaveTargetToDict(BlockData.ReadInt("bmt-" + "RocketTarget"));
            }
        }

        public override void SaveConfiguration(XDataHolder BlockData)
        {
            if (Machine.Active().GetComponent<TargetScript>().previousTargetDic.ContainsKey(selfIndex))
            {
                BlockData.Write("bmt-" + "RocketTarget", Machine.Active().GetComponent<TargetScript>().previousTargetDic[selfIndex]);
            }
        }

        protected override void OnSimulateStart()
        {
            // Set high explo to false
            hasExploded = false;
            foreach (var slider in BB.Sliders)
            {
                if (slider.Key == "charge")
                {
                    explosiveCharge = slider.Value;

                    // Make sure the high explo mode is not too imba
                    if (highExploActivated)
                    {
                        explosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
                    }
                }
            }
            // Trying to read previously saved target
            int targetIndex = -1;
            BlockBehaviour targetBlock = new BlockBehaviour();
            // Read the target's buildIndex from the dictionary
            if (!Machine.Active().GetComponent<TargetScript>().previousTargetDic.TryGetValue(selfIndex, out targetIndex))
            {
                target = null;
                return;
            }
            // Aquire target block's transform from the target's index
            try
            {
                Machine.Active().GetBlockFromIndex(targetIndex, out targetBlock);
                target = Machine.Active().GetSimBlock(targetBlock).transform;
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target block's transform");
            }
        }

        protected override void OnSimulateUpdate()
        {
            if (guidedRocketActivated && LockTargetKey.IsReleased)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                ConsoleController.ShowMessage("Ray casted");
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    target = hit.transform;

                    // Trying to save target's buildIndex to the dictionary
                    // If not a machine block, set targetIndex to -1
                    int targetIndex = -1;
                    try
                    {
                        targetIndex = target.GetComponent<BlockBehaviour>().BuildIndex;
                    }
                    catch (Exception)
                    {
                        ConsoleController.ShowMessage("Not a machine block");
                    }
                    if (targetIndex != -1)
                    {
                        SaveTargetToDict(target.GetComponent<BlockBehaviour>().BuildIndex);
                    }
                }
            }
        }

        protected override void OnSimulateLateUpdate()
        {
            try
            {
                if (guidedRocketActivated && rocket.hasFired && target != null)
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
                        if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                        {
                            RocketExplode();
                            return;
                        }
                        Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, velocityNormarlized);
                        if (angleDiff > 90)
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(torque * rotatingAxis);
                        }
                        else
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(torque * ((Mathf.Exp(angleDiff / 90f) - 1) / e) * rotatingAxis);
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
                RocketExplode();
            }
        }
        void OnCollisionStay(Collision collision)
        {
            if (rocket.hasFired && collision.impulse.magnitude > 1)
            {
                RocketExplode();
            }
        }

        private void SaveTargetToDict(int BlockID)
        {
            // Make sure the dupicated key exception is handled
            try
            {
                // Add target to the dictionary
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Add(selfIndex, BlockID);
            }
            catch (Exception)
            {
                // Remove the old record, then add the new record
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Remove(selfIndex);
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Add(selfIndex, BlockID);
            }
        }

        private void RocketExplode()
        {
            if (highExploActivated && !hasExploded && explosiveCharge != 0)
            {
                hasExploded = true;
                try
                {
                    ExplodeOnCollide bomb = rocket.gameObject.AddComponent<ExplodeOnCollide>();
                    bomb.radius = radius * explosiveCharge;
                    bomb.power = power * explosiveCharge;
                    bomb.torquePower = torquePower * explosiveCharge;
                    bomb.upPower = upPower;
                    bomb.Explodey();
                }
                catch { }
            }
            rocket.OnExplode();
        }
    }
}