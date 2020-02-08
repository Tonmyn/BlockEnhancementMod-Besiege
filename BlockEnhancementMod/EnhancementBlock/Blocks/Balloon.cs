using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockEnhancementMod
{
    class Balloon_EnhanceScript:ChangeSpeedBlock
    {
        private BalloonController  balloonController;

        public override void SafeAwake()
        {
            balloonController = GetComponent<BalloonController>();
            SpeedSlider = balloonController.BuoyancySlider;

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("气球添加进阶属性");
#endif
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();
        }


    }
}
