using System;
using System.Collections.Generic;
using System.Reflection;
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
        public Transform realCameraTransform;

        bool s = false;

        protected override void SafeStart()
        {

            CameraLookAtToggle = new MToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            //CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = CameraFollowSmoothSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = value; ChangedPropertise(); };
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = LockTargetKey.DisplayInMapper = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(CameraLookAtToggle);

            //CameraFollowSmoothSlider = new MSlider("平滑", "cameraSmooth", cameraFollowSmooth, 0, 1, false);
            //CameraFollowSmoothSlider.ValueChanged += (float value) => { cameraFollowSmooth = value; ChangedPropertise(); };
            //CurrentMapperTypes.Add(CameraFollowSmoothSlider);

            LockTargetKey = new MKey("锁定目标", "lockTarget", KeyCode.Delete);
            LockTargetKey.KeysChanged += ChangedPropertise;
            CurrentMapperTypes.Add(LockTargetKey);


#if DEBUG
            BesiegeConsoleController.ShowMessage("摄像机添加进阶属性");
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
                    //if (bd.HasKey("bmt-" + CameraFollowSmoothSlider.Key))
                    //{
                    //    CameraFollowSmoothSlider.Value = cameraFollowSmooth = bd.ReadFloat("bmt-" + CameraFollowSmoothSlider.Key);
                    //}
                    if (bd.HasKey("bmt-" + LockTargetKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + LockTargetKey.Key))
                        {
                            LockTargetKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
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
                    break;
                }

            }
        }

        protected override void OnSimulateStart()
        {
            // Get the actual camera's transform, not the joint's transform
            realCameraTransform = GetComponent<FixedCameraBlock>().CompoundTracker;
        }

        protected override void LateUpdate()
        {
            if (LockTargetKey.IsReleased)
            {
                // Aquire the target to look at
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.transform;

                    
                    ConsoleController.ShowMessage(target.gameObject.GetComponent<BlockBehaviour>().BuildingBlock.Guid.ToString());
                }

            }
            if (cameraLookAtToggled && target != null)
            {
                //ConsoleController.ShowMessage(target.name);
                // Keep the camera focusing on the target
                //BlockBehaviour block;
                Vector3 positionDiff = target.position - realCameraTransform.position;
                Vector3 rotatingAxis = (realCameraTransform.up - Vector3.Dot(positionDiff, realCameraTransform.up) * positionDiff).normalized;
                realCameraTransform.LookAt(target);
                //realCameraTransform.rotation = Quaternion.Slerp(realCameraTransform.rotation, Quaternion.LookRotation(positionDiff), cameraFollowSmooth);

               

                
            }
        }

    }
}