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
        MToggle CloseRangeExplosionToggle;
        MSlider GuidedRocketTorqueSlider;
        MSlider GuideDelaySlider;
        MSlider CloseExploRangeSlider;
        MKey LockTargetKey;

        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };
        public int selfIndex;

        public bool guidedRocketIsActivated = false;
        public bool closeRangeExploActivated = false;
        public bool hasFired = false;
        public float fireTime = 0f;
        public bool fireTimeRecorded = false;
        public float torque = 100f;
        public float closeRange = 0f;
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
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = CloseRangeExplosionToggle.DisplayInMapper = LockTargetKey.DisplayInMapper = GuideDelaySlider.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketIsActivated = GuidedRocketToggle.IsActive; };

            CloseRangeExplosionToggle = AddToggle("近炸", "CloseeRangeExplo", closeRangeExploActivated);
            CloseRangeExplosionToggle.Toggled += (bool value) =>
            {
                closeRangeExploActivated = CloseExploRangeSlider.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { closeRangeExploActivated = CloseRangeExplosionToggle.IsActive; };

            CloseExploRangeSlider = AddSlider("近炸距离", "closeRange", closeRange, 0, 10, false);
            CloseExploRangeSlider.ValueChanged += (float value) => { closeRange = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { closeRange = CloseExploRangeSlider.Value; };

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 10000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            GuideDelaySlider = AddSlider("延迟追踪", "guideDelay", guideDelay, 0, 100, false);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guideDelay = GuideDelaySlider.Value; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);
            LockTargetKey.InvokeKeysChanged();
            //LockTargetKey.KeysChanged += ChangedProperties;

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            selfIndex = GetComponent<BlockBehaviour>().BuildIndex;

            //Make sure the target list is present
            if (!Machine.Active().gameObject.GetComponent<TargetScript>())
            {
                Machine.Active().gameObject.AddComponent<TargetScript>();
            }

#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketIsActivated;
            CloseRangeExplosionToggle.DisplayInMapper = value && guidedRocketIsActivated;
            CloseExploRangeSlider.DisplayInMapper = value && closeRangeExploActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketIsActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketIsActivated;
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
            // Trying to read previously saved target
            int targetIndex = -1;
            BlockBehaviour targetBlock = new BlockBehaviour();
            // Read the target's buildIndex from the dictionary
            try
            {
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.TryGetValue(selfIndex, out targetIndex);
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target index");
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

        protected override void OnSimulateFixedUpdate()
        {
            if (guidedRocketIsActivated && LockTargetKey.IsReleased)
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
                        if (closeRangeExploActivated && positionDiff.magnitude <= closeRange)
                        {
                            rocket.OnExplode();
                            return;
                        }
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

        private void SaveTargetToDict(int BlockID)
        {
            // Make sure the dupicated key exception is handled
            try
            {
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Add(selfIndex, BlockID);
            }
            catch (Exception)
            {
                // Remove the old record, then add the new record
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Remove(selfIndex);
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Add(selfIndex, BlockID);
            }
        }
    }
}


