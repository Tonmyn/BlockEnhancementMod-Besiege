using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class SliderScript : ChangeHardnessBlock
    {

        //MMenu HardnessMenu;

        MSlider LimitSlider;

        //public int HardnessIndex = 1;
        //private int orginHardnessIndex = 1;
        //public float Limit = 1;
        //private float orginLimit = 1;

        //ConfigurableJoint ConfigurableJoint;

        public override void SafeAwake()
        {

            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/1, LanguageManager.Instance.CurrentLanguage.WoodenHardness/*, false*/);
            //HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            LimitSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Limit, "Limit", /*Limit*/1f, 0f, 2f);
            //LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedProperties(); };

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("滑块添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            LimitSlider.DisplayInMapper = value;
            base.DisplayInMapper(value);
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                ConfigurableJoint = GetComponent<ConfigurableJoint>();
                hardness = new Hardness(ConfigurableJoint);
                //if (!EnhancementEnabled)
                //{
                //    Limit = orginLimit;
                //    HardnessIndex = orginHardnessIndex;
                //}

                SoftJointLimit limit = ConfigurableJoint.linearLimit;
                limit.limit = /*Limit =*/ Mathf.Abs(/*Limit*/LimitSlider.Value);
                ConfigurableJoint.linearLimit = limit;

                hardness.SwitchWoodHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);
            }    
        }
    }
}
