using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
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

        public int Hardness = 1;

        public bool Hydraulic = false;

        public float Feed = 0.5f;

        public float ExtendLimit = 1f;

        public float RetractLimit = 1f;

        public List<KeyCode> ExtendKeyCodes = new List<KeyCode> { KeyCode.E };

        public List<KeyCode> ShrinkKeyCodes = new List<KeyCode> { KeyCode.F };
            
        protected override void SafeAwake()
        {

            HardnessMenu = AddMenu(LanguageManager.hardness, Hardness, MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            ExtendKey = AddKey(LanguageManager.extend, "Extend", ExtendKeyCodes);
            ShrinkKey = AddKey(LanguageManager.retract, "Shrink", ShrinkKeyCodes);           

            HydraulicToggle = AddToggle(LanguageManager.hydraulicMode, "Pressure", Hydraulic);
            HydraulicToggle.Toggled += (bool value) => { Hydraulic = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Hydraulic = HydraulicToggle.IsActive; };

            FeedSlider = AddSlider(LanguageManager.feedSpeed, "feed", Feed, 0f, 2f,false);
            FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Feed = FeedSlider.Value; };

            ExtendLimitSlider = AddSlider(LanguageManager.extendLimit, "ExtendLimit", ExtendLimit, 0f, 3f, false);
            ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ExtendLimit = ExtendLimitSlider.Value; };

            ShrinkLimitSlider = AddSlider(LanguageManager.retractLimit, "ShrinkLimit", RetractLimit, 0f, 3f, false);
            ShrinkLimitSlider.ValueChanged += (float value) => { RetractLimit = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { RetractLimit = ShrinkLimitSlider.Value; };



#if DEBUG
            //ConsoleController.ShowMessage("悬挂添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            ExtendKey.DisplayInMapper = value && Hydraulic;
            ShrinkKey.DisplayInMapper = value && Hydraulic;
            HydraulicToggle.DisplayInMapper = value;
            FeedSlider.DisplayInMapper = value && Hydraulic;
            ExtendLimitSlider.DisplayInMapper = value;
            ShrinkLimitSlider.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        Rigidbody RB;

        protected override void OnSimulateStart()
        {
            base.OnSimulateStart();

            CJ = GetComponent<ConfigurableJoint>();
            RB = GetComponent<Rigidbody>();

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = Mathf.Max(ExtendLimit, RetractLimit);
            CJ.linearLimit = limit;

            SwitchMatalHardness(Hardness, CJ);

        }

        protected override void OnSimulateFixedUpdate()
        {
            base.OnSimulateFixedUpdate();

            if (Hydraulic)
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
