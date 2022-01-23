using Modding.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    public  class EnhancementBlock : MonoBehaviour ,ILimitsDisplay
    {
        public static bool EnhanceMore { get { return BlockEnhancementMod.Configuration.GetValue<bool>("Enhance More"); } internal set { BlockEnhancementMod.Configuration.SetValue("Enhance More", value); } } 

        /// <summary>模块行为</summary>
        public BlockBehaviour BB { get; internal set; } 

        /// <summary>进阶属性按钮</summary>
        public MToggle EnhancementToggle;

        /// <summary>进阶属性激活</summary>
        public bool EnhancementEnabled { get; set; } = false;

        [Obsolete]
        /// <summary>模块数据加载事件 传入参数类型:XDataHolder</summary>
        public Action<XDataHolder> BlockDataLoadEvent;
        [Obsolete]
        /// <summary>模块数据储存事件 传入参数类型:XDataHolder</summary>
        public Action<XDataHolder> BlockDataSaveEvent;

        public Action<MapperType> PropertiseChangedEvent;

        private bool isFirstFrame = true;
        private bool mapperMe = false;
        private void Awake()
        {

            BB = GetComponent<BlockBehaviour>();           
            
            SafeAwake();

            //if (BB.isSimulating ) { return; }        

            EnhancementToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.Enhancement, "Enhancement", EnhancementEnabled);
            EnhancementToggle.Toggled += (bool value) => { EnhancementEnabled = value; PropertiseChangedEvent(EnhancementToggle); /*DisplayInMapper(value);*/ };

            //LoadConfiguration();    

            PropertiseChangedEvent += ChangedProperties;
            PropertiseChangedEvent += (mapperType) => { DisplayInMapper(EnhancementEnabled); };
            PropertiseChangedEvent?.Invoke(EnhancementToggle);

            StartCoroutine(onPlaced());
            //Controller.Instance.OnSave += SaveConfiguration;
            IEnumerator onPlaced()
            {
                yield return new WaitUntil(() => BB.PlacementComplete);
                if (!BB.ParentMachine.isSimulating) { OnPlaced(); }
                yield break;
            }
        }
        void Update()
        {
            if (BB.SimPhysics)
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
                if (EnhancementEnabled) 
                {
                    BuildingUpdateAlways_EnhancementEnabled();
                }

                if (BlockMapper.IsOpen && BB == BlockMapper.CurrentInstance.Block)
                {
                    if (InputManager.CopyKeys()) OnCopy();
                    if (InputManager.PasteKeys()) OnPaste();

                    if (!mapperMe)
                    {
                        BlockMapper.CurrentInstance.CopyButton.Click += OnCopy;
                        BlockMapper.CurrentInstance.PasteButton.Click += OnPaste;
                        mapperMe = true;
                    }
                }
                else
                { mapperMe = false; }

                isFirstFrame = true;
            }
        }
        private void FixedUpdate()
        {
            if (!EnhancementEnabled) return;

            if (BB.SimPhysics && !isFirstFrame) { SimulateFixedUpdate_EnhancementEnabled(); }
        }
        private void LateUpdate()
        {
            if (!EnhancementEnabled) return;

            if (BB.SimPhysics && !isFirstFrame) { SimulateLateUpdate_EnhancementEnabled(); }
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
        /// 在方块放置时要做的事
        /// </summary>
        public virtual void OnPlaced() { }
        public virtual void OnCopy() { }
        public virtual void OnPaste() { Debug.Log(mapperMe); }
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
        /// <param name="value">EnhancementEnabled.value</param>
        public virtual void DisplayInMapper(bool enhance) {  }
        /// <summary>
        /// 属性改变（滑条值改变脚本属性随之改变）
        /// </summary>
        public virtual void ChangedProperties(MapperType mapperType) { }
        /// <summary>
        /// 参数改变（联机模拟时主机对模块的一些参数初始化）
        /// </summary>
        public virtual void OnSimulateStartClient() { }

        public MKey AddKey(string displayName, string key, KeyCode defaultValue)
        {
            var mapper = BB.AddKey(displayName, key, defaultValue);
            mapper.KeysChanged += () => { PropertiseChangedEvent(mapper); };
            return mapper;
        }
        public MSlider AddSlider(string displayName,string key ,float defaultValue,float min,float max)
        {
            var mapper = BB.AddSlider(displayName, key, defaultValue, min, max);
            mapper.ValueChanged += (value) => { if (Input.GetKeyUp(KeyCode.Mouse0)) PropertiseChangedEvent(mapper); };
            return mapper;
        }
        public MLimits AddLimits(string displayName, string key, float defaultMin, float defaultMax, float highestAngle, FauxTransform fauxTransform)
        {
            var mapper = BB.AddLimits(displayName, key, defaultMin, defaultMax, highestAngle,fauxTransform, this);
            mapper.LimitsChanged += () => { if (Input.GetKeyUp(KeyCode.Mouse0)) PropertiseChangedEvent(mapper); };
            return mapper;
        }

        public MToggle AddToggle(string displayName, string key, bool defaultValue)
        {
            var mapper = BB.AddToggle(displayName, key, defaultValue);
            mapper.Toggled += (value) => {PropertiseChangedEvent(mapper);  };
            return mapper;
        }
        public MMenu AddMenu(string key, int defaultIndex, List<string> items)
        {
            var mapper = BB.AddMenu(key, defaultIndex, items);
            mapper.ValueChanged += (value) => { PropertiseChangedEvent(mapper); };
            return mapper;
        }
        public MColourSlider AddColourSlider(string displayName, string key, Color defaultValue,bool snapColors)
        {
            var mapper = BB.AddColourSlider(displayName, key, defaultValue,snapColors);
            mapper.ValueChanged += (value) => { if (Input.GetKeyUp(KeyCode.Mouse0)) PropertiseChangedEvent(mapper); };
            return mapper;
        }
        public MValue AddValue(string displayName, string key, float defaultValue)
        {
            var mapper = BB.AddValue(displayName, key, defaultValue);
            mapper.ValueChanged += (value) => { PropertiseChangedEvent(mapper); };
            return mapper;
        }
        public Transform GetLimitsDisplay()
        {
            return BB.VisualController.Block.MeshRenderer.transform;
        }
    }
    public interface IChangeSpeed
    {
        float Speed { get; set; }
        MSlider SpeedSlider { get; set; }
        MKey AddSpeedKey { get; set; }
        MKey ReduceSpeedKey { get; set; }
        MValue ChangeSpeedValue { get; set; }
    }
    public class ChangeSpeedBlock : EnhancementBlock,IChangeSpeed
    {
        public float Speed { get { return SpeedSlider.Value; } set { SpeedSlider.Value =/* Mathf.Clamp(value, SpeedSlider.Min, SpeedSlider.Max)*/value; } }
        public MSlider SpeedSlider { get; set; }
        public MKey AddSpeedKey { get; set; }
        public MKey ReduceSpeedKey { get; set; }
        public MValue ChangeSpeedValue { get; set; }

        public override void SafeAwake()
        {
            base.SafeAwake();
            AddSpeedKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.AddSpeed,"Add Speed", KeyCode.Equals);
            ReduceSpeedKey = /*BB.*/AddKey( LanguageManager.Instance.CurrentLanguage.ReduceSpeed, "Reduce Speed",KeyCode.Minus);
            ChangeSpeedValue = /*BB.*/AddValue(LanguageManager.Instance.CurrentLanguage.ChangeSpeed, "Change Speed", 0.1f);
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            try
            {
                AddSpeedKey.DisplayInMapper = ReduceSpeedKey.DisplayInMapper = ChangeSpeedValue.DisplayInMapper = value;
            }
            catch { }
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();

            try
            {
                if (AddSpeedKey.IsPressed || AddSpeedKey.EmulationPressed())
                {
                    Speed += ChangeSpeedValue.Value;
                }

                if (ReduceSpeedKey.IsPressed || ReduceSpeedKey.EmulationPressed())
                {
                    Speed -= ChangeSpeedValue.Value;
                }
            }
            catch { }
        }
    }

    public interface IChangeHardness
    {
        ConfigurableJoint ConfigurableJoint { get;}
        MMenu HardnessMenu { get;}
    }
    public class ChangeHardnessBlock :EnhancementBlock,IChangeHardness
    {
        //public int HardnessIndex { get; set; } = 1;
        public ConfigurableJoint ConfigurableJoint { get; set; }
        public MMenu HardnessMenu { get; set; }

        internal Hardness hardness;

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            HardnessMenu.DisplayInMapper = value;
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

            public Hardness SwitchHardness(int index)
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
            public Hardness GetOrginHardness(ConfigurableJoint joint)
            {
                return new Hardness(joint.projectionMode, joint.projectionAngle, joint.projectionDistance);
            }
            public ConfigurableJoint SwitchWoodHardness(int index, ConfigurableJoint joint)
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
            public ConfigurableJoint SwitchMetalHardness(int index, ConfigurableJoint joint)
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

