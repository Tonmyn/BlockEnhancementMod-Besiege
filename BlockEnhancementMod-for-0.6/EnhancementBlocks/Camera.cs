using System;
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
        public Transform realCameraTransform;
        public Quaternion defaultRotation;

        //Track target setting
        MKey LockTargetKey;
        MSlider RotateSpeedSlider;
        public Transform target;
        public float rotateSpeed = 1f;
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
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = RecordTargetToggle.DisplayInMapper = LockTargetKey.DisplayInMapper = RotateSpeedSlider.DisplayInMapper = ResetViewKey.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { cameraLookAtToggled = CameraLookAtToggle.IsActive; };

            RecordTargetToggle = AddToggle("记录目标", "RecordTarget", recordTarget);
            RecordTargetToggle.Toggled += (bool value) =>
            {
                recordTarget = value;
                ChangedProperties();
            };
            BlockDataLoadEvent += (XDataHolder BlockData) => { recordTarget = RecordTargetToggle.IsActive; };

            RotateSpeedSlider = AddSlider("追踪速度", "RotateSpeed", rotateSpeed, 1, 100, false);
            RotateSpeedSlider.ValueChanged += (float value) => { rotateSpeed = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { rotateSpeed = RotateSpeedSlider.Value; };

            LockTargetKey = AddKey("锁定目标", "LockTarget", lockKeys);
            LockTargetKey.InvokeKeysChanged();

            ResetViewKey = AddKey("暂停/恢复追踪", "ResetView", resetKeys);
            ResetViewKey.InvokeKeysChanged();

            // Get the actual camera's transform, not the joint's transform
            realCameraTransform = GetComponent<FixedCameraBlock>().CompoundTracker;
            realCameraTransform.gameObject.AddComponent<SmoothLookAt>();
            defaultRotation = realCameraTransform.rotation;
            // Add reference to the camera's buildindex
            selfIndex = GetComponent<BlockBehaviour>().BuildIndex;

#if DEBUG
            ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            CameraLookAtToggle.DisplayInMapper = value;
            RecordTargetToggle.DisplayInMapper = value && cameraLookAtToggled;
            RotateSpeedSlider.DisplayInMapper = value && cameraLookAtToggled;
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

            // Load smooth look at config
            realCameraTransform.GetComponent<SmoothLookAt>().damping = rotateSpeed;
            if (resetView)
            {
                realCameraTransform.GetComponent<SmoothLookAt>().target = null;
            }
            else
            {
                realCameraTransform.GetComponent<SmoothLookAt>().target = target;
            }
        }

        protected override void OnSimulateUpdate()
        {
            if (cameraLookAtToggled && ResetViewKey.IsReleased)
            {
                resetView = !resetView;
                if (resetView)
                {
                    realCameraTransform.GetComponent<SmoothLookAt>().target = null;
                }
                else
                {
                    realCameraTransform.GetComponent<SmoothLookAt>().target = target;
                }
                if (viewAlreadyReset)
                {
                    viewAlreadyReset = !viewAlreadyReset;
                }
            }
            if (cameraLookAtToggled && LockTargetKey.IsReleased)
            {
                // Aquire the target to look at
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                resetView = false;
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    target = hit.transform;

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

        protected override void OnSimulateLateUpdate()
        {
            if (cameraLookAtToggled && resetView && !viewAlreadyReset)
            {
                realCameraTransform.rotation = Quaternion.Slerp(realCameraTransform.rotation, defaultRotation, rotateSpeed * Time.deltaTime);
                if (realCameraTransform.rotation == defaultRotation)
                {
                    viewAlreadyReset = true;
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
    }
}