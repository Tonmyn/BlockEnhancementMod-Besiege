using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    public class Suspension : Block
    {
        SuspensionScript SS;

        MMenu HardnessMenu;

        MKey Extend;

        MKey Shrink;

        MToggle PressureToggle;

        MSlider FeedSlider;

        MSlider ExtendLimitSlider;

        MSlider ShrinkLimitSlider;

        public int Hardness = 1;

        public bool Pressure = false;

        public float Feed = 0.5f;

        public float ExtendLimit = 1f;

        public float ShrinkLimit = 1f;


        public Suspension(BlockBehaviour block):base(block)
        {
            if (BB.GetComponent<SuspensionScript>() == null)
            {


                SS = BB.gameObject.AddComponent<SuspensionScript>();

                HardnessMenu = new MMenu("Hardness", Hardness, new List<string> { "低碳钢", "中碳钢", "高碳钢" }, false);
                HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(HardnessMenu);

                Extend = new MKey("伸出", "Extend", KeyCode.E);
                Extend.KeysChanged += ChangedPropertise;
                CurrentMapperTypes.Add(Extend);

                Shrink = new MKey("收回", "Shrink", KeyCode.F);
                Shrink.KeysChanged += ChangedPropertise;
                CurrentMapperTypes.Add(Shrink);

                PressureToggle = new MToggle("液压模式", "Pressure", Pressure);
                PressureToggle.Toggled += (bool value) => { Pressure = Extend.DisplayInMapper = Shrink.DisplayInMapper = FeedSlider.DisplayInMapper = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(PressureToggle);

                FeedSlider = new MSlider("进给速度", "feed", Feed, 0f, 2f, false);
                FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(FeedSlider);

                ExtendLimitSlider = new MSlider("伸出限制", "ExtendLimit", ExtendLimit, 0f, 3f, false);
                ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(ExtendLimitSlider);

                ShrinkLimitSlider = new MSlider("收缩限制", "ShrinkLimit", ShrinkLimit, 0f, 3f, false);
                ShrinkLimitSlider.ValueChanged += (float value) => { ShrinkLimit = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(ShrinkLimitSlider);

            }
            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("悬挂添加进阶属性");
#endif

        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + HardnessMenu.Key)) { HardnessMenu.Value = Hardness = bd.ReadInt("bmt-" + HardnessMenu.Key); }

                    if (bd.HasKey("bmt-" + Extend.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + Extend.Key))
                        {
                            Extend.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }

                    if (bd.HasKey("bmt-" + Shrink.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + Shrink.Key))
                        {
                            Shrink.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }

                    if (bd.HasKey("bmt-" + PressureToggle.Key)) { PressureToggle.IsActive = Pressure = bd.ReadBool("bmt-" + PressureToggle.Key); }

                    if (bd.HasKey("bmt-" + FeedSlider.Key)) { FeedSlider.Value = Feed = bd.ReadFloat("bmt-" + FeedSlider.Key); }

                    if (bd.HasKey("bmt-" + ExtendLimitSlider.Key)) { ExtendLimitSlider.Value = ExtendLimit = bd.ReadFloat("bmt-" + ExtendLimitSlider.Key); }

                    if (bd.HasKey("bmt-" + ShrinkLimitSlider.Key)) { ShrinkLimitSlider.Value = ShrinkLimit = bd.ReadFloat("bmt-" + ShrinkLimitSlider.Key); }

                    break;
                }

            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + HardnessMenu.Key, HardnessMenu.Value);

                    blockinfo.BlockData.Write("bmt-" + Extend.Key, Tools.Get_List_keycode(Extend));

                    blockinfo.BlockData.Write("bmt-" + Shrink.Key, Tools.Get_List_keycode(Shrink));
                    blockinfo.BlockData.Write("bmt-" + PressureToggle.Key, PressureToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + FeedSlider.Key, FeedSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + ExtendLimitSlider.Key, ExtendLimitSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + ShrinkLimitSlider.Key, ShrinkLimitSlider.Value);

                    break;
                }

            }
        }

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            SS.Hardness = Hardness;
            SS.Extend = Tools.Get_List_keycode(Extend);
            SS.Shrink = Tools.Get_List_keycode(Shrink);
            SS.Pressure = Pressure;
            SS.Feed = Feed;
            SS.ExtendLimit = ExtendLimit;
            SS.ShrinkLimit = ShrinkLimit;

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            Extend.DisplayInMapper = value && Pressure;
            Shrink.DisplayInMapper = value && Pressure;
            PressureToggle.DisplayInMapper = value;
            FeedSlider.DisplayInMapper = value && Pressure;
            ExtendLimitSlider.DisplayInMapper = value;
            ShrinkLimitSlider.DisplayInMapper = value;
        }

        public class SuspensionScript : Block.BlockScript
        {

            ConfigurableJoint CJ;

            Rigidbody RB;

            MKey ExtendKey;

            MKey ShrinkKey;

            public int Hardness;

            public List<KeyCode> Extend;

            public List<KeyCode> Shrink;

            public bool Pressure;

            public float Feed;

            public float ExtendLimit;

            public float ShrinkLimit;

            private void Start()
            {
                CJ = GetComponent<ConfigurableJoint>();
                RB = GetComponent<Rigidbody>();
                ExtendKey = GetKey(Extend);
                ShrinkKey = GetKey(Shrink);

                SoftJointLimit limit = CJ.linearLimit;
                limit.limit = Mathf.Max(ExtendLimit, ShrinkLimit);
                CJ.linearLimit = limit;

                SwitchMatalHardness(Hardness, CJ);


            }

            private void FixedUpdate()
            {
                if (StatMaster.levelSimulating && Pressure)
                {
                    if (ExtendKey.IsDown)
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

                    if (ShrinkKey.IsDown)
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

  
}
