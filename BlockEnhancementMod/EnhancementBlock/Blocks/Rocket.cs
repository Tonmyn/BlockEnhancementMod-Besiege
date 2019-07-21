using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Common;

namespace BlockEnhancementMod
{
    class RocketScript : EnhancementBlock
    {
        //General setting
        MToggle GuidedRocketToggle;
        MKey LockTargetKey;
        public MKey GroupFireKey;
        MSlider GroupFireRateSlider;
        MToggle AutoGrabberReleaseToggle;
        public bool autoGrabberRelease = false;
        public float groupFireRate = 0.25f;
        private Texture2D rocketAim;
        //public Transform target;
        public TimedRocket rocket;
        public Rigidbody rocketRigidbody;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };
        public static bool MarkTarget { get; internal set; } = true;
        public bool noLongerActiveSent = false;
        public bool removedFromGroup = false;



        //No smoke mode related
        MToggle NoSmokeToggle;
        private bool noSmoke = false;
        private bool smokeStopped = false;

        //Firing record related setting
        private bool targetHit = false;
        private float randomDelay = 0;
        private float launchTime = 0f;
        private bool launchTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MToggle GuidedRocketStabilityToggle;
        MSlider GuidePredictionSlider;
        public bool guidedRocketStabilityOn = true;
        public bool guidedRocketActivated = false;
        public bool rocketExploMsgSent = false;
        public bool rocketInBuildSent = false;
        public float torque = 100f;
        public float prediction = 10f;
        public float initialDistance = 0f;
        private readonly float maxTorque = 10000;
        public Vector3 previousVelocity;
        public Vector3 acceleration;
        //public Collider targetCollider;
        private bool targetInitialCJOrHJ = false;
        private HashSet<Machine.SimCluster> clustersInSafetyRange = new HashSet<Machine.SimCluster>();
        private HashSet<Machine.SimCluster> explodedCluster = new HashSet<Machine.SimCluster>();

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey SwitchGuideModeKey;
        MMenu DefaultSearchModeMenu;
        private int searchModeIndex = 0;
        public List<string> searchMode = new List<string>() { LanguageManager.Instance.CurrentLanguage.defaultAuto, LanguageManager.Instance.CurrentLanguage.defaultManual };
        public List<KeyCode> switchGuideModeKey = new List<KeyCode> { KeyCode.RightShift };
        public float searchAngle = 10;
        private readonly float safetyRadiusAuto = 50f;
        private readonly float safetyRadiusManual = 15f;
        private readonly float maxSearchAngle = 25f;
        private readonly float maxSearchAngleNo8 = 89f;
        private readonly float searchRange = 1400f - 0f;
        public bool activeGuide = true;
        //public bool targetAquired = false;
        //public bool searchStarted = false;
        public GameObject radarObject;
        public RadarScript radar;


        //Cluster value multiplier
        private readonly float bombValue = 64;
        private readonly float guidedRocketValue = 1024;
        private readonly float waterCannonValue = 16;
        private readonly float flyingBlockValue = 2;
        private readonly float flameThrowerValue = 8;
        private readonly float cogMotorValue = 2;

        //impact & proximity fuze related setting
        MToggle ImpactFuzeToggle;
        MToggle ProximityFuzeToggle;
        MSlider ProximityFuzeRangeSlider;
        MSlider ProximityFuzeAngleSlider;
        public bool impactFuzeActivated = false;
        public bool proximityFuzeActivated = false;
        public float proximityRange = 0f;
        public float proximityAngle = 0f;
        public float triggerForceImpactFuzeOn = 50f;
        public float triggerForceImpactFuzeOff = 400f;

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
        private readonly float aeroEffectMultiplier = 5f;
        private Vector3 aeroEffectPosition = Vector3.zero;

        public override void SafeAwake()
        {
            //Load aim pic
            rocketAim = new Texture2D(16, 16);
            rocketAim.LoadImage(ModIO.ReadAllBytes("Resources" + @"/" + "Square-Red.png"));

            //Key mapper setup
            GuidedRocketToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.trackTarget, "TrackingRocket", guidedRocketActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketActivated =
                DefaultSearchModeMenu.DisplayInMapper =
                GuidedRocketTorqueSlider.DisplayInMapper =
                GuidePredictionSlider.DisplayInMapper =
                ImpactFuzeToggle.DisplayInMapper =
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

            DefaultSearchModeMenu = BB.AddMenu(LanguageManager.Instance.CurrentLanguage.searchMode, searchModeIndex, searchMode, false);
            DefaultSearchModeMenu.ValueChanged += (int value) =>
            {
                searchModeIndex = value;
                ChangedProperties();
            };

            AutoGrabberReleaseToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.autoGrabberRelease, "AutoGrabberRelease", autoGrabberRelease);
            AutoGrabberReleaseToggle.Toggled += (bool value) =>
            {
                autoGrabberRelease = value;
                ChangedProperties();
            };

            ImpactFuzeToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.impactFuze, "ImpactFuze", impactFuzeActivated);
            ImpactFuzeToggle.Toggled += (bool value) =>
            {
                impactFuzeActivated = value;
                ChangedProperties();
            };

            ProximityFuzeToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.proximityFuze, "ProximityFuze", proximityFuzeActivated);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                ProximityFuzeAngleSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };

            NoSmokeToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.noSmoke, "NoSmoke", noSmoke);
            NoSmokeToggle.Toggled += (bool value) =>
            {
                noSmoke = value;
                ChangedProperties();
            };

            HighExploToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.highExplo, "HighExplo", highExploActivated);
            HighExploToggle.Toggled += (bool value) =>
            {
                highExploActivated = value;
                ChangedProperties();
            };

            ActiveGuideRocketSearchAngleSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.searchAngle, "searchAngle", searchAngle, 0, maxSearchAngle);
            ActiveGuideRocketSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };

            GuidePredictionSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.prediction, "prediction", prediction, 0, 50);
            GuidePredictionSlider.ValueChanged += (float value) => { prediction = value; ChangedProperties(); };

            ProximityFuzeRangeSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.closeRange, "closeRange", proximityRange, 0, 10);
            ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };

            ProximityFuzeAngleSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.closeAngle, "closeAngle", proximityAngle, 0, 90);
            ProximityFuzeAngleSlider.ValueChanged += (float value) => { proximityAngle = value; ChangedProperties(); };

            GuidedRocketTorqueSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.torqueOnRocket, "torqueOnRocket", torque, 0, 100);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };

            GuidedRocketStabilityToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.rocketStability, "RocketStabilityOn", guidedRocketStabilityOn);
            GuidedRocketStabilityToggle.Toggled += (bool value) => { guidedRocketStabilityOn = value; ChangedProperties(); };

            GuideDelaySlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.guideDelay, "guideDelay", guideDelay, 0, 2);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };

            LockTargetKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.lockTarget, "lockTarget", KeyCode.Delete);
            LockTargetKey.InvokeKeysChanged();

            GroupFireKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.groupedFire, "groupFire", KeyCode.None);
            GroupFireKey.InvokeKeysChanged();

            GroupFireRateSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.groupFireRate, "groupFireRate", groupFireRate, 0.1f, 1f);
            GroupFireRateSlider.ValueChanged += (float value) => { groupFireRate = value; ChangedProperties(); };

            SwitchGuideModeKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.switchGuideMode, "ActiveSearchKey", KeyCode.RightShift);
            SwitchGuideModeKey.InvokeKeysChanged();

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            rocketRigidbody = gameObject.GetComponent<Rigidbody>();




#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            HighExploToggle.DisplayInMapper = value;
            NoSmokeToggle.DisplayInMapper = value;
            GroupFireKey.DisplayInMapper = value;
            GroupFireRateSlider.DisplayInMapper = value;
            AutoGrabberReleaseToggle.DisplayInMapper = value;
            SwitchGuideModeKey.DisplayInMapper = value && guidedRocketActivated;
            DefaultSearchModeMenu.DisplayInMapper = value && guidedRocketActivated;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidePredictionSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketStabilityToggle.DisplayInMapper = value && guidedRocketActivated;
            ImpactFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated;
        }

        public override void BuildingUpdateAlways_EnhancementEnabled()
        {
            if (!rocketInBuildSent)
            {
                if (RocketsController.Instance.playerGroupedRockets.TryGetValue(StatMaster.isMP ? rocket.ParentMachine.PlayerID : 0, out Dictionary<KeyCode, HashSet<TimedRocket>> groupedRockets))
                {
                    if (groupedRockets.TryGetValue(GroupFireKey.GetKey(0), out HashSet<TimedRocket> rockets))
                    {
                        rockets.Remove(rocket);
                    }
                }
                radar.SendClientTargetNull();
                rocketInBuildSent = true;
            }

            if (GroupFireKey.GetKey(0) == KeyCode.None)
            {
                if (AutoGrabberReleaseToggle.DisplayInMapper)
                {
                    AutoGrabberReleaseToggle.DisplayInMapper = false;
                    AutoGrabberReleaseToggle.SetValue(false);
                }
                if (GroupFireRateSlider.DisplayInMapper)
                {
                    GroupFireRateSlider.DisplayInMapper = false;
                }
            }
            else  /*(GroupFireKey.GetKey(0) != KeyCode.None)*/
            {
                if (!AutoGrabberReleaseToggle.DisplayInMapper)
                {
                    AutoGrabberReleaseToggle.DisplayInMapper = true;
                }
                if (!GroupFireRateSlider.DisplayInMapper)
                {
                    GroupFireRateSlider.DisplayInMapper = true;
                }
            }
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            smokeStopped = rocketInBuildSent = noLongerActiveSent = removedFromGroup = false;
            aeroEffectPosition = rocket.transform.up * rocket.transform.lossyScale.y / 3;
            //Initialise Dict in RocketsController
            if (GroupFireKey.GetKey(0) != KeyCode.None)
            {
                if (!RocketsController.Instance.playerGroupedRockets.ContainsKey(rocket.ParentMachine.PlayerID))
                {
                    RocketsController.Instance.playerGroupedRockets.Add(rocket.ParentMachine.PlayerID, new Dictionary<KeyCode, HashSet<TimedRocket>>());
                }
                if (!RocketsController.Instance.playerGroupedRockets[rocket.ParentMachine.PlayerID].ContainsKey(GroupFireKey.GetKey(0)))
                {
                    RocketsController.Instance.playerGroupedRockets[rocket.ParentMachine.PlayerID].Add(GroupFireKey.GetKey(0), new HashSet<TimedRocket>());
                }
                if (!RocketsController.Instance.playerGroupedRockets[rocket.ParentMachine.PlayerID][GroupFireKey.GetKey(0)].Contains(rocket))
                {
                    RocketsController.Instance.playerGroupedRockets[rocket.ParentMachine.PlayerID][GroupFireKey.GetKey(0)].Add(rocket);
                }
            }
            if (guidedRocketActivated)
            {
                // Initialisation for simulation
                launchTimeRecorded = canTrigger = /*targetAquired =*/ targetHit = bombHasExploded/* = receivedRayFromClient*/ = targetInitialCJOrHJ = rocketExploMsgSent = false;
                activeGuide = (searchModeIndex == 0);
                //target = null;
                //targetCollider = null;
                explodedCluster.Clear();
                searchAngle = Mathf.Clamp(searchAngle, 0, EnhanceMore ? maxSearchAngleNo8 : maxSearchAngle);

                //Add radar
                radarObject = new GameObject("RocketRadar");
                radar = radarObject.AddComponent<RadarScript>();
                radarObject.transform.SetParent(gameObject.transform);
                radarObject.transform.position = transform.position;
                radarObject.transform.rotation = transform.rotation;
                radarObject.transform.localPosition = Vector3.forward * 0.5f;
                radar.CreateFrustumCone(searchAngle * 2, safetyRadiusAuto, searchRange);
                radar.ClearSavedSets();
                radar.DeactivateDetectionZone();

                previousVelocity = acceleration = Vector3.zero;
                randomDelay = UnityEngine.Random.Range(0f, 0.1f);
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
                if (highExploActivated && !EnhanceMore)
                {
                    bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
                }
            }
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (gameObject.activeInHierarchy)
            {
                if (GroupFireKey.IsHeld && !StatMaster.isClient)
                {
                    if (!RocketsController.Instance.launchStarted)
                    {
                        StartCoroutine(RocketsController.Instance.LaunchRocketFromGroup(rocket.ParentMachine.PlayerID, GroupFireKey.GetKey(0)));
                    }
                }
                if (guidedRocketActivated)
                {
                    //When toggle auto aim key is released, change the auto aim status
                    if (SwitchGuideModeKey.IsReleased)
                    {
                        activeGuide = !activeGuide;
                        if (!activeGuide)
                        {
                            //target = null;
                            //targetCollider = null;
                            previousVelocity = acceleration = Vector3.zero;
                            radar.SendClientTargetNull();
                        }
                        else
                        {
                            //targetAquired = false;
                        }
                    }

                    if (LockTargetKey.IsPressed)
                    {
                        //target = null;
                        //targetCollider = null;
                        previousVelocity = acceleration = Vector3.zero;
                        radar.SendClientTargetNull();
                        if (activeGuide)
                        {
                            //When launch key is released, reset target search
                            //targetAquired = false;
                            radar.ActivateDetectionZone();
                        }
                        else
                        {
                            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            //if (StatMaster.isClient)
                            //{
                            //    SendRayToHost(ray);
                            //}
                            //else
                            //{
                            //    //Find targets in the manual search mode by casting a sphere along the ray
                            //    float manualSearchRadius = 1.25f;
                            //    RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity);
                            //    Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit);
                            //    if (hits.Length > 0)
                            //    {
                            //        for (int i = 0; i < hits.Length; i++)
                            //        {
                            //            if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                            //            {
                            //                if ((hits[i].transform.position - rocket.transform.position).magnitude >= safetyRadiusManual)
                            //                {
                            //                    target = hits[i].transform;
                            //                    targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                            //                    targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
                            //                    previousVelocity = acceleration = Vector3.zero;
                            //                    initialDistance = (hits[i].transform.position - rocket.transform.position).magnitude;
                            //                    targetAquired = true;
                            //                    break;
                            //                }
                            //            }
                            //        }
                            //        if (target == null)
                            //        {
                            //            for (int i = 0; i < hits.Length; i++)
                            //            {
                            //                if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
                            //                {
                            //                    if ((hits[i].transform.position - rocket.transform.position).magnitude >= safetyRadiusManual)
                            //                    {
                            //                        target = hits[i].transform;
                            //                        targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                            //                        targetInitialCJOrHJ = false;
                            //                        previousVelocity = acceleration = Vector3.zero;
                            //                        initialDistance = (hits[i].transform.position - rocket.transform.position).magnitude;
                            //                        targetAquired = true;
                            //                        break;
                            //                    }
                            //                }
                            //            }
                            //        }
                            //    }
                            //    if (target == null && rayHit.transform != null)
                            //    {
                            //        if ((rayHit.transform.position - rocket.transform.position).magnitude >= safetyRadiusManual)
                            //        {
                            //            target = rayHit.transform;
                            //            targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
                            //            targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
                            //            previousVelocity = acceleration = Vector3.zero;
                            //            initialDistance = (rayHit.transform.position - rocket.transform.position).magnitude;
                            //            targetAquired = true;
                            //        }

                            //    }
                            //    if (receivedRayFromClient)
                            //    {
                            //        SendTargetToClient();
                            //    }
                            //    receivedRayFromClient = false;
                            //}
                        }
                    }
                }
            }
        }

        public override void SimulateFixedUpdate_EnhancementEnabled()
        {
            if (gameObject.activeInHierarchy)
            {
                if (rocket.hasFired)
                {
                    SendRocketFired();
                    if (!rocket.hasExploded)
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
                            //Activate Detection Zone
                            //radar.ActivateDetectionZone();

                            //Record the launch time for the guide delay
                            if (!launchTimeRecorded)
                            {
                                launchTimeRecorded = true;
                                launchTime = Time.time;

                            }

                            //Rocket can be triggered after the time elapsed after firing is greater than guide delay
                            if (Time.time - launchTime >= guideDelay && !canTrigger)
                            {
                                canTrigger = true;
                            }

                            //Check if target is no longer valuable (lazy check)
                            //if (radar.target != null && !StatMaster.isClient)
                            //{
                            //    try
                            //    {
                            //        if (targetCollider == null)
                            //        {
                            //            target = null;
                            //            targetCollider = null;
                            //            targetAquired = false;
                            //            SendClientTargetNull();
                            //        }
                            //        else
                            //        {
                            //            //If proximity fuse is enabled, the rocket will explode when target is in preset range&angle
                            //            Vector3 positionDiff = targetCollider.bounds.center - rocket.transform.position;
                            //            float angleDiff = Vector3.Angle(positionDiff, transform.up);
                            //            if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                            //            {
                            //                StartCoroutine(RocketExplode());
                            //            }
                            //        }
                            //    }
                            //    catch { }
                            //    try
                            //    {
                            //        if (targetInitialCJOrHJ)
                            //        {
                            //            if (radar.target.gameObject.GetComponent<ConfigurableJoint>() == null && target.gameObject.GetComponent<HingeJoint>() == null)
                            //            {
                            //                try
                            //                {
                            //                    explodedCluster.Add(radar.target.gameObject.GetComponent<BlockBehaviour>().ParentMachine.simClusters[target.gameObject.GetComponent<BlockBehaviour>().ClusterIndex]);
                            //                }
                            //                catch { }
                            //                target = null;
                            //                targetCollider = null;
                            //                targetAquired = targetInitialCJOrHJ = false;
                            //                SendClientTargetNull();
                            //            }
                            //        }
                            //    }
                            //    catch { }
                            //    try
                            //    {
                            //        if (radar.target.gameObject.GetComponent<FireTag>().burning)
                            //        {
                            //            if (radar.target.gameObject.GetComponent<TimedRocket>() == null)
                            //            {
                            //                target = null;
                            //                targetCollider = null;
                            //                targetAquired = false;
                            //                SendClientTargetNull();
                            //            }
                            //        }
                            //    }
                            //    catch { }
                            //    try
                            //    {
                            //        if (radar.target.gameObject.GetComponent<BlockBehaviour>())
                            //        {
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<TimedRocket>().hasExploded)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //        }
                            //        else
                            //        {
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<EntityAI>().isDead)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<CastleWallBreak>().hasExploded)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<CastleFloorBreak>().hasExploded)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<BreakOnForce>().BrokenInstance.hasChanged)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<BreakOnForceNoScaling>().BrokenInstance.hasChanged)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<BreakOnForceNoSpawn>().BrokenInstance.hasChanged)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //            try
                            //            {
                            //                if (radar.target.gameObject.GetComponent<BreakOnForceBoulder>().BrokenInstance.hasChanged)
                            //                {
                            //                    target = null;
                            //                    targetCollider = null;
                            //                    targetAquired = false;
                            //                    SendClientTargetNull();
                            //                }
                            //            }
                            //            catch { }
                            //        }
                            //    }
                            //    catch { }
                            //}

                            //If no target when active guide, search for a new target
                            //if (activeGuide && !targetAquired)
                            //{
                            //   radar.ActivateDetectionZone();
                            //}
                        }
                    }
                }
                if (rocket.hasExploded && !rocketExploMsgSent)
                {
                    radar.SendClientTargetNull();
                    rocketExploMsgSent = true;
                }
            }
            else
            {
                if (!noLongerActiveSent)
                {
                    radar.SendClientTargetNull();
                    noLongerActiveSent = true;
                }
            }
        }

        public override void SimulateLateUpdate_EnhancementEnabled()
        {
            if (gameObject.activeInHierarchy)
            {
                if (!StatMaster.isClient)
                {
                    if (rocket.hasFired && !rocket.hasExploded && canTrigger)
                    {
                        if (guidedRocketStabilityOn)
                        {
                            //Add aerodynamic force to rocket
                            if (rocketRigidbody != null)
                            {
                                AddAerodynamicsToRocketVelocity();
                            }
                        }
                        if (guidedRocketActivated)
                        {
                            //if (target != null && targetCollider != null)
                            //{
                            //    // Calculating the rotating axis
                            //    Vector3 velocity = Vector3.zero;
                            //    try
                            //    {
                            //        velocity = targetCollider.attachedRigidbody.velocity - rocket.Rigidbody.velocity;
                            //        if (previousVelocity != Vector3.zero)
                            //        {
                            //            acceleration = (velocity - previousVelocity) / Time.deltaTime;
                            //        }
                            //        previousVelocity = velocity;
                            //    }
                            //    catch { }
                            //    //Add position prediction
                            //    float ratio = (targetCollider.bounds.center - rocket.transform.position).magnitude / initialDistance;
                            //    float actualPrediction = prediction * Mathf.Clamp(Mathf.Pow(ratio, 2), 0f, 1.5f);
                            //    float pathPredictionTime = Time.fixedDeltaTime * actualPrediction;
                            //    Vector3 positionDiff = targetCollider.bounds.center + velocity * pathPredictionTime + 0.5f * acceleration * pathPredictionTime * pathPredictionTime - rocket.transform.position;
                            //    float angleDiff = Vector3.Angle(positionDiff, transform.up);
                            //    bool forward = Vector3.Dot(transform.up, positionDiff) > 0;
                            //    Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, transform.up);

                            //    if (rocketRigidbody != null)
                            //    {
                            //        rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / maxSearchAngleNo8 - 1f, 2) + 1)) * rotatingAxis);
                            //    }

                            //    //Add torque to the rocket based on the angle difference
                            //    //If in auto guide mode, the rocket will restart searching when target is out of sight
                            //    //else, apply maximum torque to the rocket
                            //    //if (forward && angleDiff <= searchAngle)
                            //    //{
                            //    //    if (rocketRigidbody != null)
                            //    //    {
                            //    //        rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / maxSearchAngleNo8 - 1f, 2) + 1)) * rotatingAxis);
                            //    //    }
                            //    //}
                            //    //else
                            //    //{
                            //    //    if (!activeGuide)
                            //    //    {
                            //    //        if (rocketRigidbody != null)
                            //    //        {
                            //    //            rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * rotatingAxis);
                            //    //        }
                            //    //    }
                            //    //    else
                            //    //    {
                            //    //        target = null;
                            //    //        targetCollider = null;
                            //    //        targetAquired = false;
                            //    //        SendClientTargetNull();
                            //    //    }
                            //    //}
                            //}
                        }
                    }
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (canTrigger)
            {
                if (rocket.PowerSlider.Value > 0.1f)
                {
                    if (collision.impulse.magnitude / Time.fixedDeltaTime >= (impactFuzeActivated ? triggerForceImpactFuzeOn : triggerForceImpactFuzeOff) || collision.gameObject.name.Contains("CanonBall"))
                    {
                        StartCoroutine(RocketExplode());
                    }
                }
            }
        }

        private IEnumerator RocketExplode()
        {
            //Reset some parameter and set the rocket to explode
            //Stop the search target coroutine
            targetHit = true;
            //StopCoroutine(SearchForTarget());
            radar.SendClientTargetNull();

            Vector3 position = rocket.transform.position;
            Quaternion rotation = rocket.transform.rotation;

            if (!rocket.hasExploded)
            {
                rocket.ExplodeMessage();
            }
            if (!highExploActivated) yield break;

            if (!bombHasExploded && explosiveCharge != 0)
            {
                if (StatMaster.isHosting)
                {
                    SendExplosionPositionToAll(position);
                }
                bombHasExploded = true;
                //Generate a bomb from level editor and let it explode
                try
                {
                    GameObject bomb = (GameObject)Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject, position, rotation);
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

                Collider[] hits = Physics.OverlapSphere(rocket.transform.position, radius * bombExplosiveCharge);
                int index = 0;
                int rank = 60;

                if (hits.Length <= 0) yield break;

                foreach (var hit in hits)
                {
                    if (index > rank)
                    {
                        index = 0;
                        yield return 0;
                    }

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
                            hit.attachedRigidbody.gameObject.GetComponent<RocketScript>().StartCoroutine(RocketExplode());
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
                    else
                    {
                        continue;
                    }
                    index++;
                }
            }


        }

        //private void RocketRadarSearch()
        //{
        //    //if (!searchStarted && activeGuide)
        //    //{
        //    //    searchStarted = true;
        //    //    StopCoroutine(SearchForTarget());
        //    //    StartCoroutine(SearchForTarget());
        //    //}
        //}

        //private IEnumerator SearchForTarget()
        //{
        //    // First test the rockets that are fired
        //    Dictionary<BlockBehaviour, int> rocketTargetDict = RocketsController.Instance.rocketTargetDict;
        //    Transform rocketTarget = null;
        //    Transform clusterTarget = null;
        //    float rocketValue = 0;
        //    float clusterValue = 0;

        //    if (rocketTargetDict != null)
        //    {
        //        if (rocketTargetDict.Count > 0)
        //        {
        //            float distance = Mathf.Infinity;
        //            foreach (var rocketTargetPair in rocketTargetDict)
        //            {
        //                BlockBehaviour targetRocket = rocketTargetPair.Key;
        //                if (targetRocket != null)
        //                {
        //                    bool shouldCheckRocket = false;
        //                    if (StatMaster.isMP)
        //                    {
        //                        shouldCheckRocket = (targetRocket.ParentMachine.PlayerID != rocket.ParentMachine.PlayerID) && (rocket.Team == MPTeam.None || rocket.Team != targetRocket.Team);
        //                    }
        //                    else
        //                    {
        //                        if (targetRocket.ClusterIndex == -1)
        //                        {
        //                            shouldCheckRocket = (targetRocket.transform.position - rocket.transform.position).magnitude > safetyRadiusAuto;
        //                        }
        //                        else
        //                        {
        //                            int count = 0;
        //                            foreach (var cluster in clustersInSafetyRange)
        //                            {
        //                                if (cluster.Base.ClusterIndex == targetRocket.ClusterIndex)
        //                                {
        //                                    count++;
        //                                }
        //                            }
        //                            shouldCheckRocket = count > 0 ? false : true;
        //                        }
        //                    }
        //                    if (CheckInRange(targetRocket) && shouldCheckRocket)
        //                    {
        //                        float tempDistance = (targetRocket.transform.position - rocket.transform.position).magnitude;
        //                        if (tempDistance <= distance)
        //                        {
        //                            rocketTarget = targetRocket.transform;
        //                            distance = tempDistance;
        //                            rocketValue = guidedRocketValue;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    //Grab every machine block at the start of search
        //    HashSet<Machine.SimCluster> simClusters = new HashSet<Machine.SimCluster>();

        //    if (StatMaster.isMP)
        //    {
        //        foreach (var player in Playerlist.Players)
        //        {
        //            if (!player.isSpectator)
        //            {
        //                if (player.machine.isSimulating && !player.machine.LocalSim && player.machine.PlayerID != rocket.ParentMachine.PlayerID)
        //                {
        //                    if (rocket.Team == MPTeam.None || rocket.Team != player.team)
        //                    {
        //                        simClusters.UnionWith(player.machine.simClusters);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        simClusters.UnionWith(Machine.Active().simClusters);
        //        simClusters.ExceptWith(clustersInSafetyRange);
        //    }

        //    //Iternating the list to find the cluster that satisfy the conditions
        //    if (!targetAquired && !targetHit && simClusters.Count > 0)
        //    {
        //        try
        //        {
        //            // Remove any null cluster due to stopped simulation
        //            simClusters.RemoveWhere(cluster => cluster == null);
        //            simClusters.ExceptWith(explodedCluster);

        //            HashSet<Machine.SimCluster> simClusterForSearch = new HashSet<Machine.SimCluster>(simClusters);
        //            HashSet<Machine.SimCluster> unwantedClusters = new HashSet<Machine.SimCluster>();

        //            foreach (var cluster in simClusters)
        //            {
        //                bool inRange = CheckInRange(cluster.Base);
        //                bool skipCluster = !(inRange) || ShouldSkipCluster(cluster.Base);

        //                if (!skipCluster)
        //                {
        //                    foreach (var block in cluster.Blocks)
        //                    {
        //                        if (/*block.Type*/block.BlockID == (int)BlockType.Rocket)
        //                        {
        //                            continue;
        //                        }
        //                        skipCluster = ShouldSkipCluster(block);
        //                        if (skipCluster)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (skipCluster)
        //                {
        //                    unwantedClusters.Add(cluster);
        //                }
        //            }

        //            simClusterForSearch.ExceptWith(unwantedClusters);

        //            if (simClusterForSearch.Count > 0)
        //            {
        //                GetMostValuableCluster(simClusterForSearch, out clusterTarget, out clusterValue);
        //            }
        //        }
        //        catch { }
        //    }
        //    if (rocketTarget != null || clusterTarget != null)
        //    {
        //        target = rocketValue >= clusterValue ? rocketTarget : clusterTarget;
        //        targetCollider = target.gameObject.GetComponentInChildren<Collider>(true);
        //        targetAquired = true;
        //        searchStarted = false;
        //        previousVelocity = acceleration = Vector3.zero;
        //        initialDistance = (target.position - rocket.transform.position).magnitude;
        //        targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
        //        SendTargetToClient();
        //    }
        //    yield return null;
        //}

        //private bool CheckInRange(BlockBehaviour target)
        //{
        //    Vector3 positionDiff = target.gameObject.transform.position - rocket.transform.position;
        //    float angleDiff = Vector3.Angle(positionDiff.normalized, rocket.transform.up);
        //    bool forward = Vector3.Dot(positionDiff, rocket.transform.up) > 0;
        //    return forward && angleDiff < searchAngle;
        //}

        //private void GetMostValuableCluster(HashSet<Machine.SimCluster> simClusterForSearch, out Transform targetTransform, out float targetClusterValue)
        //{
        //    //Remove any null cluster
        //    simClusterForSearch.RemoveWhere(cluster => cluster == null);

        //    //Search for any blocks within the search radius for every block in the hitlist
        //    float[] targetValue = new float[simClusterForSearch.Count];
        //    Machine.SimCluster[] clusterArray = new Machine.SimCluster[simClusterForSearch.Count];
        //    List<Machine.SimCluster> maxClusters = new List<Machine.SimCluster>();

        //    //Start searching
        //    int i = 0;
        //    foreach (var simCluster in simClusterForSearch)
        //    {
        //        float clusterValue = simCluster.Blocks.Length + 1;
        //        clusterValue = CalculateClusterValue(simCluster.Base, clusterValue);
        //        foreach (var block in simCluster.Blocks)
        //        {
        //            clusterValue = CalculateClusterValue(block, clusterValue);
        //        }
        //        targetValue[i] = clusterValue;
        //        clusterArray[i] = simCluster;
        //        i++;
        //    }
        //    //Find the block that has the max number of blocks around it
        //    //If there are multiple withh the same highest value, randomly return one of them
        //    float maxValue = targetValue.Max();
        //    for (i = 0; i < targetValue.Length; i++)
        //    {
        //        if (targetValue[i] == maxValue)
        //        {
        //            maxClusters.Add(clusterArray[i]);
        //        }
        //    }

        //    int closestIndex = 0;
        //    float distanceMin = Mathf.Infinity;

        //    for (i = 0; i < maxClusters.Count; i++)
        //    {
        //        float distanceCurrent = (maxClusters[i].Base.gameObject.transform.position - rocket.transform.position).magnitude;
        //        if (distanceCurrent < distanceMin)
        //        {
        //            closestIndex = i;
        //            distanceMin = distanceCurrent;
        //        }
        //    }

        //    targetTransform = maxClusters[closestIndex].Base.gameObject.transform;
        //    targetClusterValue = maxValue;
        //}

        private void AddAerodynamicsToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(rocketRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * aeroEffectMultiplier;
            float velocitySqr = rocketRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            //rocketRigidbody.AddRelativeForce(Vector3.Scale(dir, -locVel) * currentVelocitySqr);

            Vector3 force = transform.localToWorldMatrix * Vector3.Scale(dir, -locVel) * currentVelocitySqr;
            rocketRigidbody.AddForceAtPosition(force, rocket.transform.position - aeroEffectPosition);
        }

        //private float CalculateClusterValue(BlockBehaviour block, float clusterValue)
        //{
        //    //Some blocks weights more than others
        //    GameObject targetObj = block.gameObject;
        //    //A bomb
        //    if (/*block.Type*/block.BlockID == (int)BlockType.Bomb)
        //    {
        //        if (!targetObj.GetComponent<ExplodeOnCollideBlock>().hasExploded)
        //        {
        //            clusterValue *= bombValue;
        //        }
        //    }
        //    //A watering watercannon
        //    if (/*block.Type*/block.BlockID == (int)BlockType.WaterCannon)
        //    {
        //        if (targetObj.GetComponent<WaterCannonController>().isActive)
        //        {
        //            clusterValue *= waterCannonValue;
        //        }
        //    }
        //    //A flying flying-block
        //    if (/*block.Type*/block.BlockID == (int)BlockType.FlyingBlock)
        //    {
        //        if (targetObj.GetComponent<FlyingController>().canFly)
        //        {
        //            clusterValue *= flyingBlockValue;
        //        }
        //    }
        //    //A flaming flamethrower
        //    if (/*block.Type*/block.BlockID == (int)BlockType.Flamethrower)
        //    {
        //        if (targetObj.GetComponent<FlamethrowerController>().isFlaming)
        //        {
        //            clusterValue *= flameThrowerValue;
        //        }
        //    }
        //    //A spinning wheel/cog
        //    if (targetObj.GetComponent<CogMotorControllerHinge>())
        //    {
        //        if (targetObj.GetComponent<CogMotorControllerHinge>().Velocity != 0)
        //        {
        //            clusterValue *= cogMotorValue;
        //        }
        //    }
        //    return clusterValue;
        //}

        //private bool ShouldSkipCluster(BlockBehaviour block)
        //{
        //    try
        //    {
        //        if (/*block.Type*/block.BlockID == (int)BlockType.Rocket)
        //        {
        //            if (!block.gameObject.activeInHierarchy)
        //            {
        //                return true;
        //            }
        //            if (block.gameObject.GetComponent<TimedRocket>().hasExploded)
        //            {
        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            if (block.fireTag.burning)
        //            {
        //                return true;
        //            }
        //            try
        //            {
        //                if (block.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
        //                {
        //                    return true;
        //                }
        //            }
        //            catch { }
        //            try
        //            {
        //                if (block.gameObject.GetComponent<ControllableBomb>().hasExploded)
        //                {
        //                    return true;
        //                }
        //            }
        //            catch { }
        //        }
        //    }
        //    catch { }
        //    return false;
        //}

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
            if (MarkTarget)
            {
                //if (target != null && targetCollider != null && !rocket.hasExploded && rocket.isSimulating && rocket != null)
                //{
                //    Vector3 markerPosition = targetCollider.bounds != null ? targetCollider.bounds.center : target.position;
                //    if (Vector3.Dot(Camera.main.transform.forward, markerPosition - Camera.main.transform.position) > 0)
                //    {
                //        int squareWidth = 16;
                //        Vector3 itemScreenPosition = Camera.main.WorldToScreenPoint(markerPosition);
                //        GUI.DrawTexture(new Rect(itemScreenPosition.x - squareWidth / 2, Camera.main.pixelHeight - itemScreenPosition.y - squareWidth / 2, squareWidth, squareWidth), rocketAim);
                //    }
                //}
            }
        }

        public void SendRocketFired()
        {
            if (!removedFromGroup)
            {
                if (StatMaster.isHosting)
                {
                    Message rocketFiredMsg = Messages.rocketFiredMsg.CreateMessage(BB);
                    ModNetworking.SendTo(Player.GetAllPlayers().Find(player => player.NetworkId == rocket.ParentMachine.PlayerID), rocketFiredMsg);
                }
                if (RocketsController.Instance.playerGroupedRockets.TryGetValue(StatMaster.isMP ? rocket.ParentMachine.PlayerID : 0, out Dictionary<KeyCode, HashSet<TimedRocket>> groupedRockets))
                {
                    if (groupedRockets.TryGetValue(GroupFireKey.GetKey(0), out HashSet<TimedRocket> rockets))
                    {
                        rockets.Remove(rocket);
                    }
                }
                removedFromGroup = true;
            }

        }

        //public void SendTargetToClient()
        //{
        //    if (StatMaster.isHosting)
        //    {
        //        //if (target != null)
        //        //{
        //        //    if (target.gameObject.GetComponent<BlockBehaviour>())
        //        //    {
        //        //        BlockBehaviour targetBB = target.gameObject.GetComponent<BlockBehaviour>();
        //        //        int id = targetBB.ParentMachine.PlayerID;
        //        //        if (rocket.ParentMachine.PlayerID != 0)
        //        //        {
        //        //            Message targetBlockBehaviourMsg = Messages.rocketTargetBlockBehaviourMsg.CreateMessage(targetBB, BB);
        //        //            foreach (var player in Player.GetAllPlayers())
        //        //            {
        //        //                if (player.NetworkId == rocket.ParentMachine.PlayerID)
        //        //                {
        //        //                    ModNetworking.SendTo(player, targetBlockBehaviourMsg);
        //        //                }
        //        //            }
        //        //        }
        //        //        ModNetworking.SendToAll(Messages.rocketLockOnMeMsg.CreateMessage(BB, id));
        //        //        RocketsController.Instance.UpdateRocketTarget(BB, id);
        //        //    }
        //        //    if (target.gameObject.GetComponent<LevelEntity>())
        //        //    {
        //        //        Message targetEntityMsg = Messages.rocketTargetEntityMsg.CreateMessage(target.gameObject.GetComponent<LevelEntity>(), BB);
        //        //        foreach (var player in Player.GetAllPlayers())
        //        //        {
        //        //            if (player.NetworkId == rocket.ParentMachine.PlayerID)
        //        //            {
        //        //                ModNetworking.SendTo(player, targetEntityMsg);
        //        //            }
        //        //        }
        //        //        ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(BB));
        //        //        RocketsController.Instance.RemoveRocketTarget(BB);
        //        //    }
        //        //}
        //    }
        //}



        //private void SendRayToHost(Ray ray)
        //{
        //    Message rayToHostMsg = Messages.rocketRayToHostMsg.CreateMessage(ray.origin, ray.direction, BB);
        //    ModNetworking.SendToHost(rayToHostMsg);
        //}

        private void SendExplosionPositionToAll(Vector3 position)
        {
            if (StatMaster.isHosting)
            {
                Message explosionPositionMsg = Messages.rocketHighExploPosition.CreateMessage(position, bombExplosiveCharge);
                ModNetworking.SendToAll(explosionPositionMsg);
            }
        }
    }





}
