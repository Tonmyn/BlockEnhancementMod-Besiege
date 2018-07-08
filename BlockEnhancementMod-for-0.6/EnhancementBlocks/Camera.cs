using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class CameraScript : EnhancementBlock
    {
        //General setting
        MToggle CameraLookAtToggle;
        public bool cameraLookAtToggled = false;
        public int selfIndex;
        public FixedCameraBlock fixedCamera;
        public FixedCameraBlock fixedCameraSim;
        public Transform smoothLook;
        public FixedCameraController fixedCameraController;
        public Quaternion defaultLocalRotation;
        public float smooth;
        public float smoothLerp;

        //Track target setting
        MKey LockTargetKey;
        public Transform target;
        public HashSet<Transform> explodedTarget = new HashSet<Transform>();
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //Pause tracking setting
        MKey PauseTrackingKey;
        public bool pauseTracking = false;
        public List<KeyCode> resetKeys = new List<KeyCode> { KeyCode.X };

        //Record target related setting
        MToggle RecordTargetToggle;
        public bool recordTarget = false;

        //Auto lookat related setting
        MSlider AutoLookAtSearchAngleSlider;
        MSlider NonCustomModeSmoothSlider;
        MKey AutoLookAtKey;
        MKey LaunchKey;
        public bool nonCustomMode = false;
        public float nonCustomSmooth = 0.25f;
        public float timeOfDestruction = 0f;
        public float targetSwitchDelay = 1.25f;
        public List<KeyCode> activeGuideKeys = new List<KeyCode> { KeyCode.RightShift };
        public float searchAngle = 90;
        public float searchRadius = 0;
        public float safetyRadius = 25f;
        public float searchSurroundingBlockRadius = 5f;
        public bool autoSearch = true;
        public bool targetAquired = false;
        public bool searchStarted = false;
        public bool restartSearch = false;
        private Collider[] hitsIn;
        private Collider[] hitsOut;
        private Collider[] hitList;

        protected override void SafeAwake()
        {
            CameraLookAtToggle = AddToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) =>
            {
                cameraLookAtToggled =
                RecordTargetToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                PauseTrackingKey.DisplayInMapper =
                NonCustomModeSmoothSlider.DisplayInMapper =
                AutoLookAtKey.DisplayInMapper =
                AutoLookAtSearchAngleSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { cameraLookAtToggled = CameraLookAtToggle.IsActive; };

            RecordTargetToggle = AddToggle("记录目标", "RecordTarget", recordTarget);
            RecordTargetToggle.Toggled += (bool value) =>
            {
                recordTarget = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { recordTarget = RecordTargetToggle.IsActive; };

            AutoLookAtSearchAngleSlider = AddSlider("搜索角度", "searchAngle", searchAngle, 0, 90, false);
            AutoLookAtSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { searchAngle = AutoLookAtSearchAngleSlider.Value; };

            NonCustomModeSmoothSlider = AddSlider("非Custom平滑", "nonCustomSmooth", nonCustomSmooth, 0, 1, false);
            NonCustomModeSmoothSlider.ValueChanged += (float value) => { nonCustomSmooth = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { nonCustomSmooth = NonCustomModeSmoothSlider.Value; };

            LockTargetKey = AddKey("锁定目标", "LockTarget", lockKeys);

            PauseTrackingKey = AddKey("暂停/恢复追踪", "ResetView", resetKeys);

            AutoLookAtKey = AddKey("主动/手动搜索切换", "ActiveSearchKey", activeGuideKeys);

            // Add reference to the camera's buildindex
            fixedCamera = GetComponent<FixedCameraBlock>();
            defaultLocalRotation = fixedCamera.CompositeTracker3.localRotation;
            selfIndex = fixedCamera.BuildIndex;


#if DEBUG
            //ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            if (fixedCamera.CamMode != FixedCameraBlock.Mode.Custom)
            {
                nonCustomMode = true;
            }
            ConsoleController.ShowMessage(fixedCamera.CamMode.ToString());
            CameraLookAtToggle.DisplayInMapper = value;
            fixedCameraController = FindObjectOfType<FixedCameraController>();
            NonCustomModeSmoothSlider.DisplayInMapper = value && cameraLookAtToggled && nonCustomMode;
            AutoLookAtKey.DisplayInMapper = value && cameraLookAtToggled;
            AutoLookAtSearchAngleSlider.DisplayInMapper = value && cameraLookAtToggled;
            RecordTargetToggle.DisplayInMapper = value && cameraLookAtToggled;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
            PauseTrackingKey.DisplayInMapper = value && cameraLookAtToggled;
        }

        public override void LoadConfiguration(XDataHolder BlockData)
        {
            if (BlockData.HasKey("bmt-" + "CameraTarget"))
            {
                SaveTargetToDict(BlockData.ReadInt("bmt-" + "CameraTarget"));
            }
        }

        public override void SaveConfiguration(XDataHolder BlockData)
        {
            if (Machine.Active().GetComponent<TargetScript>().previousTargetDic.ContainsKey(selfIndex))
            {
                BlockData.Write("bmt-" + "CameraTarget", Machine.Active().GetComponent<TargetScript>().previousTargetDic[selfIndex]);
            }
        }

        protected override void OnBuildingUpdate()
        {
            if (fixedCamera.CamMode != FixedCameraBlock.Mode.Custom && !nonCustomMode)
            {
                nonCustomMode = true;
                NonCustomModeSmoothSlider.DisplayInMapper = base.EnhancementEnable && cameraLookAtToggled && nonCustomMode;
            }
            if (fixedCamera.CamMode == FixedCameraBlock.Mode.Custom && nonCustomMode)
            {
                nonCustomMode = false;
                NonCustomModeSmoothSlider.DisplayInMapper = base.EnhancementEnable && cameraLookAtToggled && nonCustomMode;
            }
        }

        protected override void OnSimulateStart()
        {
            if (cameraLookAtToggled)
            {
                //Initialise the SmoothLook component
                fixedCameraController = FindObjectOfType<FixedCameraController>();
                foreach (var camera in fixedCameraController.cameras)
                {
                    if (camera == fixedCamera)
                    {
                        fixedCameraSim = camera;
                        smoothLook = camera.CompositeTracker3;
                        if (nonCustomMode)
                        {
                            smooth = Mathf.Clamp01(nonCustomSmooth);
                        }
                        else
                        {
                            smooth = Mathf.Clamp01(camera.Sliders.First(s => s.Key == "smooth").Value);
                        }
                        SetSmoothing();
                    }
                }

                // Initialise
                searchStarted = false;
                pauseTracking = autoSearch = targetAquired = true;
                searchRadius = Camera.main.farClipPlane;
                float searchAngleMax = Mathf.Clamp(Mathf.Atan(Mathf.Tan(fixedCameraSim.fovSlider.Value * Mathf.Deg2Rad / 2) * Camera.main.aspect) * Mathf.Rad2Deg, 0, 90);
                searchAngle = Mathf.Clamp(searchAngle, 0, searchAngleMax);
                target = null;
                explodedTarget.Clear();
                hitsIn = Physics.OverlapSphere(fixedCameraSim.CompositeTracker3.position, safetyRadius);
                StopAllCoroutines();

                // If target is recorded, try preset it.
                if (recordTarget)
                {
                    // Trying to read previously saved target
                    int targetIndex = -1;
                    BlockBehaviour targetBlock = new BlockBehaviour();
                    // Read the target's buildIndex from the dictionary
                    if (!Machine.Active().GetComponent<TargetScript>().previousTargetDic.TryGetValue(selfIndex, out targetIndex))
                    {
                        target = null;
                        return;
                    }
                    // Aquire target block's transform from the target's index
                    try
                    {

                        Machine.Active().GetBlockFromIndex(targetIndex, out targetBlock);
                        target = Machine.Active().GetSimBlock(targetBlock).transform;
                    }
                    catch (Exception)
                    {
                        ConsoleController.ShowMessage("Cannot get target block's transform");
                    }
                }
            }
        }

        protected override void OnSimulateUpdate()
        {
            if (cameraLookAtToggled && fixedCameraController.activeCamera == fixedCamera)
            {
                if (AutoLookAtKey.IsReleased)
                {
                    autoSearch = !autoSearch;
                }
                if (PauseTrackingKey.IsReleased)
                {
                    pauseTracking = !pauseTracking;
                }
                if (LockTargetKey.IsReleased)
                {
                    if (autoSearch)
                    {
                        target = null;
                        targetAquired = searchStarted = pauseTracking = false;
                    }
                    else
                    {
                        // Aquire the target to look at
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        float manualSearchRadius = 1.5f;
                        RaycastHit[] hits = Physics.SphereCastAll(ray.origin, manualSearchRadius, ray.direction, Mathf.Infinity);
                        Physics.Raycast(ray, out RaycastHit rayHit);
                        for (int i = 0; i < hits.Length; i++)
                        {
                            try
                            {
                                int index = hits[i].transform.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                                target = hits[i].transform;
                                if (recordTarget)
                                {
                                    SaveTargetToDict(index);
                                }
                                break;
                            }
                            catch { }
                            if (i == hits.Length - 1)
                            {
                                target = rayHit.transform;
                                try
                                {
                                    int index = rayHit.transform.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                                    if (recordTarget)
                                    {
                                        SaveTargetToDict(index);
                                    }
                                    break;
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (cameraLookAtToggled && fixedCameraController.activeCamera == fixedCamera)
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
                            //ConsoleController.ShowMessage("Target rocket exploded");
                            explodedTarget.Add(target);
                            targetAquired = false;
                            target = null;
                            timeOfDestruction = Time.time;
                            return;
                        }
                    }
                    catch { }
                    try
                    {
                        if (target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                        {
                            //ConsoleController.ShowMessage("Target bomb exploded");
                            explodedTarget.Add(target);
                            targetAquired = false;
                            target = null;
                            timeOfDestruction = Time.time;
                            return;
                        }
                    }
                    catch { }
                    try
                    {
                        if (target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                        {
                            //ConsoleController.ShowMessage("Target level bomb exploded");
                            explodedTarget.Add(target);
                            targetAquired = false;
                            target = null;
                            timeOfDestruction = Time.time;
                            return;
                        }
                    }
                    catch { }
                    try
                    {
                        if (target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                        {
                            //ConsoleController.ShowMessage("Target grenade exploded");
                            explodedTarget.Add(target);
                            targetAquired = false;
                            target = null;
                            timeOfDestruction = Time.time;
                            return;
                        }
                    }
                    catch { }
                }

            }
        }

        protected override void OnSimulateLateUpdate()
        {
            if (cameraLookAtToggled && fixedCameraController.activeCamera == fixedCamera)
            {

#if DEBUG
                //ConsoleController.ShowMessage("there are " + explodedTarget.Count + " targets");
#endif
                if (pauseTracking || target == null)
                {
                    smoothLook.localRotation = Quaternion.Slerp(smoothLook.localRotation, defaultLocalRotation, smoothLerp * Time.deltaTime);
                }
                else
                {
                    if (Time.time - timeOfDestruction >= targetSwitchDelay)
                    {
                        Quaternion quaternion;
                        if (fixedCameraSim.CamMode == FixedCameraBlock.Mode.FirstPerson)
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

        private void SaveTargetToDict(int BlockID)
        {
            // Make sure the dupicated key exception is handled
            try
            {
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Add(selfIndex, BlockID);
            }
            catch (Exception)
            {
                // Remove the old record, then add the new record
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Remove(selfIndex);
                Machine.Active().GetComponent<TargetScript>().previousTargetDic.Add(selfIndex, BlockID);
            }
        }

        private void SetSmoothing()
        {
            float value = 1f - smooth;
            smoothLerp = 16.126f * value * value - 1.286f * value + 0.287f;
        }

        private void CameraRadarSearch()
        {
            if (!searchStarted)
            {
                searchStarted = true;
                StopCoroutine(SearchForTarget());
                StartCoroutine(SearchForTarget());
            }
        }

        private Transform GetMostValuableBlock(HashSet<Transform> transformSet)
        {
            //Search for any blocks within the search radius for every block in the hitlist
            float[] targetValue = new float[transformSet.Count];
            Transform[] transformArray = new Transform[transformSet.Count];
            HashSet<Transform> tempTransform = new HashSet<Transform>();
            List<Transform> maxTransform = new List<Transform>();

            //Start searching
            int i = 0;
            foreach (var targetTransform in transformSet)
            {
                //Count how many colliders are around this particular collider
                Collider[] hitsAroundBlock = Physics.OverlapSphere(targetTransform.position, searchSurroundingBlockRadius);
                tempTransform.Clear();
                int count = 0;
                foreach (var hitBlock in hitsAroundBlock)
                {
                    try
                    {
                        int index = hitBlock.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                        if (tempTransform.Add(hitBlock.attachedRigidbody.gameObject.transform))
                        {
                            count++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                targetValue[i] = count;
                transformArray[i] = targetTransform;

                //Some blocks weights more than others
                GameObject targetObj = targetTransform.gameObject;
                //A bomb
                if (targetObj.GetComponent<ExplodeOnCollideBlock>())
                {
                    targetValue[i] = targetValue[i] * 70;
                }
                //A fired rocket
                if (targetObj.GetComponent<TimedRocket>())
                {
                    if (targetObj.GetComponent<TimedRocket>().hasFired)
                    {
                        targetValue[i] = targetValue[i] * 150;
                    }

                }
                //A fired watercannon
                if (targetObj.GetComponent<WaterCannonController>())
                {
                    if (targetObj.GetComponent<WaterCannonController>().isActive)
                    {
                        targetValue[i] = targetValue[i] * 50;
                    }
                }
                //A flying flying-block
                if (targetObj.GetComponent<FlyingController>())
                {
                    if (targetObj.GetComponent<FlyingController>().canFly)
                    {
                        targetValue[i] = targetValue[i] * 20;
                    }
                }
                i++;
            }
            //Find the block that has the max number of blocks around it
            float max = targetValue.Max();
            for (i = 0; i < targetValue.Length; i++)
            {
                if (targetValue[i] == max)
                {
                    maxTransform.Add(transformArray[i]);
                }
            }

            //Find the target that's closest to the centre of the view
            int closestIndex = 0;
            float angleDiffMin = Vector3.Angle((maxTransform[closestIndex].position - fixedCameraSim.CompositeTracker3.position).normalized, fixedCameraSim.CompositeTracker3.forward);
#if DEBUG
            ConsoleController.ShowMessage("Angle min last is " + angleDiffMin + " for " + closestIndex);
#endif
            for (i = 1; i < maxTransform.Count; i++)
            {
                float angleDiffCurrent = Vector3.Angle((maxTransform[i].position - fixedCameraSim.CompositeTracker3.position).normalized, fixedCameraSim.CompositeTracker3.forward);
#if DEBUG
                ConsoleController.ShowMessage("Angle min last is " + angleDiffCurrent + " for " + i);
#endif
                if (angleDiffCurrent < angleDiffMin)
                {
                    closestIndex = i;
                    angleDiffMin = angleDiffCurrent;
                }
            }
#if DEBUG
            ////ConsoleController.ShowMessage("There are " + maxTransform.Count() + " in max transform");
            ////ConsoleController.ShowMessage("Angle min last is " + angleDiffMin);
#endif
            return maxTransform[closestIndex];
        }

        IEnumerator SearchForTarget()
        {
            //Grab every machine block at the start of search
            hitsOut = Physics.OverlapSphere(smoothLook.position, searchRadius);
            HashSet<Transform> transformSet = new HashSet<Transform>();

            if (StatMaster._customLevelSimulating)
            {
                hitList = hitsOut;
            }
            else
            {
                //hitsIn = hitsIn.Where(hit => hit != null).ToArray();
                hitList = hitsOut.Except(hitsIn).ToArray();
            }
            foreach (var hit in hitList)
            {
                try
                {
                    int index = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>().BuildIndex;
                    transformSet.Add(hit.attachedRigidbody.gameObject.transform);
                }
                catch { }
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired)
            {
                HashSet<Transform> transformSetForSearch = new HashSet<Transform>(transformSet);
                HashSet<Transform> unwantedTransforms = new HashSet<Transform>();
                foreach (var targetTransform in transformSet)
                {
                    // Remove anything that's exploded
                    if (explodedTarget.Contains(targetTransform))
                    {
                        unwantedTransforms.Add(targetTransform);
                        continue;
                    }

                    // Remove anything that's not in the view
                    Vector3 positionDiff = targetTransform.position - fixedCameraSim.CompositeTracker3.position;
                    bool forward = Vector3.Dot(positionDiff, fixedCameraSim.CompositeTracker3.forward) > 0;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, fixedCameraSim.CompositeTracker3.forward);

                    if (!(forward && angleDiff < searchAngle))
                    {
                        unwantedTransforms.Add(targetTransform);
                        continue;
                    }

                    // Remove anything that's not in the same team
                    BlockBehaviour targetBB = targetTransform.gameObject.GetComponent<BlockBehaviour>();
                    if (StatMaster._customLevelSimulating)
                    {
                        if (targetBB.Team != MPTeam.None)
                        {
                            //If the block belongs to a team that is not none
                            //and is the same as the rocket, remove it from the hashset
                            if (targetBB.Team == fixedCamera.Team)
                            {
                                unwantedTransforms.Add(targetTransform);
                                continue;
                            }
                        }
                        else
                        {
                            //If no team is assigned to a block
                            //only remove it when in multiverse
                            //and the parentmachine name is the same as the rocket's parent machine
                            if (targetBB.ParentMachine.Name == fixedCamera.ParentMachine.Name)
                            {
                                unwantedTransforms.Add(targetTransform);
                                continue;
                            }
                        }
                    }
                }
                transformSetForSearch.ExceptWith(unwantedTransforms);
                //Try to find the most valuable block
                //i.e. has the most number of blocks around it within a certain radius
                //when the hitlist is not empty
                if (transformSetForSearch.Count > 0)
                {
                    //Search for any blocks within the search radius for every block in the hitlist
                    //Find the block that has the max number of colliders around it
                    //Take that block as the target
                    target = GetMostValuableBlock(transformSetForSearch);
                    targetAquired = true;
                    searchStarted = false;
                    StopCoroutine(SearchForTarget());
                }
                yield return null;
            }
        }
    }
}