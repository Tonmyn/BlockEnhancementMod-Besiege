using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class CameraScript : EnhancementBlock
    {
        MToggle CameraLookAtToggle;
        MSlider CameraFollowSmoothSlider;

        public bool cameraLookAtToggled = false;
        public float cameraFollowSmooth = 0.25f;
        public Transform target;

        protected override void SafeStart()
        {

            CameraLookAtToggle = new MToggle("追踪摄像机 Tracking Camera", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(CameraLookAtToggle);

            CameraFollowSmoothSlider = new MSlider("平滑", "cameraSmooth", cameraFollowSmooth, 0, 1, false);
            CameraFollowSmoothSlider.ValueChanged += (float value) => { cameraFollowSmooth = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(CameraFollowSmoothSlider);

#if DEBUG
            BesiegeConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            CameraLookAtToggle.DisplayInMapper = value;
            CameraFollowSmoothSlider.DisplayInMapper = value && cameraLookAtToggled;
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
                    if (bd.HasKey("bmt-" + CameraFollowSmoothSlider.Key))
                    {
                        CameraFollowSmoothSlider.Value = cameraFollowSmooth = bd.ReadFloat("bmt-" + CameraFollowSmoothSlider.Key);
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
                    blockinfo.BlockData.Write("bmt-" + CameraFollowSmoothSlider.Key, CameraFollowSmoothSlider.Value);
                    break;
                }

            }
        }

        protected override void OnSimulateStart()
        {
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (Input.GetMouseButtonDown(2))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.transform;
                }

            }
            if (cameraLookAtToggled)
            {
                Vector3 relativePos = transform.position - target.position;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(relativePos), cameraFollowSmooth);
            }
        }
    }
}


