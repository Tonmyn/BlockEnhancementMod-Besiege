using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class PistonScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        int Hardness = 0;

        //public static BlockMessage blockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Integer }),OnCallBack);

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };


#if DEBUG
            ConsoleController.ShowMessage("活塞添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        //public override void ChangedProperties()
        //{
        //    if (StatMaster.isClient)
        //    {
        //        ModNetworking.SendToHost(blockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Hardness }));
        //    }
        //    else
        //    {
        //        ChangeParameter(Hardness);
        //    }

        //}

        public override void ChangeParameter()
        {
            CJ = GetComponent<ConfigurableJoint>();

            SwitchMatalHardness(Hardness, CJ);
        }

        //public static void OnCallBack(Message message)
        //{
        //    Block block = (Block)message.GetData(0);

        //    if ((block == null ? false : block.InternalObject != null))
        //    {
        //        var script = block.InternalObject.GetComponent<PistonScript>();

        //        script.Hardness = (int)message.GetData(1);
        //        script.ChangeParameter(script.Hardness);
        //    }
        //}

    }
}
