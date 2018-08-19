using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
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
    
        protected override void SafeAwake()
        {
            steeringWheel = GetComponent<SteeringWheel>();
            rigidbody = GetComponent<Rigidbody>();

            r2cToggle = AddToggle(LanguageManager.returnToCenter, "Return to center", ReturnToCenter);
            r2cToggle.Toggled += (bool value) => { ReturnToCenter = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ReturnToCenter = r2cToggle.IsActive; };

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

        protected override void OnSimulateUpdate()
        {
            if (!(leftKey.IsDown || rightKey.IsDown) && ReturnToCenter && steeringWheel.AngleToBe != 0)
            {
                rigidbody.WakeUp();

                angleSpeed = Time.deltaTime * 100f * steeringWheel.targetAngleSpeed * rotationSpeedSlider.Value;

                steeringWheel.AngleToBe = Mathf.MoveTowardsAngle(steeringWheel.AngleToBe, 0f, angleSpeed);
            }
        }
    }


}
