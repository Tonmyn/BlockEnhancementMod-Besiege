using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace BlockEnhancementMod
{
    class WoodenScript : ChangeHardnessBlock
    {
        public override void SafeAwake()
        {        

            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/1, LanguageManager.Instance.CurrentLanguage.WoodenHardness/*, false*/);
            //HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("木头组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            //HardnessMenu.DisplayInMapper = value;
            base.DisplayInMapper(value);
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                ConfigurableJoint = GetComponent<ConfigurableJoint>();

                hardness.SwitchWoodHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);
            }      
        }
    }
}
