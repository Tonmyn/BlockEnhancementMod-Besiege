using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class SliderScript : EnhancementBlock
    {

        //SliderScript SS;

        MMenu HardnessMenu;

        MSlider LimitSlider;

        int Hardness = 1;

        float Limit = 1;

        protected override void SafeStart()
        {

                HardnessMenu = new MMenu("Hardness", Hardness, WoodHardness, false);
                HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(HardnessMenu);

                LimitSlider = new MSlider("限制", "Limit", Limit, 0f, 2f, false);
                LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(LimitSlider);

#if DEBUG
           BesiegeConsoleController.ShowMessage("滑块添加进阶属性");
#endif

        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + HardnessMenu.Key, HardnessMenu.Value);

                    blockinfo.BlockData.Write("bmt-" + LimitSlider.Key, LimitSlider.Value);

                    break;
                }

            }
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

                    if (bd.HasKey("bmt-" + LimitSlider.Key)) { LimitSlider.Value = Limit = bd.ReadFloat("bmt-" + LimitSlider.Key); }

                    break;
                }

            }
        }

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();
        //    SS.Hardness = Hardness;
        //    SS.Limit = Limit;
        //}

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        //public int Hardness;

        //public float Limit;

        protected override void OnSimulateStart()
        {

            CJ = GetComponent<ConfigurableJoint>();

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = Mathf.Abs(Limit);
            CJ.linearLimit = limit;

            SwitchWoodHardness(Hardness, CJ);
        }

    }
}
