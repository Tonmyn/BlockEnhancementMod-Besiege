using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;


namespace BlockEnhancementMod.Blocks
{
    class SpringScript :EnhancementBlock
    {

        MSlider DragSlider;

        float Drag = 2;

        //public static BlockMessage blockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Single }), OnCallBack);

        public override void SafeAwake()
        {

            DragSlider = BB.AddSlider(LanguageManager.drag, "Drag", Drag, 0f, 3f);
            DragSlider.ValueChanged += (float value) => { Drag = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Drag = DragSlider.Value; };


#if DEBUG
            ConsoleController.ShowMessage("皮筋添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            DragSlider.DisplayInMapper = value;
        }

        //public override void ChangedProperties()
        //{
        //    if (StatMaster.isClient)
        //    {
        //        ModNetworking.SendToHost(blockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Drag }));
        //    }
        //    else
        //    {
        //        ChangeParameter(Drag);
        //    }
        //}

        Rigidbody A, B;

        public override void ChangeParameter()
        {

            A = GameObject.Find("A").GetComponent<Rigidbody>();
            B = GameObject.Find("B").GetComponent<Rigidbody>();

            A.drag = B.drag = Drag;
        }

        //public static void OnCallBack(Message message)
        //{
        //    Block block = (Block)message.GetData(0);

        //    if ((block == null ? false : block.InternalObject != null))
        //    {
        //        var script = block.InternalObject.GetComponent<SpringScript>();

        //        script.Drag = (float)message.GetData(1);
        //        script.ChangeParameter(script.Drag);
        //    }
        //}


    }
}
