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

        MToggle /*colliderToggle,*/ kinematicToggle;
        MToggle transparentSupportToggle;
        //MSlider redSlider, greenSlider, blueSlider;
        MSlider /*alphaSlider,*/radiusSlider;
        BuildSurface buildSurface;
        [SerializeField]
        MeshRenderer materialRenderer;
        //SurfaceFragmentController fragmentController;
        [SerializeField]
        private Material transparentMaterial;
        private Material baseMaterial;
        [SerializeField]
        private bool isWood = false;
        [SerializeField]
        private bool isDefaultSkin = false;
        [SerializeField]
        private int visualValue = 0;
        private int lastSkinValue = 0;
        private Shader oldShader;
        [SerializeField]
        private bool transparentUsed = false;

        public override void SafeAwake()
        {
            base.SafeAwake();
            buildSurface = GetComponent<BuildSurface>();

            //colliderToggle = AddToggle("Collider", "collider", false);
            kinematicToggle = AddToggle("Kinematic", "kinematic", false);
            transparentSupportToggle = AddToggle("Transparent Support", "Transparent", false);
            transparentSupportToggle.Toggled += transparentChanged;

            //redSlider = AddSlider("red", "red", 1f, 0f, 1f);
            //redSlider.ValueChanged += colorChanged;
            //greenSlider = AddSlider("green", "green", 1f, 0f, 1f);
            //greenSlider.ValueChanged += colorChanged;
            //blueSlider = AddSlider("blue", "blue", 1f, 0f, 1f);
            //blueSlider.ValueChanged += colorChanged;
            //alphaSlider = AddSlider("Alpha", "Alpha", 1f, 0f, 1f);
            //alphaSlider.ValueChanged += colorChanged;

            radiusSlider = AddSlider("Joint Radius", "Radius", 1f, 0.1f, 1f);
            radiusSlider.ValueChanged += radiusChanged;


            //buildSurface.material.ValueChanged += materialChanged;
            //buildSurface.material.ValueChanged += (value) => { DisplayInMapper(EnhancementEnabled); };
            //buildSurface.hue.ValueChanged += (color) => { colorChanged(0f); };
            //buildSurface.paint.Toggled += (value) => { DisplayInMapper(EnhancementEnabled); };


            //materialChanged(buildSurface.material.Value);

            StartCoroutine(wait());

            IEnumerator wait()
            {
                yield return new WaitUntil(() => BB.Visual != null);
                //buildSurface.hue.ValueChanged += (color) => { materialRenderer.material.color = color; };
                buildSurface.Visual.ValueChanged += skinChanged;
                materialRenderer = transform.FindChild("Vis").GetComponent<MeshRenderer>();
                //BB.Visual.ValueChanged += skinChanged;
                //skinChanged(BB.Visual.Value);
                yield break;
            }
#if DEBUG
            ConsoleController.ShowMessage("蒙皮块添加进阶属性");
#endif
        }
        //public override void ChangedProperties(MapperType mapperType)
        //{
        //    if (mapperType.Key == EnhancementToggle.Key && (mapperType as MToggle).IsActive == false)
        //    {
        //        radiusSlider.Value = 1f;
        //        refreshColorSlider(false);
        //        Debug.Log("reset");
        //    }
        //}

        private void judgeDispaly_transparentToggle()
        {
            var factor = ((int)Mathf.Clamp01(visualValue) == 0 ? false : true);
            transparentSupportToggle.DisplayInMapper = EnhancementEnabled && factor && buildSurface.Visual != null;
        }

        public override void DisplayInMapper(bool enhance)
        {
            /*colliderToggle.DisplayInMapper =*/ kinematicToggle.DisplayInMapper = radiusSlider.DisplayInMapper = enhance;
            ///*redSlider.DisplayInMapper = greenSlider.DisplayInMapper = blueSlider.DisplayInMapper =*/ alphaSlider.DisplayInMapper = enhance && isWood /*&& !isDefaultSkin*/;
            ///
            judgeDispaly_transparentToggle();
            //Debug.Log("dim" + buildSurface.paint.IsActive);
            //alphaSlider.DisplayInMapper = enhance && buildSurface.paint.DisplayInMapper && buildSurface.paint.IsActive;
            //if (alphaSlider.DisplayInMapper)
            //{
            //    materialRenderer = transform.FindChild("Vis").GetComponent<MeshRenderer>();
            //    oldShader = materialRenderer.material.shader;
            //}
        }

        public override void BuildingUpdateAlways_EnhancementEnabled()
        {
            base.BuildingUpdateAlways_EnhancementEnabled();

            if (lastSkinValue != visualValue)
            {
                judgeDispaly_transparentToggle();

                lastSkinValue = visualValue;
            }

        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();

            buildSurface = GetComponent<BuildSurface>();
            //fragmentController = GetComponent<SurfaceFragmentController>();


            //Debug.Log(buildSurface.hasCollision.IsActive);
            //if (colliderToggle.IsActive)
            //{
            //    ////buildSurface.BlockHealth.health = Mathf.Infinity;
            //    //var ctr = fragmentController;
            //    //ctr.fragments.ToList().ForEach(fra => fra.IsBroken = fra.IsIndependent = fra.HasOwnBody = true);

            //    var type = buildSurface.currentType;
            //    type.breakImpactSettings = BuildSurface.BreakImpactSettings.Disabled;
            //    type.breakable = type.burnable = type.dentable = false;
            //    type.fragmentBreakImpactThreshold = Mathf.Infinity;
            //    type.hasHealth = true;

            //    foreach (var joint in buildSurface.Joints)
            //    {
            //        joint.projectionMode = JointProjectionMode.PositionAndRotation;
            //        joint.projectionDistance = joint.projectionAngle = 0f;
            //        joint.breakForce = Mathf.Infinity;
            //        joint.breakTorque = Mathf.Infinity;
            //    }

            //    var cols = transform.FindChild("SimColliders").GetComponentsInChildren<Collider>();
            //    foreach (var col in cols)
            //    {
            //        col.isTrigger = true;
            //    }
            //    var mcols = gameObject.GetComponentsInChildren<MeshCollider>();
            //    foreach (var mcol in mcols)
            //    {
            //        mcol.isTrigger = mcol.convex = true;
            //    }

            //    StartCoroutine(wait());

            //    IEnumerator wait()
            //    {
            //        for (int i = 0; i < 3; i++)
            //        {
            //            yield return 0;
            //        }
            //        yield break;
            //    }
            //}

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

            var value = transparentSupportToggle.IsActive;
            Debug.Log("trans: " + value.ToString());
            if (value)
            {
                transparentChanged(value);
                //transparentUsed = true;
            }
        }
        //public override void OnSimulateStartAlways()
        //{
        //    base.OnSimulateStartAlways();
        //    if (!EnhancementEnabled) return;

        //    if (isWood && !isDefaultSkin)
        //    {
        //        //colorChanged(0);
        //    }
        //}
        //private void materialChanged(int value)
        //{
        //    Debug.Log("material "  + buildSurface.paint.DisplayInMapper);
        //    isWood = value == 0 ? true : false;

        //    var flag = isWood &&/* !isDefaultSkin &&*/ EnhancementEnabled;
        //    refreshColorSlider(flag);
        //}
        private void colorChanged(float value)
        {
            Debug.Log("??");
            //if (materialRenderer == null || EnhancementEnabled == false) return;

            //if (alphaSlider.Value >= 0.99f)
            //{
            //    materialRenderer.material.shader = oldShader;
            //}
            //else
            //{
            //    materialRenderer.material.shader = Shader.Find("Transparent/Diffuse");
            //}
            ////var color = new Color(redSlider.Value, greenSlider.Value, blueSlider.Value, alphaSlider.Value * 6f);
            ////var color = materialRenderer.material.color;
            //var color = buildSurface.hue.Value;
            //color = new Color(color.r, color.g, color.b, alphaSlider.Value * 6f);
            //materialRenderer.material.color = color;
        }
        private void radiusChanged(float value)
        {
            foreach (var tri in buildSurface.JointTriggers)
            {
                tri.transform.localScale = Vector3.one * value;
            }

            foreach (var point in buildSurface.AddingPoints)
            {
                point.transform.localScale = Vector3.one * value;
            }
        }

        private void transparentChanged(bool value)
        {
            Debug.Log(buildSurface.Visual + "|" + visualValue);
            if (OptionsMaster.skinsEnabled == false || visualValue == 0) return;

            Debug.Log("trans changed: " + value.ToString());

            if (value)
            {
                //oldShader = materialRenderer.material.shader;
                //materialRenderer.material.shader = Shader.Find("Transparent/Diffuse");
                materialRenderer.material = transparentMaterial;
            }
            else
            {
                //materialRenderer.material.shader = oldShader;
                materialRenderer.material = baseMaterial;
            }

        }
        //private void skinChanged(int value)
        //{
        //    Debug.Log("skin " + value);
        //    if (!EnhancementEnabled) return;

        //    isDefaultSkin = value == 0 ? true : false;
        //    var flag = isWood/* && !isDefaultSkin*/ && lastSkinValue == value;
        //    lastSkinValue = value;
        //    StartCoroutine(wait());

        //    IEnumerator wait()
        //    {
        //        yield return new WaitForSeconds(Time.deltaTime * 3f);
        //        refreshColorSlider(flag);
        //        yield break;
        //    }
        //}

        private void skinChanged(int value)
        {
            if (!EnhancementEnabled) return;

            Debug.Log("skin changed" + value.ToString());

            visualValue = value;
            Debug.Log(lastSkinValue + "||" + visualValue);

            if (visualValue != lastSkinValue)
            {
                transparentSupportToggle.IsActive = false;
            }

            if (visualValue != 0)
            {
                if (visualValue != lastSkinValue)
                {
                    Destroy(baseMaterial);
                    Destroy(transparentMaterial);

                    baseMaterial = new Material(materialRenderer.material);
                    transparentMaterial = intiMaterial(materialRenderer.material);
                }
            }

            Material intiMaterial(Material material)
            {
                var _material = new Material(material);
                _material.name = "Transparent Material";
                _material.shader = Shader.Find("Transparent/Diffuse");

                return _material;
            }
        }

        //private void refreshColorSlider(bool display)
        //{
        //    if (display)
        //    {
        //        materialRenderer = transform.FindChild("Vis").GetComponent<MeshRenderer>();
        //        oldShader = materialRenderer.material.shader;
        //    }
        //    else
        //    {
        //        if (oldShader != null)
        //        {
        //            //materialRenderer.material.shader = oldShader;
        //        }
        //    }

        //    //redSlider.DisplayInMapper = display;
        //    //greenSlider.DisplayInMapper = display;
        //    //blueSlider.DisplayInMapper = display;
        //    alphaSlider.DisplayInMapper = display;

        //    //redSlider.Value = 1f;
        //    //greenSlider.Value = 1f;
        //    //blueSlider.Value = 1f;
        //    alphaSlider.Value = 1f;

        //    Debug.Log("refresh");
        //}
    }
}
