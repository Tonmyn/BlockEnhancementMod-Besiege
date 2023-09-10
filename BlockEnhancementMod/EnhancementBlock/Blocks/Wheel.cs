using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using System.Collections;
using InternalModding.Mods;
using InternalModding.Assemblies;
using System.Reflection;
using System.Security.AccessControl;
using InternalModding.Misc;
using System.Linq;

namespace BlockEnhancementMod.Blocks
{

    class WheelScript : CogMotoControllerHinge_GenericEnhanceScript
    {
        MToggle collisionToggle;
        MToggle CustomColliderToggle;
        MToggle ShowColliderToggle;
        MSlider FrictionSlider;
        MSlider BouncinessSlider;

        //bool Collider = false;
        //bool ShowCollider = true;
        float Friction = 0.8f;
        //float Bounciness = 0f;
        int ID = 0;

        private static GameObject WheelColliderOrgin;


        public override void SafeAwake()
        {
            ID = GetComponent<BlockVisualController>().ID;
            Friction = PSaF.GetPositionScaleAndFriction(ID).Friction;

            collisionToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.Collision, "Collision", true);

            CustomColliderToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.CustomCollider, "Custom Collider", false);

            ShowColliderToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.ShowCollider, "Show Collider", true);

            FrictionSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Friction, "Friction", Friction, 0.1f, 3f);

            BouncinessSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Bounciness, "Bounciness", /*Bounciness*/0f, 0f, 1f);

            if (WheelColliderOrgin == null)
            {
                StartCoroutine(ReadWheelMesh());
            }
            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("轮子组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            collisionToggle.DisplayInMapper = value;
            CustomColliderToggle.DisplayInMapper = value;
            ShowColliderToggle.DisplayInMapper = value && CustomColliderToggle.IsActive;
            FrictionSlider.DisplayInMapper = value;
            BouncinessSlider.DisplayInMapper = value;
        }


        /// <summary>
        /// 是否是轮子零件
        /// </summary>
        /// <param name="id">零件号</param>
        /// <returns></returns>
        private static bool IsWheel(int id)
        {
            bool result = false;

            switch (id)
            {
                case (int)BlockType.LargeWheel:
                    result = true;
                    break;
                case (int)BlockType.Wheel:
                    result = true;
                    break;
                case (int)BlockType.WheelUnpowered:
                    result = true;
                    break;
                case (int)BlockType.LargeWheelUnpowered:
                    result = true;
                    break;

                default: result = false; break;
            }
            return result;

        }

        private Collider[] Colliders;
        private MeshFilter mFilter;
        private MeshRenderer mRenderer;
        private MeshCollider mCollider;
        private PhysicMaterial wheelPhysicMaterialOrgin;

        public GameObject WheelCollider;

        public override void OnSimulateStartAlways()
        {
            if (EnhancementEnabled)
            {
                Colliders = GetComponentsInChildren<Collider>();
                wheelPhysicMaterialOrgin = Colliders[0].material;

                PhysicMaterial wheelPhysicMaterial = SetPhysicMaterial(/*Friction*/FrictionSlider.Value, /*Bounciness*/BouncinessSlider.Value, PhysicMaterialCombine.Average);
                if (/*Collider*/CustomColliderToggle.IsActive)
                {
                    if (WheelCollider != null) return;

                    //禁用原有碰撞
                    foreach (Collider c in Colliders) { if (c.name == "CubeColliders" || c.name == "Collider") ((BoxCollider)c).isTrigger = true; }
           
                    WheelCollider = (GameObject)Instantiate(WheelColliderOrgin, transform.position, transform.rotation, transform);
                    WheelCollider.SetActive(true);
                    WheelCollider.name = "Wheel Collider";
                    WheelCollider.transform.SetParent(transform);
        
                    mCollider = WheelCollider.GetComponent<MeshCollider>();
                    mCollider.convex = true;
                    mCollider.material = wheelPhysicMaterial;
                    BB.myBounds.childColliders.Add(mCollider);
      
                    if (/*ShowCollider*/ShowColliderToggle.IsActive)
                    {
                        mRenderer = WheelCollider.AddComponent<MeshRenderer>();
                        mRenderer.material.color = Color.red;
                    }

                    PSaF pas = PSaF.GetPositionScaleAndFriction(ID);

                    WheelCollider.transform.parent = transform;
                    WheelCollider.transform.rotation = transform.rotation;
                    WheelCollider.transform.position = transform.TransformPoint(transform.InverseTransformPoint(transform.position) + pas.Position);
                    WheelCollider.transform.localScale = pas.Scale;

                }
                else
                {
                    //Destroy(WheelCollider);
                    //设置原有碰撞的参数
                    foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.GetComponent<BoxCollider>().material = wheelPhysicMaterial; }
                }

                if (!collisionToggle.IsActive)
                {
                    foreach (var col in BB.myBounds.childColliders)
                    {
                        col.isTrigger = true;
                    }
                }
            }
            //else
            //{
            //    //启用原有碰撞
            //    foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.isTrigger = false; }
            //    //设置原有碰撞的参数
            //    foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.GetComponent<BoxCollider>().material = wheelPhysicMaterialOrgin; }

            //    Destroy(WheelCollider);
            //}
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();
    
            var cog = BB.GetComponent<CogMotorControllerHinge>();
            var joint = BB.GetComponent<HingeJoint>();

            if (cog == null)
                return;
            else
            {
                if (!cog.AutoBreakToggle.IsActive)
                {
                    if (cog.Input == 0f)
                    {
                        joint.useMotor = false;
                    }
                    else
                    {
                        joint.useMotor = true;
                    }
                }
            }
           
        }

        public override void SimulateFixedUpdate_EnhancementEnabled()
        {
            base.SimulateFixedUpdate_EnhancementEnabled();
            BB.Rigidbody.WakeUp();
        }

        //void OnGUI()
        //{
        //    if (BB.isSimulating)
        //    {
        //        var cog = BB.GetComponent<CogMotorControllerHinge>();
        //        var joint = BB.GetComponent<HingeJoint>();
        //        GUILayout.Label(joint.useMotor.ToString());
        //        GUI.Label(new Rect(100, 100, 200, 30), string.Format("force {0} target V {1} ", cog.motor.force, cog.motor.targetVelocity.ToString("f3")));
        //        cog.motor.force = 0f;
        //        joint.useMotor = false;
        //        //cog.motor.targetVelocity = 100;
        //    }
        //}
        private static PhysicMaterial SetPhysicMaterial(float friction, float bounciness,PhysicMaterialCombine combine)
        {
            PhysicMaterial PM = new PhysicMaterial
            {
                //静摩擦力
                staticFriction = friction,
                //动摩擦力
                dynamicFriction = friction,
                //弹力
                bounciness = bounciness,
                //摩擦力组合
                frictionCombine = combine,
                //弹力组合
                bounceCombine = PhysicMaterialCombine.Average
            };

            return PM;
        }
        private static PhysicMaterial SetPhysicMaterial(PSaF pSaF)
        {
            PhysicMaterial PM = SetPhysicMaterial(pSaF.Friction, pSaF.Bounciness, PhysicMaterialCombine.Maximum);

            return PM;
        }

        //位置缩放和摩擦
        private struct PSaF
        {
            public Vector3 Position;
            public Vector3 Scale;
            public float Friction;
            public float Bounciness;

            public static PSaF one = new PSaF { Position = Vector3.one, Scale = Vector3.one, Friction = 1 };

            public static PSaF GetPositionScaleAndFriction(int id)
            {

                PSaF psaf = new PSaF();

                if (id == (int)BlockType.Wheel)
                {
                    psaf.Position = new Vector3(0, 0, 0.175f);
                    psaf.Scale = new Vector3(0.98f, 0.98f, 1.75f);
                    psaf.Friction = 0.6f;
                    psaf.Bounciness = 0;
                    return psaf;
                }
                if (id == (int)BlockType.LargeWheel)
                {
                    psaf.Position = new Vector3(0, 0, 0.45f);
                    psaf.Scale = new Vector3(1.38f, 1.38f, 3.75f);
                    psaf.Friction = 0.8f;
                    psaf.Bounciness = 0;
                    return psaf;
                }
                if (id == (int)BlockType.WheelUnpowered)
                {
                    psaf.Position = new Vector3(0, 0, 0.175f);
                    psaf.Scale = new Vector3(0.98f, 0.98f, 1.75f);
                    psaf.Friction = 1f;
                    psaf.Bounciness = 0;
                    return psaf;
                }
                if (id == (int)BlockType.LargeWheelUnpowered)
                {
                    psaf.Position = new Vector3(0, 0, 0.45f);
                    psaf.Scale = new Vector3(1.38f, 1.38f, 1.75f);
                    psaf.Friction = 1f;
                    psaf.Bounciness = 0;
                    return psaf;
                }

                return PSaF.one;

            }

        }

        static IEnumerator ReadWheelMesh()
        {
            
            WheelColliderOrgin = new GameObject("Wheel Collider Orgin");
            WheelColliderOrgin.transform.SetParent(EnhancementBlockController.Instance.transform);
            ModMesh modMesh = ModResource.CreateMeshResource("Wheel Mesh", "Resources" + @"/" + "Wheel.obj");
            Mesh wMesh = AssetManager.Instance.LoadFormObj2("Wheel Mesh", @"Resources/Wheel.obj");

            yield return new WaitUntil(() => modMesh.Available);

            MeshFilter mFilter = WheelColliderOrgin.AddComponent<MeshFilter>();
            mFilter.sharedMesh = wMesh;
            MeshCollider meshCollider = WheelColliderOrgin.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = wMesh;
            meshCollider.convex = true;
            WheelColliderOrgin.SetActive(false);
    
        }

    }

}



