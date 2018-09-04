using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod.Blocks
{
    public class GripPadScript : EnhancementBlock
    {

        public MSlider FrictionSlider;

        public MMenu HardnessMenu;

        public float Friction = 1000;

        public int Hardness = 1;

        public static BlockMessage BlockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Integer, DataType.Single }), OnCallBack);

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, WoodHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            FrictionSlider = BB.AddSlider(LanguageManager.friction, "Friction", Friction, 0f, 1000f);
            FrictionSlider.ValueChanged += (float value) => { Friction = Mathf.Abs(value); ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Friction = FrictionSlider.Value; };

#if DEBUG
            ConsoleController.ShowMessage("摩擦垫添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
            FrictionSlider.DisplayInMapper = value;
        }

        public override void ChangedProperties()
        {
            if (StatMaster.isClient)
            {
                ModNetworking.SendToHost(BlockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Hardness, Friction }));
            }
            else
            {
                ChangeParameter(Hardness, Friction);
            }
        }

        private ConfigurableJoint CJ;

        private Collider[] colliders;

        public void ChangeParameter(float hardness,float friction)
        {
            colliders = GetComponentsInChildren<Collider>();
            CJ = GetComponent<ConfigurableJoint>();

            foreach (Collider c in colliders)
            {
                if (c.name == "Collider")
                {
                    c.material.staticFriction = c.material.dynamicFriction = Friction;

                    break;
                }

            }

            SwitchWoodHardness(Hardness, CJ);
        }

        public static void OnCallBack(Message message)
        {
            Block block = (Block)message.GetData(0);

            if ((block == null ? false : block.InternalObject != null))
            {
                var script = block.InternalObject.GetComponent<GripPadScript>();

                script.Hardness = (int)message.GetData(1);
                script.Friction = (float)message.GetData(2);
                script.ChangeParameter(script.Hardness, script.Friction);
            }
        }
    }

    
}
