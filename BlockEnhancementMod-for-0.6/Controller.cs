using BlockEnhancementMod.Blocks;
using Modding;
using Modding.Blocks;
using Modding.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{

    //public delegate PlayerMachineInfo OnLoad();

    //public delegate PlayerMachineInfo OnSave();

    class Controller : SingleInstance<Controller>
    {
        public override string Name { get; } = "Controller";

        /// <summary>存档信息</summary>
        //internal PlayerMachineInfo MI;

        internal PlayerMachineInfo PMI;

        public event Action<PlayerMachineInfo> OnLoad;

        public event Action<PlayerMachineInfo> OnSave;

        private void Awake()
        {
            //加载配置
            Events.OnMachineLoaded += LoadConfiguration;
            //储存配置
            Events.OnMachineSave += SaveConfiguration;
            //添加放置零件事件委托
            Events.OnBlockPlaced += AddSliders;

            Events.OnBlockPlaced += block => { block.InternalObject.gameObject.AddComponent<print>(); };
        }

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
        private void AddSliders(Block block)
        {        
            BlockBehaviour blockbehaviour = block.BuildingBlock.InternalObject;
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }

        /// <summary>对没有进阶属性的零件添加进阶属性控件 </summary>
        private void AddSliders(Transform block)
        {
            BlockBehaviour blockbehaviour = block.GetComponent<BlockBehaviour>();
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }

        /// <summary>添加进阶属性</summary>
        private void AddSliders(BlockBehaviour block)
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


        /// <summary>储存存档信息</summary>
        private void SaveConfiguration(PlayerMachineInfo pmi)
        {

#if DEBUG
            ConsoleController.ShowMessage("储存存档");
#endif
            Configuration.Save();

            OnSave(pmi);
        }

        /// <summary>加载存档信息</summary>
        private void LoadConfiguration(PlayerMachineInfo pmi)
        {

#if DEBUG
            ConsoleController.ShowMessage("载入存档");
#endif

            PMI = pmi;

            OnLoad(pmi);

            AddAllSliders();

            StartCoroutine(RefreshSliders());

        }


        //private void OnKeymapperOpen()
        //{
        //    //OnKeymapperOpen();

        //    if (!HasEnhancement(BlockMapper.CurrentInstance.Block))
        //    {
        //        AddSliders(BlockMapper.CurrentInstance.Block);
        //        BlockMapper.CurrentInstance.Refresh();
        //    }
        //    AddAllSliders();
        //}

    }

    public class TTest : MCustom<int>
    {
        public TTest(string displayName, string key, int defaultValue) : base(displayName, key, defaultValue)
        {

        }

        public override XData SerializeValue(int value)
        {
            return new XString(SerializationKey, value.ToString());
        }

        public override int DeSerializeValue(XData data)
        {
            return int.Parse((string)(XString)data);
        }

        public override int Value { get => base.Value; set => base.Value = value; }
    }

    public class TTestSelector : CustomSelector<int, TTest>
    {

        protected override void CreateInterface()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateInterface()
        {
            throw new NotImplementedException();
        }


    }

    class print : MonoBehaviour
    {
        public MKey mKey;

        BlockBehaviour bb;

        void Start()
        {
            bb = GetComponent<BlockBehaviour>();
            mKey = new MKey("mKey", "key", KeyCode.T);
            if (bb.isBuildBlock)
            {
               
                
                bb.AddKey(mKey);
            }
        }

        void Update()
        {
            if (mKey.IsPressed)
            {
                BesiegeConsoleController.ShowMessage("print " + mKey.GetKey(0).ToString());
            }
        }
    }
}
