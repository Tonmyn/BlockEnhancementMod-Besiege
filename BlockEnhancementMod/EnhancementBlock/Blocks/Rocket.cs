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
        public MToggle AutoGrabberReleaseToggle;
        //public bool autoGrabberRelease = false;
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
        //private bool smokeStopped = false;

        //Firing record related setting
        private float randomDelay = 0;
        private float launchTime = 0f;
        private bool launchTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MToggle GuidedRocketStabilityToggle;
        MSlider GuidePredictionSlider;
        MToggle GuidedRocketShowRadarToggle;
        //MToggle AsRadarToggle;
        //public bool guidedRocketShowRadar = false;
        //public bool guidedRocketStabilityOn = true;
        //public bool guidedRocketActivated = false;
        public bool rocketExploMsgSent = false;
        public bool rocketInBuildSent = false;
        //public bool asRadar = false;
        public float torque = 100f;
        public float prediction = 10f;

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey ChangedRadarTypeKey;
        public MKey SPTeamKey;
        MMenu RadarTypeMenu;
        //public int radarTypeMenuIndex = 0;
        //public List<string> searchMode = new List<string>() { LanguageManager.Instance.CurrentLanguage.DefaultAuto, LanguageManager.Instance.CurrentLanguage.DefaultManual, LanguageManager.Instance.CurrentLanguage.DefaultPassive };
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

        ParticleSystem smokeTrail;

        public override void SafeAwake()
        {
            //Key mapper setup
            GuidedRocketToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.TrackTarget, "TrackingRocket", /*guidedRocketActivated*/false);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                //guidedRocketActivated =
                //AsRadarToggle.DisplayInMapper =
                RadarTypeMenu.DisplayInMapper =
                GuidedRocketTorqueSlider.DisplayInMapper =
                GuidePredictionSlider.DisplayInMapper =
                GuidedRocketShowRadarToggle.DisplayInMapper =
                ImpactFuzeToggle.DisplayInMapper =
                ProximityFuzeToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                ChangedRadarTypeKey.DisplayInMapper =
                SPTeamKey.DisplayInMapper =
                ActiveGuideRocketSearchAngleSlider.DisplayInMapper =
                GuideDelaySlider.DisplayInMapper =
                GuidedRocketStabilityToggle.DisplayInMapper =
                NoSmokeToggle.DisplayInMapper =
                value;
                ChangedProperties();
            };

            RadarTypeMenu = AddMenu("Radar Type",/* radarTypeMenuIndex*/0, LanguageManager.Instance.CurrentLanguage.RadarType);
            //AsRadarToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.AsRadar, "AsRadar", /*asRadar*/false);

            AutoGrabberReleaseToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.AutoGrabberRelease, "AutoGrabberRelease",/* autoGrabberRelease*/false);
            //AutoGrabberReleaseToggle.Toggled += (bool value) =>
            //{
            //    autoGrabberRelease = value;
            //    ChangedProperties();
            //};

            GuidedRocketShowRadarToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.ShowRadar, "ShowRadar", /*guidedRocketShowRadar*/false);
            //GuidedRocketShowRadarToggle.Toggled += (bool value) =>
            //{
            //    guidedRocketShowRadar = value;
            //    ChangedProperties();
            //};

            ImpactFuzeToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.ImpactFuze, "ImpactFuze", /*impactFuzeActivated*/false);
            //ImpactFuzeToggle.Toggled += (bool value) =>
            //{
            //    impactFuzeActivated = value;
            //    ChangedProperties();
            //};

            ProximityFuzeToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.ProximityFuze, "ProximityFuze", /*proximityFuzeActivated*/false);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };

            NoSmokeToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.NoSmoke, "NoSmoke", /*noSmoke*/false);
            //NoSmokeToggle.Toggled += (bool value) =>
            //{
            //    noSmoke = value;
            //    ChangedProperties();
            //};

            HighExploToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.HighExplo, "HighExplo", /*highExploActivated*/false);
            //HighExploToggle.Toggled += (bool value) =>
            //{
            //    highExploActivated = value;
            //    ChangedProperties();
            //};

            ActiveGuideRocketSearchAngleSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.SearchAngle, "searchAngle", /*searchAngle*/60f, 0, /*maxSearchAngleNormal*/90f);
            //ActiveGuideRocketSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };

            GuidePredictionSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Prediction, "prediction", /*prediction*/10, 0, 50);
            //GuidePredictionSlider.ValueChanged += (float value) => { prediction = value; ChangedProperties(); };

            ProximityFuzeRangeSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.CloseRange, "closeRange", /*proximityRange*/0f, 0, 10f);
            //ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };

            GuidedRocketTorqueSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.TorqueOnRocket, "torqueOnRocket", /*torque*/100f, 0, 100f);
            //GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };

            GuidedRocketStabilityToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.RocketStability, "RocketStabilityOn", /*guidedRocketStabilityOn*/true);
            //GuidedRocketStabilityToggle.Toggled += (bool value) => { guidedRocketStabilityOn = value; ChangedProperties(); };

            GuideDelaySlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.GuideDelay, "guideDelay", /*guideDelay*/0f, 0, 2);
            //GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };

            LockTargetKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.LockTarget, "lockTarget", KeyCode.Delete);
            //LockTargetKey.InvokeKeysChanged();

            GroupFireKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.GroupedFire, "groupFire", KeyCode.None);
            //GroupFireKey.InvokeKeysChanged();

            GroupFireRateSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.GroupFireRate, "groupFireRate", /*groupFireRate*/0.25f, 0.1f, 1f);
            //GroupFireRateSlider.ValueChanged += (float value) => { groupFireRate = value; ChangedProperties(); };

            ChangedRadarTypeKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.SwitchGuideMode, "ActiveSearchKey", KeyCode.RightShift);
            //ChangedRadarTypeKey.InvokeKeysChanged();

            SPTeamKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.SinglePlayerTeam, "SinglePlayerTeam", KeyCode.None);
            //SPTeamKey.InvokeKeysChanged();

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            rocketRigidbody = gameObject.GetComponent<Rigidbody>();

#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            //GuidedRocketToggle.DisplayInMapper = value;
            //HighExploToggle.DisplayInMapper = value;
            //NoSmokeToggle.DisplayInMapper = value;
            //GroupFireKey.DisplayInMapper = value;
            GroupFireRateSlider.DisplayInMapper = value && (GroupFireKey.KeysCount > 0 || GroupFireKey.GetKey(0) != KeyCode.None);
            AutoGrabberReleaseToggle.DisplayInMapper = value && (GroupFireKey.KeysCount > 0 || GroupFireKey.GetKey(0) != KeyCode.None);

            var _value = value && GuidedRocketToggle.IsActive;
            var _value1 = _value && (RadarTypeMenu.Value == (int)RadarScript.RadarTypes.ActiveRadar);

            ChangedRadarTypeKey.DisplayInMapper = _value;
            SPTeamKey.DisplayInMapper = _value && (!StatMaster.isMP || Playerlist.Players.Count == 1);
            RadarTypeMenu.DisplayInMapper = _value;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = _value1;
            GuidePredictionSlider.DisplayInMapper = _value1;
            GuidedRocketTorqueSlider.DisplayInMapper = _value1;
            GuidedRocketShowRadarToggle.DisplayInMapper = _value1;
            GuidedRocketStabilityToggle.DisplayInMapper = _value1;
            ImpactFuzeToggle.DisplayInMapper = _value;
            ProximityFuzeToggle.DisplayInMapper = _value;
            ProximityFuzeRangeSlider.DisplayInMapper = _value;
            GuideDelaySlider.DisplayInMapper = _value;
            LockTargetKey.DisplayInMapper = _value;

        }

        //public override void BuildingUpdateAlways_EnhancementEnabled()
        //{
        //    if (radarTypeMenuIndex == (int)RadarScript.SearchModes.Passive)
        //    {
        //        if (AsRadarToggle.DisplayInMapper) AsRadarToggle.DisplayInMapper = asRadar = false;

        //        if (ChangedRadarTypeKey.DisplayInMapper) ChangedRadarTypeKey.DisplayInMapper = false;

        //        if (ActiveGuideRocketSearchAngleSlider.DisplayInMapper) ActiveGuideRocketSearchAngleSlider.DisplayInMapper = false;

        //        if (GuidedRocketShowRadarToggle.DisplayInMapper) GuidedRocketShowRadarToggle.DisplayInMapper = false;

        //        if (LockTargetKey.DisplayInMapper) LockTargetKey.DisplayInMapper = false;
        //    }
        //    else
        //    {
        //        if (!AsRadarToggle.DisplayInMapper) AsRadarToggle.DisplayInMapper = true;

        //        if (!ChangedRadarTypeKey.DisplayInMapper) ChangedRadarTypeKey.DisplayInMapper = true;

        //        if (!ActiveGuideRocketSearchAngleSlider.DisplayInMapper) ActiveGuideRocketSearchAngleSlider.DisplayInMapper = true;

        //        if (!GuidedRocketShowRadarToggle.DisplayInMapper) GuidedRocketShowRadarToggle.DisplayInMapper = true;

        //        if (!LockTargetKey.DisplayInMapper) LockTargetKey.DisplayInMapper = true;
        //    }

        //    if (GroupFireKey.GetKey(0) == KeyCode.None)
        //    {
        //        if (AutoGrabberReleaseToggle.DisplayInMapper)
        //        {
        //            AutoGrabberReleaseToggle.DisplayInMapper = false;
        //            AutoGrabberReleaseToggle.SetValue(false);
        //        }
        //        if (GroupFireRateSlider.DisplayInMapper)
        //        {
        //            GroupFireRateSlider.DisplayInMapper = false;
        //        }
        //    }
        //    else  /*(GroupFireKey.GetKey(0) != KeyCode.None)*/
        //    {
        //        if (!AutoGrabberReleaseToggle.DisplayInMapper)
        //        {
        //            AutoGrabberReleaseToggle.DisplayInMapper = true;
        //        }
        //        if (!GroupFireRateSlider.DisplayInMapper)
        //        {
        //            GroupFireRateSlider.DisplayInMapper = true;
        //        }
        //    }
        //}

        public override void OnSimulateStart_EnhancementEnabled()
        {
            /*smokeStopped = */
            rocketInBuildSent /*= noLongerActiveSent*/ = removedFromGroup = false;

            // Read the charge from rocket
            explosiveCharge = bombExplosiveCharge = rocket.ChargeSlider.Value;

            // Make sure the high explo mode is not too imba
            if (highExploActivated && !EnhanceMore)
            {
                bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
            }

            if (GuidedRocketToggle.IsActive)
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
                radarObject.transform.localScale = restoreScale(rocket.transform.localScale);
                radar = radarObject.GetComponent<RadarScript>() ?? radarObject.AddComponent<RadarScript>();
                radar.Setup(BB, searchRange, searchAngle, RadarTypeMenu.Value, GuidedRocketShowRadarToggle.IsActive);
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
            }

            smokeTrail = null;
            if (NoSmokeToggle.IsActive)
            {
                foreach (var value in rocket.trail)
                {
                    var emission = value.emission;
                    emission.enabled = false;
                }
            }
            else
            {
                foreach (var value in rocket.trail)
                {
                    if (value.name.ToLower() == "smoketrail")
                    {
                        smokeTrail = value;
                        break;
                    }
                }
            }


            Vector3 restoreScale(Vector3 rocketScale)
            {
                var single = 1f / rocketScale.x;
                var single1 = 1f / rocketScale.y;
                var single2 = 1f / rocketScale.z;

                return new Vector3(single, single1, single2);
            }

            //Initialise Dict in RocketsController
            if (GroupFireKey.GetKey(0) != KeyCode.None)
            {
                if (/*asRadar*/radar.RadarType == RadarScript.RadarTypes.PassiveRadar)
                {
                    if (!RocketsController.Instance.playerGroupedRadars.ContainsKey(rocket.ParentMachine.PlayerID))
                    {
                        RocketsController.Instance.playerGroupedRadars.Add(rocket.ParentMachine.PlayerID, new Dictionary<KeyCode, HashSet<RadarScript>>());
                    }
                    if (!RocketsController.Instance.playerGroupedRadars[rocket.ParentMachine.PlayerID].ContainsKey(GroupFireKey.GetKey(0)))
                    {
                        RocketsController.Instance.playerGroupedRadars[rocket.ParentMachine.PlayerID].Add(GroupFireKey.GetKey(0), new HashSet<RadarScript>());
                    }
                    if (!RocketsController.Instance.playerGroupedRadars[rocket.ParentMachine.PlayerID][GroupFireKey.GetKey(0)].Contains(radar))
                    {
                        RocketsController.Instance.playerGroupedRadars[rocket.ParentMachine.PlayerID][GroupFireKey.GetKey(0)].Add(radar);
                    }
                }
                else
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
            }
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (gameObject.activeInHierarchy)
            {
                if ((GroupFireKey.IsHeld || GroupFireKey.EmulationHeld()) && /*!asRadar*/!(radar.RadarType == RadarScript.RadarTypes.PassiveRadar) && !StatMaster.isClient)
                {
                    if (!RocketsController.Instance.launchStarted)
                    {
                        StartCoroutine(RocketsController.Instance.LaunchRocketFromGroup(rocket.ParentMachine.PlayerID, GroupFireKey.GetKey(0)));
                    }
                }

                if (radar != null)
                {
                    if (radar.RadarType == RadarScript.RadarTypes.ActiveRadar)
                    {
                        radar.Switch = rocket.hasFired;
                    }

                    if (/*guidedRocketActivated*/GuidedRocketToggle.IsActive)
                    {
                        //When toggle auto aim key is released, change the auto aim status
                        if (ChangedRadarTypeKey.IsReleased || ChangedRadarTypeKey.EmulationReleased())
                        {
                            //activeGuide = !activeGuide;
                            //radar.SearchMode = activeGuide ? RadarScript.SearchModes.Auto : RadarScript.SearchModes.Manual;
                            //radar.ChangeSearchMode();
                            if (radar.RadarType == RadarScript.RadarTypes.ActiveRadar)
                            {
                                radar.ChangeRadarType(RadarScript.RadarTypes.PassiveRadar);
                            }
                            else
                            {
                                radar.ChangeRadarType(RadarScript.RadarTypes.ActiveRadar);
                            }
                        }

                        if (LockTargetKey.IsPressed || LockTargetKey.EmulationPressed()/* && radar.Switch*/)
                        {
                            if (radar.RadarType == RadarScript.RadarTypes.PassiveRadar)
                            {
                                radar.SetTargetManual();
                            }

                            //if (/*radar.SearchMode == RadarScript.SearchModes.Auto*/radar.RadarType == RadarScript.RadarTypes.ActiveRadar)
                            //{
                            //    radar.ClearTargetNoRemoval();
                            //}
                            //else
                            //{
                            //    radar.SetTargetManual();
                            //}

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

                    //Let rocket controller know the rocket is fired
                    SendRocketFired();

                    if (!rocket.hasExploded)
                    {
                        //If no smoke mode is enabled, stop all smoke
                        //if (noSmoke && !smokeStopped)
                        //{
                        //    try
                        //    {
                        //        foreach (var smoke in rocket.trail)
                        //        {
                        //            smoke.Stop();
                        //        }
                        //        smokeStopped = true;
                        //    }
                        //    catch { }
                        //}

                        if (GuidedRocketToggle.IsActive /*guidedRocketActivated*/)
                        {
                            //Activate aerodynamic effect
                            guideController.enableAerodynamicEffect = GuidedRocketStabilityToggle.IsActive;

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
                            if (ProximityFuzeToggle.IsActive /*proximityFuzeActivated*/ /*&& canTrigger*/)
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

                    if (HighExploToggle.IsActive)
                    {
                        StartCoroutine(RocketExplode());
                    }

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
                    var em = smokeTrail.emission;
                    var r = em.rate;
                    r.constant = TrailSmokeEmissionConstant;
                    em.rate = r;
                    smokeTrail.startLifetime = TrailSmokeLifetime;
                    smokeTrail.startSize = TrailSmokeSize;
                }
            }
        }

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

                    if (hit.attachedRigidbody != null && hit.attachedRigidbody && hit.attachedRigidbody != rocket.Rigidbody && /*!rocket.prevRigidbodies.Contains(hit.attachedRigidbody) &&*/ hit.attachedRigidbody.gameObject.layer != 20 && hit.attachedRigidbody.gameObject.layer != 22 && hit.attachedRigidbody.tag != "KeepConstraintsAlways" && hit.attachedRigidbody.gameObject.layer != RadarScript.CollisionLayer)
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
                            RocketScript rocketScript = hit.attachedRigidbody.gameObject.GetComponent<RocketScript>();
                            rocketScript.StartCoroutine(rocketScript.RocketExplode());
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
