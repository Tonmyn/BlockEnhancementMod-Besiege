using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Levels;
using BlockEnhancementMod.Blocks;

namespace BlockEnhancementMod
{
    public static class Messages
    {
        //For rockets
        public static MessageType rocketTargetBlockBehaviourMsg;
        public static MessageType rocketTargetEntityMsg;
        public static MessageType rocketTargetNullMsg;
        public static MessageType rocketRayToHostMsg;
        public static MessageType rocketHighExploPosition;
        public static MessageType rocketLockOnMeMsg;
        public static MessageType rocketLostTargetMsg;
    }

    public class MessageController : MonoBehaviour
    {
        static MessageController messageControllerInstance;
        public static MessageController Instance
        {
            get
            {
                if (messageControllerInstance == null)
                {
                    messageControllerInstance = BlockEnhancementMod.mod.AddComponent<MessageController>();
                }
                return messageControllerInstance;
            }
        }


        private bool iAmLockedByRocket = false;
        private bool isFirstFrame = true;
        public static bool DisplayWarning { get; internal set; } = true;
        private FixedCameraController cameraController;
        public Dictionary<BlockBehaviour, int> rocketTargetDict;
        public Dictionary<int, Dictionary<KeyCode, Stack<TimedRocket>>> playerGroupedRockets;
        public bool launchStarted = false;
        private static readonly float transparancy = 0.5f;
        private static readonly float screenOffset = 128f;
        private static readonly float warningHeight = 60f;
        private static readonly float warningWidth = 180f;
        private static readonly float borderThickness = 10f;
        private static readonly Color warningBorderColor = new Color(1, 0, 0, transparancy);
        private static readonly Rect warningRect = new Rect(Screen.width - screenOffset - warningWidth, Screen.height - screenOffset - warningHeight, warningWidth, warningHeight);
        private static Texture2D redTexture;
        private static Texture2D RedTexture
        {
            get
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

        void FixedUpdate()
        {
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
                        rocketTargetDict.Clear();
                        playerGroupedRockets.Clear();
                        isFirstFrame = true;
                    }
                }
            }
            if (!isFirstFrame)
            {
                if (PlayerMachine.GetLocal() != null && rocketTargetDict != null && !isFirstFrame)
                {
                    try
                    {
                        foreach (var rocketTargetPair in rocketTargetDict)
                        {
                            if (!rocketTargetPair.Key.ParentMachine.isSimulating)
                            {
                                RemoveRocketTarget(rocketTargetPair.Key);
                            }
                        }
                    }
                    catch { }
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
        }

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

        readonly GUIStyle missileWarningStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 0, 0, transparancy) },
            alignment = TextAnchor.MiddleCenter,
        };

        public MessageController()
        {
            rocketTargetDict = new Dictionary<BlockBehaviour, int>();
            playerGroupedRockets = new Dictionary<int, Dictionary<KeyCode, Stack<TimedRocket>>>();
            //Initiating messages
            Messages.rocketTargetBlockBehaviourMsg = ModNetworking.CreateMessageType(DataType.Block, DataType.Block);
            Messages.rocketTargetEntityMsg = ModNetworking.CreateMessageType(DataType.Entity, DataType.Block);
            Messages.rocketTargetNullMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.rocketRayToHostMsg = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3, DataType.Block);
            Messages.rocketHighExploPosition = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Single);
            Messages.rocketLockOnMeMsg = ModNetworking.CreateMessageType(DataType.Block, DataType.Integer);
            Messages.rocketLostTargetMsg = ModNetworking.CreateMessageType(DataType.Block);

            //Initiating callbacks
            ModNetworking.Callbacks[Messages.rocketHighExploPosition] += (Message msg) =>
            {
                if (StatMaster.isClient)
                {
                    Vector3 position = (Vector3)msg.GetData(0);
                    float bombExplosiveCharge = (float)msg.GetData(1);
                    int levelBombCategory = 4;
                    int levelBombID = 5001;
                    float radius = 7f;
                    float power = 3600f;
                    float torquePower = 100000f;
                    float upPower = 0.25f;
                    try
                    {
                        GameObject bomb = UnityEngine.Object.Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject);
                        bomb.transform.position = position;
                        ExplodeOnCollide bombControl = bomb.GetComponent<ExplodeOnCollide>();
                        bomb.transform.localScale = Vector3.one * bombExplosiveCharge;
                        bombControl.radius = radius * bombExplosiveCharge;
                        bombControl.power = power * bombExplosiveCharge;
                        bombControl.torquePower = torquePower * bombExplosiveCharge;
                        bombControl.upPower = upPower;
                        bombControl.Explodey();
                    }
                    catch { }
                }
            };

            ModNetworking.Callbacks[Messages.rocketTargetBlockBehaviourMsg] += (Message msg) =>
            {
#if DEBUG
                Debug.Log("Receive block target");
#endif
                Block rocketBlock = (Block)msg.GetData(1);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.target = ((Block)msg.GetData(0)).GameObject.transform;
                rocket.targetCollider = rocket.target.gameObject.GetComponentInChildren<Collider>(true);

            };

            ModNetworking.Callbacks[Messages.rocketTargetEntityMsg] += (Message msg) =>
            {
#if DEBUG
                Debug.Log("Receive entity target");
#endif
                Block rocketBlock = (Block)msg.GetData(1);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.target = ((Entity)msg.GetData(0)).GameObject.transform;
                rocket.targetCollider = rocket.target.gameObject.GetComponentInChildren<Collider>(true);
            };

            ModNetworking.Callbacks[Messages.rocketTargetNullMsg] += (Message msg) =>
            {
#if DEBUG
                Debug.Log("Receive entity target");
#endif
                Block rocketBlock = (Block)msg.GetData(0);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.target = null;
                rocket.targetCollider = null;

            };

            ModNetworking.Callbacks[Messages.rocketRayToHostMsg] += (Message msg) =>
            {
                Block rocketBlock = (Block)msg.GetData(2);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.rayFromClient = new Ray((Vector3)msg.GetData(0), (Vector3)msg.GetData(1));
                rocket.activeGuide = false;
                rocket.receivedRayFromClient = true;
            };

            ModNetworking.Callbacks[Messages.rocketLockOnMeMsg] += (Message msg) =>
            {
                Block rocket = (Block)msg.GetData(0);
                int targetMachineID = (int)msg.GetData(1);
                UpdateRocketTarget(rocket.InternalObject, targetMachineID);

            };
            ModNetworking.Callbacks[Messages.rocketLostTargetMsg] += (Message msg) =>
            {
                Block rocket = (Block)msg.GetData(0);
                RemoveRocketTarget(rocket.InternalObject);
            };
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
            launchStarted = true;
            try
            {
                TimedRocket rocket = playerGroupedRockets[id][key].Pop();
                rocket.fireTag.Ignite();
                rocket.hasFired = true;
                rocket.hasExploded = false;
            }
            catch { }
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            launchStarted = false;
            yield return null;
        }
    }
}
