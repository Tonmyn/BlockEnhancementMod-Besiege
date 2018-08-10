using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Common;
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
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //No smoke mode related
        MToggle NoSmokeToggle;
        public bool noSmoke = false;
        public bool smokeStopped = false;

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
        public HashSet<Transform> explodedTarget = new HashSet<Transform>();

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey ActiveGuideRocketKey;
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
                guidedRocketActivated =
                GuidedRocketTorqueSlider.DisplayInMapper =
                ProximityFuzeToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                ActiveGuideRocketKey.DisplayInMapper =
                ActiveGuideRocketSearchAngleSlider.DisplayInMapper =
                GuideDelaySlider.DisplayInMapper =
                GuidedRocketStabilitySlider.DisplayInMapper =
                NoSmokeToggle.DisplayInMapper =
                value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketActivated = GuidedRocketToggle.IsActive; };

            ProximityFuzeToggle = AddToggle("近炸", "ProximityFuze", proximityFuzeActivated);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                ProximityFuzeAngleSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { proximityFuzeActivated = ProximityFuzeToggle.IsActive; };

            NoSmokeToggle = AddToggle("无烟", "NoSmoke", noSmoke);
            NoSmokeToggle.Toggled += (bool value) =>
            {
                noSmoke = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { noSmoke = NoSmokeToggle.IsActive; };

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
            LockTargetKey.InvokeKeysChanged();

            ActiveGuideRocketKey = AddKey("主动/手动搜索切换", "ActiveSearchKey", activeGuideKeys);
            ActiveGuideRocketKey.InvokeKeysChanged();

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            rocketRigidbody = gameObject.GetComponent<Rigidbody>();

#if DEBUG
            //ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            HighExploToggle.DisplayInMapper = value;
            NoSmokeToggle.DisplayInMapper = value;
            ActiveGuideRocketKey.DisplayInMapper = value && guidedRocketActivated;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketStabilitySlider.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated && guidedRocketActivated;
        }

        protected override void OnSimulateStart()
        {
            smokeStopped = false;
            if (guidedRocketActivated)
            {
                // Initialisation for simulation
                searchRadius = Camera.main.farClipPlane;
                fireTimeRecorded = canTrigger = targetAquired = searchStarted = targetHit = bombHasExploded = false;
                activeGuide = true;
                target = null;
                explodedTarget.Clear();
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
                    if (!activeGuide)
                    {
                        target = null;
                    }
                    else
                    {
                        targetAquired = false;
                    }
                }

                if (LockTargetKey.IsReleased)
                {
                    target = null;
                    if (activeGuide)
                    {
                        //When launch key is released, reset target search
                        if (rocket.hasFired)
                        {
                            targetAquired = searchStarted = false;
                            RocketRadarSearch();
                        }
                    }
                    else
                    {
                        //Find targets in the manual search mode by casting a sphere along the ray
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        float manualSearchRadius = 1.25f;
                        RaycastHit[] hits = Physics.SphereCastAll(ray, manualSearchRadius, Mathf.Infinity);
                        Physics.Raycast(ray, out RaycastHit rayHit);
#if DEBUG
                        //ConsoleController.ShowMessage("No of hits in sphere cast all " + hits.Length);
#endif
                        for (int i = 0; i < hits.Length; i++)
                        {
                            try
                            {
                                int index = hits[i].transform.gameObject.GetComponent<BlockBehaviour>().ParentMachine.PlayerID;
                                target = hits[i].transform;
                                break;
                            }
                            catch { }
                            if (i == hits.Length - 1)
                            {
                                target = rayHit.transform;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (rocket.hasFired && !rocket.hasExploded)
            {
                //If no smoke mode is enabled, stop all smoke
                if (noSmoke && !smokeStopped)
                {
                    foreach (var smoke in rocket.trail)
                    {
                        smoke.Stop();
                    }
                    smokeStopped = true;
                }

                if (guidedRocketActivated)
                {
                    //Add aerodynamic force to rocket
                    AddResistancePerpendicularToRocketVelocity();

                    //Record the launch time for the guide delay
                    if (!fireTimeRecorded)
                    {
                        fireTimeRecorded = true;
                        fireTime = Time.time;
                    }

                    //If rocket is burning, explode it
                    if (highExploActivated && rocket.gameObject.GetComponent<FireTag>().burning)
                    {
                        RocketExplode();
                    }

                    //Rocket can be triggered after the time elapsed after firing is greater than guide delay
                    if (Time.time - fireTime >= guideDelay && !canTrigger)
                    {
                        canTrigger = true;
                    }

                    //Check if target is no longer valuable (lazy check)
                    if (target != null)
                    {
                        try
                        {
                            if (target.gameObject.GetComponent<FireTag>().burning)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<TimedRocket>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                    }
                    //If no target when active guide, search for a new target
                    if (activeGuide && !targetAquired)
                    {
                        RocketRadarSearch();
                    }
                }
            }
        }

        protected override void OnSimulateLateUpdate()
        {
            if (guidedRocketActivated && rocket.hasFired && !rocket.hasExploded)
            {
                if (target != null && canTrigger)
                {
                    // Calculating the rotating axis
                    Vector3 velocity = Vector3.zero;
                    try
                    {
                        velocity = target.gameObject.GetComponent<Rigidbody>().velocity;
                    }
                    catch { }
                    //Add position prediction
                    Vector3 positionDiff = target.position + velocity * Time.fixedDeltaTime * 10 - BB.CenterOfBounds;
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
                    //If proximity fuse is enabled, the rocket will explode when target is in preset range&angle
                    if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                    {
                        RocketExplode();
                    }
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (rocket.isSimulating && !rocket.hasExploded && collision.gameObject.name.Contains("CanonBall"))
                {
                    rocket.OnExplode();
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
            try
            {
                if (rocket.isSimulating && !rocket.hasExploded && collision.gameObject.name.Contains("CanonBall"))
                {
                    rocket.OnExplode();
                }
            }
            catch { };
            if (rocket.hasFired && collision.impulse.magnitude > 1 && canTrigger)
            {
                RocketExplode();
            }
        }

        private void RocketExplode()
        {
            //Reset some parameter and set the rocket to explode
            //Stop the search target coroutine
            searchStarted = targetHit = true;
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
            if (!searchStarted && activeGuide)
            {
                searchStarted = true;
                StopCoroutine(SearchForTarget());
                StartCoroutine(SearchForTarget());
            }
        }

        IEnumerator SearchForTarget()
        {
            //Grab every machine block at the start of search
            hitsOut = Physics.OverlapSphere(rocket.transform.position, searchRadius, Game.BlockEntityLayerMask);
            HashSet<Machine.SimCluster> simClusters = new HashSet<Machine.SimCluster>();
            if (StatMaster.isMP)
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
                    BlockBehaviour hitBlockBehaviour = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();
                    int clusterIndex = hitBlockBehaviour.ClusterIndex;
                    Machine machine = hitBlockBehaviour.ParentMachine;
                    if (machine.isSimulating)
                    {
                        if (StatMaster.isMP && !machine.LocalSim)
                        {
                            if ((rocket.ParentMachine.PlayerID != hitBlockBehaviour.ParentMachine.PlayerID && rocket.Team == MPTeam.None)
                                || (rocket.Team != MPTeam.None && rocket.Team != hitBlockBehaviour.Team))
                            {
                                simClusters.Add(machine.simClusters[clusterIndex]);
                            }
                        }
                        else
                        {
                            simClusters.Add(machine.simClusters[clusterIndex]);
                        }
                    }
                }
                catch { }
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired && !targetHit && simClusters.Count > 0)
            {
                HashSet<Machine.SimCluster> simClusterForSearch = new HashSet<Machine.SimCluster>(simClusters);
                HashSet<Machine.SimCluster> unwantedClusters = new HashSet<Machine.SimCluster>();

                foreach (var cluster in simClusters)
                {
                    Vector3 positionDiff = cluster.Base.gameObject.transform.position - rocket.CenterOfBounds;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);
                    bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                    bool skipCluster = !(forward && angleDiff < searchAngle) || ShouldSkipCluster(cluster.Base);

                    if (!skipCluster)
                    {
                        foreach (var block in cluster.Blocks)
                        {
                            if (skipCluster)
                            {
                                break;
                            }
                            skipCluster = ShouldSkipCluster(block);
                        }
                    }
                    if (skipCluster)
                    {
                        unwantedClusters.Add(cluster);
                    }
                }

                simClusterForSearch.ExceptWith(unwantedClusters);

                if (simClusterForSearch.Count > 0)
                {
                    target = GetMostValuableBlock(simClusterForSearch);
                    targetAquired = true;
                    searchStarted = false;
                    StopCoroutine(SearchForTarget());
                }
                yield return null;
            }
        }

        private Transform GetMostValuableBlock(HashSet<Machine.SimCluster> simClusterForSearch)
        {
            //Search for any blocks within the search radius for every block in the hitlist
            int[] targetValue = new int[simClusterForSearch.Count];
            Machine.SimCluster[] clusterArray = new Machine.SimCluster[simClusterForSearch.Count];
            List<Machine.SimCluster> maxClusters = new List<Machine.SimCluster>();

            //Start searching
            int i = 0;
            foreach (var simCluster in simClusterForSearch)
            {
                int clusterValue = simCluster.Blocks.Length + 1;
                clusterValue = CalculateClusterValue(simCluster.Base, clusterValue);
                foreach (var block in simCluster.Blocks)
                {
                    clusterValue = CalculateClusterValue(block, clusterValue);
                }
                targetValue[i] = clusterValue;
                clusterArray[i] = simCluster;
                i++;
            }
            //Find the block that has the max number of blocks around it
            //If there are multiple withh the same highest value, randomly return one of them
            int maxValue = targetValue.Max();
            for (i = 0; i < targetValue.Length; i++)
            {
                if (targetValue[i] == maxValue)
                {
                    maxClusters.Add(clusterArray[i]);
                }
            }

            int closestIndex = 0;
            float distanceMin = Mathf.Infinity;

            for (i = 0; i < maxClusters.Count; i++)
            {
                float distanceCurrent = (maxClusters[i].Base.gameObject.transform.position - rocket.transform.position).magnitude;
                if (distanceCurrent < distanceMin)
                {
                    closestIndex = i;
                    distanceMin = distanceCurrent;
                }
            }

            return maxClusters[closestIndex].Base.gameObject.transform;
        }

        private void AddResistancePerpendicularToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(rocketRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * Mathf.Clamp(guidedRocketStabilityLevel, 0, 10);
            float velocitySqr = rocketRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            rocketRigidbody.AddRelativeForce(Vector3.Scale(dir, -locVel) * currentVelocitySqr);
        }

        private int CalculateClusterValue(BlockBehaviour block, int clusterValue)
        {
            //Some blocks weights more than others
            GameObject targetObj = block.gameObject;
            //A bomb
            if (targetObj.GetComponent<ExplodeOnCollideBlock>())
            {
                if (!targetObj.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                {
                    clusterValue *= 64;
                }
            }
            //A fired and unexploded rocket
            if (targetObj.GetComponent<TimedRocket>())
            {
                if (targetObj.GetComponent<TimedRocket>().hasFired && !targetObj.GetComponent<TimedRocket>().hasExploded)
                {
                    clusterValue *= 128;
                }
            }
            //A watering watercannon
            if (targetObj.GetComponent<WaterCannonController>())
            {
                if (targetObj.GetComponent<WaterCannonController>().isActive)
                {
                    clusterValue *= 16;
                }
            }
            //A flying flying-block
            if (targetObj.GetComponent<FlyingController>())
            {
                if (targetObj.GetComponent<FlyingController>().canFly)
                {
                    clusterValue *= 2;
                }
            }
            //A flaming flamethrower
            if (targetObj.GetComponent<FlamethrowerController>())
            {
                if (targetObj.GetComponent<FlamethrowerController>().isFlaming)
                {
                    clusterValue *= 8;
                }
            }
            //A spinning wheel/cog
            if (targetObj.GetComponent<CogMotorControllerHinge>())
            {
                if (targetObj.GetComponent<CogMotorControllerHinge>().Velocity != 0)
                {
                    clusterValue *= 2;
                }
            }
            return clusterValue;
        }

        private bool ShouldSkipCluster(BlockBehaviour block)
        {
            bool skipCluster = false;
            try
            {
                if (block.gameObject.GetComponent<FireTag>().burning)
                {
                    skipCluster = true;
                }
            }
            catch { }
            try
            {
                if (block.gameObject.GetComponent<TimedRocket>().hasExploded)
                {
                    skipCluster = true;
                }
            }
            catch { }
            try
            {
                if (block.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                {
                    skipCluster = true;
                }
            }
            catch { }
            try
            {
                if (block.gameObject.GetComponent<ControllableBomb>().hasExploded)
                {
                    skipCluster = true;
                }
            }
            catch { }

            return skipCluster;
        }
    }
}