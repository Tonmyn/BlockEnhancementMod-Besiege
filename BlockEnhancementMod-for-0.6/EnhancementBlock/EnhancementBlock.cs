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
    public  class EnhancementBlock : BlockScript
    {
        public static bool no8Workshop { get; internal set; } = false;

        /// <summary>
        /// 模块行为
        /// </summary>
        public BlockBehaviour BB { get; internal set; } 

        /// <summary>
        /// 进阶属性按钮
        /// </summary>
        public MToggle Enhancement;

        /// <summary>
        /// 进阶属性激活
        /// </summary>
        public bool enhancementEnabled { get; set; } = false;

        //public static BlockMessage BlockMessage { get; }

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
            
            SafeAwake();

            if (BB.isSimulating ) { return; }        

            Enhancement = BB.AddToggle(LanguageManager.enhancement, "Enhancement", enhancementEnabled);

            Enhancement.Toggled += (bool value) => { enhancementEnabled = value; DisplayInMapper(value); };

            //LoadConfiguration();    

            ChangedProperties(); try { BlockPropertiseChangedEvent(); } catch { }

            DisplayInMapper(enhancementEnabled);

            //Controller.Instance.OnSave += SaveConfiguration;
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
                    CJ.projectionAngle = 10f; break;
                case 2:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 5f; break;

                case 3:
                    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
                    CJ.projectionAngle = 0.5f; break;
                default:
                    CJ.projectionMode = JointProjectionMode.None; break;

            }

        }
      
    }

    public class BlockMessage
    {
        public MessageType messageType;

        public Action<Message> CallBackEvent;

        public BlockMessage(MessageType messageType,Action<Message> action)
        {
            this.messageType = messageType;
            CallBackEvent = action;

            ModNetworking.Callbacks[messageType] += action;

        }

    }

}

