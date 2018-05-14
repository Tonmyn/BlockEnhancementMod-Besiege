using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace BlockEnhancementMod.Blocks
{
    public class Smallwheel : Block
    {
        SmallWheelScript SWS;

        MSlider SpeedSlider;

        public float Speed = 5;

        public Smallwheel(BlockBehaviour block) : base(block)
        {

            if (BB.GetComponent<SmallWheelScript>() == null)
            {
                SWS = BB.gameObject.AddComponent<SmallWheelScript>();

                SpeedSlider = new MSlider("旋转速度", "Speed", Speed, 0f, 5f, false);
                SpeedSlider.ValueChanged += (float value) => { Speed = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(SpeedSlider);
            }

            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("小轮子添加进阶属性");
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

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            SWS.Speed = Speed;
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            SpeedSlider.DisplayInMapper = value;
        }

        class SmallWheelScript : MonoBehaviour
        {

            public float Speed;

            private void Start()
            {
                GetComponent<SmallWheel>().speed = Speed;
            }
        }
    }
}
