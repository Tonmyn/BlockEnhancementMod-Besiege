using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class Piston : Block
    {

        PistonScript PS;

        MMenu HardnessMenu;

        int Hardness = 0;

        public Piston(BlockBehaviour block) : base(block)
        {

            if (BB.GetComponent<PistonScript>() == null)
            {
                PS = BB.gameObject.AddComponent<PistonScript>();

                HardnessMenu = new MMenu("Hardness", Hardness, new List<string>() { "低碳钢", "中碳钢", "高碳钢" }, false);
                HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(HardnessMenu);
            }

            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("活塞添加进阶属性");
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

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            PS.Hardness = Hardness;
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
        }

        class PistonScript : BlockScript
        {

            ConfigurableJoint CJ;

            public int Hardness;

            private void Start()
            {
                CJ = GetComponent<ConfigurableJoint>();

                SwitchMatalHardness(Hardness, CJ);
            }

        }
    }
}
