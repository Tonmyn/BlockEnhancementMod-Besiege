using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod.Blocks
{
    public class GripPadScript : EnhancementBlock
    {

        MSlider FrictionSlider;
        MMenu HardnessMenu;

        public float Friction = 1000;
        public int HardnessIndex = 1;

        private ConfigurableJoint CJ;
        private Collider[] colliders;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu("Hardness", HardnessIndex, LanguageManager.Instance.CurrentLanguage.WoodenHardness, false);
            HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            FrictionSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.Friction, "Friction", Friction, 0f, 1000f);
            FrictionSlider.ValueChanged += (float value) => { Friction = Mathf.Abs(value); ChangedProperties(); };


#if DEBUG
            ConsoleController.ShowMessage("摩擦垫添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            FrictionSlider.DisplayInMapper = value;
        }
       
        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                colliders = GetComponentsInChildren<Collider>();
                CJ = GetComponent<ConfigurableJoint>();

                foreach (Collider c in colliders)
                {
                    if (c.name == "Collider")
                    {
                        c.material.staticFriction = c.material.dynamicFriction = Friction;

                        break;
                    }
                }
                Hardness.SwitchWoodHardness(HardnessIndex, CJ);
            }        
        }
    }

    
}
