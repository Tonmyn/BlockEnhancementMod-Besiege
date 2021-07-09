using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockEnhancementMod
{
    public class Balloon_EnhanceScript:ChangeSpeedBlock
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
    }
}
