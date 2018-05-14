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

        protected override void SafeStart()
        {

            HardnessMenu = new MMenu("Hardness", Hardness, new List<string> { "低碳钢", "中碳钢", "高碳钢" }, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(HardnessMenu);

            ExtendKey = new MKey("伸出", "Extend", KeyCode.E);
            ExtendKey.KeysChanged += ChangedPropertise;
            CurrentMapperTypes.Add(ExtendKey);

            ShrinkKey = new MKey("收回", "Shrink", KeyCode.F);
            ShrinkKey.KeysChanged += ChangedPropertise;
            CurrentMapperTypes.Add(ShrinkKey);

            PressureToggle = new MToggle("液压模式", "Pressure", Pressure);
            PressureToggle.Toggled += (bool value) => { Pressure = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = value; ChangedPropertise(); };
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

#if DEBUG
            BesiegeConsoleController.ShowMessage("悬挂添加进阶属性");
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

                    if (bd.HasKey("bmt-" + ExtendKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + ExtendKey.Key))
                        {
                            ExtendKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }

                    if (bd.HasKey("bmt-" + ShrinkKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + ShrinkKey.Key))
                        {
                            ShrinkKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
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

                    //blockinfo.BlockData.Write("bmt-" + ExtendKey.Key, Tools.Get_List_keycode(ExtendKey));
                    blockinfo.BlockData.Write("bmt-" + ExtendKey.Key, ExtendKey.Serialize().RawValue);
                    blockinfo.BlockData.Write("bmt-" + ShrinkKey.Key, Tools.Get_List_keycode(ShrinkKey));
                    blockinfo.BlockData.Write("bmt-" + PressureToggle.Key, PressureToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + FeedSlider.Key, FeedSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + ExtendLimitSlider.Key, ExtendLimitSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + ShrinkLimitSlider.Key, ShrinkLimitSlider.Value);

                    break;
                }

            }
        }

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();
        //    SS.Hardness = Hardness;
        //    SS.Extend = Tools.Get_List_keycode(Extend);
        //    SS.Shrink = Tools.Get_List_keycode(Shrink);
        //    SS.Pressure = Pressure;
        //    SS.Feed = Feed;
        //    SS.ExtendLimit = ExtendLimit;
        //    SS.ShrinkLimit = ShrinkLimit;

        //}

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

        //public class SuspensionScript : Block.BlockScript
        //{

        ConfigurableJoint CJ;

        Rigidbody RB;

        //MKey ExtendKey;

        //MKey ShrinkKey;

        //public int Hardness;

        //public List<KeyCode> Extend;

        //public List<KeyCode> Shrink;

        //public bool Pressure;

        //public float Feed;

        //public float ExtendLimit;

        //public float ShrinkLimit;

        protected override void OnSimulateStart()
        {
            base.OnSimulateStart();

            CJ = GetComponent<ConfigurableJoint>();
            RB = GetComponent<Rigidbody>();
            //ExtendKey = GetKey(ExtendKey);
            //ShrinkKey = GetKey(ShrinkKey);

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = Mathf.Max(ExtendLimit, ShrinkLimit);
            CJ.linearLimit = limit;

            SwitchMatalHardness(Hardness, CJ);
        }
        //    private void Start()
        //    {
        //        CJ = GetComponent<ConfigurableJoint>();
        //        RB = GetComponent<Rigidbody>();
        //        ExtendKey = GetKey(Extend);
        //        ShrinkKey = GetKey(Shrink);

        //        SoftJointLimit limit = CJ.linearLimit;
        //        limit.limit = Mathf.Max(ExtendLimit, ShrinkLimit);
        //        CJ.linearLimit = limit;

        //        SwitchMatalHardness(Hardness, CJ);


        //    }
        protected override void OnSimulateFixedUpdate()
        {
            base.OnSimulateFixedUpdate();

            if (Pressure)
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
        //    private void FixedUpdate()
        //    {
        //        if (StatMaster.levelSimulating && Pressure)
        //        {
        //            if (ExtendKey.IsDown)
        //            {

        //                RB.WakeUp();
        //                if ((CJ.targetPosition.x - Feed * 0.005f) > -ExtendLimit)
        //                {
        //                    CJ.targetPosition -= new Vector3(Feed * 0.005f, 0, 0);
        //                }
        //                else
        //                {
        //                    CJ.targetPosition = new Vector3(-ExtendLimit, 0, 0);
        //                }

        //            }

        //            if (ShrinkKey.IsDown)
        //            {
        //                RB.WakeUp();
        //                if (CJ.targetPosition.x + Feed * 0.005f < ShrinkLimit)
        //                {
        //                    CJ.targetPosition += new Vector3(Feed * 0.005f, 0, 0);
        //                }
        //                else
        //                {
        //                    CJ.targetPosition = new Vector3(ShrinkLimit, 0, 0);
        //                }
        //            }
        //        }
        //    }
        //}
    }

  
}
