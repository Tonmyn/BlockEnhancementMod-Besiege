using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class SliderScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        MSlider LimitSlider;

        int Hardness = 1;

        float Limit = 1;

        protected override void SafeAwake()
        {

            //HardnessMenu = AddMenu("Hardness", Hardness, WoodHardness, false);
            //HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            //LimitSlider = AddSlider("限制", "Limit", Limit, 0f, 2f, false);
            //LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Limit = LimitSlider.Value; };

#if DEBUG
            ConsoleController.ShowMessage("滑块添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        protected override void OnSimulateStart()
        {

            CJ = GetComponent<ConfigurableJoint>();

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = Mathf.Abs(Limit);
            CJ.linearLimit = limit;

            SwitchWoodHardness(Hardness, CJ);
        }

    }
}
