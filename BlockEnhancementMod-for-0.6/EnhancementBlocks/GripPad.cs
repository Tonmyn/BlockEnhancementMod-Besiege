using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    public class GripPadScript : EnhancementBlock
    {

        public GripPadScript GPS;

        public MSlider FrictionSlider;

        public MMenu HardnessMenu;

        public float Friction = 1000;

        public int Hardness = 1;

        protected override void SafeStart()
        {

            //GPS = BB.gameObject.AddComponent<GripPadScript>();

            HardnessMenu = new MMenu("Hardness", Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(HardnessMenu);

            FrictionSlider = new MSlider("摩擦力", "Friction", Friction, 0f, 1000f, false);
            FrictionSlider.ValueChanged += (float value) => { Friction = Mathf.Abs(value); ChangedPropertise(); };
            CurrentMapperTypes.Add(FrictionSlider);



#if DEBUG
            BesiegeConsoleController.ShowMessage("摩擦垫添加进阶属性");
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

                    if (bd.HasKey("bmt-" + FrictionSlider.Key)) { FrictionSlider.Value = Friction = bd.ReadFloat("bmt-" + FrictionSlider.Key); }

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

                    blockinfo.BlockData.Write("bmt-" + FrictionSlider.Key, FrictionSlider.Value);

                    break;
                }

            }
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            FrictionSlider.DisplayInMapper = value;
        }

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();
        //    GPS.Hardness = Hardness;
        //    GPS.Friction = Friction;
        //}

        //public int Hardness;

        //public float Friction;

        private ConfigurableJoint CJ;

        private Collider[] colliders;

        protected override void OnSimulateStart()
        {

            colliders = GetComponentsInChildren<Collider>();
            CJ = GetComponent<ConfigurableJoint>();

            foreach (Collider c in colliders)
            {
                if (c.name == "Collider")
                {
                    c.material.staticFriction = c.material.dynamicFriction = Friction;

                    break;
                }

            }

            SwitchWoodHardness(Hardness, CJ);

        }


    }

    
}
