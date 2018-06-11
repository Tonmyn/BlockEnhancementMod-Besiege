using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace BlockEnhancementMod.Blocks
{
    public class SmallwheelScript : EnhancementBlock
    {
        //SmallWheelScript SWS;

        MSlider SpeedSlider;

        public float Speed = 5;

        protected override void SafeStart()
        {

            SpeedSlider = new MSlider("旋转速度", "Speed", Speed, 0f, 5f, false);
            SpeedSlider.ValueChanged += (float value) => { Speed = value; ChangedProperties(); };
            CurrentMapperTypes.Add(SpeedSlider);

#if DEBUG
            BesiegeConsoleController.ShowMessage("小轮子添加进阶属性");
#endif
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + SpeedSlider.Key, SpeedSlider.Value);

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

                    if (bd.HasKey("bmt-" + SpeedSlider.Key)) { SpeedSlider.Value = Speed = bd.ReadFloat("bmt-" + SpeedSlider.Key); }

                    break;
                }

            }
        }

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();
        //    SWS.Speed = Speed;
        //}

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            SpeedSlider.DisplayInMapper = value;
        }


        //public float Speed;
        protected override void OnSimulateStart()
        {
            GetComponent<SmallWheel>().speed = Speed;
        }

    }
}
