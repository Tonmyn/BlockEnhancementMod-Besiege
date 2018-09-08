using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace BlockEnhancementMod
{
    class WoodenScript : EnhancementBlock
    {

        ConfigurableJoint CJ;

        MMenu HardnessMenu;

        public int Hardness = 1;

        //public static BlockMessage BlockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Integer }), OnCallBack);

        public override void SafeAwake()
        {        

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

#if DEBUG
            ConsoleController.ShowMessage("木头组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
        }

        //public override void ChangedProperties()
        //{
        //    if (StatMaster.isClient)
        //    {
        //        ModNetworking.SendToHost(BlockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Hardness }));
        //    }
        //    else
        //    {
        //        ChangeHardness(Hardness);
        //    }
        //}

        public override void ChangeParameter()
        {
            CJ = GetComponent<ConfigurableJoint>();

            SwitchWoodHardness(Hardness, CJ);
        }

        //public static void OnCallBack(Message message)
        //{
        //    Block block = (Block)message.GetData(0);

        //    if ((block == null ? false : block.InternalObject != null))
        //    {
        //        var script = block.InternalObject.GetComponent<WoodenScript>();

        //        script.Hardness = (int)message.GetData(1);
        //        script.ChangeHardness(script.Hardness);
        //    }
        //}

       
    }
}
