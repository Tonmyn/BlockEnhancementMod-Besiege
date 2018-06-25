using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class CameraScript : EnhancementBlock
    {
        MToggle CameraLookAtToggle;
        MKey LockTargetKey;
        MKey ResetViewKey;

        public bool cameraLookAtToggled = false;
        public bool resetView = false;
        public bool viewAlreadyReset = false;
        public int selfIndex;
        public Transform target;
        public Transform realCameraTransform;
        public Quaternion defaultRotation;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };
        public List<KeyCode> resetKeys = new List<KeyCode> { KeyCode.X };

        protected override void SafeAwake()
        {

            CameraLookAtToggle = AddToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = LockTargetKey.DisplayInMapper = ResetViewKey.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { cameraLookAtToggled = CameraLookAtToggle.IsActive; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);
            LockTargetKey.KeysChanged += ChangedProperties;

            ResetViewKey = AddKey("暂停/恢复追踪", "resetView", resetKeys);
            ResetViewKey.KeysChanged += ChangedProperties;

            //Make sure the target list is present
            if (!Machine.Active().gameObject.GetComponent<TargetScript>())
            {
                Machine.Active().gameObject.AddComponent<TargetScript>();
            }


            // Get the actual camera's transform, not the joint's transform
            realCameraTransform = GetComponent<FixedCameraBlock>().CompoundTracker;
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
            // Trying to read previously saved target
            int targetIndex = -1;
            BlockBehaviour targetBlock = new BlockBehaviour();
            // Read the target's buildIndex from the dictionary
            if (!Machine.Active().GetComponent<TargetScript>().previousTargetDic.TryGetValue(selfIndex, out targetIndex))
            {
                // No target found in the dictionary
                // Assign null to target
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

        protected override void OnSimulateUpdate()
        {
            if (cameraLookAtToggled && ResetViewKey.IsReleased)
            {
                resetView = !resetView;
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

        protected override void OnSimulateLateUpdate()
        {
            if (resetView && !viewAlreadyReset)
            {
                // If the tracking is temporarily disabled, the camera will look at ifself.
                realCameraTransform.rotation = defaultRotation;
                viewAlreadyReset = true;
            }
            if (cameraLookAtToggled && target != null && StatMaster.levelSimulating && !resetView)
            {
                // Keep the camera focusing on the target
                realCameraTransform.LookAt(target);
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

    class TargetScript : MonoBehaviour
    {
        public Dictionary<int, int> previousTargetDic = new Dictionary<int, int>();
    }
}