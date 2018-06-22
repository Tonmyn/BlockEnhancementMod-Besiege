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

        MToggle PressureToggle;

        MSlider FeedSlider;

        MSlider ExtendLimitSlider;

        MSlider ShrinkLimitSlider;

        public int Hardness = 1;

        public bool Pressure = false;

        public float Feed = 0.5f;

        public float ExtendLimit = 1f;

        public float ShrinkLimit = 1f;

        public List<KeyCode> ExtendKeyCodes = new List<KeyCode> { KeyCode.E };

        public List<KeyCode> ShrinkKeyCodes = new List<KeyCode> { KeyCode.F };
            
        protected override void SafeAwake()
        {

            HardnessMenu = AddMenu("Hardness", Hardness, new List<string> { "低碳钢", "中碳钢", "高碳钢" }, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            ExtendKey = AddKey("伸出", "Extend", ExtendKeyCodes);
            ShrinkKey = AddKey("收回", "Shrink", ShrinkKeyCodes);           

            PressureToggle = AddToggle("液压模式", "Pressure", Pressure);
            PressureToggle.Toggled += (bool value) => { Pressure = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Pressure = PressureToggle.IsActive; };

            FeedSlider = AddSlider("进给速度", "feed", Feed, 0f, 2f,false);
            FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Feed = FeedSlider.Value; };

            ExtendLimitSlider = AddSlider("伸出限制", "ExtendLimit", ExtendLimit, 0f, 3f, false);
            ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ExtendLimit = ExtendLimitSlider.Value; };

            ShrinkLimitSlider = AddSlider("收缩限制", "ShrinkLimit", ShrinkLimit, 0f, 3f, false);
            ShrinkLimitSlider.ValueChanged += (float value) => { ShrinkLimit = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ShrinkLimit = ShrinkLimitSlider.Value; };



#if DEBUG
            ConsoleController.ShowMessage("悬挂添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            ExtendKey.DisplayInMapper = value && Pressure;
            ShrinkKey.DisplayInMapper = value && Pressure;
            PressureToggle.DisplayInMapper = value;
            FeedSlider.DisplayInMapper = value && Pressure;
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
            limit.limit = Mathf.Max(ExtendLimit, ShrinkLimit);
            CJ.linearLimit = limit;

            SwitchMatalHardness(Hardness, CJ);

        }

        protected override void OnSimulateFixedUpdate()
        {
            base.OnSimulateFixedUpdate();

            if (Pressure)
            {
                if (ExtendKey.IsDown && !ExtendKey.ignored)
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

                if (ShrinkKey.IsDown && !ExtendKey.ignored)
                {
                    RB.WakeUp();
                    if (CJ.targetPosition.x + Feed * 0.005f < ShrinkLimit)
                    {
                        CJ.targetPosition += new Vector3(Feed * 0.005f, 0, 0);
                    }
                    else
                    {
                        CJ.targetPosition = new Vector3(ShrinkLimit, 0, 0);
                    }
                }
            }
        }
    }

  
}
