using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;


namespace BlockEnhancementMod
{
    public class SuspensionScript : EnhancementBlock
    {

        MMenu HardnessMenu;
        MKey ExtendKey;
        MKey ShrinkKey;
        MToggle HydraulicToggle;
        MSlider FeedSlider;
        MSlider ExtendLimitSlider;
        MSlider ShrinkLimitSlider;

        public int Hardness = 0;
        public bool Hydraulic = false;
        public float Feed = 0.5f;
        public float ExtendLimit = 1f;
        public float RetractLimit = 1f;

        private int orginHardness = 0;
        private float orginLimit = 1;

        ConfigurableJoint CJ;
        Rigidbody RB;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };

            ExtendKey = BB.AddKey(LanguageManager.extend, "Extend", KeyCode.E);
            ShrinkKey = BB.AddKey(LanguageManager.retract, "Shrink", KeyCode.F);           

            HydraulicToggle = BB.AddToggle(LanguageManager.hydraulicMode, "Pressure", Hydraulic);
            HydraulicToggle.Toggled += (bool value) => { Hydraulic = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = ExtendLimitSlider.DisplayInMapper = ShrinkLimitSlider.DisplayInMapper = value; ChangedProperties(); };

            FeedSlider = BB.AddSlider(LanguageManager.feedSpeed, "feed", Feed, 0f, 2f);
            FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedProperties(); };

            ExtendLimitSlider = BB.AddSlider(LanguageManager.extendLimit, "ExtendLimit", ExtendLimit, 0f, 3f);
            ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedProperties(); };

            ShrinkLimitSlider = BB.AddSlider(LanguageManager.retractLimit, "ShrinkLimit", RetractLimit, 0f, 3f);
            ShrinkLimitSlider.ValueChanged += (float value) => { RetractLimit = value; ChangedProperties(); };



#if DEBUG
            ConsoleController.ShowMessage("悬挂添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            ExtendKey.DisplayInMapper = value && Hydraulic;
            ShrinkKey.DisplayInMapper = value && Hydraulic;
            HydraulicToggle.DisplayInMapper = value;
            FeedSlider.DisplayInMapper = value && Hydraulic;
            ExtendLimitSlider.DisplayInMapper = value && Hydraulic;
            ShrinkLimitSlider.DisplayInMapper = value && Hydraulic;
        }

        public override void ChangeParameter()
        {

            CJ = GetComponent<ConfigurableJoint>();
            RB = GetComponent<Rigidbody>();

            float limit = Mathf.Max(ExtendLimit, RetractLimit);

            if (!EnhancementEnabled)
            {
                Hardness = orginHardness;

                limit = orginLimit;
            }

            SoftJointLimit SJlimit = CJ.linearLimit;
            SJlimit.limit = limit;
            CJ.linearLimit = SJlimit;

            SwitchMatalHardness(Hardness, CJ);

        }

        public override void SimulateUpdateAlways()
        {
            if (StatMaster.isClient) return;

            if (Hydraulic/* && BB.isSimulating*//* && (StatMaster.isHosting || StatMaster.isLocalSim)*/)
            {
                if (ExtendKey.IsDown /*&& !ExtendKey.ignored*/)
                {

                    RB.WakeUp();
                    if ((CJ.targetPosition.x - Feed * 0.005f) > -ExtendLimit)
                    {
                        CJ.targetPosition -= new Vector3(Feed * 0.005f, 0, 0);
                    }
                    else
                    {
                        CJ.targetPosition = new Vector3(-ExtendLimit, 0, 0);
                    }

                }

                if (ShrinkKey.IsDown /*&& !ExtendKey.ignored*/)
                {
                    RB.WakeUp();
                    if (CJ.targetPosition.x + Feed * 0.005f < RetractLimit)
                    {
                        CJ.targetPosition += new Vector3(Feed * 0.005f, 0, 0);
                    }
                    else
                    {
                        CJ.targetPosition = new Vector3(RetractLimit, 0, 0);
                    }
                }
            }
        }
    }

  
}
