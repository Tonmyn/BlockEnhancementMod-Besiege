using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

namespace BlockEnhancementMod
{
    public class EnhancementBlock : MonoBehaviour
    {
        public static bool no8Workshop { get; internal set; } = false;

        /// <summary>
        /// 模块行为
        /// </summary>
        public BlockBehaviour BB;

        /// <summary>
        /// 当前Mapper类型
        /// </summary>
        //public List<MapperType> CurrentMapperTypes;

        protected List<MapperType> myMapperTypes = new List<MapperType>();

        /// <summary>
        /// 进阶属性按钮
        /// </summary>
        public MToggle Enhancement;

        /// <summary>
        /// 进阶属性激活
        /// </summary>
        public bool EnhancementEnable = false;

        private bool isFirstFrame = true;

        internal static List<string> MetalHardness = new List<string>() { LanguageManager.lowCarbonSteel, LanguageManager.midCarbonSteel, LanguageManager.highCarbonSteel };

        internal static List<string> WoodHardness = new List<string>() { LanguageManager.softWood, LanguageManager.midSoftWood, LanguageManager.hardWood, LanguageManager.veryHardWood };

        /// <summary>模块数据加载事件 传入参数类型:XDataHolder</summary>
        public Action<XDataHolder> BlockDataLoadEvent;

        /// <summary>模块数据储存事件 传入参数类型:XDataHolder</summary>
        public Action<XDataHolder> BlockDataSaveEvent;

        public Action BlockPropertiseChangedEvent;

        private void Awake()
        {
            BB = GetComponent<BlockBehaviour>();

            //CurrentMapperTypes = BB.MapperTypes;

            SafeAwake();

            //Make sure the target list is present
            //if (!Machine.Active().gameObject.GetComponent<TargetScript>())
            //{
            //    Machine.Active().gameObject.AddComponent<TargetScript>();
            //}

            if (BB.isSimulating)
            {
                return;
            }

            Enhancement = AddToggle(LanguageManager.enhancement, "Enhancement", EnhancementEnable);

            Enhancement.Toggled += (bool value) => { EnhancementEnable = value; DisplayInMapper(value); };

            //CurrentMapperTypes.AddRange(myMapperTypes);

            LoadConfiguration();

            ChangedProperties(); try { BlockPropertiseChangedEvent(); } catch { }

            DisplayInMapper(EnhancementEnable);

            Controller.Instance.OnSave += SaveConfiguration;
        }

        private void Update()
        {
            if (BB.isSimulating)
            {
                if (isFirstFrame)
                {
                    isFirstFrame = false;
                    OnSimulateStart();
#if DEBUG
                    //ConsoleController.ShowMessage("on simulation start");
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
            if (BB.isSimulating && !isFirstFrame)
            {
                OnSimulateFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            if (BB.isSimulating && !isFirstFrame)
            {
                OnSimulateLateUpdate();
            }
        }

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

                    try { BlockDataLoadEvent(bd); } catch { };

                    LoadConfiguration(bd);

                    foreach (MapperType item in myMapperTypes)
                    {
                        string str = string.Concat(MapperType.XDATA_PREFIX + item.Key);
                        XData xDatum = bd.Read(str);
                        if (xDatum != null || !StatMaster.isPaste)
                        {
                            item.DeSerialize((xDatum ?? item.defaultData));
                        }
                    }

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

            BB.AddKey(mKey);

            //foreach (KeyCode k in keys)
            //{
            //    mKey.AddOrReplaceKey(keys.IndexOf(k), k);
            //}

            myMapperTypes.Add(mKey);

            BlockPropertiseChangedEvent += () =>
            {
                keys.Clear();
                for (int i = 0; i < mKey.KeysCount; i++)
                {
                    keys.Add(mKey.GetKey(i));
                }

            };

            //mKey.KeysChanged += () =>
            //{
            //    keys.Clear();
            //    for (int i = 0; i < mKey.KeysCount; i++)
            //    {
            //        keys.Add(mKey.GetKey(i));
            //    }
            //};

            return mKey;
        }

        protected MSlider AddSlider(string displayName, string key, float value, float min, float max, bool disableLimit)
        {
            MSlider mSlider = new MSlider(displayName, key, value, min, max, disableLimit);

            BB.AddSlider(mSlider);

            myMapperTypes.Add(mSlider);

            return mSlider;
        }

        protected MToggle AddToggle(string displayName, string key, bool defaltValue)
        {
            MToggle mToggle = new MToggle(displayName, key, defaltValue);

            BB.AddToggle(mToggle);

            myMapperTypes.Add(mToggle);

            return mToggle;
        }

        protected MMenu AddMenu(string key, int defaultIndex, List<string> items, bool footerMenu)
        {
            MMenu mMenu = new MMenu(key, defaultIndex, items, footerMenu);

            BB.AddMenu(mMenu);

            myMapperTypes.Add(mMenu);

            return mMenu;
        }

        protected MColourSlider AddColorSlider(string displayName, string key, Color value, bool snapToClosestColor)
        {
            MColourSlider mColorSlider = new MColourSlider(displayName, key, value, snapToClosestColor);

            BB.AddColourSlider(mColorSlider);

            myMapperTypes.Add(mColorSlider);

            return mColorSlider;
        }
    }

    //class TargetScript : MonoBehaviour
    //{
    //    public Dictionary<int, int> previousTargetDic = new Dictionary<int, int>();
    //}
}

