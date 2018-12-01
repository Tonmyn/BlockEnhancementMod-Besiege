using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Common;
using Modding.Levels;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {
        //General setting
        MToggle GuidedRocketToggle;
        MKey LockTargetKey;
        private Texture2D rocketAim;
        public Transform target;
        public TimedRocket rocket;
        public Rigidbody rocketRigidbody;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //Networking setting
        public bool receivedRayFromClient = false;
        public Ray rayFromClient;

        //No smoke mode related
        MToggle NoSmokeToggle;
        private bool noSmoke = false;
        private bool smokeStopped = false;

        //Firing record related setting
        private bool targetHit = false;
        private float randomDelay = 0;
        private float fireTime = 0f;
        private bool fireTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MToggle GuidedRocketStabilityToggle;
        MSlider GuidePredictionSlider;
        public bool guidedRocketStabilityOn = true;
        public bool guidedRocketActivated = false;
        public float torque = 100f;
        public float prediction = 10f;
        public float initialDistance = 0f;
        private readonly float maxTorque = 10000;
        public Vector3 previousVelocity;
        public Vector3 acceleration;
        public Collider targetCollider;
        private bool targetInitialCJOrHJ = false;
        private HashSet<Machine.SimCluster> clustersInSafetyRange = new HashSet<Machine.SimCluster>();
        private HashSet<Machine.SimCluster> explodedCluster = new HashSet<Machine.SimCluster>();

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey SwitchGuideModeKey;
        public List<KeyCode> switchGuideModeKey = new List<KeyCode> { KeyCode.RightShift };
        public float searchAngle = 10;
        private readonly float safetyRadiusAuto = 50f;
        private readonly float safetyRadiusManual = 15f;
        private readonly float maxSearchAngle = 25f;
        private readonly float maxSearchAngleNo8 = 90f;
        public bool activeGuide = true;
        public bool targetAquired = false;
        public bool searchStarted = false;

        //Cluster value multiplier
        private readonly int bombValue = 64;
        private readonly int guidedRocketValue = 1024;
        private readonly int normalRocketValue = 512;
        private readonly int waterCannonValue = 16;
        private readonly int flyingBlockValue = 2;
        private readonly int flameThrowerValue = 8;
        private readonly int cogMotorValue = 2;

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
        private bool canTrigger = false;

        //High power explosion related setting
        MToggle HighExploToggle;
        public bool highExploActivated = false;
        private bool bombHasExploded = false;
        private readonly int levelBombCategory = 4;
        private readonly int levelBombID = 5001;
        private float bombExplosiveCharge = 0;
        private float explosiveCharge = 0f;
        private readonly float radius = 7f;
        private readonly float power = 3600f;
        private readonly float torquePower = 100000f;
        private readonly float upPower = 0.25f;

        //Aerodynamics setting
        private readonly float aeroEffectMultiplier = 0.75f;

        public override void SafeAwake()
        {
            //Load aim pic
            rocketAim = new Texture2D(256, 256);
            rocketAim.LoadImage(ModIO.ReadAllBytes("Resources" + @"/" + "Square-Red.png"));
            //Key mapper setup
            GuidedRocketToggle = BB.AddToggle(LanguageManager.trackTarget, "TrackingRocket", guidedRocketActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketActivated =
                GuidedRocketTorqueSlider.DisplayInMapper =
                GuidePredictionSlider.DisplayInMapper =
                ProximityFuzeToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                SwitchGuideModeKey.DisplayInMapper =
                ActiveGuideRocketSearchAngleSlider.DisplayInMapper =
                GuideDelaySlider.DisplayInMapper =
                GuidedRocketStabilityToggle.DisplayInMapper =
                NoSmokeToggle.DisplayInMapper =
                value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketActivated = GuidedRocketToggle.IsActive; };

            ProximityFuzeToggle = BB.AddToggle(LanguageManager.proximityFuze, "ProximityFuze", proximityFuzeActivated);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                ProximityFuzeAngleSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { proximityFuzeActivated = ProximityFuzeToggle.IsActive; };

            NoSmokeToggle = BB.AddToggle(LanguageManager.noSmoke, "NoSmoke", noSmoke);
            NoSmokeToggle.Toggled += (bool value) =>
            {
                noSmoke = value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { noSmoke = NoSmokeToggle.IsActive; };

            HighExploToggle = BB.AddToggle(LanguageManager.highExplo, "HighExplo", highExploActivated);
            HighExploToggle.Toggled += (bool value) =>
            {
                highExploActivated = value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { highExploActivated = HighExploToggle.IsActive; };

            ActiveGuideRocketSearchAngleSlider = BB.AddSlider(LanguageManager.searchAngle, "searchAngle", searchAngle, 0, maxSearchAngle);
            ActiveGuideRocketSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { searchAngle = ActiveGuideRocketSearchAngleSlider.Value; };
            ///
            GuidePredictionSlider = BB.AddSlider(LanguageManager.prediction, "prediction", prediction, 0, 50);
            GuidePredictionSlider.ValueChanged += (float value) => { prediction = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { searchAngle = ActiveGuideRocketSearchAngleSlider.Value; };

            ProximityFuzeRangeSlider = BB.AddSlider(LanguageManager.closeRange, "closeRange", proximityRange, 0, 10);
            ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { proximityRange = ProximityFuzeRangeSlider.Value; };

            ProximityFuzeAngleSlider = BB.AddSlider(LanguageManager.closeAngle, "closeAngle", proximityAngle, 0, 90);
            ProximityFuzeAngleSlider.ValueChanged += (float value) => { proximityAngle = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { proximityAngle = ProximityFuzeAngleSlider.Value; };

            GuidedRocketTorqueSlider = BB.AddSlider(LanguageManager.torqueOnRocket, "torqueOnRocket", torque, 0, 100);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            GuidedRocketStabilityToggle = BB.AddToggle(LanguageManager.rocketStability, "RocketStabilityOn", guidedRocketStabilityOn);
            GuidedRocketStabilityToggle.Toggled += (bool value) => { guidedRocketStabilityOn = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketStabilityOn = GuidedRocketStabilityToggle.IsActive; };

            GuideDelaySlider = BB.AddSlider(LanguageManager.guideDelay, "guideDelay", guideDelay, 0, 2);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { guideDelay = GuideDelaySlider.Value; };

            LockTargetKey = BB.AddKey(LanguageManager.lockTarget, "lockTarget", KeyCode.Delete);
            LockTargetKey.InvokeKeysChanged();

            SwitchGuideModeKey = BB.AddKey(LanguageManager.switchGuideMode, "ActiveSearchKey", KeyCode.RightShift);
            SwitchGuideModeKey.InvokeKeysChanged();

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
            SwitchGuideModeKey.DisplayInMapper = value && guidedRocketActivated;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidePredictionSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketStabilityToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated && guidedRocketActivated;
        }

        public override void OnSimulateStart()
        {
            smokeStopped = false;
            if (guidedRocketActivated)
            {
                // Initialisation for simulation
                fireTimeRecorded = canTrigger = targetAquired = searchStarted = targetHit = bombHasExploded = receivedRayFromClient = targetInitialCJOrHJ = false;
                activeGuide = true;
                target = null;
                targetCollider = null;
                explodedCluster.Clear();
                searchAngle = Mathf.Clamp(searchAngle, 0, No8Workshop ? maxSearchAngleNo8 : maxSearchAngle);
                previousVelocity = acceleration = Vector3.zero;
                if (!StatMaster.isMP)
                {
                    clustersInSafetyRange.Clear();
                    foreach (var cluster in Machine.Active().simClusters)
                    {
                        if ((cluster.Base.transform.position - rocket.transform.position).magnitude < safetyRadiusAuto)
                        {
                            clustersInSafetyRange.Add(cluster);
                        }
                    }
                }
                StopAllCoroutines();

                // Read the charge from rocket
                explosiveCharge = bombExplosiveCharge = rocket.ChargeSlider.Value;

                // Make sure the high explo mode is not too imba
                if (highExploActivated && !No8Workshop)
                {
                    bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
                }
            }
        }

        public override void SimulateUpdateAlways()
        {
            if (guidedRocketActivated)
            {
                //When toggle auto aim key is released, change the auto aim status
                if (SwitchGuideModeKey.IsReleased)
                {
                    activeGuide = !activeGuide;
                    if (!activeGuide)
                    {
                        target = null;
                        targetCollider = null;
                        previousVelocity = acceleration = Vector3.zero;
                        SendClientTargetNull();
                    }
                    else
                    {
                        targetAquired = false;
                    }
                }

                //if (StatMaster.isHosting && receivedRayFromClient)
                //{
                //    Debug.Log("Should not see this message in client");
                //    receivedRayFromClient = false;
                //    //Find targets in the manual search mode by casting a sphere along the ray
                //    float manualSearchRadius = 1.25f;
                //    RaycastHit[] hits = Physics.SphereCastAll(rayFromClient, manualSearchRadius, Mathf.Infinity);

                //    for (int i = 0; i < hits.Length; i++)
                //    {
                //        if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                //        {
                //            target = hits[i].transform;
                //            break;
                //        }
                //    }
                //    if (target == null)
                //    {
                //        for (int i = 0; i < hits.Length; i++)
                //        {
                //            if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
                //            {
                //                target = hits[i].transform;
                //                break;
                //            }
                //        }
                //    }
                //    SendTargetToClient();
                //}

                if (LockTargetKey.IsReleased)
                {
                    target = null;
                    targetCollider = null;
                    previousVelocity = acceleration = Vector3.zero;
                    SendClientTargetNull();
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
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (StatMaster.isClient)
                        {
                            SendRayToHost(ray);
                        }
                        else
                        {
                            //Find targets in the manual search mode by casting a sphere along the ray
                            float manualSearchRadius = 1.25f;
                            RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity);
                            Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit);

                            for (int i = 0; i < hits.Length; i++)
                            {
                                if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                                {
                                    if ((hits[i].transform.position - rocket.transform.position).magnitude >= safetyRadiusManual)
                                    {
                                        target = hits[i].transform;
                                        targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                                        targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
                                        previousVelocity = acceleration = Vector3.zero;
                                        initialDistance = (hits[i].transform.position - rocket.transform.position).magnitude;
                                        targetAquired = true;
                                        break;
                                    }
                                }
                            }
                            if (target == null)
                            {
                                for (int i = 0; i < hits.Length; i++)
                                {
                                    if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
                                    {
                                        if ((hits[i].transform.position - rocket.transform.position).magnitude >= safetyRadiusManual)
                                        {
                                            target = hits[i].transform;
                                            targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                                            targetInitialCJOrHJ = false;
                                            previousVelocity = acceleration = Vector3.zero;
                                            initialDistance = (hits[i].transform.position - rocket.transform.position).magnitude;
                                            targetAquired = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (target == null)
                            {
                                if ((rayHit.transform.position - rocket.transform.position).magnitude >= safetyRadiusManual)
                                {
                                    target = rayHit.transform;
                                    targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                                    targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
                                    previousVelocity = acceleration = Vector3.zero;
                                    initialDistance = (rayHit.transform.position - rocket.transform.position).magnitude;
                                    targetAquired = true;
                                }
                            }
                            if (receivedRayFromClient)
                            {
                                SendTargetToClient();
                            }
                            receivedRayFromClient = false;
                        }
                    }
                }
            }
        }

        public override void SimulateFixedUpdateAlways()
        {
            if (rocket.hasFired && !rocket.hasExploded)
            {
                //If no smoke mode is enabled, stop all smoke
                if (noSmoke && !smokeStopped)
                {
                    try
                    {
                        foreach (var smoke in rocket.trail)
                        {
                            smoke.Stop();
                        }
                        smokeStopped = true;
                    }
                    catch { }
                }

                if (guidedRocketActivated)
                {
                    //Record the launch time for the guide delay
                    if (!fireTimeRecorded)
                    {
                        fireTimeRecorded = true;
                        fireTime = Time.time;
                        randomDelay = UnityEngine.Random.Range(0f, 0.1f);
                    }

                    //Rocket can be triggered after the time elapsed after firing is greater than guide delay
                    if (Time.time - fireTime >= guideDelay + randomDelay && !canTrigger)
                    {
                        canTrigger = true;
                    }

                    //If rocket is burning, explode it
                    if (highExploActivated && rocket.fireTag.burning && canTrigger)
                    {
                        RocketExplode();
                    }

                    //Check if target is no longer valuable (lazy check)
                    if (target != null && !StatMaster.isClient)
                    {
                        try
                        {
                            if (targetCollider == null)
                            {
                                target = null;
                                targetCollider = null;
                                targetAquired = false;
                                SendClientTargetNull();
                            }
                            else
                            {
                                //If proximity fuse is enabled, the rocket will explode when target is in preset range&angle
                                Vector3 positionDiff = targetCollider.bounds.center - rocket.transform.position;
                                float angleDiff = Vector3.Angle(positionDiff, transform.up);
                                if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                                {
                                    RocketExplode();
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            if (targetInitialCJOrHJ)
                            {
                                if (target.gameObject.GetComponent<ConfigurableJoint>() == null && target.gameObject.GetComponent<HingeJoint>() == null)
                                {
                                    try
                                    {
                                        explodedCluster.Add(target.gameObject.GetComponent<BlockBehaviour>().ParentMachine.simClusters[target.gameObject.GetComponent<BlockBehaviour>().ClusterIndex]);
                                    }
                                    catch { }
                                    target = null;
                                    targetCollider = null;
                                    targetAquired = targetInitialCJOrHJ = false;
                                    SendClientTargetNull();
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<FireTag>().burning)
                            {
                                if (target.gameObject.GetComponent<TimedRocket>() == null)
                                {
                                    target = null;
                                    targetCollider = null;
                                    targetAquired = false;
                                    SendClientTargetNull();
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<BlockBehaviour>())
                            {
                                try
                                {
                                    if (target.gameObject.GetComponent<TimedRocket>().hasExploded)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                try
                                {
                                    if (target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<EntityAI>().isDead)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<CastleWallBreak>().hasExploded)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<CastleFloorBreak>().hasExploded)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<BreakOnForce>().BrokenInstance.hasChanged)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<BreakOnForceNoScaling>().BrokenInstance.hasChanged)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<BreakOnForceNoSpawn>().BrokenInstance.hasChanged)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (target.gameObject.GetComponent<BreakOnForceBoulder>().BrokenInstance.hasChanged)
                                    {
                                        target = null;
                                        targetCollider = null;
                                        targetAquired = false;
                                        SendClientTargetNull();
                                    }
                                }
                                catch { }
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

        public override void SimulateLateUpdateAlways()
        {
            if (!StatMaster.isClient)
            {
                if (rocket.hasFired && !rocket.hasExploded)
                {
                    if (guidedRocketStabilityOn)
                    {
                        //Add aerodynamic force to rocket
                        AddAerodynamicsToRocketVelocity();
                    }

                    if (guidedRocketActivated)
                    {
                        if (target != null && targetCollider != null && canTrigger)
                        {
                            // Calculating the rotating axis
                            Vector3 velocity = Vector3.zero;
                            try
                            {
                                velocity = targetCollider.attachedRigidbody.velocity;
                                if (previousVelocity != Vector3.zero)
                                {
                                    acceleration = (velocity - previousVelocity) / Time.deltaTime;
                                }
                                previousVelocity = velocity;
                            }
                            catch { }
                            //Add position prediction
                            float ratio = (targetCollider.bounds.center - rocket.transform.position).magnitude / initialDistance;
                            float actualPrediction = prediction * Mathf.Clamp(Mathf.Pow(ratio, 2), 0f, 1.5f);
                            float pathPredictionTime = Time.fixedDeltaTime * actualPrediction;
                            Vector3 positionDiff = targetCollider.bounds.center + velocity * pathPredictionTime + 0.5f * acceleration * pathPredictionTime * pathPredictionTime - rocket.transform.position;
                            float angleDiff = Vector3.Angle(positionDiff, transform.up);
                            bool forward = Vector3.Dot(transform.up, positionDiff) > 0;
                            Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, transform.up);

                            //Add torque to the rocket based on the angle difference
                            //If in auto guide mode, the rocket will restart searching when target is out of sight
                            //else, apply maximum torque to the rocket
                            if (forward && angleDiff <= searchAngle)
                            {
                                try
                                {
                                    //rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * Mathf.Pow(angleDiff / maxSearchAngleNo8, 0.5f) * rotatingAxis);
                                    rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / maxSearchAngleNo8 - 1f, 2) + 1)) * rotatingAxis);
                                }
                                catch { }
                            }
                            else
                            {
                                if (!activeGuide)
                                {
                                    try { rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * rotatingAxis); }
                                    catch { }
                                }
                                else
                                {
                                    targetAquired = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (rocket.isSimulating && rocket.hasFired && !rocket.hasExploded
                    && (collision.gameObject.name.Contains("CanonBall") || (collision.impulse.magnitude > 1 && canTrigger)))
                {
                    RocketExplode();
                }
            }
            catch { }
        }
        void OnCollisionStay(Collision collision)
        {
            try
            {
                if (rocket.isSimulating && rocket.hasFired && !rocket.hasExploded
                    && (collision.gameObject.name.Contains("CanonBall") || (collision.impulse.magnitude > 1 && canTrigger)))
                {
                    RocketExplode();
                }
            }
            catch { }
        }

        private void RocketExplode()
        {
            //Reset some parameter and set the rocket to explode
            //Stop the search target coroutine
            searchStarted = targetHit = true;
            StopCoroutine(SearchForTarget());
            SendClientTargetNull();

            if (!highExploActivated)
            {
                if (!rocket.hasExploded) rocket.OnExplode();
            }
            else
            {
                if (!bombHasExploded && explosiveCharge != 0)
                {
                    if (StatMaster.isHosting)
                    {
                        SendExplosionPositionToAll();
                    }
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
                                    hit.attachedRigidbody.WakeUp();
                                    hit.attachedRigidbody.constraints = RigidbodyConstraints.None;
                                    hit.attachedRigidbody.AddExplosionForce(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                    hit.attachedRigidbody.AddRelativeTorque(UnityEngine.Random.insideUnitSphere.normalized * torquePower * bombExplosiveCharge);
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<RocketScript>().RocketExplode();
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
            yield return new WaitForSeconds(randomDelay);
            //Grab every machine block at the start of search
            HashSet<Machine.SimCluster> simClusters = new HashSet<Machine.SimCluster>();

            if (StatMaster.isMP)
            {
                foreach (var player in Playerlist.Players)
                {
                    if (!player.isSpectator)
                    {
                        if (player.machine.isSimulating && !player.machine.LocalSim && player.machine.PlayerID != rocket.ParentMachine.PlayerID)
                        {
                            if (rocket.Team == MPTeam.None || rocket.Team != player.team)
                            {
                                simClusters.UnionWith(player.machine.simClusters);
                            }
                        }
                    }
                }
            }
            else
            {
                simClusters.UnionWith(Machine.Active().simClusters);
                simClusters.ExceptWith(clustersInSafetyRange);
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired && !targetHit && simClusters.Count > 0)
            {
                try
                {
                    // Remove any null cluster due to stopped simulation
                    simClusters.RemoveWhere(cluster => cluster == null);
                    simClusters.ExceptWith(explodedCluster);

                    HashSet<Machine.SimCluster> simClusterForSearch = new HashSet<Machine.SimCluster>(simClusters);
                    HashSet<Machine.SimCluster> unwantedClusters = new HashSet<Machine.SimCluster>();

                    foreach (var cluster in simClusters)
                    {
                        Vector3 positionDiff = cluster.Base.gameObject.transform.position - rocket.transform.position;
                        float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);
                        bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                        bool skipCluster = !(forward && angleDiff < searchAngle) || ShouldSkipCluster(cluster.Base);

                        if (!skipCluster)
                        {
                            foreach (var block in cluster.Blocks)
                            {
                                skipCluster = ShouldSkipCluster(block);
                                if (skipCluster)
                                {
                                    break;
                                }
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
                        target = GetMostValuableCluster(simClusterForSearch);
                        targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                        targetAquired = true;
                        searchStarted = false;
                        previousVelocity = acceleration = Vector3.zero;
                        initialDistance = (target.position - rocket.transform.position).magnitude;
                        targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
                        SendTargetToClient();
                        StopCoroutine(SearchForTarget());
                    }
                }
                catch { }
                yield return null;
            }
        }

        private Transform GetMostValuableCluster(HashSet<Machine.SimCluster> simClusterForSearch)
        {
            //Remove any null cluster
            simClusterForSearch.RemoveWhere(cluster => cluster == null);

            //Search for any blocks within the search radius for every block in the hitlist
            float[] targetValue = new float[simClusterForSearch.Count];
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
            float maxValue = targetValue.Max();
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

            foreach (var cluster in maxClusters)
            {
                if (cluster.Base.Type == BlockType.Rocket)
                {
                    try
                    {
                        if (cluster.Base.gameObject.GetComponent<TimedRocket>().hasFired)
                        {
                            return cluster.Base.transform;
                        }
                    }
                    catch { }
                }
                foreach (var block in cluster.Blocks)
                {
                    if (block.Type == BlockType.Rocket)
                    {
                        try
                        {
                            if (block.gameObject.GetComponent<TimedRocket>().hasFired)
                            {
                                return block.transform;
                            }
                        }
                        catch { }
                    }
                }
            }

            return maxClusters[closestIndex].Base.gameObject.transform;
        }

        private void AddAerodynamicsToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(rocketRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * aeroEffectMultiplier;
            float velocitySqr = rocketRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            rocketRigidbody.AddRelativeForce(Vector3.Scale(dir, -locVel) * currentVelocitySqr);
        }

        private int CalculateClusterValue(BlockBehaviour block, int clusterValue)
        {
            //Some blocks weights more than others
            GameObject targetObj = block.gameObject;
            //A bomb
            if (block.Type == BlockType.Bomb)
            {
                if (!targetObj.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                {
                    clusterValue *= bombValue;
                }
            }
            //A fired and unexploded rocket
            if (block.Type == BlockType.Rocket)
            {
                if (targetObj.GetComponent<TimedRocket>().hasFired)
                {
                    if (targetObj.GetComponent<RocketScript>().targetAquired)
                    {
                        clusterValue *= guidedRocketValue;
                    }
                    else
                    {
                        clusterValue *= normalRocketValue;
                    }
                }
            }
            //A watering watercannon
            if (block.Type == BlockType.WaterCannon)
            {
                if (targetObj.GetComponent<WaterCannonController>().isActive)
                {
                    clusterValue *= waterCannonValue;
                }
            }
            //A flying flying-block
            if (block.Type == BlockType.FlyingBlock)
            {
                if (targetObj.GetComponent<FlyingController>().canFly)
                {
                    clusterValue *= flyingBlockValue;
                }
            }
            //A flaming flamethrower
            if (block.Type == BlockType.Flamethrower)
            {
                if (targetObj.GetComponent<FlamethrowerController>().isFlaming)
                {
                    clusterValue *= flameThrowerValue;
                }
            }
            //A spinning wheel/cog
            if (targetObj.GetComponent<CogMotorControllerHinge>())
            {
                if (targetObj.GetComponent<CogMotorControllerHinge>().Velocity != 0)
                {
                    clusterValue *= cogMotorValue;
                }
            }
            return clusterValue;
        }

        private bool ShouldSkipCluster(BlockBehaviour block)
        {
            try
            {
                if (block.Type == BlockType.Rocket)
                {
                    if (block.gameObject.GetComponent<TimedRocket>().hasExploded)
                    {
                        return true;
                    }
                }
                else
                {
                    if (block.fireTag.burning)
                    {
                        return true;
                    }
                    try
                    {
                        if (block.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                        {
                            return true;
                        }
                    }
                    catch { }
                    try
                    {
                        if (block.gameObject.GetComponent<ControllableBomb>().hasExploded)
                        {
                            return true;
                        }
                    }
                    catch { }

                }
            }
            catch { }
            return false;
        }

        private void OnGUI()
        {
            if (StatMaster.isMP && StatMaster.isHosting)
            {
                if (rocket.ParentMachine.PlayerID != 0)
                {
                    return;
                }
            }
            DrawTargetRedSquare();
        }

        private void DrawTargetRedSquare()
        {
            if (target != null && targetCollider.bounds != null && !rocket.hasExploded && rocket.isSimulating && rocket != null)
            {
                if (Vector3.Dot(Camera.main.transform.forward, targetCollider.bounds.center - Camera.main.transform.position) > 0)
                {
                    int squareWidth = 16;
                    Vector3 itemScreenPosition = Camera.main.WorldToScreenPoint(targetCollider.bounds.center);
                    GUI.DrawTexture(new Rect(itemScreenPosition.x - squareWidth / 2, Camera.main.pixelHeight - itemScreenPosition.y - squareWidth / 2, squareWidth, squareWidth), rocketAim);
                }
            }
        }

        private void SendTargetToClient()
        {
            if (target != null)
            {
                if (target.gameObject.GetComponent<BlockBehaviour>())
                {
                    BlockBehaviour targetBB = target.gameObject.GetComponent<BlockBehaviour>();
                    int id = targetBB.ParentMachine.PlayerID;
                    if (rocket.ParentMachine.PlayerID != 0)
                    {
                        Message targetBlockBehaviourMsg = Messages.rocketTargetBlockBehaviourMsg.CreateMessage(targetBB, BB);
                        ModNetworking.SendTo(Player.GetAllPlayers()[rocket.ParentMachine.PlayerID], targetBlockBehaviourMsg);
                    }
                    ModNetworking.SendToAll(Messages.rocketLockOnMeMsg.CreateMessage(BB, id));
                    BlockEnhancementMod.mod.GetComponent<MessageController>().UpdateRocketTarget(BB, id);
                }
                if (target.gameObject.GetComponent<LevelEntity>())
                {
                    Message targetEntityMsg = Messages.rocketTargetEntityMsg.CreateMessage(target.gameObject.GetComponent<LevelEntity>(), BB);
                    ModNetworking.SendTo(Player.GetAllPlayers()[rocket.ParentMachine.PlayerID], targetEntityMsg);

                    ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(BB));
                    BlockEnhancementMod.mod.GetComponent<MessageController>().RemoveRocketTarget(BB);
                }
            }
        }

        private void SendClientTargetNull()
        {
            if (StatMaster.isHosting)
            {
                Message rocketTargetNullMsg = Messages.rocketTargetNullMsg.CreateMessage(BB);
                ModNetworking.SendTo(Player.GetAllPlayers()[rocket.ParentMachine.PlayerID], rocketTargetNullMsg);

                Message rocketLostTargetMsg = Messages.rocketLostTargetMsg.CreateMessage(BB);
                ModNetworking.SendInSimulation(rocketLostTargetMsg);
                BlockEnhancementMod.mod.GetComponent<MessageController>().RemoveRocketTarget(BB);
            }
        }

        private void SendRayToHost(Ray ray)
        {
            Message rayToHostMsg = Messages.rocketRayToHostMsg.CreateMessage(ray.origin, ray.direction, BB);
            ModNetworking.SendToHost(rayToHostMsg);
        }

        private void SendExplosionPositionToAll()
        {
            Message explosionPositionMsg = Messages.rocketHighExploPosition.CreateMessage(rocket.transform.position, bombExplosiveCharge);
            ModNetworking.SendToAll(explosionPositionMsg);
        }
    }
}
