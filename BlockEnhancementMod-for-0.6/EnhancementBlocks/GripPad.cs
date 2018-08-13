using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    public class GripPadScript : EnhancementBlock
    {

        public MSlider FrictionSlider;

        public MMenu HardnessMenu;

        public float Friction = 1000;

        public int Hardness = 1;

        protected override void SafeAwake()
        {

            HardnessMenu = AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            FrictionSlider = AddSlider(LanguageManager.friction, "Friction", Friction, 0f, 1000f, false);
            FrictionSlider.ValueChanged += (float value) => { Friction = Mathf.Abs(value); ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Friction = FrictionSlider.Value; };



#if DEBUG
            ConsoleController.ShowMessage("摩擦垫添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            FrictionSlider.DisplayInMapper = value;
        }

        private ConfigurableJoint CJ;

        private Collider[] colliders;

        protected override void OnSimulateStart()
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

            SwitchWoodHardness(Hardness, CJ);

        }

    }

    
}
