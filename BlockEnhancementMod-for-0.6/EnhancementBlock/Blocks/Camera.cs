using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Modding;
using Modding.Blocks;
using Modding.Levels;
using Modding.Common;
using UnityEngine;

namespace BlockEnhancementMod
{
    class CameraScript : EnhancementBlock
    {
        //General setting
        MToggle CameraLookAtToggle;
        public bool cameraLookAtToggled = false;
        private int selfIndex;
        public FixedCameraBlock fixedCamera;
        private Transform smoothLook;
        public FixedCameraController fixedCameraController;
        private Quaternion defaultLocalRotation;
        public float smooth;
        public float smoothLerp;
        private float newCamFOV, orgCamFOV, camFOVSmooth;

        //Networking setting
        private Transform clientTarget;
        private Ray rayFromClient;
        private bool receivedRayFromClient = false;
        private int clientPlayerID;

        //Track target setting
        MKey LockTargetKey;
        public Transform target;
        private HashSet<Transform> explodedTarget = new HashSet<Transform>();
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };
        private List<Collider> blockColliders = new List<Collider>();
        private List<Collider> levelEntityColliders = new List<Collider>();

        //Pause tracking setting
        MKey PauseTrackingKey;
        public bool pauseTracking = false;
        public List<KeyCode> pauseKeys = new List<KeyCode> { KeyCode.X };

        //Auto lookat related setting
        MSlider NonCustomModeSmoothSlider;
        MKey AutoLookAtKey;
        private bool firstPersonMode = false;
        public float firstPersonSmooth = 0.25f;
        private float timeOfDestruction = 0f;
        private readonly float targetSwitchDelay = 1.25f;
        public List<KeyCode> activeGuideKeys = new List<KeyCode> { KeyCode.RightShift };
        private float searchAngle = 90;
        private readonly float safetyRadius = 25f;
        private bool autoSearch = true;
        private bool targetAquired = false;
        private bool searchStarted = false;
        private readonly float displayTime = 1f;
        private float switchTime = Mathf.NegativeInfinity;
        private bool activateTimeRecorded = false;

        private void MessageInitialisation()
        {
            ModNetworking.Callbacks[Messages.cameraTargetBlockBehaviourMsg] += (Message msg) =>
            {
                target = ((BlockBehaviour)msg.GetData(0)).gameObject.transform;
                Debug.Log(target.gameObject.name);
                pauseTracking = false;
            };
            ModNetworking.Callbacks[Messages.cameraTargetEntityMsg] += (Message msg) =>
            {
                target = ((LevelEntity)msg.GetData(0)).gameObject.transform;
                Debug.Log(target.gameObject.name);
                pauseTracking = false;
            };
            ModNetworking.Callbacks[Messages.cameraRayToHostMsg] += (Message msg) =>
            {
                rayFromClient = new Ray((Vector3)msg.GetData(0), (Vector3)msg.GetData(1));
                clientPlayerID = msg.Sender.NetworkId;
                receivedRayFromClient = true;
            };
        }

        public override void SafeAwake()
        {
            CameraLookAtToggle = BB.AddToggle(LanguageManager.trackTarget, "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) =>
            {
                cameraLookAtToggled =
                LockTargetKey.DisplayInMapper =
                PauseTrackingKey.DisplayInMapper =
                NonCustomModeSmoothSlider.DisplayInMapper =
                AutoLookAtKey.DisplayInMapper =
                value;
                ChangedProperties();
            };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { cameraLookAtToggled = CameraLookAtToggle.IsActive; };

            NonCustomModeSmoothSlider = BB.AddSlider(LanguageManager.firstPersonSmooth, "nonCustomSmooth", firstPersonSmooth, 0, 1);
            NonCustomModeSmoothSlider.ValueChanged += (float value) => { firstPersonSmooth = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { firstPersonSmooth = NonCustomModeSmoothSlider.Value; };

            LockTargetKey = BB.AddKey(LanguageManager.lockTarget, "LockTarget", KeyCode.Delete);

            PauseTrackingKey = BB.AddKey(LanguageManager.pauseTracking, "ResetView", KeyCode.X);

            AutoLookAtKey = BB.AddKey(LanguageManager.switchGuideMode, "ActiveSearchKey", KeyCode.RightShift);

            // Add reference to the camera's buildindex
            fixedCamera = GetComponent<FixedCameraBlock>();
            smoothLook = fixedCamera.CompositeTracker3;
            defaultLocalRotation = smoothLook.localRotation;
            selfIndex = fixedCamera.BuildIndex;

            //Initialise Messages
            MessageInitialisation();
#if DEBUG
            ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            if (fixedCamera.CamMode == FixedCameraBlock.Mode.FirstPerson)
            {
                firstPersonMode = true;
            }
            CameraLookAtToggle.DisplayInMapper = value;
            NonCustomModeSmoothSlider.DisplayInMapper = value && cameraLookAtToggled && firstPersonMode;
            AutoLookAtKey.DisplayInMapper = value && cameraLookAtToggled;
            //RecordTargetToggle.DisplayInMapper = value && cameraLookAtToggled;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
            PauseTrackingKey.DisplayInMapper = value && cameraLookAtToggled;
        }

        public override void BuildingUpdate()
        {
            if (fixedCamera.CamMode != FixedCameraBlock.Mode.FirstPerson && firstPersonMode)
            {
                firstPersonMode = false;
                NonCustomModeSmoothSlider.DisplayInMapper = cameraLookAtToggled && firstPersonMode;
            }
            if (fixedCamera.CamMode == FixedCameraBlock.Mode.FirstPerson && !firstPersonMode)
            {
                firstPersonMode = true;
                NonCustomModeSmoothSlider.DisplayInMapper = cameraLookAtToggled && firstPersonMode;
            }
        }

        public override void OnSimulateStart()
        {
            if (cameraLookAtToggled)
            {
                //Initialise the SmoothLook component
                fixedCameraController = FindObjectOfType<FixedCameraController>();

                foreach (var camera in fixedCameraController.cameras)
                {
                    if (camera.BuildIndex == selfIndex)
                    {
                        if (firstPersonMode)
                        {
                            smooth = Mathf.Clamp01(firstPersonSmooth);
                        }
                        else
                        {
                            smooth = Mathf.Clamp01(camera.SmoothSlider.Value);
                        }
                        SetSmoothing();
                    }
                }
                newCamFOV = orgCamFOV = fixedCamera.fovSlider.Value;
                camFOVSmooth = Mathf.Exp(smooth) / Mathf.Exp(1) / 2f;
                // Initialise
                searchStarted = false;
                pauseTracking = autoSearch = targetAquired = true;
                float searchAngleMax = Mathf.Clamp(Mathf.Atan(Mathf.Tan(fixedCamera.fovSlider.Value * Mathf.Deg2Rad / 2) * Camera.main.aspect) * Mathf.Rad2Deg, 0, 90);
                searchAngle = Mathf.Clamp(searchAngle, 0, searchAngleMax);
                target = null;
                explodedTarget.Clear();
                StopAllCoroutines();
            }
        }

        public override void SimulateUpdateAlways()
        {
            //if (StatMaster.isHosting)
            //{
            //    Debug.Log("Received Ray from Client: " + receivedRayFromClient);
            //}
            //if (StatMaster.isHosting && receivedRayFromClient)
            //{
            //    Debug.Log("Received Ray from Client: " + receivedRayFromClient);
            //    receivedRayFromClient = false;
            //    float manualSearchRadius = 1.25f;
            //    RaycastHit[] hits = Physics.SphereCastAll(rayFromClient, manualSearchRadius, Mathf.Infinity);
            //    Physics.Raycast(rayFromClient, out RaycastHit rayHit);
            //    for (int i = 0; i < hits.Length; i++)
            //    {
            //        if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
            //        {
            //            clientTarget = hits[i].transform;
            //            break;
            //        }
            //    }
            //    if (clientTarget == null)
            //    {
            //        for (int i = 0; i < hits.Length; i++)
            //        {
            //            if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
            //            {
            //                clientTarget = hits[i].transform;
            //                break;
            //            }
            //        }
            //    }
            //    Debug.Log("Client target is null? " + (clientTarget == null));
            //    SendTargetToClient();
            //}
            if (cameraLookAtToggled)
            {
                if (fixedCameraController.activeCamera != null)
                {
                    if (fixedCameraController.activeCamera.CompositeTracker3 == smoothLook)
                    {
                        if (fixedCameraController.activeCamera.CamMode == FixedCameraBlock.Mode.FirstPerson || fixedCameraController.activeCamera.CamMode == FixedCameraBlock.Mode.Custom)
                        {
                            Camera activeCam = FindObjectOfType<MouseOrbit>().cam;
                            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                            {
                                newCamFOV = Mathf.Clamp(activeCam.fieldOfView - Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")) * 2.5f, 1, orgCamFOV);
                            }
                            if (activeCam.fieldOfView != newCamFOV)
                            {
                                activeCam.fieldOfView = Mathf.SmoothStep(activeCam.fieldOfView, newCamFOV, camFOVSmooth);
                            }
                        }
                        if (!activateTimeRecorded)
                        {
                            switchTime = Time.time;
                            activateTimeRecorded = true;
                        }
                        if (AutoLookAtKey.IsReleased)
                        {
                            autoSearch = !autoSearch;
                            switchTime = Time.time;
                        }
                        if (PauseTrackingKey.IsReleased)
                        {
                            pauseTracking = !pauseTracking;
                        }
                        if (LockTargetKey.IsReleased)
                        {
                            target = null;
                            if (autoSearch)
                            {
                                targetAquired = searchStarted = false;
                                CameraRadarSearch();
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
                                //    RaycastHit[] hits = Physics.SphereCastAll(ray, manualSearchRadius, Mathf.Infinity);
                                //    Physics.Raycast(ray, out RaycastHit rayHit);
                                //    for (int i = 0; i < hits.Length; i++)
                                //    {
                                //        if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                                //        {
                                //            target = hits[i].transform;
                                //            pauseTracking = false;
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
                                //                pauseTracking = false;
                                //                break;
                                //            }
                                //        }
                                //    }
                                //    if (target == null && !StatMaster.isMP)
                                //    {
                                //        target = rayHit.transform;
                                //        pauseTracking = false;
                                //    }
                                //    SaveTargetToController();
                                //}

                                if (StatMaster.isClient)
                                {
                                    blockColliders.Clear();
                                    foreach (var player in Playerlist.Players)
                                    {
                                        if (!player.isSpectator && player.machine.isSimulating)
                                        {
                                            blockColliders.AddRange(player.machine.SimulationMachine.GetComponentsInChildren<Collider>(true));
                                        }
                                    }
                                    foreach (var collider in blockColliders)
                                    {
                                        collider.enabled = true;
                                    }
                                }

                                // Aquire the target to look at
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                float manualSearchRadius = 1.25f;
                                RaycastHit[] hits = Physics.SphereCastAll(ray, manualSearchRadius, Mathf.Infinity);
                                Physics.Raycast(ray, out RaycastHit rayHit);
                                for (int i = 0; i < hits.Length; i++)
                                {
                                    try
                                    {
                                        int playerID = hits[i].transform.gameObject.GetComponent<BlockBehaviour>().ParentMachine.PlayerID;
                                        target = hits[i].transform;
                                        pauseTracking = false;
                                        break;
                                    }
                                    catch { }
                                }
                                if (target == null)
                                {
                                    target = rayHit.transform;
                                    pauseTracking = false;
                                }
                                if (StatMaster.isClient)
                                {
                                    foreach (var collider in blockColliders)
                                    {
                                        collider.enabled = false;
                                    }
                                }
                            }
                        }
                    }
                }
                if (fixedCameraController.activeCamera != fixedCamera)
                {
                    if (activateTimeRecorded)
                    {
                        activateTimeRecorded = false;
                    }
                }
            }
        }

        public override void SimulateFixedUpdateAlways()
        {
            if (cameraLookAtToggled && fixedCameraController.activeCamera != null)
            {
                if (fixedCameraController.activeCamera.CompositeTracker3 == smoothLook)
                {
                    if (autoSearch && !targetAquired)
                    {
                        CameraRadarSearch();
                    }
                    if (target != null)
                    {
                        try
                        {
                            if (target.gameObject.GetComponent<TimedRocket>().hasExploded)
                            {
                                timeOfDestruction = Time.time;
                                explodedTarget.Add(target);
                                targetAquired = false;
                                target = null;
                                return;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                            {
                                timeOfDestruction = Time.time;
                                explodedTarget.Add(target);
                                targetAquired = false;
                                target = null;
                                return;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                            {
                                timeOfDestruction = Time.time;
                                explodedTarget.Add(target);
                                targetAquired = false;
                                target = null;
                                return;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                            {
                                timeOfDestruction = Time.time;
                                explodedTarget.Add(target);
                                targetAquired = false;
                                target = null;
                                return;
                            }
                        }
                        catch { }
                    }

                }
            }
        }

        public override void SimulateLateUpdateAlways()
        {
            if (cameraLookAtToggled && fixedCameraController.activeCamera != null)
            {
                if (fixedCameraController.activeCamera.CompositeTracker3 == smoothLook)
                {
                    if (pauseTracking)
                    {
                        smoothLook.localRotation = Quaternion.Slerp(smoothLook.localRotation, defaultLocalRotation, smoothLerp * Time.deltaTime);
                    }
                    else
                    {
                        if (Time.time - timeOfDestruction >= targetSwitchDelay)
                        {
                            if (target == null)
                            {
                                smoothLook.localRotation = Quaternion.Slerp(smoothLook.localRotation, defaultLocalRotation, smoothLerp * Time.deltaTime);
                            }
                            else
                            {
                                Quaternion quaternion;
                                if (firstPersonMode)
                                {
                                    quaternion = Quaternion.LookRotation(target.position - smoothLook.position, transform.up);
                                }
                                else
                                {
                                    quaternion = Quaternion.LookRotation(target.position - smoothLook.position);
                                }
                                smoothLook.rotation = Quaternion.Slerp(smoothLook.rotation, quaternion, smoothLerp * Time.deltaTime);
                            }
                        }
                    }
                }
            }
        }

        private void SetSmoothing()
        {
            float value = 1f - smooth;
            smoothLerp = 16.126f * value * value - 1.286f * value + 0.287f;
        }

        private void CameraRadarSearch()
        {
            if (!searchStarted && autoSearch)
            {
                searchStarted = true;
                StopCoroutine(SearchForTarget());
                StartCoroutine(SearchForTarget());
            }
        }

        private Transform GetMostValuableBlock(HashSet<Machine.SimCluster> simClusterForSearch)
        {
            //Remove any null cluster
            simClusterForSearch.RemoveWhere(cluster => cluster == null);

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
            int maxValue = targetValue.Max();
            for (i = 0; i < targetValue.Length; i++)
            {
                if (targetValue[i] == maxValue)
                {
                    maxClusters.Add(clusterArray[i]);
                }
            }

            //Find the target that's closest to the centre of the view
            int closestIndex = 0;
            float angleDiffMin = 180f;

            for (i = 0; i < maxClusters.Count; i++)
            {
                float angleDiffCurrent = Vector3.Angle((maxClusters[i].Base.gameObject.transform.position - smoothLook.position).normalized, smoothLook.forward);
                if (angleDiffCurrent < angleDiffMin)
                {
                    closestIndex = i;
                    angleDiffMin = angleDiffCurrent;
                }
            }

            return maxClusters[closestIndex].Base.gameObject.transform;
        }

        IEnumerator SearchForTarget()
        {
            //Grab every machine block at the start of search
            HashSet<Machine.SimCluster> simClusters = new HashSet<Machine.SimCluster>();

            if (StatMaster.isMP)
            {
                foreach (var player in Playerlist.Players)
                {
                    if (!player.isSpectator)
                    {
                        if (player.machine.isSimulating && !player.machine.LocalSim && player.machine.PlayerID != fixedCamera.ParentMachine.PlayerID)
                        {
                            if (fixedCamera.Team == MPTeam.None || fixedCamera.Team != player.team)
                            {
                                simClusters.UnionWith(player.machine.simClusters);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var cluster in global::Machine.Active().simClusters)
                {
                    if ((cluster.Base.transform.position - fixedCamera.Position).magnitude > safetyRadius)
                    {
                        simClusters.Add(cluster);
                    }
                }
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired && simClusters.Count > 0)
            {
                // Remove any null cluster due to stopped simulation
                simClusters.RemoveWhere(cluster => cluster == null);

                HashSet<Machine.SimCluster> simClusterForSearch = new HashSet<Machine.SimCluster>(simClusters);
                HashSet<Machine.SimCluster> unwantedClusters = new HashSet<Machine.SimCluster>();

                foreach (var cluster in simClusters)
                {
                    Vector3 positionDiff = cluster.Base.gameObject.transform.position - smoothLook.position;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, smoothLook.forward);
                    bool forward = Vector3.Dot(positionDiff, smoothLook.forward) > 0;
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
                    target = GetMostValuableBlock(simClusterForSearch);
                    SaveTargetToController();
                    targetAquired = true;
                    pauseTracking = false;
                    searchStarted = false;
                    StopCoroutine(SearchForTarget());
                }
                yield return null;
            }
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
   
        private void SaveTargetToController()
        {
            if (target != null)
            {
                FindObjectOfType<Controller>().targetSavedInController = target;
#if DEBUG
                Debug.Log("Target saved to controller");
#endif
            }
        }

        //private void OnGUI()
        //{
        //    if (fixedCameraController != null)
        //    {
        //        if (cameraLookAtToggled && fixedCameraController.activeCamera != null)
        //        {
        //            if (fixedCameraController.activeCamera.CompositeTracker3 == smoothLook)
        //            {
        //                if ((Time.time - switchTime) / Time.timeScale <= displayTime)
        //                {
        //                    GUI.TextArea(new Rect(1, 1, 20, 150), "CAM TRACKING: " + (autoSearch ? "AUTO" : "MANUAL"), camModeStyle);
        //                }
        //            }
        //        }
        //    }
        //}

        readonly GUIStyle camModeStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft,
        };

        private void SendTargetToClient()
        {
            if (clientTarget != null)
            {
#if DEBUG
                Debug.Log("Sending target to client");
#endif
                if (clientTarget.gameObject.GetComponent<BlockBehaviour>())
                {
#if DEBUG
                    Debug.Log("Target is a block");
#endif
                    Message targetBlockBehaviourMsg = Messages.cameraTargetBlockBehaviourMsg.CreateMessage(clientTarget.gameObject.GetComponent<BlockBehaviour>());
                    ModNetworking.SendTo(Player.GetAllPlayers()[clientPlayerID], targetBlockBehaviourMsg);
                }
                if (clientTarget.gameObject.GetComponent<LevelEntity>())
                {
#if DEBUG
                    Debug.Log("Target is a level entity");
#endif
                    Message targetEntityMsg = Messages.cameraTargetEntityMsg.CreateMessage(clientTarget.gameObject.GetComponent<LevelEntity>());
                    ModNetworking.SendTo(Player.GetAllPlayers()[clientPlayerID], targetEntityMsg);
                }
            }
        }

        private void SendRayToHost(Ray ray)
        {
            Message cameraRayToHostMsg = Messages.cameraRayToHostMsg.CreateMessage(ray.origin, ray.direction);
            ModNetworking.SendToHost(cameraRayToHostMsg);
#if DEBUG
            ConsoleController.ShowMessage("Message Sent to Host");
#endif
        }
    }
}