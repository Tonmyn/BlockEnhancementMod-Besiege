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

        public int Hardness = 1;
        private int orginHardness = 1;
        public float Limit = 1;
        private float orginLimit = 1;

        ConfigurableJoint CJ;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };

            LimitSlider = BB.AddSlider(LanguageManager.limit, "Limit", Limit, 0f, 2f);
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

        public override void ChangeParameter()
        {

            CJ = GetComponent<ConfigurableJoint>();

            if (!EnhancementEnabled)
            {
                Limit = orginLimit;
                Hardness = orginHardness;
            }

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = Limit = Mathf.Abs(Limit);
            CJ.linearLimit = limit;

            SwitchWoodHardness(Hardness, CJ);
        }
    }
}
