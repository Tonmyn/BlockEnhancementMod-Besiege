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
        public MSlider GroupFireRateSlider;
        public MToggle AutoGrabberReleaseToggle;
        public TimedRocket rocket;
        public Rigidbody rocketRigidbody;

        public float TrailSmokeEmissionConstant { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Emission Constant"); } }
        public float TrailSmokeLifetime { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Lifetime"); } }
        public float TrailSmokeSize { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Size"); } }

        public bool removedFromGroup = false;

        //No smoke mode related
        MToggle NoSmokeToggle;

        //Firing record related setting
        private float randomDelay = 0;
        private float launchTime = 0f;
        private bool launchTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MToggle GuidedRocketStabilityToggle;
        MSlider GuidePredictionSlider;
        MToggle GuidedRocketShowRadarToggle;
        public bool rocketExploMsgSent = false;
        public bool rocketInBuildSent = false;

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey ManualOverrideKey;
        public MKey SPTeamKey;
        MMenu RadarTypeMenu;
        public float searchAngle = 60f;
        private readonly float maxSearchAngleNormal = 90f;
        private readonly float maxSearchAngleNo8 = 175f;
        private float searchRange = 0;
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
            GuidedRocketToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.TrackTarget, "TrackingRocket", false);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                RadarTypeMenu.DisplayInMapper =
                GuidedRocketTorqueSlider.DisplayInMapper =
                GuidePredictionSlider.DisplayInMapper =
                GuidedRocketShowRadarToggle.DisplayInMapper =
                ImpactFuzeToggle.DisplayInMapper =
                ProximityFuzeToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                ManualOverrideKey.DisplayInMapper =
                SPTeamKey.DisplayInMapper =
                ActiveGuideRocketSearchAngleSlider.DisplayInMapper =
                GuideDelaySlider.DisplayInMapper =
                GuidedRocketStabilityToggle.DisplayInMapper =
                NoSmokeToggle.DisplayInMapper =
                value;
                ChangedProperties();
            };

            RadarTypeMenu = AddMenu("Radar Type", 0, LanguageManager.Instance.CurrentLanguage.RadarType);

            AutoGrabberReleaseToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.AutoGrabberRelease, "AutoGrabberRelease", false);

            GuidedRocketShowRadarToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ShowRadar, "ShowRadar", false);

            ImpactFuzeToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ImpactFuze, "ImpactFuze", false);

            ProximityFuzeToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ProximityFuze, "ProximityFuze", false);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };

            NoSmokeToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.NoSmoke, "NoSmoke", false);

            HighExploToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.HighExplo, "HighExplo", false);

            ActiveGuideRocketSearchAngleSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.SearchAngle, "searchAngle", 60f, 0, maxSearchAngleNormal);

            GuidePredictionSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.Prediction, "prediction", 10, 0, 50);

            ProximityFuzeRangeSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.CloseRange, "closeRange", 0f, 0, 10f);

            GuidedRocketTorqueSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.TorqueOnRocket, "torqueOnRocket", 100f, 0, 100f);

            GuidedRocketStabilityToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.RocketStability, "RocketStabilityOn", true);

            GuideDelaySlider = AddSlider(LanguageManager.Instance.CurrentLanguage.GuideDelay, "guideDelay", 0f, 0, 2);

            LockTargetKey = AddKey(LanguageManager.Instance.CurrentLanguage.LockTarget, "lockTarget", KeyCode.Delete);

            GroupFireKey = AddKey(LanguageManager.Instance.CurrentLanguage.GroupedFire, "groupFire", KeyCode.None);

            GroupFireRateSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.GroupFireRate, "groupFireRate", 0.25f, 0.1f, 1f);

            ManualOverrideKey = AddKey(LanguageManager.Instance.CurrentLanguage.ManualOverride, "ActiveSearchKey", KeyCode.RightShift);

            SPTeamKey = AddKey(LanguageManager.Instance.CurrentLanguage.SinglePlayerTeam, "SinglePlayerTeam", KeyCode.None);

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            rocketRigidbody = gameObject.GetComponent<Rigidbody>();

#if DEBUG
            ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GroupFireRateSlider.DisplayInMapper = value && (GroupFireKey.KeysCount > 0 || GroupFireKey.GetKey(0) != KeyCode.None);
            AutoGrabberReleaseToggle.DisplayInMapper = value && (GroupFireKey.KeysCount > 0 || GroupFireKey.GetKey(0) != KeyCode.None);

            var _value = value && GuidedRocketToggle.IsActive;
            var _value1 = _value && (RadarTypeMenu.Value == (int)RadarScript.RadarTypes.ActiveRadar);

            ManualOverrideKey.DisplayInMapper = _value1;
            SPTeamKey.DisplayInMapper = _value && (!StatMaster.isMP || Playerlist.Players.Count == 1);
            RadarTypeMenu.DisplayInMapper = _value;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = _value1;
            GuidePredictionSlider.DisplayInMapper = _value;
            GuidedRocketTorqueSlider.DisplayInMapper = _value;
            GuidedRocketShowRadarToggle.DisplayInMapper = _value1;
            GuidedRocketStabilityToggle.DisplayInMapper = _value;
            ImpactFuzeToggle.DisplayInMapper = _value;
            ProximityFuzeToggle.DisplayInMapper = _value;
            ProximityFuzeRangeSlider.DisplayInMapper = _value;
            GuideDelaySlider.DisplayInMapper = _value;
            LockTargetKey.DisplayInMapper = _value1;
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            rocketInBuildSent = removedFromGroup = false;

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
                launchTimeRecorded = bombHasExploded = rocketExploMsgSent = false;
                searchAngle = Mathf.Clamp(ActiveGuideRocketSearchAngleSlider.Value, 0, EnhanceMore ? maxSearchAngleNo8 : maxSearchAngleNormal);
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
                guideController.Setup(rocket, rocketRigidbody, radar, searchAngle, Mathf.Clamp(GuidedRocketTorqueSlider.Value, 0, 100), GuidePredictionSlider.Value);

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

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (gameObject.activeInHierarchy)
            {
                if ((GroupFireKey.IsHeld || GroupFireKey.EmulationHeld()) && !StatMaster.isClient)
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

                    if (GuidedRocketToggle.IsActive)
                    {
                        //When toggle auto aim key is released, change the auto aim status
                        if (ManualOverrideKey.IsReleased || ManualOverrideKey.EmulationReleased())
                        {
                            if (radar.RadarType == RadarScript.RadarTypes.ActiveRadar)
                            {
                                radar.canBeOverridden = !radar.canBeOverridden;
                                if (!radar.canBeOverridden)
                                {
                                    radar.ClearTarget(false);
                                }
                            }
                        }

                        if (LockTargetKey.IsPressed || LockTargetKey.EmulationPressed()/* && radar.Switch*/)
                        {
                            if (radar.RadarType == RadarScript.RadarTypes.ActiveRadar)
                            {
                                if (radar.canBeOverridden)
                                {
                                    radar.SetTargetManual();
                                }
                                else
                                {
                                    //保留重新搜索目标的能力
                                    radar.ClearTarget(false);
                                }
                            }
                        }
                    }
                }

                if (rocket.hasFired)
                {
                    //Let rocket controller know the rocket is fired
                    SendRocketFired();

                    if (!rocket.hasExploded)
                    {
                        if (GuidedRocketToggle.IsActive)
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
                            if (Time.time - launchTime >= guideDelay)
                            {
                                guideController.Switch = true;
                            }

                            //Proximity fuse behaviour
                            if (ProximityFuzeToggle.IsActive)
                            {
                                if (radar.TargetDistance <= proximityRange + 1f)
                                {
                                    StartCoroutine(RocketExplode());
                                }
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
            if (!rocket.hasFired) return;
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

                    if (hit.attachedRigidbody != null && hit.attachedRigidbody && hit.attachedRigidbody != rocket.Rigidbody && hit.attachedRigidbody.gameObject.layer != 20 && hit.attachedRigidbody.gameObject.layer != 22 && hit.attachedRigidbody.tag != "KeepConstraintsAlways" && hit.attachedRigidbody.gameObject.layer != RadarScript.CollisionLayer)
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
