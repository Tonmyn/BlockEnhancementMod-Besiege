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
        MSlider LimitSlider;

        public int Hardness = 0;
        private int orginHardness = 0;
        public float Limit = 1.1f;
        private float orginLimit = 1.1f;

        private SliderCompress SC;
        private ConfigurableJoint CJ;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, MetalHardness, false);
            HardnessMenu.ValueChanged += (value) => { Hardness = value; ChangedProperties(); };

            LimitSlider = BB.AddSlider(LanguageManager.limit, "Limit", Limit, 0, orginLimit);
            LimitSlider.ValueChanged += (value) => { Limit = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("活塞添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
        }

        public override void ChangeParameter()
        {
            SC = GetComponent<SliderCompress>();
            CJ = GetComponent<ConfigurableJoint>();

            if (!EnhancementEnabled) { Hardness = orginHardness; Limit = orginLimit; }

            SC.newLimit = Limit * FlipToSign(SC.Flipped);
            SwitchMatalHardness(Hardness, CJ);

            int FlipToSign(bool value) { return value == true ? 1 : -1; }
        }   
    }
}
