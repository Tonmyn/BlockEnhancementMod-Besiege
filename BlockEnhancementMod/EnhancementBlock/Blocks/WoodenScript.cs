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

        public int HardnessIndex = 1;
        //private int orginHardnessIndex = 1;

        public override void SafeAwake()
        {        

            HardnessMenu = BB.AddMenu("Hardness", HardnessIndex, LanguageManager.Instance.CurrentLanguage.WoodenHardness, false);
            HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("木头组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                CJ = GetComponent<ConfigurableJoint>();

                //if (!EnhancementEnabled) { HardnessIndex = orginHardnessIndex; }

                Hardness.SwitchWoodHardness(HardnessIndex, CJ);
            }      
        }
    }
}
