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
        public Rigidbody rocketRigidbody;
        public int selfIndex;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //Firing record related setting
        public bool hasFired = false;
        public bool targetHit = false;
        public float fireTime = 0f;
        public bool fireTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MSlider GuidedRocketStabilitySlider;
        public float guidedRocketStabilityLevel = 2f;
        public bool guidedRocketActivated = false;
        public float torque = 100f;

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey ActiveGuideRocketKey;
        MKey LaunchKey;
        public List<KeyCode> activeGuideKeys = new List<KeyCode> { KeyCode.RightShift };
        public float searchAngle = 65;
        public float searchRadius = 0;
        public float safetyRadius = 15f;
        public float searchSurroundingBlockRadius = 5f;
        public bool activeGuide = true;
        public bool targetAquired = false;
        public bool searchStarted = false;
        public bool restartSearch = false;
        private Collider[] hitsIn;
        private Collider[] hitsOut;
        private Collider[] hitList;


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
        public bool canTrigger = false;
        public float guideDelay = 0f;

        //High power explosion related setting
        MToggle HighExploToggle;
        public bool highExploActivated = false;
        public bool bombHasExploded = false;
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
            //Key mapper setup
            GuidedRocketToggle = AddToggle("追踪目标", "TrackingRocket", guidedRocketActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketActivated = RecordTargetToggle.DisplayInMapper = GuidedRocketTorqueSlider.DisplayInMapper = ProximityFuzeToggle.DisplayInMapper = LockTargetKey.DisplayInMapper = ActiveGuideRocketKey.DisplayInMapper = ActiveGuideRocketSearchAngleSlider.DisplayInMapper = GuideDelaySlider.DisplayInMapper = GuidedRocketStabilitySlider.DisplayInMapper = value;
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

            GuidedRocketStabilitySlider = AddSlider("火箭稳定性", "RocketStability", guidedRocketStabilityLevel, 0, 10, true);
            GuidedRocketStabilitySlider.ValueChanged += (float value) => { guidedRocketStabilityLevel = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketStabilityLevel = GuidedRocketStabilitySlider.Value; };

            GuideDelaySlider = AddSlider("延迟追踪", "guideDelay", guideDelay, 0, 100, false);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guideDelay = GuideDelaySlider.Value; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);

            ActiveGuideRocketKey = AddKey("主动/手动搜索切换", "ActiveSearchKey", activeGuideKeys);

            //Add reference to TimedRocket
            if (StatMaster.isClient)
            {
                //Machine machine = gameObject.GetComponent<BlockBehaviour>().ParentMachine;
                //ServerMachine serverMachine = machine.GetComponent<ServerMachine>();
                ////futre proof with new modloader
                //serverMachine.GetBlockFromIndex(transform.GetComponent<BlockBehaviour>().BuildIndex, out BB);
                //rocket = BB.gameObject.GetComponent<TimedRocket>();
                //rocketRigidbody = BB.Rigidbody;
                //selfIndex = BB.BuildIndex;
            }
            else
            {
                rocket = gameObject.GetComponent<TimedRocket>();
                rocketRigidbody = gameObject.GetComponent<Rigidbody>();
                selfIndex = BB.BuildIndex;
            }



#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            HighExploToggle.DisplayInMapper = value;
            ActiveGuideRocketKey.DisplayInMapper = value && guidedRocketActivated;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && guidedRocketActivated;
            RecordTargetToggle.DisplayInMapper = value && guidedRocketActivated && guidedRocketActivated;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketStabilitySlider.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated && guidedRocketActivated;
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
            if (guidedRocketActivated)
            {
                // Initialisation for simulation
                searchRadius = Camera.main.farClipPlane;
                fireTimeRecorded = canTrigger = targetAquired = searchStarted = targetHit = bombHasExploded = false;
                activeGuide = true;
                target = null;
                hitsIn = Physics.OverlapSphere(rocket.transform.position, safetyRadius);
                StopAllCoroutines();

                // Read the charge from rocket
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
                // Get the launch key from rocket
                foreach (var key in BB.Keys)
                {
                    if (key.Key == "launch")
                    {
                        LaunchKey = key;
                    }
                }
                if (recordTarget && !activeGuide)
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
        }

        protected override void OnSimulateUpdate()
        {
            if (guidedRocketActivated)
            {
                //When toggle auto aim key is released, change the auto aim status
                if (ActiveGuideRocketKey.IsReleased)
                {
                    activeGuide = !activeGuide;
                }
                //When launch key is released, reset target search
                if (activeGuide && rocket.hasFired)
                {
                    if (LaunchKey.IsReleased)
                    {
                        targetAquired = false;
                    }
                    if (!targetAquired)
                    {
                        RocketRadarSearch();
                    }
                }
                if (!activeGuide && LockTargetKey.IsReleased)
                {
                    //Find targets in the manual search mode by casting a sphere along the ray
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float manualSearchRadius = 2f;
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
                        catch { }
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
                            catch { }
                        }
                    }
                }
            }
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (guidedRocketActivated && rocket.hasFired)
            {
                try
                {
                    AddResistancePerpendicularToRocketVelocity();
                    if (target != null)
                    {
                        //Record the launch time for the guide delay
                        if (!fireTimeRecorded)
                        {
                            fireTimeRecorded = true;
                            fireTime = Time.time;
                        }
                        if (highExploActivated && rocket.gameObject.GetComponent<FireTag>().burning)
                        {
                            RocketExplode();
                        }
                        if (Time.time - fireTime >= guideDelay)
                        {
                            if (!canTrigger)
                            {
                                canTrigger = true;
                            }
                            try
                            {
                                if (target.gameObject.GetComponent<TimedRocket>().hasExploded)
                                {
                                    //ConsoleController.ShowMessage("Target rocket exploded");
                                    target = null;
                                    targetAquired = false;
                                    return;
                                }
                            }
                            catch { }
                            try
                            {
                                if (target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                                {
                                    //ConsoleController.ShowMessage("Target bomb exploded");
                                    target = null;
                                    targetAquired = false;
                                    return;
                                }
                            }
                            catch { }
                            try
                            {
                                if (target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                                {
                                    //ConsoleController.ShowMessage("Target level bomb exploded");
                                    target = null;
                                    targetAquired = false;
                                    return;
                                }
                            }
                            catch { }
                            try
                            {
                                if (target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                                {
                                    //ConsoleController.ShowMessage("Target grenade exploded");
                                    target = null;
                                    targetAquired = false;
                                    return;
                                }
                            }
                            catch { }
                            try
                            {
                                // Calculating the rotating axis
                                Vector3 velocity = Vector3.zero;
                                try
                                {
                                    velocity = target.gameObject.GetComponent<Rigidbody>().velocity;
                                }
                                catch { }
                                //Add position prediction
                                Vector3 positionDiff = target.gameObject.GetComponent<BlockBehaviour>().CenterOfBounds + velocity * Time.fixedDeltaTime * 10 - transform.gameObject.GetComponent<BlockBehaviour>().CenterOfBounds;
                                float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);
                                bool forward = Vector3.Dot(transform.up, positionDiff) > 0;
                                Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, transform.up);

                                //Add torque to the rocket based on the angle difference
                                //If in auto guide mode, the rocket will restart searching when target is out of sight
                                //else, apply maximum torque to the rocket
                                if (forward && angleDiff <= searchAngle)
                                {
                                    try { rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * 10000 * ((-Mathf.Pow(angleDiff / 90f - 1f, 2) + 1)) * rotatingAxis); }
                                    catch { }
                                }
                                else
                                {
                                    if (!activeGuide)
                                    {
                                        try { rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * 10000 * rotatingAxis); }
                                        catch { }
                                    }
                                    else
                                    {
                                        targetAquired = false;
                                    }
                                }
                                if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                                {
                                    RocketExplode();
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    //Rocket will destroy itself upon explosion hence cause Null Reference Exception
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            //Rocket will explode upon collision when time delay has elapsed
            try
            {
                if (collision.gameObject.name.Contains("CanonBall"))
                {
                    RocketExplode();
                }
            }
            catch { }
            if (rocket.hasFired && collision.impulse.magnitude > 1 && canTrigger)
            {
                RocketExplode();
            }
        }
        void OnCollisionStay(Collision collision)
        {
            if (rocket.hasFired && collision.impulse.magnitude > 1 && canTrigger)
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
            //Reset some parameter and set the rocket to explode
            //Stop the search target coroutine
            searchStarted = false;
            targetHit = true;
            StopCoroutine(SearchForTarget());

            if (!highExploActivated)
            {
                if (!rocket.hasExploded) rocket.OnExplode();
            }
            else
            {
                if (!bombHasExploded && explosiveCharge != 0)
                {
                    bombHasExploded = true;
                    //Generate a bomb from level editor and let it explode
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

                    //Add explode and ignition effects to the affected objects
                    try
                    {
                        Collider[] hits = Physics.OverlapSphere(rocket.transform.position, radius * bombExplosiveCharge);
                        foreach (var hit in hits)
                        {
                            if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject.layer != 22)
                            {
                                try
                                {
                                    if (hit.attachedRigidbody.gameObject.GetComponent<RocketScript>()) continue;
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.WakeUp();
                                    hit.attachedRigidbody.constraints = RigidbodyConstraints.None;
                                    hit.attachedRigidbody.AddExplosionForce(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                    hit.attachedRigidbody.AddRelativeTorque(UnityEngine.Random.insideUnitSphere.normalized * torquePower * bombExplosiveCharge);
                                }
                                catch { }

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
                    if (!rocket.hasExploded)
                    {
                        rocket.OnExplode();
                    }
                }

            }
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
            //Grab every machine block at the start of search
            hitsOut = Physics.OverlapSphere(rocket.transform.position, searchRadius);
            HashSet<Transform> transformSet = new HashSet<Transform>();

            if (StatMaster._customLevelSimulating)
            {
                hitList = hitsOut;
            }
            else
            {
                //hitsIn = hitsIn.Where(hit => hit != null).ToArray();
                hitList = hitsOut.Except(hitsIn).ToArray();
            }
            foreach (var hit in hitList)
            {
                try
                {
                    int index = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                    transformSet.Add(hit.attachedRigidbody.gameObject.transform);
                }
                catch { }
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired && !targetHit)
            {
                HashSet<Transform> unwantedTransforms = new HashSet<Transform>();
                foreach (var targetTransform in transformSet)
                {
                    Vector3 positionDiff = targetTransform.gameObject.GetComponent<BlockBehaviour>().CenterOfBounds - transform.gameObject.GetComponent<BlockBehaviour>().CenterOfBounds;
                    bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);

                    if (!(forward && angleDiff < searchAngle))
                    {
                        unwantedTransforms.Add(targetTransform);
                        continue;
                    }

                    BlockBehaviour targetBB = targetTransform.gameObject.GetComponent<BlockBehaviour>();
                    if (StatMaster._customLevelSimulating)
                    {
                        if (targetBB.Team != MPTeam.None)
                        {
                            //If the block belongs to a team that is not none
                            //and is the same as the rocket, remove it from the hashset
                            if (targetBB.Team == rocket.Team)
                            {
                                unwantedTransforms.Add(targetTransform);
                                continue;
                            }
                        }
                        else
                        {
                            //If no team is assigned to a block
                            //only remove it when in multiverse
                            //and the parentmachine name is the same as the rocket's parent machine
                            if (targetBB.ParentMachine.Name == rocket.ParentMachine.Name)
                            {
                                unwantedTransforms.Add(targetTransform);
                                continue;
                            }
                        }
                    }
                }
                transformSet.ExceptWith(unwantedTransforms);

                //Try to find the most valuable block
                //i.e. has the most number of blocks around it within a certain radius
                //when the hitlist is not empty
                if (transformSet.Count > 0)
                {
                    //Search for any blocks within the search radius for every block in the hitlist
                    //Find the block that has the max number of colliders around it
                    //Take that block as the target
                    target = GetMostValuableBlock(transformSet);
                    targetAquired = true;
                    searchStarted = false;
                    StopCoroutine(SearchForTarget());
                }
                yield return null;
            }
        }

        private Transform GetMostValuableBlock(HashSet<Transform> transformSet)
        {
            //Search for any blocks within the search radius for every block in the hitlist
            float[] targetValue = new float[transformSet.Count];
            Transform[] transformArray = new Transform[transformSet.Count];
            HashSet<Transform> tempTransform = new HashSet<Transform>();
            List<Transform> maxTransform = new List<Transform>();

            //Start searching
            int i = 0;
            foreach (var targetTransform in transformSet)
            {
                //Count how many colliders are around this particular collider
                Collider[] hitsAroundBlock = Physics.OverlapSphere(targetTransform.position, searchSurroundingBlockRadius);
                tempTransform.Clear();
                int count = 0;
                foreach (var hitBlock in hitsAroundBlock)
                {
                    try
                    {
                        int index = hitBlock.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                        if (tempTransform.Add(hitBlock.attachedRigidbody.gameObject.transform))
                        {
                            count++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                targetValue[i] = count;
                transformArray[i] = targetTransform;

                //Some blocks weights more than others
                GameObject targetObj = targetTransform.gameObject;
                //A bomb
                if (targetObj.GetComponent<ExplodeOnCollideBlock>())
                {
                    targetValue[i] = targetValue[i] * 70;
                }
                //A fired rocket
                if (targetObj.GetComponent<TimedRocket>())
                {
                    if (targetObj.GetComponent<TimedRocket>().hasFired)
                    {
                        targetValue[i] = targetValue[i] * 150;
                    }

                }
                //A fired watercannon
                if (targetObj.GetComponent<WaterCannonController>())
                {
                    if (targetObj.GetComponent<WaterCannonController>().isActive)
                    {
                        targetValue[i] = targetValue[i] * 50;
                    }
                }
                //A flying flying-block
                if (targetObj.GetComponent<FlyingController>())
                {
                    if (targetObj.GetComponent<FlyingController>().canFly)
                    {
                        targetValue[i] = targetValue[i] * 20;
                    }
                }
                i++;
            }
            //Find the block that has the max number of blocks around it
            //If there are multiple withh the same highest value, randomly return one of them
            float max = targetValue[0];
            for (i = 1; i < targetValue.Length; i++)
            {
                if (targetValue[i] > max)
                {
                    max = targetValue[i];
                }
            }
            for (i = 0; i < targetValue.Length; i++)
            {
                if (targetValue[i] == max)
                {
                    maxTransform.Add(transformArray[i]);
                }
            }
            int closestIndex = 0;
            float distance = (maxTransform[0].position - rocket.transform.position).magnitude;
            for (i = 1; i < maxTransform.Count; i++)
            {
                if ((maxTransform[i].gameObject.GetComponent<BlockBehaviour>().CenterOfBounds + maxTransform[i].gameObject.GetComponent<Rigidbody>().velocity * Time.fixedDeltaTime * 10 - rocket.transform.gameObject.GetComponent<BlockBehaviour>().CenterOfBounds).magnitude < distance)
                {
                    closestIndex = i;
                }
            }
            return maxTransform[closestIndex];
        }

        private void AddResistancePerpendicularToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(rocketRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * Mathf.Clamp(guidedRocketStabilityLevel, 0, 10);
            float velocitySqr = rocketRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            rocketRigidbody.AddRelativeForce(Vector3.Scale(dir, -locVel) * currentVelocitySqr);
        }
    }
}