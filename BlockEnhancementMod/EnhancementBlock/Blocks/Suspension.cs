using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;


namespace BlockEnhancementMod
{
    public class SuspensionScript : ChangeSpeedBlock,IChangeHardness
    {

        public MMenu HardnessMenu { get; private set; }
        MKey ExtendKey;
        MKey ShrinkKey;
        MToggle HydraulicToggle;
        MToggle R2CToggle;
        //MSlider DamperSlider;
        MSlider FeedSlider;
        MSlider ExtendLimitSlider;
        MSlider ShrinkLimitSlider;

        //public float Damper = 1f;
        //public int HardnessIndex = 0;
        //public bool Hydraulic = false;
        //public bool R2C = false;
        //public float Feed = 0.5f;
        //public float ExtendLimit = 1f;
        //public float RetractLimit = 1f;

        //private int orginHardnessIndex = 0;
        //private float orginLimit = 1;

        public ConfigurableJoint ConfigurableJoint { get; private set; }
        Rigidbody RB;

        public override void SafeAwake()
        {


            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/0, LanguageManager.Instance.CurrentLanguage.MetalHardness/*, false*/);

            ExtendKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.Extend, "Extend", KeyCode.E);
            ShrinkKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.Retract, "Shrink", KeyCode.F);           

            HydraulicToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.HydraulicMode, "Pressure", /*Hydraulic*/false);

            R2CToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.ReturnToCenter, "Return to center",/* R2C*/false);

            //DamperSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Damper, "Damper", /*Damper*/1f, 0f, 5f);

            FeedSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.FeedSpeed, "feed", /*Feed*/0.5f, 0f, 2f);

            ExtendLimitSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.ExtendLimit, "ExtendLimit", /*ExtendLimit*/1f, 0f, 3f);

            ShrinkLimitSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.RetractLimit, "ShrinkLimit", /*RetractLimit*/1f, 0f, 3f);

            SpeedSlider = FeedSlider;

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("悬挂添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            var _value = value && HydraulicToggle.IsActive;

            base.DisplayInMapper(_value);

            HardnessMenu.DisplayInMapper = value;

            //DamperSlider.DisplayInMapper = value;
            ExtendKey.DisplayInMapper = _value;
            ShrinkKey.DisplayInMapper = _value;
            HydraulicToggle.DisplayInMapper = value;
            R2CToggle.DisplayInMapper = _value;
            FeedSlider.DisplayInMapper = _value;
            ExtendLimitSlider.DisplayInMapper = _value;
            ShrinkLimitSlider.DisplayInMapper = _value;
        }

        public override void OnSimulateStartAlways()
        {
            if (EnhancementEnabled)
            {
                ConfigurableJoint = GetComponent<ConfigurableJoint>();
                RB = GetComponent<Rigidbody>();
                ChangeHardnessBlock.Hardness hardness = new ChangeHardnessBlock.Hardness(ConfigurableJoint);

                float limit = Mathf.Max(/*ExtendLimit*/ExtendLimitSlider.Value, /*RetractLimit*/ShrinkLimitSlider.Value);

                //if (!EnhancementEnabled)
                //{
                //    HardnessIndex = orginHardnessIndex;

                //    limit = orginLimit;
                //}

                SoftJointLimit SJlimit = ConfigurableJoint.linearLimit;
                SJlimit.limit = limit;            
                ConfigurableJoint.linearLimit = SJlimit;

                var drive = ConfigurableJoint.xDrive;
                //drive.positionDamper *= /*Damper*/DamperSlider.Value;
                ConfigurableJoint.xDrive = drive;

                hardness.SwitchMetalHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);
            }        
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();
        }

        public override void SimulateFixedUpdate_EnhancementEnabled()
        {
            base.SimulateFixedUpdate_EnhancementEnabled();

            if (StatMaster.isClient) return;

            if (/*Hydraulic*/HydraulicToggle.IsActive)
            {
                float? target = null;

                CalculationTarget();
                if (target != null)
                {
                    SuspensionMoveTowards((float)target, /*Feed*/FeedSlider.Value * (ConfigurableJoint.swapBodies ? -1f : 1f));
                }

                void CalculationTarget()
                {
                    bool pressed = false;

                    if (ExtendKey.IsHeld || ExtendKey.EmulationHeld())
                    {
                        pressed = true;
                        target = -/*ExtendLimit*/ExtendLimitSlider.Value;
                    }

                    if (ShrinkKey.IsHeld || ShrinkKey.EmulationHeld())
                    {
                        pressed = true;
                        target = /*RetractLimit*/ShrinkLimitSlider.Value;
                    }

                    if (/*R2C*/R2CToggle.IsActive && !pressed && ConfigurableJoint.targetPosition != Vector3.zero)
                    {
                        target = 0f;
                    }
                }
            }

            void SuspensionMoveTowards(float target, float feed, float delta = 0.005f)
            {
                RB.WakeUp();
                ConfigurableJoint.targetPosition = Vector3.MoveTowards(ConfigurableJoint.targetPosition, new Vector3(target, 0, 0), feed * delta);
            }
        }
    }
}
