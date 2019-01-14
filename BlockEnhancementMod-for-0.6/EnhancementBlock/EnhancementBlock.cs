using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    public  class EnhancementBlock : MonoBehaviour
    {
        public static bool EnhanceMore { get; internal set; } = false;

        /// <summary>模块行为</summary>
        public BlockBehaviour BB { get; internal set; } 

        /// <summary>进阶属性按钮</summary>
        public MToggle Enhancement;

        /// <summary>进阶属性激活</summary>
        public bool EnhancementEnabled { get; set; } = false;

        internal static List<string> MetalHardness = new List<string>() { LanguageManager.lowCarbonSteel, LanguageManager.midCarbonSteel, LanguageManager.highCarbonSteel };

        internal static List<string> WoodHardness = new List<string>() { LanguageManager.softWood, LanguageManager.midSoftWood, LanguageManager.hardWood, LanguageManager.veryHardWood };

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

            Enhancement = BB.AddToggle(LanguageManager.enhancement, "Enhancement", EnhancementEnabled);
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
                    if (EnhancementEnabled) { OnSimulateStart(); }
                    
                    if (!StatMaster.isClient) { ChangeParameter(); }
                }
                SimulateUpdateAlways();

                if (!EnhancementEnabled) return;

                if (StatMaster.isHosting) { SimulateUpdateHost(); }
                if (StatMaster.isClient) { SimulateUpdateClient(); }
                SimulateUpdateEnhancementEnableAlways();
            }
            else
            {
                if (EnhancementEnabled) { BuildingUpdate(); }
                isFirstFrame = true;
            }
        }

        private void FixedUpdate()
        {
            if (!EnhancementEnabled) return;

            if (BB.isSimulating && !isFirstFrame) { SimulateFixedUpdateAlways(); }
        }

        private void LateUpdate()
        {
            if (!EnhancementEnabled) return;

            if (BB.isSimulating && !isFirstFrame) { SimulateLateUpdateAlways(); }
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
        public virtual void OnSimulateStart() { }
        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        public virtual void SimulateUpdateHost() { }
        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        public virtual void SimulateUpdateClient() { }
        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        public virtual void SimulateUpdateEnhancementEnableAlways() { }
        public virtual void SimulateUpdateAlways() { }
        /// <summary>
        /// 在模拟模式下的FixedUpdate
        /// </summary>
        public virtual void SimulateFixedUpdateAlways() { }
        /// <summary>
        /// 在模拟模式下的LateUpdate
        /// </summary>
        public virtual void SimulateLateUpdateAlways() { }
        /// <summary>
        /// 建造模式下的Update
        /// </summary>
        public virtual void BuildingUpdate() { }


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
        public virtual void ChangeParameter() { }


        /// <summary>
        /// 设置金属硬度
        /// </summary>
        /// <param name="Hardness">硬度</param>
        /// <param name="CJ">关节</param>
        internal static void SwitchMatalHardness(int Hardness, ConfigurableJoint CJ)
        {
            switch (Hardness)
            {
                case 1:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 0.5f; break;
                case 2:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 0; break;
                default:
                    CJ.projectionMode = JointProjectionMode.None; break;

            }
        }
        /// <summary>
        /// 设置木头硬度
        /// </summary>
        /// <param name="Hardness">硬度</param>
        /// <param name="CJ">关节</param>
        internal static void SwitchWoodHardness(int Hardness, ConfigurableJoint CJ)
        {
            switch (Hardness)
            {
                case 0:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 10f;
                    CJ.projectionDistance = 5; break;
                case 2:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 5f;
                    CJ.projectionDistance = 2.5f; break;
                case 3:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 0f;
                    CJ.projectionDistance = 0; break;
                default:
                    CJ.projectionMode = JointProjectionMode.None;
                    CJ.projectionDistance = 0;
                    CJ.projectionAngle = 0; break;

            }

        }
      
    }

}

