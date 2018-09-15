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

        MToggle r2cToggle;

        public bool ReturnToCenter = false;
        private bool orginReturnToCenter = false;

        float angleSpeed;

        MSlider rotationSpeedSlider;
        Rigidbody rigidbody;
        MKey leftKey;
        MKey rightKey;

        public override void SafeAwake()
        {
            steeringWheel = GetComponent<SteeringWheel>();

            r2cToggle = BB.AddToggle(LanguageManager.returnToCenter, "Return to center", ReturnToCenter);
            r2cToggle.Toggled += (bool value) => { ReturnToCenter = value; ChangedProperties(); };

            leftKey = steeringWheel.Keys.First(match => match.Key == "left");
            rightKey = steeringWheel.Keys.First(match => match.Key == "right");
            rotationSpeedSlider = steeringWheel.Sliders.First(match => match.Key == "rotation-speed");

#if DEBUG
            ConsoleController.ShowMessage("转向关节添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            r2cToggle.DisplayInMapper = value;
        }

        public override void ChangeParameter()
        {
            
            rigidbody = GetComponent<Rigidbody>();

            if (!EnhancementEnabled) { ReturnToCenter = orginReturnToCenter; }
        }

        public override void SimulateUpdateAlways()
        {
            if (StatMaster.isClient) return;

            if (!(leftKey.IsDown || rightKey.IsDown) && ReturnToCenter && steeringWheel.AngleToBe != 0)
            {
                rigidbody.WakeUp();

                angleSpeed = Time.deltaTime * 100f * steeringWheel.targetAngleSpeed * rotationSpeedSlider.Value;

                steeringWheel.AngleToBe = Mathf.MoveTowardsAngle(steeringWheel.AngleToBe, 0f, angleSpeed);
            }
        }
    }


}
