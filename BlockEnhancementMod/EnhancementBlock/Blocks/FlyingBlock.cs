using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockEnhancementMod
{
    class FlyingBlock_EnhanceScript : ChangeSpeedBlock
    {
        private FlyingController flyingController;

        public override void SafeAwake()
        {
            flyingController = GetComponent<FlyingController>();
            SpeedSlider = flyingController.SpeedSlider;

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("风扇添加进阶属性");
#endif
        }
    }
}
