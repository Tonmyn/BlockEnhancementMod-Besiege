using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class SliderScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        MSlider LimitSlider;

        public int HardnessIndex = 1;
        //private int orginHardnessIndex = 1;
        public float Limit = 1;
        //private float orginLimit = 1;

        ConfigurableJoint CJ;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu("Hardness", HardnessIndex, LanguageManager.Instance.CurrentLanguage.WoodenHardness, false);
            HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            LimitSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.limit, "Limit", Limit, 0f, 2f);
            LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("滑块添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                CJ = GetComponent<ConfigurableJoint>();

                //if (!EnhancementEnabled)
                //{
                //    Limit = orginLimit;
                //    HardnessIndex = orginHardnessIndex;
                //}

                SoftJointLimit limit = CJ.linearLimit;
                limit.limit = Limit = Mathf.Abs(Limit);
                CJ.linearLimit = limit;

                Hardness.SwitchWoodHardness(HardnessIndex, CJ);
            }    
        }
    }
}
