using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    class SliderScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        MSlider LimitSlider;

        int Hardness = 1;

        float Limit = 1;

        public static BlockMessage blockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Integer, DataType.Single }), OnCallBack);

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            LimitSlider = BB.AddSlider(LanguageManager.limit, "Limit", Limit, 0f, 2f);
            LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Limit = LimitSlider.Value; };

#if DEBUG
            ConsoleController.ShowMessage("滑块添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            LimitSlider.DisplayInMapper = value;
        }

        ConfigurableJoint CJ;

        public override void ChangedProperties()
        {
            if (StatMaster.isClient)
            {
                ModNetworking.SendToHost(blockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Hardness, Limit }));
            }
            else
            {
                ChangeParameter(Hardness, Limit);
            }
        }

        public void ChangeParameter(int hardness,float Limit)
        {

            CJ = GetComponent<ConfigurableJoint>();

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = this.Limit = Mathf.Abs(Limit);
            CJ.linearLimit = limit;

            SwitchWoodHardness(hardness, CJ);
        }

        public static void OnCallBack(Message message)
        {
            Block block = (Block)message.GetData(0);

            if ((block == null ? false : block.InternalObject != null))
            {
                var script = block.InternalObject.GetComponent<SliderScript>();

                script.Hardness = (int)message.GetData(1);
                script.Limit = (float)message.GetData(2);
                script.ChangeParameter(script.Hardness, script.Limit);
            }
        }

    }
}
