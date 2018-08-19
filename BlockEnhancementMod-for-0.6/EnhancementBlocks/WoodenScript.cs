using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace BlockEnhancementMod
{
    class WoodenScript : EnhancementBlock
    {

        ConfigurableJoint CJ;

        MMenu HardnessMenu;

        public int Hardness = 1;

        protected override void SafeAwake()
        {
            CJ = GetComponent<ConfigurableJoint>();

            HardnessMenu = AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };
        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
        }

        public override void ChangedProperties()
        {
            SwitchWoodHardness(Hardness, CJ);
        }
    }
}
