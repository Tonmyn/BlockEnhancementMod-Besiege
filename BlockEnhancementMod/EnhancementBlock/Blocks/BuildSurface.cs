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

        MToggle colliderToggle,kinematicToggle;
        BuildSurface buildSurface;
        SurfaceFragmentController fragmentController;

        public override void SafeAwake()
        {
            base.SafeAwake();

            colliderToggle = AddToggle("Collider", "collider", false);
            kinematicToggle = AddToggle("Kinematic", "kinematic", false);
        }
        public override void DisplayInMapper(bool enhance)
        {
            colliderToggle.DisplayInMapper = kinematicToggle.DisplayInMapper = enhance;
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            buildSurface = GetComponent<BuildSurface>();
            fragmentController = GetComponent<SurfaceFragmentController>();

            if (colliderToggle.IsActive)
            {

                buildSurface.isValid = false;
                buildSurface.BlockHealth.health = Mathf.Infinity;

                foreach (var joint in buildSurface.Joints)
                {
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    joint.projectionDistance = joint.projectionAngle = 0f;
                    joint.breakForce = Mathf.Infinity;
                    joint.breakTorque = Mathf.Infinity;
                }

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

       

                    yield break;
                }
            }

            if (kinematicToggle.IsActive)
            {
                StartCoroutine(wait1());

                IEnumerator wait1()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        yield return 0;
                    }
                    buildSurface.Rigidbody.isKinematic = true;
                    buildSurface.gameObject.isStatic = true;
                    yield break;
                }
             
            }
        }
    }
}
