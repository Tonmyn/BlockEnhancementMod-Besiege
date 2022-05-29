using Modding;
using Modding.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlockEnhancementMod
{
    class RocketScript : EnhancementBlock
    {
        //General setting
        MToggle GuidedRocketToggle;
        MKey LockTargetKey;
        MMenu SettingMenu;
        public MKey GroupFireKey;
        public MSlider GroupFireRateSlider;
        public MToggle AutoEjectToggle;
        public TimedRocket rocket;
        public Rigidbody rocketRigidbody;

        ParticleSystem smokeTrail;
        //public TrailSmokePropertise trailSmokePropertise = new TrailSmokePropertise
        //{
        //    EmissionConstant = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Emission Constant"),
        //    Lifetime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Lifetime"),
        //    Size = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Size"),
        //    StartColor = BlockEnhancementMod.Configuration.GetValue<Color>("Rocket Smoke Start Color"),
        //    EndColor = BlockEnhancementMod.Configuration.GetValue<Color>("Rocket Smoke End Color"),
        //    StartColorTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Start Color Time"),
        //    EndColorTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke End Color Time"),
        //    StartAlpha = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Start Alpha"),
        //    EndAlpha = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke End Alpha"),
        //    StartAlphaTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Start Alpha Time"),
        //    EndAlphaTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke End Alpha Time"),
        //};
        public static TrailSmokePropertise trailSmokePropertise = new TrailSmokePropertise
        {
            EmissionConstant = BlockEnhancementMod.ModSetting.RocketSmokeEmissionConstant,
            Lifetime = BlockEnhancementMod.ModSetting.RocketSmokeLifetime,
            Size = BlockEnhancementMod.ModSetting.RocketSmokeSize,
            StartColor = BlockEnhancementMod.ModSetting.RocketSmokeStartColor,
            EndColor = BlockEnhancementMod.ModSetting.RocketSmokeEndColor,
            StartColorTime = BlockEnhancementMod.ModSetting.RocketSmokeStartColorTime,
            EndColorTime = BlockEnhancementMod.ModSetting.RocketSmokeEndColorTime,
            StartAlpha = BlockEnhancementMod.ModSetting.RocketSmokeStartAlpha,
            EndAlpha = BlockEnhancementMod.ModSetting.RocketSmokeEndAlpha,
            StartAlphaTime = BlockEnhancementMod.ModSetting.RocketSmokeStartAlphaTime,
            EndAlphaTime = BlockEnhancementMod.ModSetting.RocketSmokeEndAlphaTime,
        };

        //public float TrailSmokeEmissionConstant { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Emission Constant"); } }
        //public float TrailSmokeLifetime { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Lifetime"); } }
        //public float TrailSmokeSize { get { return BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Size"); } }
        //public Color TrailSmokeStartColor { get { return BlockEnhancementMod.Configuration.GetValue<Color>("Rocket Smoke Start Color"); } }
        //public Color TrailSmokeEndColor { get { return BlockEnhancementMod.Configuration.GetValue<Color>("Rocket Smoke End Color"); } }
        //public float TrailSmokeStartAlpha { get { return BlockEnhancementMod.Configuration.GetValue<float >("Rocket Smoke Start Alpha"); } }
        //public float TrailSmokeEndAlpha { get { return BlockEnhancementMod.Configuration.GetValue<float >("Rocket Smoke End Alpha"); } }
        //public float TrailSmokeStartAlphaTime { get { return BlockEnhancementMod.Configuration.GetValue<float >("Rocket Smoke Start Color Time"); } }
        //public float TrailSmokeEndAlphaTime { get { return BlockEnhancementMod.Configuration.GetValue<float >("Rocket Smoke Start Color Alpha"); } }


        //No smoke mode related
        MToggle NoSmokeToggle;

        //Firing record related setting
        private float launchTime = 0f;
        private bool launchTimeRecorded = false;
        public bool removedFromGroup = false;

        //Guide related setting
        MSlider TorqueSlider;
        MToggle StabilityToggle;
        //MSlider PredictionSlider;
        MToggle ShowRadarToggle;
        MToggle ShowPredictionToggle;
        MSlider ProjectileSpeedSlider;
        MSlider DragSlider;
        public bool rocketExploMsgSent = false;
        public bool rocketInBuildSent = false;

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MMenu RadarTypeMenu;
        MKey ManualOverrideKey;
        public MKey SPTeamKey;
        private readonly float maxSearchAngleNormal = 90f;
        private readonly float maxSearchAngleNo8 = 175f;
        public GameObject radarObject;
        public RadarScript radar;
        public GameObject guideObject;
        public GuideController guideController;

        //impact & proximity fuze related setting
        MToggle ImpactFuzeToggle;
        MToggle ProximityFuzeToggle;
        MSlider ProximityFuzeRangeSlider;
        public float triggerForceImpactFuzeOn = 50f;
        public float triggerForceImpactFuzeOff = 1200f;

        //Guide delay related setting
        MSlider GuideDelaySlider;

        //High power explosion related setting
        MToggle HighExploToggle;
        private bool bombHasExploded = false;
        private readonly int levelBombCategory = 4;
        private readonly int levelBombID = 5001;
        private float bombExplosiveCharge = 0;
        private float explosiveCharge = 0f;
        private readonly float radius = 7f;
        private readonly float power = 3600f;
        private readonly float torquePower = 100000f;
        private readonly float upPower = 0.25f;

        public struct TrailSmokePropertise
        {
            public float EmissionConstant { get; set; }
            public float Lifetime;
            public float Size;
            public Color StartColor;
            public Color EndColor;
            public float StartColorTime;
            public float EndColorTime;
            public float StartAlpha;
            public float EndAlpha;
            public float StartAlphaTime;
            public float EndAlphaTime;

            public override string ToString()
            {
                return StartAlphaTime + "-" + EndAlphaTime;
            }
        }
        public override void SafeAwake()
        {
            //Key mapper setup
            //Menus
            SettingMenu = AddMenu("SettingType", 0, LanguageManager.Instance.CurrentLanguage.SettingType);

            RadarTypeMenu = AddMenu("Radar Type", 0, LanguageManager.Instance.CurrentLanguage.RadarType);

            //Toggles
            ShowRadarToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ShowRadar, "ShowRadar", false);

            ImpactFuzeToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ImpactFuze, "ImpactFuze", false);

            ProximityFuzeToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ProximityFuze, "ProximityFuze", false);

            StabilityToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.RocketStability, "RocketStabilityOn", false);

            AutoEjectToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.AutoRelease, "AutoGrabberRelease", false);

            NoSmokeToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.NoSmoke, "NoSmoke", false);

            HighExploToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.HighExplo, "HighExplo", false);

            ShowPredictionToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ShowProjectileInterception, "ShowPrediction", false);

            GuidedRocketToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.TrackTarget, "TrackingRocket", false); //Keep this as the last toggle

            //Sliders
            ActiveGuideRocketSearchAngleSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.SearchAngle, "searchAngle", 60f, 0, maxSearchAngleNormal);

            TorqueSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.TorqueOnRocket, "torqueOnRocket", 100f, 0, 100f);

            GuideDelaySlider = AddSlider(LanguageManager.Instance.CurrentLanguage.GuideDelay, "guideDelay", 0f, 0, 2);

            //PredictionSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.Prediction, "prediction", 10, 0, 50);

            ProximityFuzeRangeSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.CloseRange, "closeRange", 0f, 0, 10f);

            GroupFireRateSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.GroupFireRate, "groupFireRate", 0.25f, 0.1f, 1f);

            ProjectileSpeedSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.ProjectileSpeed, "CannonBallSpeed", 1f, 0.1f, 1000f);

            DragSlider = AddSlider("炮弹阻力", "CannonBallDrag", 0.2f, 0f, 1f);

            //Keys
            LockTargetKey = AddKey(LanguageManager.Instance.CurrentLanguage.LockTarget, "lockTarget", KeyCode.Delete);

            GroupFireKey = AddKey(LanguageManager.Instance.CurrentLanguage.GroupedFire, "groupFire", KeyCode.None);

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
            base.DisplayInMapper(value);

            var _value = value && GuidedRocketToggle.IsActive; //for guided rocket
            var _value1 = _value && SettingMenu.Value == 1; //Radar setting
            var _value2 = _value1 && (RadarTypeMenu.Value == (int)RadarScript.RadarTypes.ActiveRadar); //for active radar
            var _value3 = _value && SettingMenu.Value == 0; //Rocket setting guided
            var _value4 = (GuidedRocketToggle.IsActive ? _value3 : value);
            var _value5 = (EnhancementToggle.IsActive ? _value4 : true);

            GuidedRocketToggle.DisplayInMapper = value;

            //Display when guided is ON
            SettingMenu.DisplayInMapper = _value;

            //Display when radar setting is selected
            SPTeamKey.DisplayInMapper = _value1 && (!StatMaster.isMP || Playerlist.Players.Count == 1);
            RadarTypeMenu.DisplayInMapper = _value1;
            //PredictionSlider.DisplayInMapper = _value1;
            TorqueSlider.DisplayInMapper = _value1;
            GuideDelaySlider.DisplayInMapper = _value1;

            //Display for active Radar only
            ManualOverrideKey.DisplayInMapper = _value2;
            ShowRadarToggle.DisplayInMapper = _value2;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = _value2;
            LockTargetKey.DisplayInMapper = _value2;
            ShowPredictionToggle.DisplayInMapper = _value2;
            ProjectileSpeedSlider.DisplayInMapper = _value2 && ShowPredictionToggle.IsActive;
            DragSlider.DisplayInMapper = /*_value2 && ShowPredictionToggle.IsActive*/ false;

            //Display for rocket setting
            StabilityToggle.DisplayInMapper = _value3;
            ImpactFuzeToggle.DisplayInMapper = _value3;
            ProximityFuzeToggle.DisplayInMapper = _value3;
            ProximityFuzeRangeSlider.DisplayInMapper = _value3 && ProximityFuzeToggle.IsActive;

            //Display for guided OFF & rocket setting when guided ON
            AutoEjectToggle.DisplayInMapper = _value4 && GroupFireKey.GetKey(0) != KeyCode.None;
            GroupFireKey.DisplayInMapper = _value4;
            GroupFireRateSlider.DisplayInMapper = _value4 && GroupFireKey.GetKey(0) != KeyCode.None;
            NoSmokeToggle.DisplayInMapper = _value4;
            HighExploToggle.DisplayInMapper = _value4;

            //Display for BE off & rocket setting when guided ON
            rocket.LaunchKey.DisplayInMapper = _value5;
            rocket.DelaySlider.DisplayInMapper = _value5;
            rocket.ChargeSlider.DisplayInMapper = _value5;
            rocket.PowerSlider.DisplayInMapper = _value5;

            //Tried to hide colour slider, but failed.
            //try { rocket.ColourSlider.DisplayInMapper = _value5; }
            //catch (System.Exception) { }

        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            rocketInBuildSent = removedFromGroup = false;

            // Read the charge from rocket
            explosiveCharge = bombExplosiveCharge = rocket.ChargeSlider.Value;

            // Make sure the high explo mode is not too imba
            if (HighExploToggle.IsActive && !EnhanceMore)
            {
                bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
            }

            if (GuidedRocketToggle.IsActive)
            {
                // Initialisation for simulation
                launchTimeRecorded = bombHasExploded = rocketExploMsgSent = false;
                float searchAngle = Mathf.Clamp(ActiveGuideRocketSearchAngleSlider.Value, 0, EnhanceMore ? maxSearchAngleNo8 : maxSearchAngleNormal);
                float searchRange = EnhanceMore ? 5000f : 2000f;

                //Add radar
                //Collider[] selfColliders = rocket.gameObject.GetComponentsInChildren<Collider>();
                radarObject = new GameObject("RocketRadar");
                radarObject.transform.SetParent(rocket.transform);
                radarObject.transform.position = transform.position;
                radarObject.transform.rotation = transform.rotation;
                radarObject.transform.localPosition = Vector3.forward * 0.5f;
                radarObject.transform.localScale = restoreScale(rocket.transform.localScale);
                radar = radarObject.GetComponent<RadarScript>() ?? radarObject.AddComponent<RadarScript>();
                radar.Setup(BB, rocketRigidbody, searchRange, searchAngle, RadarTypeMenu.Value, ShowRadarToggle.IsActive);
                radar.Setup(ShowPredictionToggle.IsActive, ProjectileSpeedSlider.Value, DragSlider.Value);

                //Workaround when radar can be ignited hence explode the rocket
                FireTag fireTag = radarObject.AddComponent<FireTag>();
                fireTag.enabled = true;
                Rigidbody rigidbody = radarObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.mass = 0.0001f;
                rigidbody.drag = 0f;

                //Stop colliding with its own colliders
                //if (selfColliders.Length > 0)
                //{
                //    foreach (var collider in selfColliders)
                //    {
                //        Physics.IgnoreCollision(collider, radar.meshCollider, true);
                //    }
                //}

                //Set up Guide controller
                guideObject = new GameObject("GuideController");
                guideObject.transform.SetParent(rocket.transform);
                guideObject.transform.position = transform.position;
                guideObject.transform.rotation = transform.rotation;
                guideObject.transform.localScale = Vector3.one;
                guideController = guideObject.GetComponent<GuideController>() ?? guideObject.AddComponent<GuideController>();
                guideController.Setup(rocket, rocketRigidbody, radar, rocket.PowerSlider.Value, searchAngle, Mathf.Clamp(TorqueSlider.Value, 0, 100), false);

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

                        var colt = smokeTrail.colorOverLifetime;

                        //Debug.Log(colt.color.colorMin + "||" + colt.color.colorMax);
                        //Debug.Log(colt.color.mode);
                        //Debug.Log(colt.color.gradient.alphaKeys.Length);
                        //colt.color.gradient.alphaKeys.ToList().ForEach(key => Debug.Log(key.alpha + "-" + key.time));
                        //Debug.Log(colt.color.gradient.colorKeys.Length);
                        //colt.color.gradient.colorKeys.ToList().ForEach(key => Debug.Log(key.color + "-" + key.time));

                        //Debug.Log("??" + trailSmokePropertise.ToString());
                        colt.color = new ParticleSystem.MinMaxGradient(new Gradient()
                        {
                            alphaKeys = new GradientAlphaKey[]
                            {
                                new GradientAlphaKey(0f,0f),
                                new GradientAlphaKey(0.5f,0.01f),
                                new GradientAlphaKey(trailSmokePropertise.StartAlpha,trailSmokePropertise.StartAlphaTime),
                                new GradientAlphaKey(trailSmokePropertise.EndAlpha,trailSmokePropertise.EndAlphaTime),
                                new GradientAlphaKey(0f,0.8f)
                            },
                            colorKeys = new GradientColorKey[]
                            {
                                   new GradientColorKey(new Color(1f,1f,0f,1f),0f),
                                     new GradientColorKey(new Color(0.882f,0.365f,0.176f,1f),0.019f),
                            new GradientColorKey(trailSmokePropertise.StartColor,trailSmokePropertise.StartColorTime),
                            new GradientColorKey(trailSmokePropertise.EndColor,trailSmokePropertise.EndColorTime)
                            }
                        }); ;
                        //var solt = smokeTrail.sizeOverLifetime;
                        //solt.size = new ParticleSystem.MinMaxCurve();
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
            if (GroupFireKey.GetKey(0) != KeyCode.None && !GroupFireKey.Ignored)
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
                if (!GroupFireKey.Ignored)
                {
                    if ((GroupFireKey.IsHeld || GroupFireKey.EmulationHeld()) && !StatMaster.isClient)
                    {
                        if (!RocketsController.Instance.launchStarted)
                        {
                            StartCoroutine(RocketsController.Instance.LaunchRocketFromGroup(rocket.ParentMachine.PlayerID, GroupFireKey.GetKey(0)));
                        }
                    }
                }

                if (radar != null)
                {
                    if (!StatMaster.isClient)
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
                                //radar.meshRenderer.enabled = radar.canBeOverridden && ShowRadarToggle.IsActive && radar.Switch;
                                radar.DisplayRadarZone = radar.canBeOverridden && ShowRadarToggle.IsActive && radar.Switch;
                                //radar.meshCollider.enabled = radar.canBeOverridden && radar.Switch;
                                radar.canBeOverridden = !radar.canBeOverridden;
                                if (!radar.canBeOverridden)
                                {
                                    radar.ClearTarget(false);
                                }
                            }
                        }

                        if (LockTargetKey.IsPressed || LockTargetKey.EmulationPressed())
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
                            guideController.enableAerodynamicEffect = StabilityToggle.IsActive;

                            //Record the launch time for the guide delay
                            if (!launchTimeRecorded)
                            {
                                launchTimeRecorded = true;
                                launchTime = Time.time;
                            }

                            //Rocket can be triggered after the time elapsed after firing is greater than guide delay
                            if (Time.time - launchTime >= GuideDelaySlider.Value + 0.15f && TorqueSlider.Value > 0)
                            {
                                guideController.Switch = true;
                            }

                            //Proximity fuse behaviour
                            if (ProximityFuzeToggle.IsActive)
                            {
                                if (radar.TargetDistance <= ProximityFuzeRangeSlider.Value + 1f)
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
                    r.constant = /*TrailSmokeEmissionConstant*/trailSmokePropertise.EmissionConstant;
                    em.rate = r;
                    smokeTrail.startLifetime = /*TrailSmokeLifetime*/trailSmokePropertise.Lifetime;
                    smokeTrail.startSize = /*TrailSmokeSize*/trailSmokePropertise.Size;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!EnhancementToggle.IsActive) return;
            if (!rocket.hasFired) return;
            if (rocket.PowerSlider.Value > 0.1f)
            {
                if (collision.impulse.magnitude / Time.fixedDeltaTime >= (ImpactFuzeToggle.IsActive ? triggerForceImpactFuzeOn : triggerForceImpactFuzeOff) || collision.gameObject.name.Contains("CanonBall"))
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
            if (!HighExploToggle.IsActive) yield break;

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
