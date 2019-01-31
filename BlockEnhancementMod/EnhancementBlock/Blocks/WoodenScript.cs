using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace BlockEnhancementMod
{
    class WoodenScript : EnhancementBlock
    {

        ConfigurableJoint CJ;

        MMenu HardnessMenu;

        public int Hardness = 1;
        private int orginHardness = 1;

        public override void SafeAwake()
        {        

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("木头组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
        }

        public override void ChangeParameter()
        {
            CJ = GetComponent<ConfigurableJoint>();

            if (!EnhancementEnabled) { Hardness = orginHardness; }

            SwitchWoodHardness(Hardness, CJ);
        }
    }
}
