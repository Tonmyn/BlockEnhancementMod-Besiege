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
        MToggle collisionToggle;

        public override void SafeAwake()
        {
            collisionToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.Collision, "Collision", true);
            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("无动力轮子组件添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            collisionToggle.DisplayInMapper = value;
        }


        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            if (!collisionToggle.IsActive)
            {
#if DEBUG
                Debug.Log("close collision");
#endif
                var cols = BB.transform.GetComponentsInChildren<Collider>();
                foreach (var col in cols)
                {
                    col.isTrigger = true;
                }
            }
        }
    }
}
