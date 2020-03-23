using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Levels;
using BlockEnhancementMod.Blocks;
using Localisation;

namespace BlockEnhancementMod
{
    class RocketsController : SingleInstance<RocketsController>
    {
        public override string Name { get; } = "Rockets Controller";

        private bool iAmLockedByRocket = false;
        private bool isFirstFrame = true;
        public static bool DisplayWarning { get { return BlockEnhancementMod.Configuration.GetValue<bool>("Display Warning"); } internal set { BlockEnhancementMod.Configuration.SetValue("Display Warning", value); } }
        private FixedCameraController cameraController;
        public Dictionary<BlockBehaviour, int> rocketTargetDict;
        public Dictionary<int, Dictionary<KeyCode, HashSet<TimedRocket>>> playerGroupedRockets;
        public Dictionary<int, Dictionary<KeyCode, HashSet<RadarScript>>> playerGroupedRadars;
        public bool launchStarted = false;
        private static readonly float transparancy = 0.5f;
        private static readonly float screenOffset = 128f;
        private static readonly float warningHeight = 60f;
        private static readonly float warningWidth = 180f;
        private static readonly float borderThickness = 10f;
        private static readonly Color warningBorderColor = new Color(1, 0, 0, transparancy);
        private static readonly Rect warningRect = new Rect(Screen.width - screenOffset - warningWidth, Screen.height - screenOffset - warningHeight, warningWidth, warningHeight);
        private static Texture2D redTexture;
        public static Texture2D redSquareAim = new Texture2D(16, 16);

        private static Texture2D RedTexture
        {
            get/**/
            {
                if (redTexture == null)
                {
                    redTexture = new Texture2D(1, 1);
                    redTexture.SetPixel(0, 0, warningBorderColor);
                    redTexture.Apply();
                }

                return redTexture;
            }
        }
        private Rect counterRect = new Rect(Screen.width - screenOffset - warningWidth, Screen.height - 0.5f * screenOffset - warningHeight, warningWidth, warningHeight);
        public static bool DisplayRocketCount { get { return BlockEnhancementMod.Configuration.GetValue<bool>(" Display Rocket Count"); } internal set { BlockEnhancementMod.Configuration.SetValue("Display Rocket Count", value); } }

        public RocketsController()
        {
            rocketTargetDict = new Dictionary<BlockBehaviour, int>();
            playerGroupedRockets = new Dictionary<int, Dictionary<KeyCode, HashSet<TimedRocket>>>();
            playerGroupedRadars = new Dictionary<int, Dictionary<KeyCode, HashSet<RadarScript>>>();

            initRadarSomething();


            void initRadarSomething()
            {
                redSquareAim.LoadImage(ModIO.ReadAllBytes(@"Resources/Square-Red.png"));
                SetRadarIgnoreCollosionLayer();

                void SetRadarIgnoreCollosionLayer()
                {
                    Physics.IgnoreLayerCollision(RadarScript.CollisionLayer, RadarScript.CollisionLayer, true);
                    Physics.IgnoreLayerCollision(RadarScript.CollisionLayer, 29, true);
                }
            }

        }

        void FixedUpdate()
        {
            if (!StatMaster.levelSimulating)
            {
                if (playerGroupedRockets.Count > 0)
                {
                    playerGroupedRockets.Clear();
                }
                if (playerGroupedRadars.Count > 0)
                {
                    playerGroupedRadars.Clear();
                }
            }
            if (PlayerMachine.GetLocal() != null)
            {
                if (PlayerMachine.GetLocal().InternalObject.isSimulating)
                {
                    if (isFirstFrame)
                    {
                        isFirstFrame = launchStarted = false;
                        cameraController = FindObjectOfType<FixedCameraController>();
                        rocketTargetDict.Clear();
                    }
                }
                else
                {
                    if (!isFirstFrame)
                    {
                        if (playerGroupedRockets.ContainsKey(PlayerMachine.GetLocal().InternalObject.PlayerID))
                        {
                            playerGroupedRockets.Remove(PlayerMachine.GetLocal().InternalObject.PlayerID);
                        }
                        if (playerGroupedRadars.ContainsKey(PlayerMachine.GetLocal().InternalObject.PlayerID))
                        {
                            playerGroupedRadars.Remove(PlayerMachine.GetLocal().InternalObject.PlayerID);
                        }
                        rocketTargetDict.Clear();
                        isFirstFrame = true;
                    }
                }
            }
            if (!isFirstFrame)
            {
                if (PlayerMachine.GetLocal() != null && rocketTargetDict != null && !isFirstFrame)
                {
                    if (rocketTargetDict.Count == 0)
                    {
                        iAmLockedByRocket = false;
                    }
                    else
                    {
                        foreach (var rocketTargetPair in rocketTargetDict)
                        {
                            if (PlayerMachine.GetLocal() != null)
                            {
                                if (rocketTargetPair.Value == (StatMaster.isMP ? PlayerMachine.GetLocal().Player.NetworkId : 0))
                                {
                                    iAmLockedByRocket = true;
                                    break;
                                }
                                else
                                {
                                    iAmLockedByRocket = false;
                                }
                            }
                            else
                            {
                                iAmLockedByRocket = false;
                            }
                        }

                    }
                }
            }
        }

        readonly GUIStyle missileWarningStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 0, 0, transparancy) },
            alignment = TextAnchor.MiddleCenter,
        };
        readonly GUIStyle groupedRocketsCounterStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(0, 1, 0, transparancy) },
            alignment = TextAnchor.MiddleCenter,
        };

        private void DrawBorder()
        {
            //Top
            GUI.DrawTexture(new Rect(warningRect.xMin, warningRect.yMin, warningRect.width, borderThickness), RedTexture);
            // Left
            GUI.DrawTexture(new Rect(warningRect.xMin, warningRect.yMin + borderThickness, borderThickness, warningRect.height - 2 * borderThickness), RedTexture);
            // Right
            GUI.DrawTexture(new Rect(warningRect.xMax - borderThickness, warningRect.yMin + borderThickness, borderThickness, warningRect.height - 2 * borderThickness), RedTexture);
            // Bottom
            GUI.DrawTexture(new Rect(warningRect.xMin, warningRect.yMax - borderThickness, warningRect.width, borderThickness), RedTexture);
        }

        private void OnGUI()
        {
            if (iAmLockedByRocket && DisplayWarning)
            {
                if (cameraController != null)
                {
                    if (cameraController.activeCamera != null)
                    {
                        if (cameraController.activeCamera.CamMode == FixedCameraBlock.Mode.FirstPerson)
                        {
                            DrawBorder();
                            GUI.Box(warningRect, "Missile Alert", missileWarningStyle);
                        }
                    }
                }
            }
            if (DisplayRocketCount)
            {
                if (cameraController != null)
                {
                    if (cameraController.activeCamera != null)
                    {
                        if (cameraController.activeCamera.CamMode == FixedCameraBlock.Mode.FirstPerson)
                        {
                            if (playerGroupedRockets.TryGetValue(StatMaster.isMP ? PlayerMachine.GetLocal().Player.NetworkId : 0, out Dictionary<KeyCode, HashSet<TimedRocket>> groupedRockets))
                            {
                                string textString = "";
                                foreach (var group in groupedRockets)
                                {
                                    textString += KeyCodeConverter.GetKey(group.Key).ToString() + ": " + group.Value.Count + Environment.NewLine;
                                }
                                GUI.Box(counterRect, LanguageManager.Instance.CurrentLanguage.RemainingRockets + Environment.NewLine + textString, groupedRocketsCounterStyle);
                            }

                        }
                    }
                }
            }
        }

        public void UpdateRocketFiredStatus(TimedRocket rocket)
        {
            if (playerGroupedRockets.TryGetValue(StatMaster.isMP ? PlayerMachine.GetLocal().Player.NetworkId : 0, out Dictionary<KeyCode, HashSet<TimedRocket>> groupedRockets))
            {
                RocketScript rocketScript = rocket.GetComponent<RocketScript>();
                if (rocketScript != null)
                {
                    if (groupedRockets.TryGetValue(rocketScript.GroupFireKey.GetKey(0), out HashSet<TimedRocket> rockets))
                    {
                        rockets.Remove(rocket);
                    }
                }
            }
        }

        public void UpdateRocketTarget(BlockBehaviour rocket, int targetMachineID)
        {
            if (rocketTargetDict.ContainsKey(rocket))
            {
                rocketTargetDict[rocket] = targetMachineID;
            }
            else
            {
                rocketTargetDict.Add(rocket, targetMachineID);
            }
        }

        public void RemoveRocketTarget(BlockBehaviour rocket)
        {
            if (rocketTargetDict.ContainsKey(rocket))
            {
                rocketTargetDict.Remove(rocket);
            }
        }

        public IEnumerator LaunchRocketFromGroup(int id, KeyCode key)
        {
            if (!playerGroupedRockets.TryGetValue(id, out Dictionary<KeyCode, HashSet<TimedRocket>> timedRocketDict))
            {
#if DEBUG
                Debug.Log("Cannot get rocket dict");
#endif
                launchStarted = false;
                yield return null;
            }
            if (!timedRocketDict.TryGetValue(key, out HashSet<TimedRocket> timedRockets))
            {
#if DEBUG
                Debug.Log("Cannot get rocket list");
#endif
                launchStarted = false;
                yield return null;
            }

#if DEBUG
            Debug.Log("Rocket count: " + timedRockets.Count);
#endif

            launchStarted = true;
            float defaultDelay = 0.25f;

            TimedRocket rocket;
            RocketScript rocketScript;

            if (timedRockets.Count > 0)
            {
                rocket = timedRockets.First();
                timedRockets.Remove(rocket);
                if (rocket != null)
                {
                    rocketScript = rocket.GetComponent<RocketScript>();
                    if (rocketScript != null)
                    {
                        if (rocketScript./*autoGrabberRelease*/AutoGrabberReleaseToggle.IsActive && rocket.grabbers.Count > 0)
                        {
                            List<JoinOnTriggerBlock> allGrabbers = new List<JoinOnTriggerBlock>(rocket.grabbers);
                            foreach (var grabber in allGrabbers)
                            {
                                grabber?.OnKeyPressed();
                            }
                        }
                        defaultDelay = Mathf.Clamp(rocketScript.groupFireRate, 0.1f, 1f);
                        rocket.LaunchMessage();

                        if (rocketScript.radarTypeMenuIndex == /*(int)RadarScript.SearchModes.Passive*/(int)RadarScript.RadarTypes.PassiveRadar)
                        {
                            RadarScript passiveRocketRadar = rocketScript.radar;
                            if (passiveRocketRadar != null)
                            {
                                if (playerGroupedRadars.TryGetValue(rocket.ParentMachine.PlayerID, out Dictionary<KeyCode, HashSet<RadarScript>> radarsDict))
                                {
                                    if (radarsDict.TryGetValue(rocketScript.GroupFireKey.GetKey(0), out HashSet<RadarScript> radars))
                                    {
                                        if (radars.Count > 0)
                                        {
                                            passiveRocketRadar.sourceRadars = radars;
                                            //RadarScript radar = radars.ElementAt(UnityEngine.Random.Range(0, radars.Count));
                                            //passiveRocketRadar.sourceRadar = radar;
                                            //passiveRocketRadar.SetTarget(radar.target);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            StartCoroutine(ResetLaunchState(defaultDelay));
            yield return null;
        }

        public IEnumerator ResetLaunchState(float delay)
        {
            yield return new WaitForSeconds(delay);
            launchStarted = false;
        }
    }
}
