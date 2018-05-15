using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class DecouplerScript : EnhancementBlock
    {
        //DecouplerScript DS;

        MSlider ExplodeForceSlider;

        MSlider ExplodeTorqueSlider;

        float ExplodeForce = 1000f;

        float ExplodeTorque = 2000f;

        protected override void SafeStart()
        {
            //DS = BB.gameObject.AddComponent<DecouplerScript>();

            ExplodeForceSlider = new MSlider("爆炸力", "ExplodeForce", ExplodeForce, 0, 3000f, false);
            ExplodeForceSlider.ValueChanged += (float value) => { ExplodeForce = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(ExplodeForceSlider);

            ExplodeTorqueSlider = new MSlider("爆炸扭矩", "ExplodeTorque", ExplodeTorque, 0, 2500f, false);
            ExplodeTorqueSlider.ValueChanged += (float value) => { ExplodeTorque = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(ExplodeTorqueSlider);


#if DEBUG
            BesiegeConsoleController.ShowMessage("分离铰链添加进阶属性");
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

                    if (bd.HasKey("bmt-" + ExplodeForceSlider.Key)) { ExplodeForceSlider.Value = ExplodeForce = bd.ReadFloat("bmt-" + ExplodeForceSlider.Key); }

                    if (bd.HasKey("bmt-" + ExplodeTorqueSlider.Key)) { ExplodeTorqueSlider.Value = ExplodeTorque = bd.ReadFloat("bmt-" + ExplodeTorqueSlider.Key); }

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

                    blockinfo.BlockData.Write("bmt-" + ExplodeForceSlider.Key, ExplodeForceSlider.Value);

                    blockinfo.BlockData.Write("bmt-" + ExplodeTorqueSlider.Key, ExplodeTorqueSlider.Value);

                    break;
                }

            }
        }

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();

        //    DS.ExplodeForce = ExplodeForce;
        //    DS.ExplodeTorque = ExplodeTorque;
        //}

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            ExplodeForceSlider.DisplayInMapper = value;
            ExplodeTorqueSlider.DisplayInMapper = value;

        }


        //public float ExplodeForce;

        //public float ExplodeTorque;

        private ExplosiveBolt EB;

        protected override void OnSimulateStart()
        {
            EB = GetComponent<ExplosiveBolt>();
            EB.explodePower = ExplodeForce;
            EB.explodeTorquePower = ExplodeTorque;
        }

    }


}
