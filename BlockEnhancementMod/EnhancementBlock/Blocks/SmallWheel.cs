﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    public class SmallwheelScript : EnhancementBlock
    {

        MSlider SpeedSlider;

        //public float Speed = 5;
        //private float orginSpeed = 5;

        SmallWheel SW;

        public override void SafeAwake()
        {

            SpeedSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.RotatingSpeed, "Speed", /*Speed*/5f, 0f, 5f);
            //SpeedSlider.ValueChanged += (float value) => { Speed = value; ChangedProperties(); };


#if DEBUG
            ConsoleController.ShowMessage("小轮子添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            SpeedSlider.DisplayInMapper = value;
        }

        public override void OnSimulateStartAlways()
        {
            if (EnhancementEnabled)
            {
                SW = GetComponent<SmallWheel>();

                //if (!EnhancementEnabled) { Speed = orginSpeed; }

                SW.speed = /*Speed*/SpeedSlider.Value;
            }          
        }
    }
}
