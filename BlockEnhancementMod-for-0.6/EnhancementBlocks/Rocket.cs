using System;
using System.Linq;
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

        //Active guide related setting
        MToggle ActiveGuideRocketToggle;
        public float searchRadius = 10000f;
        public bool activeGuideRocket = false;
        public bool targetAquired = false;
        public string previousMachine;


        //Record target related setting
        MToggle RecordTargetToggle;
        public bool recordTarget = false;

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
        public int levelBombCategory = 4;
        public int levelBombID = 5001;
        public float bombExplosiveCharge = 0;
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
                guidedRocketActivated = RecordTargetToggle.DisplayInMapper = GuidedRocketTorqueSlider.DisplayInMapper = ProximityFuzeToggle.DisplayInMapper = LockTargetKey.DisplayInMapper = ActiveGuideRocketToggle.DisplayInMapper = GuideDelaySlider.DisplayInMapper = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketActivated = GuidedRocketToggle.IsActive; };

            RecordTargetToggle = AddToggle("记录目标", "RecordTarget", recordTarget);
            RecordTargetToggle.Toggled += (bool value) =>
            {
                recordTarget = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { recordTarget = RecordTargetToggle.IsActive; };

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

            ActiveGuideRocketToggle = AddToggle("主动搜索", "ActiveSearch", activeGuideRocket);
            ActiveGuideRocketToggle.Toggled += (bool value) =>
            {
                activeGuideRocket = value;
                LockTargetKey.DisplayInMapper = RecordTargetToggle.DisplayInMapper = !value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { activeGuideRocket = ActiveGuideRocketToggle.IsActive; };

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
            RecordTargetToggle.DisplayInMapper = value && guidedRocketActivated && !activeGuideRocket;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated && !activeGuideRocket;
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
            // Initialisation for simulation
            fireTimeRecorded = false;
            target = null;

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
                        bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
                    }
                }
            }
            if (recordTarget && !activeGuideRocket)
            {
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
        }

        protected override void OnSimulateUpdate()
        {
            if (guidedRocketActivated && activeGuideRocket && !targetAquired && rocket.hasFired)
            {
                RocketRadarSearch();
            }
            if (guidedRocketActivated && !activeGuideRocket && LockTargetKey.IsReleased)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    target = hit.transform;
                    if (recordTarget)
                    {
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
                    if (Time.time - fireTime >= guideDelay)
                    {
                        // Calculating the rotating axis
                        Vector3 positionDiff = target.position - transform.position;
                        float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);
                        bool forward = Vector3.Dot(transform.up, positionDiff) > 0;
                        if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                        {
                            RocketExplode();
                            return;
                        }
                        Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, transform.up);
                        if (!forward)
                        {
                            if (!activeGuideRocket)
                            {
                                transform.GetComponent<Rigidbody>().AddTorque(torque * rotatingAxis);
                            }
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
            if (rocket.hasFired && collision.impulse.magnitude > 1 && (Time.time - fireTime >= guideDelay))
            {
                RocketExplode();
            }
        }
        void OnCollisionStay(Collision collision)
        {
            if (rocket.hasFired && collision.impulse.magnitude > 1 && (Time.time - fireTime >= guideDelay))
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
                    GameObject bomb = (GameObject)Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject, rocket.transform.position, rocket.transform.rotation);
                    ExplodeOnCollide bombControl = bomb.GetComponent<ExplodeOnCollide>();
                    bomb.transform.localScale = Vector3.one * bombExplosiveCharge;
                    bombControl.radius = radius * bombExplosiveCharge;
                    bombControl.power = power * bombExplosiveCharge;
                    bombControl.torquePower = torquePower * bombExplosiveCharge;
                    bombControl.upPower = upPower;
                    bombControl.Explodey();
                    Collider[] hits = Physics.OverlapSphere(rocket.transform.position, radius * bombExplosiveCharge);
                    foreach (var hit in hits)
                    {
                        if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject.layer != 22)
                        {
                            hit.attachedRigidbody.WakeUp();
                            hit.attachedRigidbody.constraints = RigidbodyConstraints.None;
                            hit.attachedRigidbody.AddExplosionForce(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                            hit.attachedRigidbody.AddRelativeTorque(UnityEngine.Random.insideUnitSphere.normalized * torquePower * bombExplosiveCharge);
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<FireTag>().Ignite();
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<ExplodeMultiplier>().Explodey(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<SimpleBirdAI>().Explode();
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<EnemyAISimple>().Die();
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<CastleWallBreak>().BreakExplosion(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<BreakOnForce>().BreakExplosion(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<InjuryController>().activeType = InjuryType.Fire;
                                hit.attachedRigidbody.gameObject.GetComponent<InjuryController>().Kill();
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
            rocket.OnExplode();
        }

        private void RocketRadarSearch()
        {
            Collider[] hitsOut = Physics.OverlapSphere(rocket.transform.position, searchRadius);
            Collider[] hitsIn = Physics.OverlapSphere(rocket.transform.position, 1);
            IEnumerable<Collider> hitList = hitsOut.Except(hitsIn);
            foreach (var hit in hitList)
            {
                try
                {
                    GameObject targetObj = hit.attachedRigidbody.gameObject;
                    BlockBehaviour blockBehaviour = targetObj.GetComponent<BlockBehaviour>();

                    if (StatMaster.SimulationState == SimulationState.GlobalSimulation)
                    {
                        if (!(blockBehaviour.ParentMachine.Name == previousMachine))
                        {
                            int clusterSize = 0;
                            int clusterID = 0;
                            for (int i = 0; i < blockBehaviour.ParentMachine.ClusterCount; i++)
                            {
                                int length = blockBehaviour.ParentMachine.simClusters[i].Blocks.Length;
                                if (length > clusterSize)
                                {
                                    clusterSize = length;
                                    clusterID = i;
                                }
                            }
                            BlockBehaviour targetBlock = blockBehaviour.ParentMachine.simClusters[clusterID].Blocks[Mathf.FloorToInt(clusterSize / 2)];
                            Transform targetTransform = targetBlock.GetComponent<Transform>();

                            Vector3 positionDiff = targetTransform.position - transform.position;
                            bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                            float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);

                            bool targetInSearchRange = angleDiff <= 65 && forward;
                            bool inMyTeam = targetBlock.Team.ToString().Equals(rocket.Team.ToString()) ||
                                (targetBlock.Team == MPTeam.None && targetBlock.ParentMachine.Name != rocket.ParentMachine.Name);

                            if (targetInSearchRange && !inMyTeam)
                            {
                                target = targetTransform;
                                previousMachine = targetBlock.ParentMachine.Name;
                                targetAquired = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        Rigidbody targetObjRigidbody = hit.attachedRigidbody;
                        Transform targetObjTransform = hit.attachedRigidbody.gameObject.transform;

                        Vector3 positionDiff = targetObjTransform.position - transform.position;
                        bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                        float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);

                        bool targetInSearchRange = angleDiff <= 65 && forward;
                        bool targetIsHighPriority = blockBehaviour.ParentMachine.simClusters[blockBehaviour.ClusterIndex].Blocks.Length >= 2;
                        bool inMyTeam = blockBehaviour.Team.ToString().Equals(rocket.Team.ToString());

                        if (targetInSearchRange && targetIsHighPriority && !inMyTeam)
                        {
                            ConsoleController.ShowMessage(blockBehaviour.Team.ToString());
                            target = targetObjTransform;
                            targetAquired = true;
                            return;
                        }
                    }
                }
                catch { }
            }
        }
    }
}