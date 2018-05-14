using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class Propeller : Block
    {
        PropellerScript PS;

        MKey SwitchKey;

        MMenu HardnessMenu;

        MToggle EffectToggle;

        int Hardness = 1;

        bool Effect = true;

        public Propeller(BlockBehaviour block) : base(block)
        {
            if (BB.GetComponent<PropellerScript>() == null)
            {

                PS = BB.gameObject.AddComponent<PropellerScript>();

                SwitchKey = new MKey("气动开关", "Switch", KeyCode.O);
                SwitchKey.KeysChanged += ChangedPropertise;
                CurrentMapperTypes.Add(SwitchKey);

                HardnessMenu = new MMenu("", Hardness, WoodHardness, false);
                HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(HardnessMenu);

                EffectToggle = new MToggle("初始生效", "Effect", Effect);
                EffectToggle.Toggled += (bool value) => { Effect = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(EffectToggle);

            }
            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("桨叶添加进阶属性");
#endif
        }

        /// <summary>
        /// 是否是桨叶零件
        /// </summary>
        /// <param name="id"></param>
        /// <returns>零件号</returns>
        public static bool IsPropeller(int id)
        {
            bool result;

            switch (id)
            {
                case (int)BlockType.Propeller:
                    result = true;
                    break;

                case (int)BlockType.SmallPropeller:
                    result = true;
                    break;
                case 52:
                    result = true;
                    break;

                default: result = false; break;
            }
            return result;

        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + SwitchKey.Key, Tools.Get_List_keycode(SwitchKey));

                    blockinfo.BlockData.Write("bmt-" + HardnessMenu.Key, HardnessMenu.Value);

                    blockinfo.BlockData.Write("bmt-" + EffectToggle.Key, EffectToggle.IsActive);

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

                    if (bd.HasKey("bmt-" + SwitchKey.Key))
                    {
                        string[] strs = bd.ReadStringArray("bmt-" + SwitchKey.Key);
                        foreach (string str in strs)
                        {
                            SwitchKey.AddOrReplaceKey(Array.IndexOf(strs,str), (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }

                    if (bd.HasKey("bmt-" + HardnessMenu.Key)) { HardnessMenu.Value = Hardness = bd.ReadInt("bmt-" + HardnessMenu.Key); }

                    if (bd.HasKey("bmt-" + EffectToggle.Key)) { EffectToggle.IsActive = Effect = bd.ReadBool("bmt-" + EffectToggle.Key); }

                    break;
                }

            }
        }

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            PS.Switch = Tools.Get_List_keycode(SwitchKey);
            PS.Hardness = Hardness;
            PS.Effect = Effect;
            
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            SwitchKey.DisplayInMapper = value;
            HardnessMenu.DisplayInMapper = value;
            EffectToggle.DisplayInMapper = value;
        }

        class PropellerScript : BlockScript
        {
            public List<KeyCode> Switch;

            public int Hardness;

            public bool Effect;

            private MKey SwitchKey;

            private ConfigurableJoint CJ;

            private AxialDrag AD;

            private void Start()
            {
                SwitchKey = GetKey(Switch);
                CJ = GetComponent<ConfigurableJoint>();
                AD = GetComponent<AxialDrag>();

                AD.enabled = Effect;

                SwitchWoodHardness(Hardness,CJ);

            }

            private void Update()
            {
                if (SwitchKey.IsPressed)
                {
                    AD.enabled = Effect = !Effect;
                }
            }

        }
    }
}
