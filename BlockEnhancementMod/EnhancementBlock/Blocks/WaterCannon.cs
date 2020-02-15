using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockEnhancementMod
{
    class WaterCannonScript : ChangeSpeedBlock
    {
        MToggle BoilingToggle;
        //public bool Boiling = false;
        //private bool orginBoiling = false;

        WaterCannonController WCC;
        BlockVisualController BVC;
        FireTag FT;

        public override void SafeAwake()
        {
            BoilingToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.Boiling, "Boiling", /*Boiling*/false);
            //BoilingToggle.Toggled += (bool value) => { Boiling = value; ChangedProperties(); };

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("水炮添加进阶属性");
#endif
        }

        //public override void DisplayInMapper(bool value)
        //{
        //    BoilingToggle.DisplayInMapper = value;
        //    base.DisplayInMapper(value);
        //}

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                WCC = GetComponent<WaterCannonController>();
                BVC = GetComponent<BlockVisualController>();
                FT = GetComponent<FireTag>();

                SpeedSlider = WCC.StrengthSlider;
                //if (!EnhancementEnabled) { Boiling = orginBoiling; }
            } 
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();
            if (StatMaster.isClient) return;

            if (/*Boiling*/BoilingToggle.IsActive)
            {
                WCC.boiling = WCC.prevBoilingState = true;
                //BVC.heating.glowTimer = 1f;
                FT.burning = true;
            }
        }

    }
}
