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

        protected override void SafeAwake()
        {
            RotationToggle = AddToggle(LanguageManager.cvJoint, "Rotation", Rotation);
            RotationToggle.Toggled += (bool value) => { Rotation = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Rotation = RotationToggle.IsActive; };
#if DEBUG
            //ConsoleController.ShowMessage("球铰添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            RotationToggle.DisplayInMapper = value;
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
       


    }
}
