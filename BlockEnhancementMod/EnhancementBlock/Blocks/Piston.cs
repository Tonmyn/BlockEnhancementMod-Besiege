using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod.Blocks
{
    class PistonScript : ChangeSpeedBlock,IChangeHardness
    {

        public MMenu HardnessMenu { get; private set; }
        MSlider DamperSlider;
        MSlider LimitSlider;

        //public int HardnessIndex = 0;
        //private int orginHardnessIndex = 0;
        //public float Damper = 1;
        //public float Limit = 1.1f;
        //private float orginLimit = 1.1f;

        private SliderCompress SC;
        public ConfigurableJoint ConfigurableJoint { get; private set; }

        public override void SafeAwake()
        {

            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/0, LanguageManager.Instance.CurrentLanguage.MetalHardness/*, false*/);
            //HardnessMenu.ValueChanged += (value) => { HardnessIndex = value; ChangedProperties(); };

            DamperSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Damper, "Damper",/* Damper*/1f, 0f, 5f);
            //DamperSlider.ValueChanged += (value) => { Damper = value; ChangedProperties(); };

            LimitSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Limit, "Limit", /*Limit*/1.1f, 0, /*Limit*/1.1f);
            //LimitSlider.ValueChanged += (value) => { Limit = value; ChangedProperties(); };

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("活塞添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            DamperSlider.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
            base.DisplayInMapper(value);
        }

        public override void OnSimulateStartAlways()
        {
            if (EnhancementEnabled)
            {
                SC = GetComponent<SliderCompress>();
                ConfigurableJoint = GetComponent<ConfigurableJoint>();
                ChangeHardnessBlock.Hardness hardness = new ChangeHardnessBlock.Hardness(ConfigurableJoint);

                SpeedSlider = SC.SpeedSlider;
                //if (!EnhancementEnabled) { HardnessIndex = orginHardnessIndex; Limit = orginLimit; }

                SC.newLimit = /*Limit*/LimitSlider.Value * FlipToSign(SC.Flipped);

                var drive = ConfigurableJoint.xDrive;
                drive.positionDamper *= /*Damper*/DamperSlider.Value;
                ConfigurableJoint.xDrive = drive;

                hardness.SwitchMetalHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);

                int FlipToSign(bool value) { return value == true ? 1 : -1; }
            }
        }
    }
}
