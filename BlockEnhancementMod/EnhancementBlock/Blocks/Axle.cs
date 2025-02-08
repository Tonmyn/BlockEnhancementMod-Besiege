using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class AxleScript : EnhancementBlock
    {

        public MToggle CollisionToggle;

        private Collider col;

        public override void SafeAwake()
        {

            CollisionToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.Collision, "Collision", false);

#if DEBUG
            ConsoleController.ShowMessage("万向节添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            CollisionToggle.DisplayInMapper = value;
        }

        public override void OnSimulateStartAlways()
        {
            if (EnhancementEnabled)
            {
                col = GetComponent<CapsuleCollider>();

                if (CollisionToggle.IsActive)
                {
                    col.enabled = false;
                }
            }        
        }
    }

}
