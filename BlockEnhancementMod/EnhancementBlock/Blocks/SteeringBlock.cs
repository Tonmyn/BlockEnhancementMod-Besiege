using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class SteeringBlockScript : SteeringWheel_GenericEnhanceScript
    {
        MLimits limitSlider;

        public override void SafeAwake()
        {
            base.SafeAwake();

            FauxTransform fauxTransform = new FauxTransform(new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), Vector3.one * 0.35f);
            limitSlider = AddLimits("Limit Angle", "limit angle", 45f, 45f, 180f, fauxTransform);
            limitSlider.UseLimitsToggle.Toggled += (value) => { steeringWheel.allowLimits = value; };

            StartCoroutine(wait(false));

            IEnumerator wait(bool value)
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return 0;
                }
                if (EnhancementEnabled == false)
                {
                    limitSlider.UseLimitsToggle.IsActive = value;
                }
                yield break;
            }
        }

        public override void DisplayInMapper(bool enhance)
        {
            base.DisplayInMapper(enhance);
            var useLimit = limitSlider.UseLimitsToggle.IsActive;

            limitSlider.DisplayInMapper = enhance && useLimit;
            limitSlider.UseLimitsToggle.DisplayInMapper = enhance;
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();
            StartCoroutine(wait());

            IEnumerator wait()
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return 0;
                }

                if (limitSlider.IsActive)
                {
                    steeringWheel.allowLimits = limitSlider.IsActive;
                    steeringWheel.limits = new Vector2(-limitSlider.Min, limitSlider.Max);
                    steeringWheel.LimitsSlider.Max = limitSlider.Max;
                    steeringWheel.LimitsSlider.Min = limitSlider.Min;

                }
                else
                {
                    steeringWheel.allowLimits = false;
   
                }
                yield break;
            }

      
        }
    }
}
