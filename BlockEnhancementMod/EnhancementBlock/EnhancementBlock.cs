using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    public  class EnhancementBlock : MonoBehaviour
    {
        public static bool EnhanceMore { get; internal set; } =BlockEnhancementMod.Configuration.EnhanceMore;

        /// <summary>模块行为</summary>
        public BlockBehaviour BB { get; internal set; } 

        /// <summary>进阶属性按钮</summary>
        public MToggle Enhancement;

        /// <summary>进阶属性激活</summary>
        public bool EnhancementEnabled { get; set; } = false;

        [Obsolete]
        /// <summary>模块数据加载事件 传入参数类型:XDataHolder</summary>
        public Action<XDataHolder> BlockDataLoadEvent;
        [Obsolete]
        /// <summary>模块数据储存事件 传入参数类型:XDataHolder</summary>
        public Action<XDataHolder> BlockDataSaveEvent;

        public Action BlockPropertiseChangedEvent;

        private bool isFirstFrame = true;

        private void Awake()
        {

            BB = GetComponent<BlockBehaviour>();           
            
            SafeAwake();

            //if (BB.isSimulating ) { return; }        

            Enhancement = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.Enhancement, "Enhancement", EnhancementEnabled);
            Enhancement.Toggled += (bool value) => { EnhancementEnabled = value; DisplayInMapper(value); };

            //LoadConfiguration();    

            ChangedProperties(); try { BlockPropertiseChangedEvent(); } catch { }

            DisplayInMapper(EnhancementEnabled);

            //Controller.Instance.OnSave += SaveConfiguration;
        }

        void Update()
        {
            if (BB.isSimulating)
            {
                if (isFirstFrame)
                {
                    isFirstFrame = false;
                    if (EnhancementEnabled) { OnSimulateStart_EnhancementEnabled(); }
                    
                    if (!StatMaster.isClient) { OnSimulateStartClient(); }
                    OnSimulateStartAlways();
                }
                SimulateUpdateAlways();

                if (!EnhancementEnabled) return;

                if (StatMaster.isHosting) { SimulateUpdateHost_EnhancementEnabled(); }
                if (StatMaster.isClient) { SimulateUpdateClient_EnhancementEnabled(); }
                SimulateUpdateAlways_EnhancementEnable();
            }
            else
            {
                if (EnhancementEnabled) { BuildingUpdateAlways_EnhancementEnabled(); }
                isFirstFrame = true;
            }
        }

        private void FixedUpdate()
        {
            if (!EnhancementEnabled) return;

            if (BB.isSimulating && !isFirstFrame) { SimulateFixedUpdate_EnhancementEnabled(); }
        }

        private void LateUpdate()
        {
            if (!EnhancementEnabled) return;

            if (BB.isSimulating && !isFirstFrame) { SimulateLateUpdate_EnhancementEnabled(); }
        }

        [Obsolete]
        private void SaveConfiguration(PlayerMachineInfo pmi)
        {
#if DEBUG
            ConsoleController.ShowMessage("On save en");
#endif

            if (pmi == null)
            {
                return;
            }

            foreach (var blockinfo in pmi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.Data;

                    try { BlockDataSaveEvent(bd); } catch { }

                    SaveConfiguration(bd);

                    //bool flag = (!StatMaster.SavingXML ? false : OptionsMaster.BesiegeConfig.ExcludeDefaultSaveData);

                    //foreach (MapperType item in myMapperTypes)
                    //{
                    //    if (!flag)
                    //    {
                    //        bd.Write(item.Serialize());
                    //    }
                    //}

                    break;
                }
            }


        }

        [Obsolete]
        private void LoadConfiguration()
        {
#if DEBUG
            ConsoleController.ShowMessage("On load en");
#endif
            if (EnhancementBlockController.Instance.PMI == null)
            {
                return;
            }

            foreach (var blockinfo in EnhancementBlockController.Instance.PMI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.Data;

                    try { BlockDataLoadEvent(bd); } catch { };

                    LoadConfiguration(bd);

                    //foreach (MapperType item in myMapperTypes)
                    //{
                    //    string str = string.Concat(MapperType.XDATA_PREFIX + item.Key);
                    //    XData xDatum = bd.Read(str);
                    //    if (xDatum != null || !StatMaster.isPaste)
                    //    {
                    //        item.DeSerialize((xDatum ?? item.defaultData));
                    //    }
                    //}

                    break;
                }
            }
        }
        [Obsolete]
        /// <summary>
        /// 储存配置
        /// </summary>
        /// <param name="mi">当前存档信息</param>
        public virtual void SaveConfiguration(XDataHolder BlockData) { }
        [Obsolete]
        /// <summary>
        /// 加载配置
        /// </summary>
        public virtual void LoadConfiguration(XDataHolder BlockData) { }
        /// <summary>
        /// 安全唤醒 模块只需要关心自己要添加什么控件就行了
        /// </summary>
        public virtual void SafeAwake() { }
        /// <summary>
        /// 在模拟开始的第一帧 要做的事
        /// </summary>
        public virtual void OnSimulateStartAlways() { }
        public virtual void OnSimulateStart_EnhancementEnabled() { }
        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        public virtual void SimulateUpdateHost_EnhancementEnabled() { }
        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        public virtual void SimulateUpdateClient_EnhancementEnabled() { }
        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        public virtual void SimulateUpdateAlways_EnhancementEnable() { }
        public virtual void SimulateUpdateAlways() { }
        /// <summary>
        /// 在模拟模式下的FixedUpdate
        /// </summary>
        public virtual void SimulateFixedUpdate_EnhancementEnabled() { }
        /// <summary>
        /// 在模拟模式下的LateUpdate
        /// </summary>
        public virtual void SimulateLateUpdate_EnhancementEnabled() { }
        /// <summary>
        /// 建造模式下的Update
        /// </summary>
        public virtual void BuildingUpdateAlways_EnhancementEnabled() { }


        /// <summary>
        /// 显示在Mapper里面
        /// </summary>
        public virtual void DisplayInMapper(bool value) { }
        /// <summary>
        /// 属性改变（滑条值改变脚本属性随之改变）
        /// </summary>
        public virtual void ChangedProperties() { }
        /// <summary>
        /// 参数改变（联机模拟时主机对模块的一些参数初始化）
        /// </summary>
        public virtual void OnSimulateStartClient() { }


        ///// <summary>
        ///// 设置金属硬度
        ///// </summary>
        ///// <param name="Hardness">硬度</param>
        ///// <param name="CJ">关节</param>
        //internal static void SwitchMatalHardness(int Hardness, ConfigurableJoint CJ)
        //{
        //    switch (Hardness)
        //    {
        //        case 1:
        //            CJ.projectionMode = JointProjectionMode.PositionAndRotation;
        //            CJ.projectionAngle = 0.5f; break;
        //        case 2:
        //            CJ.projectionMode = JointProjectionMode.PositionAndRotation;
        //            CJ.projectionAngle = 0; break;
        //        default:
        //            CJ.projectionMode = JointProjectionMode.None; break;

        //    }
        //}
        ///// <summary>
        ///// 设置木头硬度
        ///// </summary>
        ///// <param name="Hardness">硬度</param>
        ///// <param name="CJ">关节</param>
        //internal static void SwitchWoodHardness(int Hardness, ConfigurableJoint CJ)
        //{
        //    switch (Hardness)
        //    {
        //        case 0:
        //            CJ.projectionMode = JointProjectionMode.PositionAndRotation;
        //            CJ.projectionAngle = 10f;
        //            CJ.projectionDistance = 5; break;
        //        case 2:
        //            CJ.projectionMode = JointProjectionMode.PositionAndRotation;
        //            CJ.projectionAngle = 5f;
        //            CJ.projectionDistance = 2.5f; break;
        //        case 3:
        //            CJ.projectionMode = JointProjectionMode.PositionAndRotation;
        //            CJ.projectionAngle = 0f;
        //            CJ.projectionDistance = 0; break;
        //        default:
        //            CJ.projectionMode = JointProjectionMode.None;
        //            CJ.projectionDistance = 0;
        //            CJ.projectionAngle = 0; break;

        //    }

        //}

        public struct Hardness
        {
            public JointProjectionMode projectionMode;
            public float projectionAngle;
            public float projectionDistance;

            //Material material;

            public Hardness(JointProjectionMode mode, float angle, float distance/*, Material material = Material.None*/)
            {
                projectionMode = mode;
                projectionAngle = angle;
                projectionDistance = distance;

                //this.material = material;
            }
            public Hardness(ConfigurableJoint joint/*, Material material = Material.None*/)
            {
                projectionMode = joint.projectionMode;
                projectionAngle = joint.projectionAngle;
                projectionDistance = joint.projectionDistance;

                //this.material = material;
            }

            public ConfigurableJoint toConfigurableJoint(ConfigurableJoint joint)
            {
                joint.projectionMode = projectionMode;
                joint.projectionAngle = projectionAngle;
                joint.projectionDistance = projectionDistance;

                return joint;
            }

            enum Material
            {
                LowCarbonSteel, MedianSoftWood = 0,
                MidCarbonSteel = 1,
                HighCarbonSteel = 2,
                SoftWood = 3,
                HardWood = 4,
                VeryHardWood = 5,
                None = 6,
            }

            public static Hardness SwitchHardness(int index)
            {
                var hardness = new Hardness();

                switch (index)
                {
                    case 1:
                        hardness.projectionMode = JointProjectionMode.PositionAndRotation;
                        hardness.projectionAngle = 0.5f; break;
                    case 2:
                        hardness.projectionMode = JointProjectionMode.PositionAndRotation;
                        hardness.projectionAngle = 0; break;
                    case 3:
                        hardness.projectionMode = JointProjectionMode.PositionAndRotation;
                        hardness.projectionAngle = 10f;
                        hardness.projectionDistance = 5; break;
                    case 4:
                        hardness.projectionMode = JointProjectionMode.PositionAndRotation;
                        hardness.projectionAngle = 5f;
                        hardness.projectionDistance = 2.5f; break;
                    case 5:
                        hardness.projectionMode = JointProjectionMode.PositionAndRotation;
                        hardness.projectionAngle = 0f;
                        hardness.projectionDistance = 0; break;
                    default:
                        hardness.projectionMode = JointProjectionMode.None; break;
                }
                return hardness;
            }
            public static Hardness GetOrginHardness(ConfigurableJoint joint)
            {
                return new Hardness(joint.projectionMode, joint.projectionAngle, joint.projectionDistance);
            }
            public static ConfigurableJoint SwitchWoodHardness(int index, ConfigurableJoint joint)
            {
                switch (index)
                {
                    case 0:
                        return SwitchHardness(index + 3).toConfigurableJoint(joint);
                    case 2:
                        return SwitchHardness(index + 2).toConfigurableJoint(joint);
                    case 3:
                        return SwitchHardness(index + 2).toConfigurableJoint(joint);
                    default:
                        return GetOrginHardness(joint).toConfigurableJoint(joint);
                }
            }
            public static ConfigurableJoint SwitchMetalHardness(int index, ConfigurableJoint joint)
            {
                switch (index)
                {
                    case 1:
                        return SwitchHardness(index).toConfigurableJoint(joint);
                    case 2:
                        return SwitchHardness(index).toConfigurableJoint(joint);
                    default:
                        return GetOrginHardness(joint).toConfigurableJoint(joint);
                }
            }
        }


    }

   
}

