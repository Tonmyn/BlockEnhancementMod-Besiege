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
        MSlider DamperSlider;
        MSlider FeedSlider;
        MSlider ExtendLimitSlider;
        MSlider ShrinkLimitSlider;

        public float Damper = 1f;
        public int HardnessIndex = 0;
        public bool Hydraulic = false;
        public bool R2C = false;
        public float Feed = 0.5f;
        public float ExtendLimit = 1f;
        public float RetractLimit = 1f;

        //private int orginHardnessIndex = 0;
        //private float orginLimit = 1;

        ConfigurableJoint CJ;
        Rigidbody RB;

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu("Hardness", HardnessIndex, LanguageManager.Instance.CurrentLanguage.MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            ExtendKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.Extend, "Extend", KeyCode.E);
            ShrinkKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.Retract, "Shrink", KeyCode.F);           

            HydraulicToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.HydraulicMode, "Pressure", Hydraulic);
            HydraulicToggle.Toggled += (bool value) => { Hydraulic = R2CToggle.DisplayInMapper = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = ExtendLimitSlider.DisplayInMapper = ShrinkLimitSlider.DisplayInMapper = value; ChangedProperties(); };

            R2CToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.ReturnToCenter, "Return to center", R2C);
            R2CToggle.Toggled += (bool value) => { R2C = value; ChangedProperties(); };

            DamperSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.Damper, "Damper", Damper, 0f, 5f);
            DamperSlider.ValueChanged += (value) => { Damper = value; ChangedProperties(); };

            FeedSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.FeedSpeed, "feed", Feed, 0f, 2f);
            FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedProperties(); };

            ExtendLimitSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.ExtendLimit, "ExtendLimit", ExtendLimit, 0f, 3f);
            ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedProperties(); };

            ShrinkLimitSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.RetractLimit, "ShrinkLimit", RetractLimit, 0f, 3f);
            ShrinkLimitSlider.ValueChanged += (float value) => { RetractLimit = value; ChangedProperties(); };



#if DEBUG
            ConsoleController.ShowMessage("悬挂添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            DamperSlider.DisplayInMapper = value;
            ExtendKey.DisplayInMapper = value && Hydraulic;
            ShrinkKey.DisplayInMapper = value && Hydraulic;
            HydraulicToggle.DisplayInMapper = value;
            R2CToggle.DisplayInMapper = value && Hydraulic;
            FeedSlider.DisplayInMapper = value && Hydraulic;
            ExtendLimitSlider.DisplayInMapper = value && Hydraulic;
            ShrinkLimitSlider.DisplayInMapper = value && Hydraulic;
        }

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                CJ = GetComponent<ConfigurableJoint>();
                RB = GetComponent<Rigidbody>();

                float limit = Mathf.Max(ExtendLimit, RetractLimit);

                //if (!EnhancementEnabled)
                //{
                //    HardnessIndex = orginHardnessIndex;

                //    limit = orginLimit;
                //}

                SoftJointLimit SJlimit = CJ.linearLimit;
                SJlimit.limit = limit;            
                CJ.linearLimit = SJlimit;

                var drive = CJ.xDrive;
                drive.positionDamper *= Damper;
                CJ.xDrive = drive;

                Hardness.SwitchMetalHardness(HardnessIndex, CJ);
            }        
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
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

                    if (ExtendKey.IsHeld)
                    {
                        pressed = true;
                        target = -ExtendLimit;
                    }

                    if (ShrinkKey.IsHeld)
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

            void SuspensionMoveTowards(float target, float feed, float delta = 0.005f)
            {
                RB.WakeUp();
                CJ.targetPosition = Vector3.MoveTowards(CJ.targetPosition, new Vector3(target, 0, 0), feed * delta);
            }
        }   
    }

  
}
