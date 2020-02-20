using Modding;
using Modding.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public TimedRocket rocket;
        public Rigidbody rocketRigidbody;
        //public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        public float TrailSmokeEmissionConstant { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Emission Constant"); } }
        public float TrailSmokeLifetime { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Lifetime"); } }
        public float TrailSmokeSize { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Size"); } }

        public bool removedFromGroup = false;

        //No smoke mode related
        MToggle NoSmokeToggle;
        private bool noSmoke = false;
        private bool smokeStopped = false;

        //Firing record related setting
        private float randomDelay = 0;
        private float launchTime = 0f;
        private bool launchTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MToggle GuidedRocketStabilityToggle;
        MSlider GuidePredictionSlider;
        MToggle GuidedRocketShowRadar;
        public bool guidedRocketShowRadar = false;
        public bool guidedRocketStabilityOn = true;
        public bool guidedRocketActivated = false;
        public bool rocketExploMsgSent = false;
        public bool rocketInBuildSent = false;
        public float torque = 100f;
        public float prediction = 10f;

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey SwitchGuideModeKey;
        public MKey SPTeamKey;
        MMenu DefaultSearchModeMenu;
        private int searchModeIndex = 0;
        public List<string> searchMode = new List<string>() { LanguageManager.Instance.CurrentLanguage.DefaultAuto, LanguageManager.Instance.CurrentLanguage.DefaultManual };
        //public List<KeyCode> switchGuideModeKey = new List<KeyCode> { KeyCode.RightShift };
        //public List<KeyCode> singlePlayerTeamKey = new List<KeyCode> { KeyCode.None };
        public float searchAngle = 60f;

        //private readonly float safetyRadiusAuto = 50f;
        private readonly float maxSearchAngleNormal = 90f;
        private readonly float maxSearchAngleNo8 = 175f;
        private float searchRange = 0;
        //public bool activeGuide = true;
        public GameObject radarObject;
        public RadarScript radar;
        public GameObject guideObject;
        public GuideController guideController;

        //impact & proximity fuze related setting
        MToggle ImpactFuzeToggle;
        MToggle ProximityFuzeToggle;
        MSlider ProximityFuzeRangeSlider;
        public bool impactFuzeActivated = false;
        public bool proximityFuzeActivated = false;
        public float proximityRange = 0f;
        public float triggerForceImpactFuzeOn = 50f;
        public float triggerForceImpactFuzeOff = 400f;

        //Guide delay related setting
        MSlider GuideDelaySlider;
        public float guideDelay = 0f;
        //private bool canTrigger = false;

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

        ParticleSystem st;

        public override void SafeAwake()
        {
            //Key mapper setup
            GuidedRocketToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.TrackTarget, "TrackingRocket", guidedRocketActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketActivated =
                DefaultSearchModeMenu.DisplayInMapper =
                GuidedRocketTorqueSlider.DisplayInMapper =
                GuidePredictionSlider.DisplayInMapper =
                GuidedRocketShowRadar.DisplayInMapper =
                ImpactFuzeToggle.DisplayInMapper =
                ProximityFuzeToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                SwitchGuideModeKey.DisplayInMapper =
                SPTeamKey.DisplayInMapper =
                ActiveGuideRocketSearchAngleSlider.DisplayInMapper =
                GuideDelaySlider.DisplayInMapper =
                GuidedRocketStabilityToggle.DisplayInMapper =
                NoSmokeToggle.DisplayInMapper =
                value;
                ChangedProperties();
            };

            DefaultSearchModeMenu = BB.AddMenu(LanguageManager.Instance.CurrentLanguage.SearchMode, searchModeIndex, searchMode, false);
            DefaultSearchModeMenu.ValueChanged += (int value) =>
            {
                searchModeIndex = value;
                ChangedProperties();
            };

            AutoGrabberReleaseToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.AutoGrabberRelease, "AutoGrabberRelease", autoGrabberRelease);
            AutoGrabberReleaseToggle.Toggled += (bool value) =>
            {
                autoGrabberRelease = value;
                ChangedProperties();
            };

            GuidedRocketShowRadar = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.ShowRadar, "ShowRadar", guidedRocketShowRadar);
            GuidedRocketShowRadar.Toggled += (bool value) =>
            {
                guidedRocketShowRadar = value;
                ChangedProperties();
            };

            ImpactFuzeToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.ImpactFuze, "ImpactFuze", impactFuzeActivated);
            ImpactFuzeToggle.Toggled += (bool value) =>
            {
                impactFuzeActivated = value;
                ChangedProperties();
            };

            ProximityFuzeToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.ProximityFuze, "ProximityFuze", proximityFuzeActivated);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };

            NoSmokeToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.NoSmoke, "NoSmoke", noSmoke);
            NoSmokeToggle.Toggled += (bool value) =>
            {
                noSmoke = value;
                ChangedProperties();
            };

            HighExploToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.HighExplo, "HighExplo", highExploActivated);
            HighExploToggle.Toggled += (bool value) =>
            {
                highExploActivated = value;
                ChangedProperties();
            };

            ActiveGuideRocketSearchAngleSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.SearchAngle, "searchAngle", searchAngle, 0, maxSearchAngleNormal);
            ActiveGuideRocketSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };

            GuidePredictionSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.Prediction, "prediction", prediction, 0, 50);
            GuidePredictionSlider.ValueChanged += (float value) => { prediction = value; ChangedProperties(); };

            ProximityFuzeRangeSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.CloseRange, "closeRange", proximityRange, 0, 10);
            ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };

            GuidedRocketTorqueSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.TorqueOnRocket, "torqueOnRocket", torque, 0, 100);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };

            GuidedRocketStabilityToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.RocketStability, "RocketStabilityOn", guidedRocketStabilityOn);
            GuidedRocketStabilityToggle.Toggled += (bool value) => { guidedRocketStabilityOn = value; ChangedProperties(); };

            GuideDelaySlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.GuideDelay, "guideDelay", guideDelay, 0, 2);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };

            LockTargetKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.LockTarget, "lockTarget", KeyCode.Delete);
            LockTargetKey.InvokeKeysChanged();

            GroupFireKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.GroupedFire, "groupFire", KeyCode.None);
            GroupFireKey.InvokeKeysChanged();

            GroupFireRateSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.GroupFireRate, "groupFireRate", groupFireRate, 0.1f, 1f);
            GroupFireRateSlider.ValueChanged += (float value) => { groupFireRate = value; ChangedProperties(); };

            SwitchGuideModeKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.SwitchGuideMode, "ActiveSearchKey", KeyCode.RightShift);
            SwitchGuideModeKey.InvokeKeysChanged();

            SPTeamKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.SinglePlayerTeam, "SinglePlayerTeam", KeyCode.None);
            SPTeamKey.InvokeKeysChanged();

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
            SPTeamKey.DisplayInMapper = value && guidedRocketActivated && (!StatMaster.isMP || Playerlist.Players.Count == 1);
            DefaultSearchModeMenu.DisplayInMapper = value && guidedRocketActivated;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidePredictionSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketShowRadar.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketStabilityToggle.DisplayInMapper = value && guidedRocketActivated;
            ImpactFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated;
        }

        public override void BuildingUpdateAlways_EnhancementEnabled()
        {

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
            smokeStopped = rocketInBuildSent /*= noLongerActiveSent*/ = removedFromGroup = false;
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
                launchTimeRecorded /*= canTrigger*/  = bombHasExploded = rocketExploMsgSent = false;
                //activeGuide = (searchModeIndex == 0);
                searchAngle = Mathf.Clamp(searchAngle, 0, EnhanceMore ? maxSearchAngleNo8 : maxSearchAngleNormal);
                searchRange = EnhanceMore ? 5000f : 2000f;

                //Add radar
                Collider[] selfColliders = rocket.gameObject.GetComponentsInChildren<Collider>();
                radarObject = new GameObject("RocketRadar");
                radarObject.transform.SetParent(rocket.transform);
                radarObject.transform.position = transform.position;
                radarObject.transform.rotation = transform.rotation;
                radarObject.transform.localPosition = Vector3.forward * 0.5f;
                radarObject.transform.localScale = Vector3.one;
                radar = radarObject.GetComponent<RadarScript>() ?? radarObject.AddComponent<RadarScript>();
                radar.Setup(BB, searchRange, searchAngle, searchModeIndex, guidedRocketShowRadar);
                //radar.parentBlock = BB;

                //Workaround when radar can be ignited hence explode the rocket
                FireTag fireTag = radarObject.AddComponent<FireTag>();
                fireTag.enabled = true;
                Rigidbody rigidbody = radarObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.mass = 0.0001f;
                rigidbody.drag = 0f;

                //Initialise radar at the start of simulation
                //radar.searchAngle = searchAngle;
                //radar.CreateFrustumCone(searchRange);
                //radar.showRadar = guidedRocketShowRadar;
                //radar.ClearSavedSets();

                //Stop colliding with its own colliders
                if (selfColliders.Length > 0)
                {
                    foreach (var collider in selfColliders)
                    {
                        Physics.IgnoreCollision(collider, radar.meshCollider, true);
                    }
                }

                //If Local play, get blocks in the safety range.
                //if (!StatMaster.isMP)
                //{
                //    radar.GetBlocksInSafetyRange();
                //}

                //Set up Guide controller
                guideObject = new GameObject("GuideController");
                guideObject.transform.SetParent(rocket.transform);
                guideObject.transform.position = transform.position;
                guideObject.transform.rotation = transform.rotation;
                guideObject.transform.localScale = Vector3.one;
                guideController = guideObject.GetComponent<GuideController>() ?? guideObject.AddComponent<GuideController>();
                guideController.Setup(rocket, rocketRigidbody, radar, searchAngle, Mathf.Clamp(torque, 0, 100), prediction);

                //previousVelocity = acceleration = Vector3.zero;
                randomDelay = UnityEngine.Random.Range(0f, 0.1f);

                StopAllCoroutines();

                // Read the charge from rocket
                explosiveCharge = bombExplosiveCharge = rocket.ChargeSlider.Value;

                // Make sure the high explo mode is not too imba
                if (highExploActivated && !EnhanceMore)
                {
                    bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
                }
            }

            /*ParticleSystem*/ st = null;
            foreach (var value in rocket.trail)
            {
                if (value.name.ToLower() == "smoketrail")
                {
                    st = value;
                    break;
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

                if (radar != null)
                {
                    radar.Switch = rocket.hasFired;

                    if (guidedRocketActivated)
                    {
                        //When toggle auto aim key is released, change the auto aim status
                        if (SwitchGuideModeKey.IsReleased)
                        {
                            //activeGuide = !activeGuide;
                            //radar.SearchMode = activeGuide ? RadarScript.SearchModes.Auto : RadarScript.SearchModes.Manual;
                            radar.ChangeSearchMode();
                        }

                        if (LockTargetKey.IsPressed/* && radar.Switch*/)
                        {
                            if (radar.SearchMode == RadarScript.SearchModes.Auto)
                            {
                                radar.ClearTargetNoRemoval();
                            }
                            else
                            {
                                radar.SetTargetManual();
                            }
                            //radar.SendClientTargetNull();
                            //radar.ClearTarget();
                            //if (radar.SearchMode == RadarScript.SearchModes.Manual)
                            //{
                            //    radar.SetTargetManual();
                            //}
                            //radar.SetTargetManual();
                        }
                    }
                }

                if (rocket.hasFired)
                {
                    //Activate Detection Zone
                    //if (!radar.Switch /*&& canTrigger*/) /*radar.Switch = true*/;
                    //if (radar.SearchMode == RadarScript.SearchModes.Auto && radar.target == null) radar.Switch = true;


                    //Activate aerodynamic effect
                    guideController.enableAerodynamicEffect = guidedRocketStabilityOn;

                    //Let rocket controller know the rocket is fired
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
                            //Record the launch time for the guide delay
                            if (!launchTimeRecorded)
                            {
                                launchTimeRecorded = true;
                                launchTime = Time.time;

                            }

                            //Rocket can be triggered after the time elapsed after firing is greater than guide delay
                            if (Time.time - launchTime >= guideDelay/* && !canTrigger*/)
                            {
                                //canTrigger = true;
                                guideController.Switch = true;
                            }

                            //Proximity fuse behaviour
                            if (proximityFuzeActivated /*&& canTrigger*/)
                            {
                                //if (radar.target != null)
                                //{
                                //if (radar.target.positionDiff.magnitude <= proximityRange+1f) StartCoroutine(RocketExplode());
                                if (radar.TargetDistance <= proximityRange + 1f)
                                {
                                    StartCoroutine(RocketExplode());
                                }
                                //}
                            }
                        }
                    }
                }
                if (rocket.hasExploded && !rocketExploMsgSent)
                {
                    Destroy(radarObject);
                    Destroy(guideObject);
                    try
                    {
                        if (RocketsController.Instance.playerGroupedRockets.TryGetValue(StatMaster.isMP ? rocket.ParentMachine.PlayerID : 0, out Dictionary<KeyCode, HashSet<TimedRocket>> groupedRockets))
                        {
                            if (groupedRockets.TryGetValue(GroupFireKey.GetKey(0), out HashSet<TimedRocket> rockets))
                            {
                                rockets.Remove(rocket);
                            }
                        }
                    }
                    catch { }
                    rocketExploMsgSent = true;
                }

                if (!NoSmokeToggle.IsActive)
                {
                    var em = st.emission;
                    var r = em.rate;
                    r.constant = TrailSmokeEmissionConstant;
                    em.rate = r;
                    st.startLifetime = TrailSmokeLifetime;
                    st.startSize = TrailSmokeSize;
                }
            }
        }

        //public override void SimulateFixedUpdate_EnhancementEnabled()
        //{
        //    if (gameObject.activeInHierarchy)
        //    {
        //        if (rocket.hasFired)
        //        {
        //            //Activate Detection Zone
        //            if (!radar.Switch && canTrigger) radar.Switch = true;

        //            //Activate aerodynamic effect
        //            guideController.enableAerodynamicEffect = guidedRocketStabilityOn;

        //            //Let rocket controller know the rocket is fired
        //            SendRocketFired();

        //            if (!rocket.hasExploded)
        //            {
        //                //If no smoke mode is enabled, stop all smoke
        //                if (noSmoke && !smokeStopped)
        //                {
        //                    try
        //                    {
        //                        foreach (var smoke in rocket.trail)
        //                        {
        //                            smoke.Stop();
        //                        }
        //                        smokeStopped = true;
        //                    }
        //                    catch { }
        //                }

        //                if (guidedRocketActivated)
        //                {
        //                    //Record the launch time for the guide delay
        //                    if (!launchTimeRecorded)
        //                    {
        //                        launchTimeRecorded = true;
        //                        launchTime = Time.time;

        //                    }

        //                    //Rocket can be triggered after the time elapsed after firing is greater than guide delay
        //                    if (Time.time - launchTime >= guideDelay && !canTrigger)
        //                    {
        //                        canTrigger = true;
        //                    }

        //                    //Proximity fuse behaviour
        //                    if (proximityFuzeActivated && canTrigger)
        //                    {
        //                        if (radar.target != null)
        //                        {
        //                            if (radar.target.positionDiff.magnitude <= proximityRange) StartCoroutine(RocketExplode());
        //                        }

        //                    }
        //                }
        //            }
        //        }
        //        if (rocket.hasExploded && !rocketExploMsgSent)
        //        {
        //            Destroy(radarObject);
        //            Destroy(guideObject);
        //            try
        //            {
        //                if (RocketsController.Instance.playerGroupedRockets.TryGetValue(StatMaster.isMP ? rocket.ParentMachine.PlayerID : 0, out Dictionary<KeyCode, HashSet<TimedRocket>> groupedRockets))
        //                {
        //                    if (groupedRockets.TryGetValue(GroupFireKey.GetKey(0), out HashSet<TimedRocket> rockets))
        //                    {
        //                        rockets.Remove(rocket);
        //                    }
        //                }
        //            }
        //            catch { }
        //            rocketExploMsgSent = true;
        //        }
        //    }
        //}

        private void OnCollisionEnter(Collision collision)
        {
            if (/*!canTrigger*/!rocket.hasFired) return;
            if (rocket.PowerSlider.Value > 0.1f)
            {
                if (collision.impulse.magnitude / Time.fixedDeltaTime >= (impactFuzeActivated ? triggerForceImpactFuzeOn : triggerForceImpactFuzeOff) || collision.gameObject.name.Contains("CanonBall"))
                {
                    StartCoroutine(RocketExplode());
                }
            }
        }

        private IEnumerator RocketExplode()
        {
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

                Collider[] hits = Physics.OverlapSphere(rocket.transform.position, radius * bombExplosiveCharge, Game.BlockEntityLayerMask);
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

                    if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject.layer != 22 && hit.attachedRigidbody.gameObject.layer != RadarScript.CollisionLayer)
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
