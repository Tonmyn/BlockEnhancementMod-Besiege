using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class DecouplerScript : EnhancementBlock
    {

        MSlider ExplodeForceSlider;

        MSlider ExplodeTorqueSlider;

        float ExplodeForce = 1000f;

        float ExplodeTorque = 2000f;

        public static BlockMessage BlockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Single,DataType.Single }), OnCallBack);

        public override void SafeAwake()
        {

            ExplodeForceSlider = BB.AddSlider(LanguageManager.explodeForce, "ExplodeForce", ExplodeForce, 0, 3000f);
            ExplodeForceSlider.ValueChanged += (float value) => { ExplodeForce = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { ExplodeForce = ExplodeForceSlider.Value; };

            ExplodeTorqueSlider = BB.AddSlider(LanguageManager.explodeTorque, "ExplodeTorque", ExplodeTorque, 0, 2500f);
            ExplodeTorqueSlider.ValueChanged += (float value) => { ExplodeTorque = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { ExplodeTorque = ExplodeTorqueSlider.Value; };


#if DEBUG
            ConsoleController.ShowMessage("分离铰链添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            ExplodeForceSlider.DisplayInMapper = value;
            ExplodeTorqueSlider.DisplayInMapper = value;

        }

        public override void ChangedProperties()
        {
            if (StatMaster.isClient)
            {
                ModNetworking.SendToHost(BlockMessage.messageType.CreateMessage(new object[] { Block.From(BB), ExplodeForce, ExplodeTorque }));
            }
            else
            {
                ChangeParameter(ExplodeForce, ExplodeTorque);
            }
        }

        private ExplosiveBolt EB;

        public void ChangeParameter(float force ,float torque)
        {
            EB = GetComponent<ExplosiveBolt>();
            EB.explodePower = force;
            EB.explodeTorquePower = torque;
        }

        public static void OnCallBack(Message message)
        {
            Block block = (Block)message.GetData(0);

            if ((block == null ? false : block.InternalObject != null))
            {
                var script = block.InternalObject.GetComponent<DecouplerScript>();

                script.ExplodeForce = (float)message.GetData(1);
                script.ExplodeTorque = (float)message.GetData(2);
                script.ChangeParameter(script.ExplodeForce, script.ExplodeTorque);
            }
        }
    }


}
