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
        MToggle NearToggle;

        public bool ReturnToCenter = false;
        public bool Near = true;
        private bool orginReturnToCenter = false;

        

        MSlider rotationSpeedSlider;
        Rigidbody rigidbody;
        MKey leftKey;
        MKey rightKey;
        MKey lastKey;

        public override void SafeAwake()
        {
            steeringWheel = GetComponent<SteeringWheel>();

            r2cToggle = BB.AddToggle(LanguageManager.returnToCenter, "Return to center", ReturnToCenter);
            r2cToggle.Toggled += (bool value) => { ReturnToCenter = NearToggle.DisplayInMapper = value; ChangedProperties(); };

            NearToggle = BB.AddToggle(LanguageManager.near, "Near", Near);
            NearToggle.Toggled += (bool value) => { Near = value; ChangedProperties(); };

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
            NearToggle.DisplayInMapper = value && ReturnToCenter;
        }

        public override void ChangeParameter()
        {
            
            rigidbody = GetComponent<Rigidbody>();

            if (!EnhancementEnabled) { ReturnToCenter = orginReturnToCenter; }
        }

        public override void SimulateUpdateAlways()
        {
            if (StatMaster.isClient) return;

            getLastKey();

            if (!(leftKey.IsDown || rightKey.IsDown) && ReturnToCenter && steeringWheel.AngleToBe != 0)
            {
                rigidbody.WakeUp();

                float angleSpeed = Time.deltaTime * 100f * steeringWheel.targetAngleSpeed * rotationSpeedSlider.Value;

                float target = 0;

                if (!Near && lastKey != null)
                {
                    float sign = Mathf.Sign(steeringWheel.AngleToBe);
                   
                    if (lastKey.Key == "left" && sign < 0)
                    {
                        target = 179;
                    }
                    else if (lastKey.Key == "right" && sign > 0)
                    {
                        target = -179;
                    }
                    else
                    {
                        target = 0;
                    }              
                }
                else
                {
                    target = 0;
                }
                steeringWheel.AngleToBe = Mathf.MoveTowardsAngle(steeringWheel.AngleToBe, target, angleSpeed);
            }

            void getLastKey()
            {
                if (steeringWheel.AngleToBe != 0f)
                {
                    if (leftKey.IsReleased) lastKey = leftKey;
                    if (rightKey.IsReleased) lastKey = rightKey;
                }
                else
                {
                    lastKey = null;
                }
            }
        }
    }


}
