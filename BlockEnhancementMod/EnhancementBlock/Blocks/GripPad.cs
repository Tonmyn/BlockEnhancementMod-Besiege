using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    public class GripPadScript : ChangeSpeedBlock,IChangeHardness
    {

        MSlider FrictionSlider;
        public MMenu HardnessMenu { get; private set; }

        //public float Friction = 1000;
        //public int HardnessIndex = 1;
        public ConfigurableJoint ConfigurableJoint { get; private set; }

        private Collider[] colliders;

        public override void SafeAwake()
        {

            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/1, LanguageManager.Instance.CurrentLanguage.WoodenHardness/*, false*/);
            //HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            FrictionSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Friction, "Friction", /*Friction*/1f, 0f, /*1000f*/5f);
            //FrictionSlider.ValueChanged += (float value) => { /*Friction*/FrictionSlider.Value = Mathf.Abs(value); ChangedProperties(); };

            SpeedSlider = FrictionSlider;
            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("摩擦垫添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            FrictionSlider.DisplayInMapper = value;
            base.DisplayInMapper(value);
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                colliders = GetComponentsInChildren<Collider>();
                ConfigurableJoint = GetComponent<ConfigurableJoint>();
                ChangeHardnessBlock.Hardness hardness = new ChangeHardnessBlock.Hardness(ConfigurableJoint);

                foreach (Collider c in colliders)
                {
                    if (c.name == "Collider")
                    {
                        c.material.staticFriction = c.material.dynamicFriction = /*Friction*/FrictionSlider.Value * 1000f;

                        break;
                    }
                }
                hardness.SwitchWoodHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);
            }        
        }
    }

    
}
