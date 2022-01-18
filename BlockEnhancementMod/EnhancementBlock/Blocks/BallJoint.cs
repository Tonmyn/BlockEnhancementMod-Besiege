﻿using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class BallJointScript : EnhancementBlock
    {

        public MToggle RotationToggle;

        private ConfigurableJoint CJ;

        public override void SafeAwake()
        {

            RotationToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.CvJoint, "Rotation", false);

#if DEBUG
            ConsoleController.ShowMessage("球铰添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            RotationToggle.DisplayInMapper = value;
        }

        public override void OnSimulateStartAlways()
        {
            if (EnhancementEnabled)
            {
                CJ = GetComponent<ConfigurableJoint>();

                if (RotationToggle.IsActive)
                {
                    CJ.angularYMotion = ConfigurableJointMotion.Locked;
                    CJ.breakTorque = Mathf.Infinity;
                }
            }        
        }
    }

}
