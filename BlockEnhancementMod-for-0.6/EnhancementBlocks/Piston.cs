using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class PistonScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        int Hardness = 0;

        protected override void SafeStart()
        {

            HardnessMenu = AddMenu("Hardness", Hardness, new List<string>() { "低碳钢", "中碳钢", "高碳钢" }, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };


#if DEBUG
            ConsoleController.ShowMessage("活塞添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        protected override void OnSimulateStart()
        {

            CJ = GetComponent<ConfigurableJoint>();

            SwitchMatalHardness(Hardness, CJ);

        }
    }
}
