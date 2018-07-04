using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class CameraScript : EnhancementBlock
    {
        //General setting
        BlockBehaviour cameraBB;
        MToggle CameraLookAtToggle;
        public bool cameraLookAtToggled = false;
        public int selfIndex;
        public FixedCameraBlock fixedCamera;
        public SmoothLookAt smoothLook;
        public FixedCameraController fixedCameraController;
        public Quaternion defaultLocalRotation;
        public float smooth;
        public float smoothLerp;

        //Track target setting
        MKey LockTargetKey;
        public Transform target;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //Pause tracking setting
        MKey ResetViewKey;
        public bool resetView = false;
        public bool viewAlreadyReset = false;
        public List<KeyCode> resetKeys = new List<KeyCode> { KeyCode.X };

        //Record target related setting
        MToggle RecordTargetToggle;
        public bool recordTarget = false;

        protected override void SafeAwake()
        {
            CameraLookAtToggle = AddToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = RecordTargetToggle.DisplayInMapper = LockTargetKey.DisplayInMapper = ResetViewKey.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { cameraLookAtToggled = CameraLookAtToggle.IsActive; };

            RecordTargetToggle = AddToggle("记录目标", "RecordTarget", recordTarget);
            RecordTargetToggle.Toggled += (bool value) =>
            {
                recordTarget = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { recordTarget = RecordTargetToggle.IsActive; };

            LockTargetKey = AddKey("锁定目标", "LockTarget", lockKeys);
            LockTargetKey.InvokeKeysChanged();

            ResetViewKey = AddKey("暂停/恢复追踪", "ResetView", resetKeys);
            ResetViewKey.InvokeKeysChanged();

            // Add reference to the camera's buildindex
            fixedCamera = GetComponent<FixedCameraBlock>();
            cameraBB = GetComponent<BlockBehaviour>();
            selfIndex = cameraBB.BuildIndex;

#if DEBUG
            ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            CameraLookAtToggle.DisplayInMapper = value;
            RecordTargetToggle.DisplayInMapper = value && cameraLookAtToggled;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
            ResetViewKey.DisplayInMapper = value && cameraLookAtToggled;
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

        protected override void OnSimulateStart()
        {
            if (cameraLookAtToggled)
            {
                fixedCameraController = FindObjectOfType<FixedCameraController>();
                foreach (var camera in fixedCameraController.cameras)
                {
                    if (camera == fixedCamera)
                    {
                        smoothLook = camera.CompoundTracker.gameObject.AddComponent<SmoothLookAt>();
                        defaultLocalRotation = camera.CompoundTracker.localRotation;
                        foreach (var slider in fixedCamera.Sliders)
                        {
                            if (slider.Key == "smooth")
                            {
                                smooth = slider.Value;
                            }
                        }
                        SetSmoothing();
                    }
                    else
                    {
                        ConsoleController.ShowMessage("Not this camera");
                    }
                }


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
                        if (!resetView)
                        {
                            smoothLook.target = target;
                        }
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
            if (cameraLookAtToggled)
            {
                if (ResetViewKey.IsReleased)
                {
                    resetView = !resetView;
                    if (resetView)
                    {
                        viewAlreadyReset = false;
                        smoothLook.target = null;
                    }
                    else
                    {
                        smoothLook.target = target;
                    }

                }
                if (LockTargetKey.IsReleased)
                {
                    // Aquire the target to look at
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        target = hit.transform;
                        resetView = false;
                        if (recordTarget)
                        {
                            // Trying to save target's buildIndex to the dictionary
                            // If not a machine block, set targetIndex to -1
                            int targetIndex = -1;
                            try
                            {
                                targetIndex = target.GetComponent<BlockBehaviour>().BuildIndex;
                            }
                            catch (Exception)
                            {
                                ConsoleController.ShowMessage("Not a machine block");
                            }
                            if (targetIndex != -1)
                            {
                                SaveTargetToDict(target.GetComponent<BlockBehaviour>().BuildIndex);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnSimulateLateUpdate()
        {
            if (cameraLookAtToggled && fixedCameraController.activeCamera == fixedCamera)
            {
                if (resetView && !viewAlreadyReset)
                {
                    smoothLook.transform.localRotation = Quaternion.Slerp(smoothLook.transform.localRotation, defaultLocalRotation, smoothLerp * Time.deltaTime);
                    if (smoothLook.transform.localRotation == defaultLocalRotation)
                    {
                        viewAlreadyReset = true;
                    }
                }
                if (!resetView && smoothLook.target != target)
                {
                    smoothLook.target = target;
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
            if (fixedCamera.CamMode != FixedCameraBlock.Mode.FirstPerson)
            {
                smoothLook.damping = smoothLerp = 16.126f * value * value - 1.286f * value + 0.287f;
            }
            else
            {
                smoothLook.damping = smoothLerp = 60f;
            }
        }
    }
}