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
        MSlider alphaSlider;
        BuildSurface buildSurface;
        SurfaceFragmentController fragmentController;
        MeshRenderer materialRenderer;

        private bool isWood = false;
        private Shader oldShader;
        public override void SafeAwake()
        {
            base.SafeAwake();
            buildSurface = GetComponent<BuildSurface>();

            colliderToggle = AddToggle("Collider", "collider", false);
            kinematicToggle = AddToggle("Kinematic", "kinematic", false);

            alphaSlider = AddSlider("Alpha", "Alpha", 1f, 0f, 1f);
            alphaSlider.ValueChanged += alphaValueChanged;
            buildSurface.material.ValueChanged += materialChanged;
            materialChanged(buildSurface.material.Value);
        }
        public override void DisplayInMapper(bool enhance)
        {
            colliderToggle.DisplayInMapper = kinematicToggle.DisplayInMapper = enhance;
            materialChanged(buildSurface.material.Value);
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            buildSurface = GetComponent<BuildSurface>();
            fragmentController = GetComponent<SurfaceFragmentController>();

            if (colliderToggle.IsActive)
            {
                //buildSurface.BlockHealth.health = Mathf.Infinity;
                var ctr = fragmentController;
                ctr.fragments.ToList().ForEach(fra => fra.IsBroken = fra.IsIndependent = fra.HasOwnBody = true);

                var type = buildSurface.currentType;
                type.breakImpactSettings = BuildSurface.BreakImpactSettings.Disabled;
                type.breakable = type.burnable = type.dentable = false;
                type.fragmentBreakImpactThreshold = Mathf.Infinity;
                type.hasHealth = true;
               
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

        private void materialChanged(int value)
        {
            isWood = value == 0 ? true : false;
            alphaSlider.DisplayInMapper = isWood && EnhancementEnabled;

            if (EnhancementEnabled == false) return;
            if (isWood)
            {
                materialRenderer = transform.FindChild("Vis").GetComponent<MeshRenderer>();
                oldShader = materialRenderer.material.shader;
                alphaValueChanged(alphaSlider.Value);
            }
        }

        private void alphaValueChanged(float value)
        {
            if (materialRenderer == null || EnhancementEnabled == false) return;

            if (value >= 0.99f)
            {
                materialRenderer.material.shader = oldShader;
            }
            else
            {
                materialRenderer.material.shader = Shader.Find("Transparent/Diffuse");
                var color = materialRenderer.material.color;
                materialRenderer.material.color = new Color(color.r, color.g, color.b, value * 6f);
            }   
        }
    }
}
