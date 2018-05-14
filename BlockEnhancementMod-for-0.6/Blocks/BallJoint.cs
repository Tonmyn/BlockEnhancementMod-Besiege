using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class BallJoint : Block
    {

        public MToggle RotationToggle;

        public BallJointScript BJS;

        public bool Rotation = false;

        public struct Data
        {
            public bool Rotation;
        }

        public BallJoint(BlockBehaviour block) : base(block)
        {

            if (BB.GetComponent<BallJointScript>() == null)
            {
                BJS = BB.gameObject.AddComponent<BallJointScript>();

                RotationToggle = new MToggle("万向节", "Rotation", Rotation);
                RotationToggle.Toggled += (bool value) => { Rotation = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(RotationToggle);

            }
            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            BesiegeConsoleController.ShowMessage("球铰添加进阶属性");
#endif

        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);         

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

                    if (bd.HasKey("bmt-" + RotationToggle.Key)) { RotationToggle.IsActive = Rotation = bd.ReadBool("bmt-" + RotationToggle.Key); }

                    break;
                }

            }
    
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            RotationToggle.DisplayInMapper = value;
        }

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            BJS.Rotation = Rotation;
        }

        public class BallJointScript : BlockScript
        {

            public ConfigurableJoint CJ;

            public bool Rotation;

            private float BreakTorque;

            private void Start()
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
        }
    }

    
}
