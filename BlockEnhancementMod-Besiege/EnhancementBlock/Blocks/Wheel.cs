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
        int ID = 0;

        static GameObject WheelColliderOrgin;

        public override void SafeAwake()
        {
            ID = GetComponent<BlockVisualController>().ID;
            Friction = PSaF.GetPositionScaleAndFriction(ID).Friction;

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
                    foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.isTrigger = true; }

                    WheelCollider = (GameObject)Instantiate(WheelColliderOrgin, transform.position, transform.rotation, transform);
                    WheelCollider.SetActive(true);
                    WheelCollider.name = "Wheel Collider";
                    WheelCollider.transform.SetParent(transform);

                    mFilter = WheelCollider.AddComponent<MeshFilter>();
                    mFilter.sharedMesh = WheelCollider.GetComponent<MeshCollider>().sharedMesh;

                    mCollider = WheelCollider.GetComponent<MeshCollider>();
                    mCollider.convex = true;
                    mCollider.material = SetPhysicMaterial(Friction, Bounciness);

                    if (ShowCollider)
                    {
                        mRenderer = WheelCollider.AddComponent<MeshRenderer>();
                        mRenderer.material.color = Color.red;
                    }

                    PSaF pas = PSaF.GetPositionScaleAndFriction(ID);

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

            }
            else
            {
                //启用原有碰撞
                foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.enabled = true; }
                //设置原有碰撞的参数
                foreach (Collider c in Colliders) { if (c.name == "CubeColliders") c.GetComponent<BoxCollider>().material = SetPhysicMaterial(PSaF.GetPositionScaleAndFriction(ID).Friction, 0f); }

                Destroy(WheelCollider);


            }

        }

        private static PhysicMaterial SetPhysicMaterial(float friction, float bounciness)
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
                frictionCombine = PhysicMaterialCombine.Minimum,
                //弹力组合
                bounceCombine = PhysicMaterialCombine.Minimum
            };

            return PM;
        }

        //位置缩放和摩擦
        private struct PSaF
        {
            public Vector3 Position;
            public Vector3 Scale;
            public float Friction;

            public static PSaF one = new PSaF { Position = Vector3.one, Scale = Vector3.one, Friction = 1 };

            public static PSaF GetPositionScaleAndFriction(int id)
            {

                PSaF psaf = new PSaF();

                if (id == (int)BlockType.Wheel)
                {
                    psaf.Position = new Vector3(0, 0, 0.175f);
                    psaf.Scale = new Vector3(0.98f, 0.98f, 1.75f);
                    psaf.Friction = 0.6f;
                    return psaf;
                }
                if (id == (int)BlockType.LargeWheel)
                {
                    psaf.Position = new Vector3(0, 0, 0.45f);
                    psaf.Scale = new Vector3(1.38f, 1.38f, 3.75f);
                    psaf.Friction = 0.8f;
                    return psaf;
                }
                if (id == (int)BlockType.WheelUnpowered)
                {
                    psaf.Position = new Vector3(0, 0, 0.175f);
                    psaf.Scale = new Vector3(0.98f, 0.98f, 1.75f);
                    psaf.Friction = 1f;
                    return psaf;
                }
                if (id == (int)BlockType.LargeWheelUnpowered)
                {
                    psaf.Position = new Vector3(0, 0, 0.45f);
                    psaf.Scale = new Vector3(1.38f, 1.38f, 1.75f);
                    psaf.Friction = 1f;
                    return psaf;
                }

                return PSaF.one;

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



