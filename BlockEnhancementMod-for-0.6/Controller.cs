using BlockEnhancementMod.Blocks;
using BlockEnhancementMod.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockEnhancementMod
{

    public delegate void OnBlockPlaced(Transform block);

    class Controller : SingleInstance<Controller>
    {
        public override string Name { get; } = "Controller";

        /// <summary>存档信息</summary>
        internal MachineInfo MI;
        
        public event OnBlockPlaced OnBlockPlaced;

        private BlockBehaviour _lastBlock;

        private int machineBlockCount = 1;

        private bool _keyMapperOpen;

        private string currentSceneName;

        private void Start()
        {
            //加载配置
            XmlLoader.OnLoad += LoadConfiguration;

            //储存配置
            XmlSaver.OnSave += SaveConfiguration;

            //添加放置零件事件委托
            OnBlockPlaced += AddSliders;

            ////添加打开菜单事件委托
            //Game.OnKeymapperOpen += OnKeymapperOpen;
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        private void Update()
        {

            if (BlockMapper.CurrentInstance != null && BlockMapper.CurrentInstance.Block != null)
            {
                //AddPiece.Instance.

                // Check for open keymapper
                if (!_keyMapperOpen)
                {
                    OnKeymapperOpen();
                    _keyMapperOpen = true;
                }

                if (BlockMapper.CurrentInstance.Block != _lastBlock)
                {
                    OnKeymapperOpen();
                    _lastBlock = BlockMapper.CurrentInstance.Block;
                }

            }
            else
            {
                if (Machine.Active() != null && currentSceneName != SceneManager.GetActiveScene().name)
                {
                    AddAllSliders();
                    currentSceneName = SceneManager.GetActiveScene().name;
                }
                _keyMapperOpen = false;
            }


            if (!StatMaster.levelSimulating)
            {

                if (Machine.Active())
                {
                    int currentCount = Machine.Active().BuildingBlocks.Count;
                    if (currentCount > machineBlockCount)
                    {
                        if (OnBlockPlaced != null)
                        {
                            OnBlockPlaced(Machine.Active().BuildingBlocks[currentCount - 1].transform);
                        }
#if DEBUG
                        ConsoleController.ShowMessage("on place");
#endif
                    }
                    machineBlockCount = currentCount;
                }
            }
        }

        /// <summary>反射获取菜单私有属性</summary>
        public FieldInfo MapperTypesField = typeof(SaveableDataHolder).GetField("mapperTypes", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>是否有进阶属性</summary>
        public bool HasEnhancement(BlockBehaviour block)
        {
            return block.MapperTypes.Exists(match => match.Key == "Enhancement");
        }

        /// <summary>对所有没有进阶属性的零件添加进阶属性控件</summary>
        public void AddAllSliders()
        {
            foreach (BlockBehaviour block in Machine.Active().BuildingBlocks.FindAll(block => !HasEnhancement(block)))
            {
                AddSliders(block);
            }
        }

        /// <summary>对没有进阶属性的零件添加进阶属性控件 </summary>
        private void AddSliders(Transform block)
        {
            BlockBehaviour blockbehaviour = block.GetComponent<BlockBehaviour>();
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }

        /// <summary>添加进阶属性</summary>
        public void AddSliders(BlockBehaviour block)
        {
#if DEBUG
            ConsoleController.ShowMessage(string.Format("Block ID: {0}", block.BlockID.ToString()));
#endif

            if (dic_EnhancementBlock.ContainsKey(block.BlockID))
            {
                var EB = dic_EnhancementBlock[block.BlockID];

                if (block.GetComponent(EB) == null)
                {
                    block.gameObject.AddComponent(EB);
                }
            }
        }

        /// <summary>模块扩展脚本字典   通过字典自动为模块加载扩展脚本</summary>
        public Dictionary<int, Type> dic_EnhancementBlock = new Dictionary<int, Type>
        {
            {(int)BlockType.BallJoint,typeof(BallJointScript) },
            {(int)BlockType.Cannon,typeof(CannonScript) },
            //{(int)BlockType.CogLargeUnpowered,typeof(cog) },
            //{(int)BlockType.CogMediumPowered,typeof(CannonScript) },
            //{(int)BlockType.CogMediumUnpowered,typeof(CannonScript) },
            {(int)BlockType.Decoupler,typeof(DecouplerScript) },
            {(int)BlockType.GripPad,typeof(GripPadScript) },
            {(int)BlockType.Piston,typeof(PistonScript) },
            //{(int)BlockType.Propeller,typeof(PropellerScript) },
            //{(int)BlockType.SmallPropeller,typeof(PropellerScript) },
            {(int)BlockType.Slider,typeof(SliderScript) },
            {(int)BlockType.SmallWheel,typeof(SmallwheelScript) },
            //{(int)BlockType.SpinningBlock,typeof(SpinningScript) },
            {(int)BlockType.Spring,typeof(SpringScript) },
            //{(int)BlockType.SteeringHinge,typeof(ste) },
            {(int)BlockType.Suspension,typeof(SuspensionScript) },
            //{(int)BlockType.Wheel,typeof(WheelScript) },
            //{(int)BlockType.LargeWheel,typeof(WheelScript) },
            //{(int)BlockType.LargeWheelUnpowered,typeof(WheelScript) },
            //{(int)BlockType.WheelUnpowered,typeof(WheelScript) },
            {(int)BlockType.Rocket,typeof(RocketScript)},
            {(int)BlockType.CameraBlock,typeof(CameraScript)}
        };

        /// <summary>刷新菜单组件</summary>
        public IEnumerator RefreshSliders()
        {
            int i = 0;
            while (i++ < 3)
            {
                yield return new WaitForEndOfFrame();
            }
            foreach (BlockBehaviour block in Machine.Active().BuildingBlocks)
            {
                AddSliders(block);
            }

#if DEBUG
            ConsoleController.ShowMessage("Refresh");
#endif
        }

        public delegate void SaveConfigurationEvent(MachineInfo mi);

        public event SaveConfigurationEvent OnSave;

        /// <summary>储存存档信息</summary>
        public virtual void SaveConfiguration(MachineInfo mi)
        {
            Configuration.Save();

            OnSave(mi);
        }

        /// <summary>加载存档信息</summary>
        public virtual void LoadConfiguration(MachineInfo mi)
        {

#if DEBUG
            ConsoleController.ShowMessage("载入存档");
#endif

            if (Machine.Active().gameObject.GetComponent<CameraCompositeTrackerScript>())
            {
                Machine.Active().gameObject.GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Clear();
            }

            MI = mi;

            StartCoroutine(RefreshSliders());     

            //load?.Invoke();

        }

        public virtual void OnKeymapperOpen()
        {

            if (!HasEnhancement(BlockMapper.CurrentInstance.Block))
            {
                AddSliders(BlockMapper.CurrentInstance.Block);
                BlockMapper.CurrentInstance.Refresh();
            }
            AddAllSliders();
        }

    }
}
