using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class SqrBalloonScript : ChangeSpeedBlock
    {
        private MKey effectKey;
        //private MSlider powerSlider;
        private MToggle toggleToggle,dragTogetherToggle;
        private SqrBalloonController sqrBalloonController;

        public bool Effected { get { return effected; }set { effected = value; } }
        private bool effected = false;
        private float lastBuoyancy = 0f, defaultDrag = 0f, defaultAngularDrag = 0f;

        public override void SafeAwake()
        {
            effectKey = AddKey(LanguageManager.Instance.CurrentLanguage.Effected, "Effect", UnityEngine.KeyCode.E);
            toggleToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ToggleMode, "Toggle", true);
            dragTogetherToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.DragTogether, "Together", true);

            sqrBalloonController = GetComponent<SqrBalloonController>();
            SpeedSlider = sqrBalloonController.PowerSlider;

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("热气球添加进阶属性");
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

            lastBuoyancy = Speed;
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
                        lastBuoyancy = Speed;
                    }

                    if (ReduceSpeedKey.IsPressed || ReduceSpeedKey.EmulationPressed())
                    {
                        Speed -= ChangeSpeedValue.Value;
                        lastBuoyancy = Speed;
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
                Speed = effected ? 0f : lastBuoyancy;
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
