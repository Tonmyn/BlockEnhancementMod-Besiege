using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class WheelScript : EnhancementBlock
    {
        //WheelScript WS;

        MKey BrakeKey;

        MSlider BrakeForceSlider;

        MToggle ColliderToggle;

        MToggle FrictionToggle;

        MSlider FrictionSlider;

        MSlider LerpSlider;

        float BrakeForce = 1; 

        bool Collider;

        bool FrictionT = false;

        float Friction = 0.8f;

        float Lerp;

        protected override void SafeStart()
        {

                BrakeKey = new MKey("刹车", "Brake", KeyCode.None);
                BrakeKey.KeysChanged += ChangedPropertise;
                CurrentMapperTypes.Add(BrakeKey);

                BrakeForceSlider = new MSlider("刹车力度", "BrakeForce", BrakeForce, 0, 5, false);
                BrakeForceSlider.ValueChanged += (float value) => { BrakeForce = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(BrakeForceSlider);

                ColliderToggle = new MToggle("自定碰撞", "Collider", Collider);
                ColliderToggle.Toggled += (bool value) => { Collider = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(ColliderToggle);

                FrictionToggle = new MToggle("自定摩擦", "FrictionT", FrictionT);
                FrictionToggle.Toggled += (bool value) => { FrictionT = FrictionSlider.DisplayInMapper = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(FrictionToggle);

                FrictionSlider = new MSlider("摩擦力", "Friction", Friction, 0f, 1f, false);
                FrictionSlider.ValueChanged += (float value) => { Friction = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(FrictionSlider);

                if (BB.BlockID == (int)BlockType.Wheel || BB.BlockID == (int)BlockType.LargeWheel)
                {
                    if (BB.BlockID == (int)BlockType.Wheel)
                    { Lerp = 16; }
                    else
                    { Lerp = 8; }
                    LerpSlider = new MSlider("插值", "Lerp", Lerp, 0f, 30f, false);
                    LerpSlider.ValueChanged += (float value) => { Lerp = value; ChangedPropertise(); };
                    CurrentMapperTypes.Add(LerpSlider);

                }

#if DEBUG
            BesiegeConsoleController.ShowMessage("动力组件添加进阶属性");
#endif

        }

        //public override void ChangedPropertise()
        //{

        //    WS.Brake = Tools.Get_List_keycode(BrakeKey);
        //    WS.BrakeForce = BrakeForce;
        //    WS.Collider = Collider;
        //    WS.FrictionT = FrictionT;
        //    WS.Friction = Friction;
        //    WS.Lerp = Lerp;
            
        //}

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            BrakeKey.DisplayInMapper = value;
            BrakeForceSlider.DisplayInMapper = value;
            ColliderToggle.DisplayInMapper = value;
            FrictionToggle.DisplayInMapper = value;
            FrictionSlider.DisplayInMapper = value && FrictionToggle.IsActive;
            if (LerpSlider!=null)
            {
                LerpSlider.DisplayInMapper = value;
            }
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;                  

                    if (bd.HasKey("bmt-" + BrakeKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + BrakeKey.Key))
                        {           
                            BrakeKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
                    }

                    if (bd.HasKey("bmt-" + BrakeForceSlider.Key)) { BrakeForceSlider.Value = BrakeForce = bd.ReadFloat("bmt-" + BrakeForceSlider.Key); }

                    if (bd.HasKey("bmt-" + ColliderToggle.Key)) { ColliderToggle.IsActive = Collider = bd.ReadBool("bmt-" + ColliderToggle.Key); }

                    if (bd.HasKey("bmt-" + FrictionToggle.Key)) { FrictionToggle.IsActive = FrictionT = bd.ReadBool("bmt-" + FrictionToggle.Key); }

                    if (bd.HasKey("bmt-" + FrictionSlider.Key)) { FrictionSlider.Value = Friction = bd.ReadFloat("bmt-" + FrictionSlider.Key); }

                    if (LerpSlider != null)
                    {
                        if (bd.HasKey("bmt-" + LerpSlider.Key)) { LerpSlider.Value = Lerp = bd.ReadFloat("bmt-" + LerpSlider.Key); }
                    }

                    break;
                }

            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + BrakeKey.Key, Tools.Get_List_keycode(BrakeKey));

                    blockinfo.BlockData.Write("bmt-" + BrakeForceSlider.Key, BrakeForceSlider.Value);

                    blockinfo.BlockData.Write("bmt-" + ColliderToggle.Key, ColliderToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + FrictionToggle.Key, FrictionToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + FrictionSlider.Key, FrictionSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + LerpSlider.Key, LerpSlider.Value);

                    break;
                }

            }
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

        //MKey BrakeKey;

        HingeJoint HJ;

        Collider[] Colliders;

        CogMotorControllerHinge CMCH;


        //public List<KeyCode> Brake;

        //public float BrakeForce;

        //public bool Collider;

        //public bool FrictionT;

        //public float Friction;

        //public float Lerp;

        public Mesh WheelMesh;



        private MeshFilter mFilter;

        private MeshRenderer mRenderer;

        private MeshCollider mCollider;

        private PhysicMaterial PM;

        public GameObject WheelCollider;

        private float angleDrag;

        protected override void OnSimulateStart()
        {

            //BrakeKey = GetKey(Brake);
            HJ = GetComponent<HingeJoint>();
            Colliders = GetComponentsInChildren<Collider>();
            MyId = GetComponent<BlockVisualController>().ID;

            angleDrag = GetComponent<Rigidbody>().angularDrag;

            JointLimits jl = HJ.limits;
            jl.min = jl.max = jl.bounciness = jl.bounceMinVelocity = 0;
            HJ.limits = jl;


            HJ.spring = new JointSpring() { damper = 10000, spring = 10000, targetPosition = 0 };

            if (Collider)
            {

                //禁用原有碰撞
                foreach (Collider c in Colliders)
                {
                    if (c.name == "CubeColliders")
                    {
                        c.enabled = false;
                    }
                }

                WheelCollider = new GameObject("Wheel Collider");

                mFilter = WheelCollider.AddComponent<MeshFilter>();
                mFilter.mesh = WheelMesh = Block.MeshFromObj(Application.dataPath + "/Mods/Resources/BlockEnhancement/Wheel/Wheel.obj");

                mCollider = WheelCollider.AddComponent<MeshCollider>();
                mCollider.convex = true;

                PM = mCollider.material;

                //静摩擦力
                PM.staticFriction = Friction;
                //动摩擦力
                PM.dynamicFriction = Friction;
                //摩擦力组合
                PM.frictionCombine = PhysicMaterialCombine.Multiply;
                //弹力
                PM.bounciness = 0;
                //弹力组合
                PM.bounceCombine = PhysicMaterialCombine.Minimum;


#if DEBUG
                mRenderer = WheelCollider.AddComponent<MeshRenderer>();
                mRenderer.material.color = Color.red;
#endif

                PaS pas = GetPositionAndScale(MyId);

                WheelCollider.transform.parent = mCollider.transform.parent = transform;
                WheelCollider.transform.rotation = mCollider.transform.rotation = transform.rotation;
                WheelCollider.transform.position = mCollider.transform.position = transform.TransformPoint(transform.InverseTransformPoint(transform.position) + pas.Position);
                WheelCollider.transform.localScale = mCollider.transform.localScale = pas.Scale;
                WheelCollider.AddComponent<DestroyIfEditMode>();

            }
            else if (FrictionT)
            {
                //设置原有碰撞的参数
                foreach (Collider c in Colliders)
                {
                    if (c.name == "CubeColliders")
                    {

                        PhysicMaterial PM = c.GetComponent<BoxCollider>().material;

                        //静摩擦力
                        PM.staticFriction = Friction;
                        //动摩擦力
                        PM.dynamicFriction = Friction;
                        Debug.Log(PM.bounciness);
                        //摩擦力组合
                        PM.frictionCombine = PhysicMaterialCombine.Multiply;
                        //弹力
                        PM.bounciness = 0;
                        //弹力组合
                        PM.bounceCombine = PhysicMaterialCombine.Minimum;
                    }
                }

            }

            if (MyId == (int)BlockType.Wheel || MyId == (int)BlockType.LargeWheel)
            {
                CMCH = GetComponent<CogMotorControllerHinge>();
                CMCH.speedLerpSmooth = Lerp;
            }


        }

        protected override void OnSimulateFixedUpdate()
        {
            if (StatMaster.levelSimulating)
            {

                if (HJ)
                {

                    if (BrakeKey.IsDown)
                    {
                        HJ.motor = new JointMotor() { force = Mathf.MoveTowards(0f, 5000f, BrakeForce * 1000f), targetVelocity = 0 };
                    }
                    if (BrakeKey.IsPressed)
                    {
                        GetComponent<Rigidbody>().angularDrag = 100f * BrakeForce;
                        HJ.useSpring = true;
                    }
                    if (BrakeKey.IsReleased)
                    {
                        GetComponent<Rigidbody>().angularDrag = angleDrag;
                        HJ.useSpring = false;
                    }

                }

            }
            else if (Collider)
            {
                //启用原有碰撞
                foreach (Collider c in Colliders)
                {
                    if (c.name == "CubeColliders")
                    {
                        c.enabled = enabled;
                    }
                }

            }
        }

        private struct PaS
        {
            public Vector3 Position;
            public Vector3 Scale;

            public static PaS one = new PaS { Position = Vector3.one, Scale = Vector3.one };

        }

        private PaS GetPositionAndScale(int id)
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



