using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class Balloon_EnhanceScript:ChangeSpeedBlock
    {
        private MKey effectKey;
        private MToggle toggleToggle,dragTogetherToggle;
        private BalloonController  balloonController;

        private bool effected = false;
        private float lastSpeed = 0f, defaultDrag = 0f, defaultAngularDrag = 0f;
        public override void SafeAwake()
        {
            effectKey = AddKey("Effect", "Effect", UnityEngine.KeyCode.E);
            toggleToggle = AddToggle("Toggle", "Toggle", true);
            dragTogetherToggle = AddToggle("drag together", "together", true);

            balloonController = GetComponent<BalloonController>();
            SpeedSlider = balloonController.BuoyancySlider;

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("气球添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            effectKey.DisplayInMapper = value;
            toggleToggle.DisplayInMapper = value;
            dragTogetherToggle.DisplayInMapper = value;
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            lastSpeed = Speed;
            defaultDrag = GetComponent<Rigidbody>().drag;
            defaultAngularDrag = GetComponent<Rigidbody>().angularDrag;
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            try
            {
                if (!effected)
                {
                    if (AddSpeedKey.IsPressed || AddSpeedKey.EmulationPressed())
                    {
                        Speed += ChangeSpeedValue.Value;
                        lastSpeed = Speed;
                    }

                    if (ReduceSpeedKey.IsPressed || ReduceSpeedKey.EmulationPressed())
                    {
                        Speed -= ChangeSpeedValue.Value;
                        lastSpeed = Speed;
                    }
                }
            }
            catch { }

            if (toggleToggle.IsActive)
            {
                if (effectKey.IsPressed || effectKey.EmulationPressed())
                {
                    effected = !effected;
                    setValue();
                }
            }
            else
            {
                if (effectKey.IsHeld || effectKey.EmulationHeld())
                {
                    effected = true;
                }
                else
                {
                    effected = false;
                }
                setValue();
            }

            void setValue()
            {
                Speed = effected ? 0f : lastSpeed;
                if (dragTogetherToggle.IsActive)
                {
                    var rigi = GetComponent<Rigidbody>();
                    rigi.drag = effected ? 0f : defaultDrag;
                    rigi.angularDrag = effected ? 0f : defaultAngularDrag;
                }
            }
        }
    }
}
