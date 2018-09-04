using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class BallJointScript : EnhancementBlock
    {

        public MToggle RotationToggle;

        public bool Rotation = false;

        public ConfigurableJoint CJ;

        private float BreakTorque;

        public static BlockMessage BlockMessage { get; } = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Boolean }), OnCallBack);

        public override void SafeAwake()
        {

            RotationToggle = BB.AddToggle(LanguageManager.cvJoint, "Rotation", Rotation);
            RotationToggle.Toggled += (bool value) => { Rotation = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Rotation = RotationToggle.IsActive; };

#if DEBUG
            ConsoleController.ShowMessage("球铰添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            RotationToggle.DisplayInMapper = value;
        }

        public override void ChangedProperties()
        {
            if (StatMaster.isClient)
            {
                ModNetworking.SendToHost(BlockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Rotation }));
                Debug.Log("send");
            }
            else
            {
                ChangeParameter(Rotation);
            }
        }

        public void ChangeParameter(bool value)
        {
            CJ = GetComponent<ConfigurableJoint>();
            BreakTorque = CJ.breakTorque;

            if (value)
            {
                CJ.angularYMotion = ConfigurableJointMotion.Locked;
                CJ.breakTorque = Mathf.Infinity;
            }
            else
            {
                CJ.angularYMotion = ConfigurableJointMotion.Free;
                CJ.breakTorque = BreakTorque;
            }
        }

        public static void OnCallBack(Message message)
        {
            Block block = (Block)message.GetData(0);

            if ((block == null ? false : block.InternalObject != null))
            {
                var script = block.InternalObject.GetComponent<BallJointScript>();
                
                script.Rotation = (bool)message.GetData(1);
                script.ChangeParameter(script.Rotation);
            }
        }

    }

}
