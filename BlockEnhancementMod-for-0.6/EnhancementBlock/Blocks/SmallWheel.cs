using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    public class SmallwheelScript : EnhancementBlock
    {

        MSlider SpeedSlider;

        public float Speed = 5;

        //public static BlockMessage blockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Single }), OnCallBack);

        public override void SafeAwake()
        {

            SpeedSlider = BB.AddSlider(LanguageManager.rotatingSpeed, "Speed", Speed, 0f, 5f);
            SpeedSlider.ValueChanged += (float value) => { Speed = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Speed = SpeedSlider.Value; };

#if DEBUG
            ConsoleController.ShowMessage("小轮子添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            SpeedSlider.DisplayInMapper = value;
        }

        //public override void ChangedProperties()
        //{
        //    if (StatMaster.isClient)
        //    {
        //        ModNetworking.SendToHost(blockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Speed }));
        //    }
        //    else
        //    {
        //        ChangeParameter(Speed);
        //    }
        //}

        public override void ChangeParameter()
        {
            GetComponent<SmallWheel>().speed = Speed;
        }

        //public static void OnCallBack(Message message)
        //{
        //    Block block = (Block)message.GetData(0);

        //    if ((block == null ? false : block.InternalObject != null))
        //    {
        //        var script = block.InternalObject.GetComponent<SmallwheelScript>();

        //        script.Speed = (float)message.GetData(1);
        //        script.ChangeParameter(script.Speed);
        //    }
        //}



    }
}
