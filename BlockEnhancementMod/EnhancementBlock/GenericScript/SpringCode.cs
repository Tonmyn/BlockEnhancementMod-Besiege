using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockEnhancementMod
{
    class SpringCode_GenericEnhanceScript:ChangeSpeedBlock
    {
        internal SpringCode springCode;
        public override void SafeAwake()
        {
            springCode = GetComponent<SpringCode>();
            SpeedSlider = springCode.SpeedSlider;

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("皮筋添加进阶属性");
#endif
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();
        }
    }
}
