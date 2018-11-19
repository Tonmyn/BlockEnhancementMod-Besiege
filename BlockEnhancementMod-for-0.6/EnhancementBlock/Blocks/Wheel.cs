using System;
using System.Collections.Generic;
using UnityEngine;
//using BlockEnhancementMod.Tools;

namespace BlockEnhancementMod.Blocks
{

    class WheelScript : EnhancementBlock
    {

        MToggle ColliderToggle;
        MSlider FrictionSlider;
        MSlider BouncinessSlider;

        bool Collider = false;
        float Friction = 0.8f;
        float Bounciness = 0f;

        public override void SafeAwake()
        {

            FrictionSlider = BB.AddSlider(LanguageManager.friction, "Friction", Friction, 0.1f, 3f);
            FrictionSlider.ValueChanged += (float value) => { Friction = value; ChangedProperties(); };

            BouncinessSlider = BB.AddSlider(LanguageManager.bounciness, "Bounciness", Bounciness, 0f, 1f);
            BouncinessSlider.ValueChanged += (float value) => { Bounciness = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("动力组件添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            //ColliderToggle.DisplayInMapper = value;
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

        public Mesh WheelMesh;

        private MeshFilter mFilter;

        private MeshRenderer mRenderer;

        private MeshCollider mCollider;

        public GameObject WheelCollider;




        public override void ChangeParameter()
        {

            Colliders = GetComponentsInChildren<Collider>();
            MyId = GetComponent<BlockVisualController>().ID;

            if (EnhancementEnabled)
            {
                if (Collider)
                {
                    //禁用原有碰撞
                    foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.enabled = false; }

                    WheelCollider = new GameObject("Wheel Collider");

                    mFilter = WheelCollider.AddComponent<MeshFilter>();
                    //mFilter.mesh = WheelMesh = SimpleMesh.MeshFromObj(Application.dataPath + "/Mods/Resources/BlockEnhancement/Wheel.obj");

                    mCollider = WheelCollider.AddComponent<MeshCollider>();
                    mCollider.convex = true;

                    mCollider.material = SetPhysicMaterial(Friction, Bounciness);

#if DEBUG
                    mRenderer = WheelCollider.AddComponent<MeshRenderer>();
                    mRenderer.material.color = Color.red;
#endif

                    PaS pas = PaS.GetPositionAndScale(MyId);

                    WheelCollider.transform.parent = mCollider.transform.parent = transform;
                    WheelCollider.transform.rotation = mCollider.transform.rotation = transform.rotation;
                    WheelCollider.transform.position = mCollider.transform.position = transform.TransformPoint(transform.InverseTransformPoint(transform.position) + pas.Position);
                    WheelCollider.transform.localScale = mCollider.transform.localScale = pas.Scale;
                    WheelCollider.AddComponent<DestroyIfEditMode>();

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
#if DEBUG
            Debug.Log("enable  " + EnhancementEnabled);
#endif
        }

        private PhysicMaterial SetPhysicMaterial(float friction, float bounciness)
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
                    pas.Position = new Vector3(0, 0, 0.165f);
                    pas.Scale = Vector3.one;
                    return pas;
                }
                if (id == (int)BlockType.LargeWheel)
                {
                    pas.Position = new Vector3(0, 0, 0.165f);
                    pas.Scale = Vector3.one;
                    return pas;
                }
                if (id == (int)BlockType.WheelUnpowered)
                {
                    pas.Position = new Vector3(0, 0, 0.165f);
                    pas.Scale = Vector3.one;
                    return pas;
                }
                if (id == (int)BlockType.LargeWheelUnpowered)
                {
                    pas.Position = new Vector3(0, 0, 0.165f);
                    pas.Scale = Vector3.one;
                    return pas;
                }

                return PaS.one;

            }

        }

    }
}



