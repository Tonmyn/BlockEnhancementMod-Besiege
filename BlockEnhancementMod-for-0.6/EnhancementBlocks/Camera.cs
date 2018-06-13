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
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        protected override void SafeStart()
        {

            CameraLookAtToggle = new MToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = LockTargetKey.DisplayInMapper = value; ChangedProperties(); };
            CurrentMapperTypes.Add(CameraLookAtToggle);

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys, () =>
             {
                 foreach (KeyCode k in lockKeys)
                 {
                     ConsoleController.ShowMessage(string.Format("Key :{0}", k.ToString()));
                 }            
             }
            );
            //LockTargetKey = new MKey("锁定目标", "lockTarget", lockKeys[0]);
            ////for (int i = 0; i < lockKeys.Count; i++)
            ////{
            ////    LockTargetKey.AddOrReplaceKey(i, lockKeys[i]);
            ////}
            //LockTargetKey.KeysChanged += ChangedProperties;
            //CurrentMapperTypes.Add(LockTargetKey);

            if (!Machine.Active().gameObject.GetComponent<CameraCompositeTrackerScript>())
            {
                //ConsoleController.ShowMessage(GetComponent<FixedCameraBlock>().CompositeTracker.gameObject.name);
                Machine.Active().gameObject.AddComponent<CameraCompositeTrackerScript>();
            }

            
#if DEBUG
            ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }
        //public override void ChangedProperties()
        //{
        //    lockKeys.Clear();
        //    for (int i = 0; i < LockTargetKey.KeysCount; i++)
        //    {
        //        lockKeys.Add(LockTargetKey.GetKey(i));
        //        ConsoleController.ShowMessage(LockTargetKey.GetKey(i).ToString());
        //    }
        //}

        public override void DisplayInMapper(bool value)
        {
            //base.DisplayInMapper(value);
            CameraLookAtToggle.DisplayInMapper = value;
            //CameraFollowSmoothSlider.DisplayInMapper = value && cameraLookAtToggled;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
        }

        //public override void LoadConfiguration()
        //{
        //    base.LoadConfiguration();

        //    if (Controller.MI == null)
        //    {
        //        return;
        //    }

        //    foreach (var blockinfo in Controller.MI.Blocks)
        //    {
        //        if (blockinfo.Guid == BB.Guid)
        //        {
        //            XDataHolder bd = blockinfo.BlockData;

        //            if (bd.HasKey("bmt-" + CameraLookAtToggle.Key))
        //            {
        //                CameraLookAtToggle.IsActive = cameraLookAtToggled = bd.ReadBool("bmt-" + CameraLookAtToggle.Key);
        //            }
        //            if (bd.HasKey("bmt-" + LockTargetKey.Key))
        //            {
        //                int index = 0;
        //                foreach (string str in bd.ReadStringArray("bmt-" + LockTargetKey.Key))
        //                {
        //                    LockTargetKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
        //                }
        //            }
        //            if (bd.HasKey("bmt-" + "CameraTarget"))
        //            {
        //                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(GetComponent<BlockBehaviour>().BuildIndex, bd.ReadInt("bmt-" + "CameraTarget"));
        //            }
        //            break;
        //        }

        //    }
        //}

        public override void LoadConfiguration(XDataHolder BlockData)
        {

            if (BlockData.HasKey("bmt-" + CameraLookAtToggle.Key))
            {
                CameraLookAtToggle.IsActive = cameraLookAtToggled = BlockData.ReadBool("bmt-" + CameraLookAtToggle.Key);
            }
            //if (BlockData.HasKey("bmt-" + LockTargetKey.Key))
            //{
            //    int index = 0;
            //    foreach (string str in bd.ReadStringArray("bmt-" + LockTargetKey.Key))
            //    {
            //        LockTargetKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
            //    }
            //}
            if (BlockData.HasKey("bmt-" + "CameraTarget"))
            {
                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(GetComponent<BlockBehaviour>().BuildIndex, BlockData.ReadInt("bmt-" + "CameraTarget"));
            }


        }

        //public override void SaveConfiguration(MachineInfo mi)
        //{
        //    base.SaveConfiguration(mi);

        //    foreach (var blockinfo in mi.Blocks)
        //    {
        //        if (blockinfo.Guid == BB.Guid)
        //        {
        //            blockinfo.BlockData.Write("bmt-" + CameraLookAtToggle.Key, CameraLookAtToggle.IsActive);
        //            blockinfo.BlockData.Write("bmt-" + LockTargetKey.Key, LockTargetKey.Serialize().RawValue);
        //            //ConsoleController.ShowMessage(Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Count.ToString());
        //            if (Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.ContainsKey(GetComponent<BlockBehaviour>().BuildIndex))
        //            {
        //                blockinfo.BlockData.Write("bmt-" + "CameraTarget", Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic[GetComponent<BlockBehaviour>().BuildIndex]);
        //            }
        //            break;
        //        }

        //    }
        //}

        public override void SaveConfiguration(XDataHolder BlockData)
        {

            BlockData.Write("bmt-" + CameraLookAtToggle.Key, CameraLookAtToggle.IsActive);
            //BlockData.Write("bmt-" + LockTargetKey.Key, LockTargetKey.Serialize().RawValue);
            //ConsoleController.ShowMessage(Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Count.ToString());
            if (Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.ContainsKey(GetComponent<BlockBehaviour>().BuildIndex))
            {
                BlockData.Write("bmt-" + "CameraTarget", Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic[GetComponent<BlockBehaviour>().BuildIndex]);
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
                        ////ConsoleController.ShowMessage(targetIndex.ToString());
                        if (targetIndex != -1)
                        {
                            try
                            {
                                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(transform.GetComponent<BlockBehaviour>().BuildIndex, target.GetComponent<BlockBehaviour>().BuildIndex);
                            }
                            catch (Exception)
                            {
                                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Remove(GetComponent<BlockBehaviour>().BuildIndex
                                    );
                                ConsoleController.ShowMessage("Old dic entry removed");
                                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(GetComponent<BlockBehaviour>().BuildIndex, target.GetComponent<BlockBehaviour>().BuildIndex);
                                ConsoleController.ShowMessage("New dic entry added");
                            }
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