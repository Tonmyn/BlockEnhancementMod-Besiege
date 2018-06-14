using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace BlockEnhancementMod.Blocks
{
    public class SmallwheelScript : EnhancementBlock
    {

        MSlider SpeedSlider;

        public float Speed = 5;

        protected override void SafeStart()
        {

            SpeedSlider = AddSlider("旋转速度", "Speed", Speed, 0f, 5f, false);
            SpeedSlider.ValueChanged += (float value) => { Speed = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Speed = SpeedSlider.Value; };

#if DEBUG
            ConsoleController.ShowMessage("小轮子添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            SpeedSlider.DisplayInMapper = value;
        }

        protected override void OnSimulateStart()
        {
            GetComponent<SmallWheel>().speed = Speed;
        }

    }
}
