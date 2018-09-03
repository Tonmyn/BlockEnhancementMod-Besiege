using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{

    class SteeringHinge : EnhancementBlock
    {

        SteeringWheel steeringWheel;

        public MToggle r2cToggle;

        public bool ReturnToCenter = false;

        public float angleSpeed;

        MSlider rotationSpeedSlider;

        Rigidbody rigidbody;

        MKey leftKey;

        MKey rightKey;

        public static BlockMessage blockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Boolean }), OnCallBack);

        public override void SafeAwake()
        {
            steeringWheel = GetComponent<SteeringWheel>();

            r2cToggle = BB.AddToggle(LanguageManager.returnToCenter, "Return to center", ReturnToCenter);
            r2cToggle.Toggled += (bool value) => { ReturnToCenter = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { ReturnToCenter = r2cToggle.IsActive; };

            leftKey = steeringWheel.KeyList.ToList().Find(match => match.Key == "left");
            rightKey = steeringWheel.KeyList.ToList().Find(match => match.Key == "right");
            rotationSpeedSlider = steeringWheel.Sliders.ToList().Find(match => match.Key == "rotation-speed");

#if DEBUG
            ConsoleController.ShowMessage("转向关节添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            r2cToggle.DisplayInMapper = value;
        }

        public override void ChangedProperties()
        {
            if (StatMaster.isClient)
            {
                ModNetworking.SendToHost(blockMessage.messageType.CreateMessage(new object[] { Block.From(BB), ReturnToCenter}));
            }
            else
            {
                ChangeParameter(ReturnToCenter);
            }
        }

        public void ChangeParameter(bool value)
        {

            rigidbody = GetComponent<Rigidbody>();

            ReturnToCenter = value;
        }

        void Update()
        {
            if (BB.isSimulating/* && (StatMaster.isHosting || StatMaster.isLocalSim)*/)
            {
                if (!(leftKey.IsDown || rightKey.IsDown) && ReturnToCenter && steeringWheel.AngleToBe != 0)
                {
                    rigidbody.WakeUp();

                    angleSpeed = Time.deltaTime * 100f * steeringWheel.targetAngleSpeed * rotationSpeedSlider.Value;

                    steeringWheel.AngleToBe = Mathf.MoveTowardsAngle(steeringWheel.AngleToBe, 0f, angleSpeed);
                }
            }
        }

        public static void OnCallBack(Message message)
        {
            Block block = (Block)message.GetData(0);

            if ((block == null ? false : block.InternalObject != null))
            {
                var script = block.InternalObject.GetComponent<SteeringHinge>();

                script.ReturnToCenter = (bool)message.GetData(1);
                script.ChangeParameter(script.ReturnToCenter);
            }
        }
    }


}
