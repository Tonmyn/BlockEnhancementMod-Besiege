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
        MToggle R2CToggle;
        MSlider FeedSlider;
        MSlider ExtendLimitSlider;
        MSlider ShrinkLimitSlider;

        public int HardnessIndex = 0;
        public bool Hydraulic = false;
        public bool R2C = false;
        public float Feed = 0.5f;
        public float ExtendLimit = 1f;
        public float RetractLimit = 1f;

        private int orginHardnessIndex = 0;
        private float orginLimit = 1;

        ConfigurableJoint CJ;
        Rigidbody RB;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu("Hardness", HardnessIndex, LanguageManager.Instance.CurrentLanguage.MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            ExtendKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.extend, "Extend", KeyCode.E);
            ShrinkKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.retract, "Shrink", KeyCode.F);           

            HydraulicToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.hydraulicMode, "Pressure", Hydraulic);
            HydraulicToggle.Toggled += (bool value) => { Hydraulic = R2CToggle.DisplayInMapper = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = ExtendLimitSlider.DisplayInMapper = ShrinkLimitSlider.DisplayInMapper = value; ChangedProperties(); };

            R2CToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.returnToCenter, "Return to center", R2C);
            R2CToggle.Toggled += (bool value) => { R2C = value; ChangedProperties(); };

            FeedSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.feedSpeed, "feed", Feed, 0f, 2f);
            FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedProperties(); };

            ExtendLimitSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.extendLimit, "ExtendLimit", ExtendLimit, 0f, 3f);
            ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedProperties(); };

            ShrinkLimitSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.retractLimit, "ShrinkLimit", RetractLimit, 0f, 3f);
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
            R2CToggle.DisplayInMapper = value && Hydraulic;
            FeedSlider.DisplayInMapper = value && Hydraulic;
            ExtendLimitSlider.DisplayInMapper = value && Hydraulic;
            ShrinkLimitSlider.DisplayInMapper = value && Hydraulic;
        }

        public override void OnSimulateStart_Client()
        {

            CJ = GetComponent<ConfigurableJoint>();
            RB = GetComponent<Rigidbody>();

            float limit = Mathf.Max(ExtendLimit, RetractLimit);

            if (!EnhancementEnabled)
            {
                HardnessIndex = orginHardnessIndex;

                limit = orginLimit;
            }

            SoftJointLimit SJlimit = CJ.linearLimit;
            SJlimit.limit = limit;
            CJ.linearLimit = SJlimit;

            Hardness.SwitchMetalHardness(HardnessIndex, CJ);

        }

        public override void SimulateUpdate_EnhancementEnable()
        {
            if (StatMaster.isClient) return;

            if (Hydraulic)
            {             
                float? target = null;

                CalculationTarget();
                if (target != null)
                {
                    SuspensionMoveTowards((float)target, Feed);
                }

                void CalculationTarget()
                {
                    bool pressed = false;

                    if (ExtendKey.IsDown)
                    {
                        pressed = true;
                        target = -ExtendLimit;
                    }

                    if (ShrinkKey.IsDown)
                    {
                        pressed = true;
                        target = RetractLimit;
                    }

                    if (R2C && !pressed && CJ.targetPosition != Vector3.zero)
                    {
                        target = 0f;
                    }
                }
            }
        }

        public void SuspensionMoveTowards(float target,float feed,float delta = 0.005f)
        {
            RB.WakeUp();
            CJ.targetPosition = Vector3.MoveTowards(CJ.targetPosition, new Vector3(target, 0, 0), feed * delta);
        }
    }

  
}
