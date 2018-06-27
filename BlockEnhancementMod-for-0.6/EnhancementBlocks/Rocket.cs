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
        MSlider ActiveGuideRocketSearchAngleSlider;
        public float searchAngle = 65;
        public float searchRadius = 10000f;
        public float safetyRadius = 5f;
        public float searchSurroundingBlockRadius = 10f;
        public bool activeGuideRocket = false;
        public bool targetAquired = false;
        public bool searchStarted = false;
        public bool restartSearch = false;
        public string previousMachine;
        private Collider[] hitsIn;
        private Collider[] hitsOut;
        private List<Collider> hitList;
        private GameObject targetObj;
        private BlockBehaviour blockBehaviour;


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
                activeGuideRocket = ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value;
                LockTargetKey.DisplayInMapper = RecordTargetToggle.DisplayInMapper = !value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { activeGuideRocket = ActiveGuideRocketToggle.IsActive; };

            ActiveGuideRocketSearchAngleSlider = AddSlider("搜索角度", "searchAngle", searchAngle, 0, 90, false);
            ActiveGuideRocketSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { searchAngle = ActiveGuideRocketSearchAngleSlider.Value; };

            ProximityFuzeRangeSlider = AddSlider("近炸距离", "closeRange", proximityRange, 0, 10, false);
            ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { proximityRange = ProximityFuzeRangeSlider.Value; };

            ProximityFuzeAngleSlider = AddSlider("近炸角度", "closeAngle", proximityAngle, 0, 90, false);
            ProximityFuzeAngleSlider.ValueChanged += (float value) => { proximityAngle = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { proximityAngle = ProximityFuzeAngleSlider.Value; };

            GuidedRocketTorqueSlider = AddSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 100, true);
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
            ActiveGuideRocketToggle.DisplayInMapper = value;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && activeGuideRocket;
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
            targetAquired = false;
            searchStarted = false;
            target = null;
            hitsIn = Physics.OverlapSphere(rocket.transform.position, safetyRadius);
            StopCoroutine(SearchForTarget());
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
                previousMachine = null;
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
                //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //if (Physics.Raycast(ray, out RaycastHit hit))
                //{
                //    target = hit.transform;
                //    if (recordTarget)
                //    {
                //        // Trying to save target's buildIndex to the dictionary
                //        // If not a machine block, set targetIndex to -1
                //        int targetIndex = -1;
                //        try
                //        {
                //            targetIndex = target.GetComponent<BlockBehaviour>().BuildIndex;
                //        }
                //        catch (Exception)
                //        {
                //            ConsoleController.ShowMessage("Not a machine block");
                //        }
                //        if (targetIndex != -1)
                //        {
                //            SaveTargetToDict(target.GetComponent<BlockBehaviour>().BuildIndex);
                //        }
                //    }
                //}

                //try sphercastall
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float manualSearchRadius = 5;
                RaycastHit[] hits = Physics.SphereCastAll(ray.origin, manualSearchRadius, ray.direction, Mathf.Infinity);
                Physics.Raycast(ray, out RaycastHit rayHit);
                for (int i = 0; i < hits.Length; i++)
                {
                    try
                    {
                        int index = hits[i].transform.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                        target = hits[i].transform;
                        if (recordTarget)
                        {
                            SaveTargetToDict(index);
                        }
                        break;
                    }
                    catch (Exception)
                    {
                    }
                    if (i == hits.Length - 1)
                    {
                        target = rayHit.transform;
                        try
                        {
                            int index = rayHit.transform.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                            if (recordTarget)
                            {
                                SaveTargetToDict(index);
                            }
                            break;
                        }
                        catch (Exception)
                        {
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
                        if (forward && angleDiff <= searchAngle)
                        {
                            transform.GetComponent<Rigidbody>().AddTorque(Mathf.Clamp(torque, 0, 100) * 1000 * ((Mathf.Exp(angleDiff / 90f) - 1) / e) * rotatingAxis);
                        }
                        else
                        {
                            if (!activeGuideRocket)
                            {
                                transform.GetComponent<Rigidbody>().AddTorque(Mathf.Clamp(torque, 0, 100) * 1000 * rotatingAxis);
                            }
                            else
                            {
                                targetAquired = false;
                            }
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
            searchStarted = false;
            StopCoroutine(SearchForTarget());
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
                }
                catch { }
                try
                {
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
            if (!searchStarted)
            {
                searchStarted = true;
                StartCoroutine(SearchForTarget());
            }
        }

        IEnumerator SearchForTarget()
        {
            while (!targetAquired)
            {
                hitsOut = Physics.OverlapSphere(rocket.transform.position, searchRadius);
                hitList = StatMaster._customLevelSimulating ? hitsOut.ToList<Collider>() : hitsOut.Except(hitsIn).ToList<Collider>();

                for (int i = hitList.Count - 1; i > -1; i--)
                {
                    Collider hit = hitList[i];
                    //Make sure the hit is a block
                    //if not, remove it from the list
                    try
                    {
                        int index = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                    }
                    catch (Exception)
                    {
                        hitList.Remove(hit);
                        continue;
                    }

                    targetObj = hit.attachedRigidbody.gameObject;
                    blockBehaviour = targetObj.GetComponent<BlockBehaviour>();

                    //Make sure the hit block is within the search range
                    //if not, remove it from the list
                    Vector3 positionDiff = targetObj.transform.position - transform.position;
                    bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);

                    if (!(forward && angleDiff < searchAngle))
                    {
                        hitList.Remove(hit);
                        continue;
                    }

                    //Make sure the block is not in my team
                    //if yes, remove it from the list
                    if (StatMaster._customLevelSimulating)
                    {
                        if (blockBehaviour.Team != MPTeam.None)
                        {
                            //If the block belongs to a team that is not none
                            //and is the same as the rocket, remove it from the list
                            if (blockBehaviour.Team == rocket.Team)
                            {
                                hitList.Remove(hit);
                                ConsoleController.ShowMessage("Not in my team, removed");
                                continue;
                            }
                        }
                        else
                        {
                            //If no team is assigned to a block
                            //only remove it when in multiverse
                            //and the parentmachine name is the same as the rocket's parent machine
                            if (blockBehaviour.ParentMachine.Name == rocket.ParentMachine.Name)
                            {
                                hitList.Remove(hit);
                                ConsoleController.ShowMessage("Not in my team, removed");
                                continue;
                            }
                        }
                    }
                }

                //Try to find the most valuable block
                //i.e. has the most number of blocks around it within a certain radius
                //when the hitlist is not empty
                if (hitList.Count > 0)
                {
                    //Search for any blocks within the search radius for every block in the hitlist
                    //Find the block that has the max number of blocks around it
                    int valueableBlockIndex = GetMostValuableBlock();

                    //Take that block as the target
                    Collider collider = hitList[valueableBlockIndex];
                    target = collider.attachedRigidbody.gameObject.GetComponent<Transform>();
                    previousMachine = collider.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().ParentMachine.Name;
                    targetAquired = true;
                    searchStarted = false;
                    StopCoroutine(SearchForTarget());
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private int GetMostValuableBlock()
        {
            //Search for any blocks within the search radius for every block in the hitlist
            int[] targetCount = new int[hitList.Count];
            for (int i = 0; i < hitList.Count; i++)
            {
                Collider[] hitsAroundBlock = Physics.OverlapSphere(hitList[i].transform.position, searchSurroundingBlockRadius);
                int count = 0;
                foreach (var hitBlock in hitsAroundBlock)
                {
                    try
                    {
                        int index = hitList[i].attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    count++;
                }
                targetCount[i] = count;
            }

            //Find the block that has the max number of blocks around it
            return targetCount.ToList().IndexOf(targetCount.Max());
        }
    }
}