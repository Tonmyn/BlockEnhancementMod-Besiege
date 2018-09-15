using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class PistonScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        public int Hardness = 0;
        private int orginHardness = 0;

        private ConfigurableJoint CJ;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("活塞添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
        }



        public override void ChangeParameter()
        {
            CJ = GetComponent<ConfigurableJoint>();

            if (!EnhancementEnabled) { Hardness = orginHardness; }

            SwitchMatalHardness(Hardness, CJ);
        }   
    }
}
