using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using System.Collections;

namespace BlockEnhancementMod.Blocks
{

    class WheelScript : EnhancementBlock
    {

        MToggle ColliderToggle;
        MToggle ShowColliderToggle;
        MSlider FrictionSlider;
        MSlider BouncinessSlider;

        bool Collider = false;
        bool ShowCollider = true;
        float Friction = 0.8f;
        float Bounciness = 0f;

        static GameObject WheelColliderOrgin;

        public override void SafeAwake()
        {
            ColliderToggle = BB.AddToggle(LanguageManager.customCollider, "Custom Collider", Collider);
            ColliderToggle.Toggled += (value) => { Collider = ShowColliderToggle.DisplayInMapper = value; ChangedProperties(); };

            ShowColliderToggle = BB.AddToggle(LanguageManager.showCollider, "Show Collider", ShowCollider);
            ShowColliderToggle.Toggled += (value) => { ShowCollider = value; ChangedProperties(); };

            FrictionSlider = BB.AddSlider(LanguageManager.friction, "Friction", Friction, 0.1f, 3f);
            FrictionSlider.ValueChanged += (float value) => { Friction = value; ChangedProperties(); };

            BouncinessSlider = BB.AddSlider(LanguageManager.bounciness, "Bounciness", Bounciness, 0f, 1f);
            BouncinessSlider.ValueChanged += (float value) => { Bounciness = value; ChangedProperties(); };

            if (WheelColliderOrgin == null)
            {
                StartCoroutine(ReadWheelMesh());
            }

#if DEBUG
            ConsoleController.ShowMessage("动力组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            ColliderToggle.DisplayInMapper = value;
            ShowColliderToggle.DisplayInMapper = value && Collider;
            FrictionSlider.DisplayInMapper = value;
            BouncinessSlider.DisplayInMapper = value;
        }


        /// <summary>
        /// 是否是轮子零件
        /// </summary>
        /// <param name="id"></param>
        /// <returns>零件号</returns>
        public static bool IsWheel(int id)
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

        int MyId;

        Collider[] Colliders;

   

        private MeshFilter mFilter;

        private MeshRenderer mRenderer;

        private MeshCollider mCollider;

        public GameObject WheelCollider;

        public override void ChangeParameter()
        {

            Colliders = GetComponentsInChildren<Collider>();          

            if (EnhancementEnabled)
            {

                if (Collider)
                {
                    //if (StatMaster.isMP && StatMaster.isClient) return;
                    if (WheelCollider != null) return;

                    //禁用原有碰撞
                    foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.enabled = false; }

                    WheelCollider = (GameObject)Instantiate(WheelColliderOrgin, transform.position, transform.rotation, transform);
                    WheelCollider.SetActive(true);
                    WheelCollider.name = "Wheel Collider";
                    WheelCollider.transform.SetParent(transform);

                    mFilter = WheelCollider.AddComponent<MeshFilter>();
                    //mFilter.mesh = WheelMesh = SimpleMesh.MeshFromObj(Application.dataPath + "/Mods/Resources/BlockEnhancement/Wheel.obj");
                    mFilter.sharedMesh = WheelCollider.GetComponent<MeshCollider>().sharedMesh;
                    //MeshFilter meshFilter = WheelColliderOrgin.AddComponent<MeshFilter>();
                    //meshFilter.mesh = modMesh = ModResource.GetMesh("Wheel Mesh");

                    //MeshRenderer meshRenderer = WheelColliderOrgin.AddComponent<MeshRenderer>();
                    //meshRenderer.material.color = Color.red;

                    mCollider = WheelCollider.GetComponent<MeshCollider>();
                    //mCollider.convex = true;

                    mCollider.material = SetPhysicMaterial(Friction, Bounciness);

                    if (ShowCollider)
                    {
                        mRenderer = WheelCollider.AddComponent<MeshRenderer>();
                        mRenderer.material.color = Color.red;
                    }
 

                    MyId = GetComponent<BlockVisualController>().ID;
                    PaS pas = PaS.GetPositionAndScale(MyId);

                    WheelCollider.transform.parent /*= mCollider.transform.parent*/ = transform;
                    WheelCollider.transform.rotation /*= mCollider.transform.rotation*/ = transform.rotation;
                    WheelCollider.transform.position /*= mCollider.transform.position*/ = transform.TransformPoint(transform.InverseTransformPoint(transform.position) + pas.Position);
                    WheelCollider.transform.localScale /*= mCollider.transform.localScale*/ = pas.Scale;
                    //WheelCollider.AddComponent<DestroyIfEditMode>();

                }
                else
                {
                    Destroy(WheelCollider);
                }

                //设置原有碰撞的参数
                foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.GetComponent<BoxCollider>().material = SetPhysicMaterial(Friction, Bounciness); }

                //设置地形的摩擦力合并方式为平均
                foreach (var v in GameObject.Find("Terrain Terraced").GetComponentsInChildren<MeshCollider>())
                {
                    v.sharedMaterial.frictionCombine = PhysicMaterialCombine.Average;
                    v.sharedMaterial.bounceCombine = PhysicMaterialCombine.Average;
                    break;
                }

            }
            else
            {
                //启用原有碰撞
                foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.enabled = true; }
                //设置原有碰撞的参数
                foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.GetComponent<BoxCollider>().material = SetPhysicMaterial(0.8f, 0f); }

                Destroy(WheelCollider);


            }

        }

        private static PhysicMaterial SetPhysicMaterial(float friction, float bounciness)
        {
            PhysicMaterial PM = new PhysicMaterial();

            //静摩擦力
            PM.staticFriction = friction;
            //动摩擦力
            PM.dynamicFriction = friction;
            //弹力
            PM.bounciness = bounciness;
            //摩擦力组合
            PM.frictionCombine = PhysicMaterialCombine.Minimum;
            //弹力组合
            PM.bounceCombine = PhysicMaterialCombine.Minimum;

            return PM;
        }

        private struct PaS
        {
            public Vector3 Position;
            public Vector3 Scale;

            public static PaS one = new PaS { Position = Vector3.one, Scale = Vector3.one };

            public static PaS GetPositionAndScale(int id)
            {

                PaS pas = new PaS();

                if (id == (int)BlockType.Wheel)
                {
                    pas.Position = new Vector3(0, 0, 0.175f);
                    pas.Scale = new Vector3(0.98f, 0.98f, 1.75f);
                    return pas;
                }
                if (id == (int)BlockType.LargeWheel)
                {
                    pas.Position = new Vector3(0, 0, 0.45f);
                    pas.Scale = new Vector3(1.38f, 1.38f, 3.75f);
                    return pas;
                }
                if (id == (int)BlockType.WheelUnpowered)
                {
                    pas.Position = new Vector3(0, 0, 0.175f);
                    pas.Scale = new Vector3(0.98f,0.98f,1.75f);
                    return pas;
                }
                if (id == (int)BlockType.LargeWheelUnpowered)
                {
                    pas.Position = new Vector3(0, 0, 0.45f);
                    pas.Scale = new Vector3(1.38f, 1.38f, 1.75f);
                    return pas;
                }

                return PaS.one;

            }

        }

        static IEnumerator ReadWheelMesh()
        {
            WheelColliderOrgin = new GameObject("Wheel Collider Orgin");
            WheelColliderOrgin.transform.SetParent(Controller.Instance.transform);
            ModMesh modMesh = ModResource.CreateMeshResource("Wheel Mesh", "Resources" + @"/" + "Wheel.obj");

            yield return new WaitUntil(() => modMesh.Available);
            
            MeshCollider meshCollider = WheelColliderOrgin.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = ModResource.GetMesh("Wheel Mesh");
            meshCollider.convex = true;
            WheelColliderOrgin.SetActive(false);


        }

    }
}



