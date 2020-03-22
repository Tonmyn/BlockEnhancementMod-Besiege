using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockEnhancementMod.Blocks;

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
        public MouseOrbit mouseOrbit;
        private Quaternion defaultLocalRotation;
        public float smooth;
        public float smoothLerp;
        private float newCamFOV, orgCamFOV, camFOVSmooth;
        private bool firstPerson = false;

        //Track target setting
        MKey LockTargetKey;
        public Transform target;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };
        private List<Collider> blockColliders = new List<Collider>();
        private HashSet<Machine.SimCluster> clustersInSafetyRange = new HashSet<Machine.SimCluster>();

        //Pause tracking setting
        MKey PauseTrackingKey;
        public bool pauseTracking = false;
        public List<KeyCode> pauseKeys = new List<KeyCode> { KeyCode.X };

        //Auto lookat related setting
        MSlider NonCustomModeSmoothSlider;
        MKey AutoLookAtKey;
        MKey ZoomInKey;
        MKey ZoomOutKey;
        MSlider ZoomSpeedSlider;
        MMenu ZoomControlModeMenu;
        private int zoomControlModeIndex = 0;
        private float zoomSpeed = 2f;
        public List<string> zoomControlMode = new List<string>() { LanguageManager.Instance.CurrentLanguage.MouseWheelZoomControl, LanguageManager.Instance.CurrentLanguage.KeyboardZoomControl };
        private bool firstPersonMode = false;
        private bool targetInitialCJOrHJ = false;
        public float firstPersonSmooth = 0.25f;
        private float timeOfDestruction = 0f;
        private readonly float targetSwitchDelay = 1.25f;
        public List<KeyCode> activeGuideKeys = new List<KeyCode> { KeyCode.RightShift };
        private float searchAngle = 90;
        private readonly float safetyRadiusAuto = 50f;
        private readonly float safetyRadiusManual = 15f;
        private bool autoSearch = true;
        private bool targetAquired = false;
        private bool searchStarted = false;
        private readonly float displayTime = 1f;
        private float switchTime = Mathf.NegativeInfinity;
        private bool activateTimeRecorded = false;

        //Cluster value multiplier
        private readonly int bombValue = 64;
        private readonly int guidedRocketValue = 1024;
        private readonly int normalRocketValue = 512;
        private readonly int waterCannonValue = 16;
        private readonly int flyingBlockValue = 2;
        private readonly int flameThrowerValue = 8;
        private readonly int cogMotorValue = 2;

        public override void SafeAwake()
        {
            CameraLookAtToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.TrackTarget, "TrackingCamera", cameraLookAtToggled);
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

            ZoomControlModeMenu = BB.AddMenu(LanguageManager.Instance.CurrentLanguage.ZoomControlMode, zoomControlModeIndex, zoomControlMode, false);
            ZoomControlModeMenu.ValueChanged += (int value) =>
            {
                zoomControlModeIndex = value;
                ChangedProperties();
            };

            NonCustomModeSmoothSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.FirstPersonSmooth, "nonCustomSmooth", firstPersonSmooth, 0, 1);
            NonCustomModeSmoothSlider.ValueChanged += (float value) => { firstPersonSmooth = value; ChangedProperties(); };

            LockTargetKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.LockTarget, "LockTarget", KeyCode.Delete);

            PauseTrackingKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.PauseTracking, "ResetView", KeyCode.X);

            AutoLookAtKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.SwitchGuideMode, "ActiveSearchKey", KeyCode.RightShift);

            ZoomInKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.ZoomIn, "ZoomInKey", KeyCode.Equals);

            ZoomOutKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.ZoomOut, "ZoomOutKey", KeyCode.Minus);

            ZoomSpeedSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.ZoomSpeed, "ZoomSpeed", zoomSpeed, 0, 20);
            ZoomSpeedSlider.ValueChanged += (float value) => { zoomSpeed = value; ChangedProperties(); };

            // Add reference to the camera's buildindex
            fixedCamera = GetComponent<FixedCameraBlock>();
            smoothLook = fixedCamera.CompositeTracker3;
            defaultLocalRotation = smoothLook.localRotation;
            selfIndex = fixedCamera.BuildIndex;
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
            ZoomInKey.DisplayInMapper = value;
            ZoomOutKey.DisplayInMapper = value;
            ZoomSpeedSlider.DisplayInMapper = value;
            ZoomControlModeMenu.DisplayInMapper = value;
            CameraLookAtToggle.DisplayInMapper = value;
            NonCustomModeSmoothSlider.DisplayInMapper = value && cameraLookAtToggled && firstPersonMode;
            AutoLookAtKey.DisplayInMapper = value && cameraLookAtToggled;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
            PauseTrackingKey.DisplayInMapper = value && cameraLookAtToggled;
        }

        public override void BuildingUpdateAlways_EnhancementEnabled()
        {
            if (fixedCamera.CamMode != FixedCameraBlock.Mode.FirstPerson && firstPersonMode)
            {
                firstPersonMode = false;
                NonCustomModeSmoothSlider.DisplayInMapper = cameraLookAtToggled && firstPersonMode;
                ZoomControlModeMenu.DisplayInMapper = ZoomSpeedSlider.DisplayInMapper = firstPersonMode;
            }
            if (fixedCamera.CamMode == FixedCameraBlock.Mode.FirstPerson && !firstPersonMode)
            {
                firstPersonMode = true;
                NonCustomModeSmoothSlider.DisplayInMapper = cameraLookAtToggled && firstPersonMode;
                ZoomControlModeMenu.DisplayInMapper = ZoomSpeedSlider.DisplayInMapper = firstPersonMode;
            }
            if (ZoomControlModeMenu.DisplayInMapper)
            {
                if (!ZoomInKey.DisplayInMapper && !ZoomOutKey.DisplayInMapper && zoomControlModeIndex == 1)
                {
                    ZoomInKey.DisplayInMapper = ZoomOutKey.DisplayInMapper = true;
                }
                if (ZoomInKey.DisplayInMapper && ZoomOutKey.DisplayInMapper && zoomControlModeIndex == 0)
                {
                    ZoomInKey.DisplayInMapper = ZoomOutKey.DisplayInMapper = false;
                }
            }
            if (!ZoomControlModeMenu.DisplayInMapper && ZoomInKey.DisplayInMapper && ZoomOutKey.DisplayInMapper)
            {
                ZoomInKey.DisplayInMapper = ZoomOutKey.DisplayInMapper = ZoomControlModeMenu.DisplayInMapper;
            }
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            firstPerson = fixedCamera.CamMode == FixedCameraBlock.Mode.FirstPerson;

            //Initialise the SmoothLook component
            fixedCameraController = FindObjectOfType<FixedCameraController>();
            mouseOrbit = FindObjectOfType<MouseOrbit>();
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
            if (cameraLookAtToggled)
            {
                // Initialise
                searchStarted = targetInitialCJOrHJ = false;
                pauseTracking = autoSearch = targetAquired = true;
                float searchAngleMax = Mathf.Clamp(Mathf.Atan(Mathf.Tan(fixedCamera.fovSlider.Value * Mathf.Deg2Rad / 2) * Camera.main.aspect) * Mathf.Rad2Deg, 0, 90);
                searchAngle = Mathf.Clamp(searchAngle, 0, searchAngleMax);
                target = null;
                if (!StatMaster.isMP)
                {
                    clustersInSafetyRange.Clear();
                    foreach (var cluster in Machine.Active().simClusters)
                    {
                        if ((cluster.Base.transform.position - fixedCamera.transform.position).magnitude < safetyRadiusAuto)
                        {
                            clustersInSafetyRange.Add(cluster);
                        }
                    }
                }
                StopAllCoroutines();
            }
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (fixedCameraController?.activeCamera?.CompositeTracker3 == smoothLook)
            {
                if (firstPerson)
                {
                    Camera activeCam = mouseOrbit.cam;
                    if (zoomControlModeIndex == 0)
                    {
                        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                        {
                            newCamFOV = Mathf.Clamp(activeCam.fieldOfView - Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")) * zoomSpeed, 1, orgCamFOV);
                        }
                    }
                    else
                    {
                        if (ZoomInKey.IsHeld || ZoomInKey.EmulationHeld())
                        {
                            newCamFOV = Mathf.Clamp(activeCam.fieldOfView - zoomSpeed, 1, orgCamFOV);
                        }
                        if (ZoomOutKey.IsHeld || ZoomOutKey.EmulationHeld())
                        {
                            newCamFOV = Mathf.Clamp(activeCam.fieldOfView + zoomSpeed, 1, orgCamFOV);
                        }
                    }
                    if (activeCam.fieldOfView != newCamFOV)
                    {
                        activeCam.fieldOfView = Mathf.SmoothStep(activeCam.fieldOfView, newCamFOV, camFOVSmooth);
                    }
                }
                if (cameraLookAtToggled)
                {
                    if (!activateTimeRecorded)
                    {
                        switchTime = Time.time;
                        activateTimeRecorded = true;
                    }
                    if (AutoLookAtKey.IsReleased || AutoLookAtKey.EmulationReleased())
                    {
                        autoSearch = !autoSearch;
                        switchTime = Time.time;
                    }
                    if (PauseTrackingKey.IsReleased || PauseTrackingKey.EmulationReleased())
                    {
                        pauseTracking = !pauseTracking;
                    }
                    if (LockTargetKey.IsReleased || LockTargetKey.EmulationReleased())
                    {
                        target = null;
                        if (autoSearch)
                        {
                            targetAquired = searchStarted = false;
                            CameraRadarSearch();
                        }
                        else
                        {
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
                            if (hits.Length > 0)
                            {
                                for (int i = 0; i < hits.Length; i++)
                                {
                                    if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                                    {
                                        if ((hits[i].transform.position - fixedCamera.transform.position).magnitude >= safetyRadiusManual)
                                        {
                                            target = hits[i].transform;
                                            pauseTracking = false;
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
                                            if ((hits[i].transform.position - fixedCamera.transform.position).magnitude >= safetyRadiusManual)
                                            {
                                                target = hits[i].transform;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (target == null && rayHit.transform != null)
                            {
                                if ((rayHit.transform.position - fixedCamera.transform.position).magnitude >= safetyRadiusManual)
                                {
                                    target = rayHit.transform;
                                    pauseTracking = false;
                                }
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
                if (fixedCameraController.activeCamera != fixedCamera)
                {
                    if (activateTimeRecorded)
                    {
                        activateTimeRecorded = false;
                    }
                }
            }
        }

        public override void SimulateFixedUpdate_EnhancementEnabled()
        {
            if (cameraLookAtToggled)
            {
                if (fixedCameraController?.activeCamera?.CompositeTracker3 == smoothLook)
                {
                    if (autoSearch && !targetAquired)
                    {
                        CameraRadarSearch();
                    }
                    if (target != null)
                    {
                        Debug.Log("??");
                        try
                        {
                            if (targetInitialCJOrHJ)
                            {
                                if (target.gameObject.GetComponent<ConfigurableJoint>() == null && target.gameObject.GetComponent<HingeJoint>() == null)
                                {
                                    timeOfDestruction = Time.time;
                                    targetInitialCJOrHJ = false;
                                    targetAquired = false;
                                    target = null;
                                    return;
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<TimedRocket>().hasExploded)
                            {
                                timeOfDestruction = Time.time;
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

        public override void SimulateLateUpdate_EnhancementEnabled()
        {
            if (cameraLookAtToggled)
            {
                if (fixedCameraController?.activeCamera?.CompositeTracker3 == smoothLook)
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
                                    quaternion = Quaternion.LookRotation(target.position - smoothLook.position, fixedCamera.CompositeTracker2.up);
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

        private void GetMostValuableCluster(HashSet<Machine.SimCluster> simClusterForSearch, out Transform targetTransform, out float targetClusterValue)
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

            targetTransform = maxClusters[closestIndex].Base.gameObject.transform;
            targetClusterValue = maxValue;
        }

        private bool CheckInRange(BlockBehaviour target)
        {
            Vector3 positionDiff = target.gameObject.transform.position - smoothLook.position;
            float angleDiff = Vector3.Angle(positionDiff.normalized, smoothLook.forward);
            bool forward = Vector3.Dot(positionDiff, smoothLook.forward) > 0;
            return forward && angleDiff < searchAngle;
        }

        IEnumerator SearchForTarget()
        {
            // First test the rockets that are fired
            Dictionary<BlockBehaviour, int> rocketTargetDict = RocketsController.Instance.rocketTargetDict;
            Transform rocketTarget = null;
            Transform clusterTarget = null;
            float rocketValue = 0;
            float clusterValue = 0;

            if (rocketTargetDict != null)
            {
                if (rocketTargetDict.Count > 0)
                {
                    float distance = Mathf.Infinity;
                    foreach (var rocketTargetPair in rocketTargetDict)
                    {
                        BlockBehaviour targetRocket = rocketTargetPair.Key;
                        if (targetRocket != null)
                        {
                            bool shouldCheckRocket = false;
                            if (StatMaster.isMP)
                            {
                                shouldCheckRocket = targetRocket.ParentMachine.PlayerID != fixedCamera.ParentMachine.PlayerID && (fixedCamera.Team == MPTeam.None || fixedCamera.Team != targetRocket.Team);
                            }
                            else
                            {
                                if (targetRocket.ClusterIndex == -1)
                                {
                                    shouldCheckRocket = (targetRocket.transform.position - fixedCamera.transform.position).magnitude > safetyRadiusAuto;
                                }
                                else
                                {
                                    int count = 0;
                                    foreach (var cluster in clustersInSafetyRange)
                                    {
                                        if (cluster.Base.ClusterIndex == targetRocket.ClusterIndex)
                                        {
                                            count++;
                                        }
                                    }
                                    shouldCheckRocket = count > 0 ? false : true;
                                }

                            }
                            if (CheckInRange(targetRocket) && shouldCheckRocket)
                            {
                                float tempDistance = (targetRocket.transform.position - fixedCamera.transform.position).magnitude;
                                if (tempDistance <= distance)
                                {
                                    rocketTarget = targetRocket.transform;
                                    distance = tempDistance;
                                    rocketValue = guidedRocketValue;
                                }
                            }
                        }
                    }
                }
            }
            yield return new WaitForEndOfFrame();

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
                simClusters.UnionWith(Machine.Active().simClusters);
                clustersInSafetyRange.RemoveWhere(cluster => cluster == null);

                if (clustersInSafetyRange.Count > 0)
                {
                    simClusters.ExceptWith(clustersInSafetyRange);
                }
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired && simClusters.Count > 0)
            {
                try
                {
                    // Remove any null cluster due to stopped simulation
                    simClusters.RemoveWhere(cluster => cluster == null);

                    HashSet<Machine.SimCluster> simClusterForSearch = new HashSet<Machine.SimCluster>(simClusters);
                    HashSet<Machine.SimCluster> unwantedClusters = new HashSet<Machine.SimCluster>();

                    foreach (var cluster in simClusters)
                    {
                        bool inRange = CheckInRange(cluster.Base);
                        bool skipCluster = !(inRange) || ShouldSkipCluster(cluster.Base);

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
                        GetMostValuableCluster(simClusterForSearch, out clusterTarget, out clusterValue);
                    }
                }
                catch { }
                if (rocketTarget != null || clusterTarget != null)
                {
                    target = rocketValue >= clusterValue ? rocketTarget : clusterTarget;
                    targetAquired = true;
                    pauseTracking = false;
                    searchStarted = false;
                    targetInitialCJOrHJ = target.gameObject.GetComponent<ConfigurableJoint>() != null || target.gameObject.GetComponent<HingeJoint>() != null;
                }
                yield return null;
            }
        }

        private int CalculateClusterValue(BlockBehaviour block, int clusterValue)
        {
            //Some blocks weights more than others
            GameObject targetObj = block.gameObject;
            //A bomb
            if (/*block.Type*/block.BlockID == (int)BlockType.Bomb)
            {
                if (!targetObj.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                {
                    clusterValue *= bombValue;
                }
            }
            //A fired and unexploded rocket
            if (/*block.Type*/block.BlockID == (int)BlockType.Rocket)
            {
                if (targetObj.GetComponent<TimedRocket>().hasFired)
                {
                    if (targetObj.GetComponent<RocketScript>().radar != null)
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
            if (/*block.Type*/block.BlockID == (int)BlockType.WaterCannon)
            {
                if (targetObj.GetComponent<WaterCannonController>().isActive)
                {
                    clusterValue *= waterCannonValue;
                }
            }
            //A flying flying-block
            if (/*block.Type*/block.BlockID == (int)BlockType.FlyingBlock)
            {
                if (targetObj.GetComponent<FlyingController>().canFly)
                {
                    clusterValue *= flyingBlockValue;
                }
            }
            //A flaming flamethrower
            if (/*block.Type*/block.BlockID == (int)BlockType.Flamethrower)
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
                if (/*block.Type*/block.BlockID == (int)BlockType.Rocket)
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
            if (fixedCameraController?.activeCamera?.CompositeTracker3 == smoothLook && (Time.time - switchTime) / Time.timeScale <= displayTime)
            {
                GUI.TextArea(new Rect(1, 1, 20, 150), "CAM TRACKING: " + (autoSearch ? "AUTO" : "MANUAL"), camModeStyle);
            }
        }

        readonly GUIStyle camModeStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft,
        };
    }
}