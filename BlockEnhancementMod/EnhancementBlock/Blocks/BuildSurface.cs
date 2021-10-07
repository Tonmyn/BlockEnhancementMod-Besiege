using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class BuildSurfaceScript : EnhancementBlock
    {

        MToggle ColliderToggle;

        public override void SafeAwake()
        {
            base.SafeAwake();

            ColliderToggle = AddToggle("Collider", "collider", false);
        }
        public override void DisplayInMapper(bool enhance)
        {
            ColliderToggle.DisplayInMapper = enhance;
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            if (ColliderToggle.IsActive)
            {
                var cols = transform.FindChild("SimColliders").GetComponentsInChildren<Collider>();
                foreach (var col in cols)
                {
                    //col.enabled = false;
                    col.isTrigger = true;
                }
            }
          

        }
    }
}
