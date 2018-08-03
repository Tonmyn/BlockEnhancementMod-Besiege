using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class EnhancementBlock : MonoBehaviour
    {
        /// <summary>
        /// 模块行为
        /// </summary>
        public BlockBehaviour BB;

        /// <summary>
        /// 当前Mapper类型
        /// </summary>
        public List<MapperType> CurrentMapperTypes;

        /// <summary>
        /// 进阶属性按钮
        /// </summary>
        public MToggle Enhancement;

        /// <summary>
        /// 进阶属性激活
        /// </summary>
        public bool EnhancementEnable = false;

        private bool isFirstFrame = true;
      
        internal static List<string> MetalHardness = new List<string>() { "低碳钢", "中碳钢", "高碳钢" };

        internal static List<string> WoodHardness = new List<string>() { "朽木", "桦木", "梨木", "檀木" };

        public delegate void BlockDataLoadHandle(XDataHolder BlockData);

        /// <summary>模块数据加载事件 传入参数类型:XDataHolder</summary>
        public event BlockDataLoadHandle BlockDataLoadEvent;

        public delegate void BlockDataSaveHandle(XDataHolder BlockData);

        /// <summary>模块数据储存事件 传入参数类型:XDataHolder</summary>
        public event BlockDataSaveHandle BlockDataSaveEvent;

        private void Awake()
        {
            BB = GetComponent<BlockBehaviour>();
            
            CurrentMapperTypes = BB.MapperTypes;

            Enhancement = AddToggle("进阶属性", "Enhancement", EnhancementEnable);

            Enhancement.Toggled += (bool value) => { EnhancementEnable = value; DisplayInMapper(value); };

            SafeAwake();

            if (!StatMaster.levelSimulating)
            {
                LoadConfiguration();

                ChangedProperties();

                DisplayInMapper(EnhancementEnable);

                //Controller.Instance.OnSave += SaveConfiguration;
            }
            // Controller.Instance.MapperTypesField.SetValue(BB, CurrentMapperTypes);
        }

        private void Start()
        {

            //BB = GetComponent<BlockBehaviour>();

            //CurrentMapperTypes = BB.MapperTypes;

            //Enhancement = AddToggle("进阶属性", "Enhancement", EnhancementEnable);

            //Enhancement.Toggled += (bool value) => { EnhancementEnable = value; DisplayInMapper(value); };

            //SafeAwake();

            //if (!StatMaster.levelSimulating)
            //{
            //    LoadConfiguration();

            //    ChangedProperties();

            //    DisplayInMapper(EnhancementEnable);

            //    Controller.Instance.OnSave += SaveConfiguration;
            //}
            //Controller.Instance.MapperTypesField.SetValue(BB, CurrentMapperTypes);

        }

        private void Update()
        {
            if (StatMaster.levelSimulating)
            {
                if (isFirstFrame)
                {
                    isFirstFrame = false;
                    OnSimulateStart();
#if DEBUG
                    ConsoleController.ShowMessage("on simulation start");
#endif
                }
                OnSimulateUpdate();
            }
            else
            {
                OnBuildingUpdate();
                isFirstFrame = true;
            }
        }

        private void FixedUpdate()
        {
            if (StatMaster.levelSimulating)
            {
                OnSimulateFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            if (StatMaster.levelSimulating)
            {
                OnSimulateLateUpdate();
            }
        }

        private void SaveConfiguration(MachineInfo Mi)
        {

            if (Mi == null)
            {
                return;
            }

            foreach (var blockinfo in Mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    BlockDataSaveEvent(bd);

                    SaveConfiguration(bd);

                    break;
                }
            }
        }

        private void LoadConfiguration()
        {
            if (Controller.Instance.PMI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.Instance.PMI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.Data;

                    BlockDataLoadEvent(bd);

                    LoadConfiguration(bd);

                    break;
                }
            }
        }

        /// <summary>
        /// 储存配置
        /// </summary>
        /// <param name="mi">当前存档信息</param>
        public virtual void SaveConfiguration(XDataHolder BlockData) { }

        /// <summary>
        /// 加载配置
        /// </summary>
        public virtual void LoadConfiguration(XDataHolder BlockData) { }

        /// <summary>
        /// 显示在Mapper里面
        /// </summary>
        public virtual void DisplayInMapper(bool value) { }

        /// <summary>
        /// 属性改变（滑条值改变脚本属性随之改变）
        /// </summary>
        public virtual void ChangedProperties() { }

        /// <summary>
        /// 安全唤醒 模块只需要关心自己要添加什么控件就行了
        /// </summary>
        protected virtual void SafeAwake() { }

        /// <summary>
        /// 在模拟开始的第一帧 要做的事
        /// </summary>
        protected virtual void OnSimulateStart() { }

        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        protected virtual void OnSimulateUpdate() { }
        
        /// <summary>
        /// 在模拟模式下的FixedUpdate
        /// </summary>
        protected virtual void OnSimulateFixedUpdate() { }

        /// <summary>
        /// 在模拟模式下的LateUpdate
        /// </summary>
        protected virtual void OnSimulateLateUpdate() { }

        /// <summary>
        /// 建造模式下的Update
        /// </summary>
        protected virtual void OnBuildingUpdate() { }

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
                    CJ.projectionAngle = 5f; break;
                case 2:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 0.5f; break;

                case 3:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 0; break;
                default:
                    CJ.projectionMode = JointProjectionMode.None; break;

            }

        }

        protected MKey AddKey(string displayName, string key, List<KeyCode> keys)
        {
            MKey mKey = new MKey(displayName, key, keys[0]);

            CurrentMapperTypes.Add(mKey);

            Data_Load_Save_event(mKey);

            return mKey;  
        }

        protected MSlider AddSlider(string displayName,string key,float value,float min,float max,bool disableLimit)
        {
            MSlider mSlider = new MSlider(displayName,key,value,min,max,disableLimit);

            CurrentMapperTypes.Add(mSlider);

            Data_Load_Save_event(mSlider);

            return mSlider;
        }

        protected MToggle AddToggle(string displayName, string key, bool defaltValue)
        {
            MToggle mToggle = new MToggle(displayName, key, defaltValue);

            //CurrentMapperTypes.Add(mToggle);
            BB.AddToggle(mToggle);

            Data_Load_Save_event(mToggle);

            return mToggle;
        }

        protected MMenu AddMenu(string key,int defaultIndex,List<string> items,bool footerMenu)
        {
            MMenu mMenu = new MMenu(key, defaultIndex, items, footerMenu);

            CurrentMapperTypes.Add(mMenu);

            Data_Load_Save_event(mMenu);

            return mMenu;
        }

        protected MColourSlider AddColorSlider(string displayName,string key,Color value,bool snapToClosestColor)
        {
            MColourSlider mColorSlider = new MColourSlider(displayName, key, value, snapToClosestColor);

            CurrentMapperTypes.Add(mColorSlider);

            Data_Load_Save_event(mColorSlider);

            return mColorSlider;
        }

        private void Data_Load_Save_event(MapperType mapperType)
        {
            BlockDataLoadEvent += (XDataHolder data) =>
            {
                XData xDatum = data.Read(MapperType.XDATA_PREFIX + mapperType.Key);
                if (xDatum != null || !StatMaster.isPaste)
                {
                    mapperType.DeSerialize((xDatum == null ? mapperType.defaultData : xDatum));
                }
            };

            BlockDataSaveEvent += (XDataHolder data) =>
            {
                bool flag = (!StatMaster.SavingXML ? false : OptionsMaster.BesiegeConfig.ExcludeDefaultSaveData);
                if (!flag || !mapperType.isDefaultValue)
                {
                    data.Write(mapperType.Serialize());
                }
            };
        }
    }

}

