using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class FlamethrowerScript: EnhancementBlock
    {
        FlamethrowerController flamethrowerController;

        public float ThrustForce = 0f;

        public Color FlameColor = Color.white;

        MSlider thrustForceSlider;

        MColourSlider flameColorSlider;

        Rigidbody rigidbody;

        ParticleSystem particleSystem;

        public static BlockMessage BlockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Single, DataType.Color }), OnCallBack);

        public override void SafeAwake()
        {

            thrustForceSlider = BB.AddSlider(LanguageManager.thrustForce, "Thrust Force", ThrustForce, 0f, 5f);
            thrustForceSlider.ValueChanged += (float value) => { ThrustForce = value; ChangedProperties(); };           
            flameColorSlider = BB.AddColourSlider(LanguageManager.flameColor, "Flame Color", FlameColor, false);
            flameColorSlider.ValueChanged += (Color value) => { FlameColor = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { ThrustForce = thrustForceSlider.Value; FlameColor = flameColorSlider.Value; };
#if DEBUG
            ConsoleController.ShowMessage("喷火器添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            thrustForceSlider.DisplayInMapper = value;
            flameColorSlider.DisplayInMapper = value;
        }

        public override void ChangedProperties()
        {
            if (StatMaster.isClient)
            {
                ModNetworking.SendToHost(BlockMessage.messageType.CreateMessage(new object[] { Block.From(BB), ThrustForce, FlameColor }));
            }
            else
            {
                ChangeParameter(ThrustForce, FlameColor);
            }
        }

        public void ChangeParameter(float force,Color color)
        {
            flamethrowerController = GetComponent<FlamethrowerController>();
            particleSystem = flamethrowerController.fireParticles;
            rigidbody = GetComponent<Rigidbody>();

            particleSystem.startColor = color;
            ThrustForce = force;
        }

        void Update()
        {
   
        }

        public void SimulateFixedUpdateHost()
        {
            Debug.Log("??");

            if (ThrustForce != 0 && flamethrowerController.isFlaming)
            {
                rigidbody.AddRelativeForce(-Vector3.forward * ThrustForce * 100f);
            }
        }

        public static void OnCallBack(Message message)
        {
            Block block = (Block)message.GetData(0);

            if ((block == null ? false : block.InternalObject != null))
            {
                var script = block.InternalObject.GetComponent<FlamethrowerScript>();

                script.ThrustForce = (float)message.GetData(1);
                script.FlameColor = (Color)message.GetData(2);
                script.ChangeParameter(script.ThrustForce, script.FlameColor);
            }
        }
    }
}
