﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class CogMotoControllerHinge_GenericEnhanceScript : ChangeSpeedBlock
    {
        private CogMotorControllerHinge cmcl;
        public override void SafeAwake()
        {
            try
            {
                cmcl = GetComponent<CogMotorControllerHinge>();
                if (cmcl != null)
                {
                    SpeedSlider = cmcl.SpeedSlider;
                    EnableChangeSpeed = true;
                }
                else
                {
                    EnableChangeSpeed = false;
                }
            }
            catch { }

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("动力铰链添加进阶属性");
#endif
        }
    }
}
