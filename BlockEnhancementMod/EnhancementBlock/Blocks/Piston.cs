using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod.Blocks
{
    class PistonScript : EnhancementBlock
    {

        MMenu HardnessMenu;
        MSlider DamperSlider;
        MSlider LimitSlider;

        public int HardnessIndex = 0;
        //private int orginHardnessIndex = 0;
        public float Damper = 1;
        public float Limit = 1.1f;
        //private float orginLimit = 1.1f;

        private SliderCompress SC;
        private ConfigurableJoint CJ;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu("Hardness", HardnessIndex, LanguageManager.Instance.CurrentLanguage.MetalHardness, false);
            HardnessMenu.ValueChanged += (value) => { HardnessIndex = value; ChangedProperties(); };

            DamperSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.Damper, "Damper", Damper, 0f, 5f);
            DamperSlider.ValueChanged += (value) => { Damper = value; ChangedProperties(); };

            LimitSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.Limit, "Limit", Limit, 0, Limit);
            LimitSlider.ValueChanged += (value) => { Limit = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("活塞添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            DamperSlider.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                SC = GetComponent<SliderCompress>();
                CJ = GetComponent<ConfigurableJoint>();

                //if (!EnhancementEnabled) { HardnessIndex = orginHardnessIndex; Limit = orginLimit; }

                SC.newLimit = Limit * FlipToSign(SC.Flipped);

                var drive = CJ.xDrive;
                drive.positionDamper *= Damper;
                CJ.xDrive = drive;

                Hardness.SwitchMetalHardness(HardnessIndex, CJ);

                int FlipToSign(bool value) { return value == true ? 1 : -1; }
            }     
        }   
    }
}
