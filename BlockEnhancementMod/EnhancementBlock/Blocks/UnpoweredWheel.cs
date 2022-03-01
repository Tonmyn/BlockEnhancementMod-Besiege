using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;

namespace BlockEnhancementMod
{
    class UnpoweredWheel : EnhancementBlock
    {
        MSlider FrictionSlider;
        MSlider BouncinessSlider;
        MToggle collisionToggle;
        float Friction = 0.8f;

        public override void SafeAwake()
        {
            FrictionSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Friction, "Friction", Friction, 0.1f, 3f);
            BouncinessSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Bounciness, "Bounciness", /*Bounciness*/0f, 0f, 1f);

            collisionToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.ShowCollider, "Collision", true);
            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("无动力轮子组件添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            FrictionSlider.DisplayInMapper = value;
            BouncinessSlider.DisplayInMapper = value;
        }


        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            if (collisionToggle.IsActive)
            {
                Debug.Log("close collision");
                var cols = BB.transform.GetComponentsInChildren<Collider>();
                foreach (var col in cols)
                {
                    col.isTrigger = true;
                }
            }
        }
    }
}
