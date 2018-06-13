using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class BallJointScript : EnhancementBlock
    {
   
        public MToggle RotationToggle;

        public bool Rotation = false;

        public ConfigurableJoint CJ;

        private float BreakTorque;

        protected override void SafeStart()
        {
            RotationToggle = new MToggle("万向节", "Rotation", Rotation);
            RotationToggle.Toggled += (bool value) => { Rotation = value; ChangedProperties(); };
            CurrentMapperTypes.Add(RotationToggle);

            ConsoleController.ShowMessage("球铰添加进阶属性...");   
        }

        protected override void OnSimulateStart()
        {
            CJ = GetComponent<ConfigurableJoint>();
            BreakTorque = CJ.breakTorque;

            if (Rotation)
            {
                CJ.angularYMotion = ConfigurableJointMotion.Locked;
                CJ.breakTorque = Mathf.Infinity;
            }
            else
            {
                CJ.angularYMotion = ConfigurableJointMotion.Free;
                CJ.breakTorque = BreakTorque;
            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + RotationToggle.Key, RotationToggle.IsActive);

                    break;
                }

            }

        }

        public override void LoadConfiguration()
        {

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + RotationToggle.Key)) { RotationToggle.IsActive = Rotation = bd.ReadBool("bmt-" + RotationToggle.Key); }

                    break;
                }

            }

        }

        public override void DisplayInMapper(bool value)
        {
            RotationToggle.DisplayInMapper = value;
        }

    }
}
