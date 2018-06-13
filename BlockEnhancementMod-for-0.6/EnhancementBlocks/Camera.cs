using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class CameraScript : EnhancementBlock
    {
        MToggle CameraLookAtToggle;
        //MSlider CameraFollowSmoothSlider;
        MKey LockTargetKey;

        public bool cameraLookAtToggled = false;
        //public float cameraFollowSmooth = 0.25f;
        public Transform target;
        Transform realCameraTransform;

        protected override void SafeStart()
        {

            CameraLookAtToggle = new MToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = LockTargetKey.DisplayInMapper = value; ChangedProperties(); };
            CurrentMapperTypes.Add(CameraLookAtToggle);

            LockTargetKey = new MKey("锁定目标", "lockTarget", KeyCode.Delete);
            LockTargetKey.KeysChanged += ChangedProperties;
            CurrentMapperTypes.Add(LockTargetKey);

            if (!Machine.Active().gameObject.GetComponent<CameraCompositeTrackerScript>())
            {
                ConsoleController.ShowMessage(GetComponent<FixedCameraBlock>().CompositeTracker.gameObject.name);
                Machine.Active().gameObject.AddComponent<CameraCompositeTrackerScript>();
            }

#if DEBUG
            ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            //base.DisplayInMapper(value);
            CameraLookAtToggle.DisplayInMapper = value;
            //CameraFollowSmoothSlider.DisplayInMapper = value && cameraLookAtToggled;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + CameraLookAtToggle.Key))
                    {
                        CameraLookAtToggle.IsActive = cameraLookAtToggled = bd.ReadBool("bmt-" + CameraLookAtToggle.Key);
                    }
                    if (bd.HasKey("bmt-" + LockTargetKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + LockTargetKey.Key))
                        {
                            LockTargetKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }
                    if (bd.HasKey("bmt-" + "CameraTarget"))
                    {
                        Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(GetComponent<BlockBehaviour>().BuildIndex, bd.ReadInt("bmt-" + "CameraTarget"));
                    }
                    break;
                }

            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    blockinfo.BlockData.Write("bmt-" + CameraLookAtToggle.Key, CameraLookAtToggle.IsActive);
                    //blockinfo.BlockData.Write("bmt-" + CameraFollowSmoothSlider.Key, CameraFollowSmoothSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + LockTargetKey.Key, LockTargetKey.Serialize().RawValue);
                    if (Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Count > 0)
                    {
                        blockinfo.BlockData.Write("bmt-" + "CameraTarget", target.GetComponent<BlockBehaviour>().BuildIndex);
                    }
                    break;
                }

            }
        }

        protected override void OnSimulateStart()
        {
            // Get the actual camera's transform, not the joint's transform
            realCameraTransform = GetComponent<FixedCameraBlock>().CompoundTracker;
            int targetIndex = -1;
            BlockBehaviour targetBlock = new BlockBehaviour();
            BlockBehaviour simBlock = new BlockBehaviour();
            try
            {
                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.TryGetValue(GetComponent<BlockBehaviour>().BuildIndex, out targetIndex);
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target index");
            }
            try
            {
                Machine.Active().GetBlockFromIndex(targetIndex, out targetBlock);
                simBlock = Machine.Active().GetSimBlock(targetBlock);
                //ConsoleController.ShowMessage(simBlock.Guid.ToString());
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target block");
            }
            try
            {
                target = simBlock.transform;
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target transform");
            }
        }

        protected override void LateUpdate()
        {
            if (cameraLookAtToggled)
            {
                if (LockTargetKey.IsReleased)
                {
                    // Aquire the target to look at
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        target = hit.transform;
                        long targetIndex = -1;
                        try
                        {
                            targetIndex = target.GetComponent<BlockBehaviour>().BuildIndex;
                        }
                        catch (Exception)
                        {
                            ConsoleController.ShowMessage("Not a machine block");
                        }
                        //ConsoleController.ShowMessage(targetIndex.ToString());
                        if (targetIndex != -1)
                        {
                            try
                            {
                                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(transform.GetComponent<BlockBehaviour>().BuildIndex, target.GetComponent<BlockBehaviour>().BuildIndex);
                            }
                            catch (ArgumentNullException)
                            {
                                ConsoleController.ShowMessage("Cannot find build index");
                            }
                            catch (ArgumentException)
                            {
                                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Remove(GetComponent<BlockBehaviour>().BuildIndex
                                    );
                                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(GetComponent<BlockBehaviour>().BuildIndex, target.GetComponent<BlockBehaviour>().BuildIndex);
                            }
                            //ConsoleController.ShowMessage(GetComponent<BlockBehaviour>().BuildIndex.ToString());
                            //ConsoleController.ShowMessage(target.GetComponent<BlockBehaviour>().BuildIndex.ToString());
                        }
                    }

                }
                if (target != null && StatMaster.levelSimulating)
                {
                    // Keep the camera focusing on the targetX
                    Vector3 positionDiff = target.position - realCameraTransform.position;
                    Vector3 rotatingAxis = (realCameraTransform.up - Vector3.Dot(positionDiff, realCameraTransform.up) * positionDiff).normalized;
                    realCameraTransform.LookAt(target);
                }
            }
        }
    }

    class CameraCompositeTrackerScript : MonoBehaviour
    {
        //public Transform PreviousTarget { get; set; }
        public Dictionary<int, int> previousTargetDic = new Dictionary<int, int>();
    }
}