using System;
using System.Collections;
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
                var mcols = gameObject.GetComponentsInChildren<MeshCollider>();
                foreach (var mcol in mcols)
                {
                    mcol.isTrigger = mcol.convex = true;
                }
          
                StartCoroutine(wait());

                IEnumerator wait()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        yield return 0;
                    }
                    var joint = transform.GetComponent<ConfigurableJoint>();
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    joint.projectionDistance = joint.projectionAngle = 0f;
                    joint.breakForce *= 3f;
                    joint.breakTorque *= 3f;

                    yield break;
                }
            }
          

        }
    }
}
