using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class SteeringWheel_GenericEnhanceScript : ChangeSpeedBlock
    {
        private SteeringWheel steeringWheel;
        public override void SafeAwake()
        {
            steeringWheel = GetComponent<SteeringWheel>();
            SpeedSlider = steeringWheel.SpeedSlider;

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("转向关节添加进阶属性");
#endif
        }
    }
}
