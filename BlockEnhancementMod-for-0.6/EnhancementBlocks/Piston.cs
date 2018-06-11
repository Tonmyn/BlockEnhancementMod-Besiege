using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class PistonScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        int Hardness = 0;

        protected override void SafeStart()
        {


            HardnessMenu = new MMenu("Hardness", Hardness, new List<string>() { "低碳钢", "中碳钢", "高碳钢" }, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(HardnessMenu);


#if DEBUG
            BesiegeConsoleController.ShowMessage("活塞添加进阶属性");
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

                    break;
                }

            }
        }

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();
        //    PS.Hardness = Hardness;
        //}

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        //public int Hardness;

        protected override void OnSimulateStart()
        {

            CJ = GetComponent<ConfigurableJoint>();

            SwitchMatalHardness(Hardness, CJ);

        }
    }
}
