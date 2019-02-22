using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{   
    class PropellerScript : EnhancementBlock
    {
  
        MKey SwitchKey;
        MMenu HardnessMenu;
        MToggle EffectToggle;
        MToggle ToggleToggle;

        int Hardness = 1;
        bool Effect = true,Toggle = true;

        public override void SafeAwake()
        {

            SwitchKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.enabled, "Switch", KeyCode.O);
            SwitchKey.KeysChanged += ChangedProperties;

            HardnessMenu = BB.AddMenu("Hardness", Hardness, LanguageManager.Instance.CurrentLanguage.WoodenHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };

            EffectToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.enabledOnAwake, "Effect", Effect);
            EffectToggle.Toggled += (bool value) => { Effect = value; ChangedProperties(); };

            ToggleToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.toggleMode, "Toggle Mode", Toggle);
            ToggleToggle.Toggled += (value) => { Toggle = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("桨叶添加进阶属性");
#endif
        }

        private Dictionary<int, Vector3> Dic_AxisDrag = new Dictionary<int, Vector3>
        {
            { (int)BlockType.Propeller,new Vector3(0,0.015f,0) },
            { (int)BlockType.SmallPropeller,new Vector3(0,0.015f,0) },
            { (int)BlockType.Unused3,new Vector3(0,0.015f,0)},
            { (int)BlockType.Wing , new Vector3(0,0.04f,0) },
            { (int)BlockType.WingPanel , new Vector3(0,0.02f,0) },
        };
     
        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            SwitchKey.DisplayInMapper = value;
            HardnessMenu.DisplayInMapper = value;
            EffectToggle.DisplayInMapper = value;
            ToggleToggle.DisplayInMapper = value;
        }

        private ConfigurableJoint CJ;
        private AxialDrag AD;
        private int MyId;

        public override void OnSimulateStart()
        {
            MyId = GetComponent<BlockVisualController>().ID;
            CJ = GetComponent<ConfigurableJoint>();
            AD = GetComponent<AxialDrag>();

            SetVelocityCap(Effect);
                
            SwitchWoodHardness(Hardness, CJ);
        }
        public override void SimulateUpdateEnhancementEnableAlways()
        {

                if (SwitchKey.IsPressed)
                {
                    Effect = !Effect;
                    SetVelocityCap(Effect);
                }

            if(!Toggle)
            {
                if (SwitchKey.IsReleased)
                {
                    Effect = !Effect;
                    SetVelocityCap(Effect);
                }
            }       
        }

        private void SetVelocityCap(bool value)
        {
            AD.AxisDrag = (value == false) ? Vector3.zero : Dic_AxisDrag[MyId];
        }
    }
}
