using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{

    class SteeringHinge : EnhancementBlock
    {

        protected override void SafeAwake()
        {
            GetComponent<SteeringWheel>
        }

    }


}
