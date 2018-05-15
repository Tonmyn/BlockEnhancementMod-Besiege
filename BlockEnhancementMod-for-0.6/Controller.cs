using BlockEnhancementMod.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{

    public delegate void OnBlockPlaced(Transform block);

    class Controller : SingleInstance<Controller>
    {
        public override string Name { get; } = "Controller";

        /// <summary>
        /// 存档信息
        /// </summary>
        internal static MachineInfo MI;

        internal static bool Refresh = false;

        private bool _keyMapperOpen;

        private BlockBehaviour _lastBlock;

        

        public static event OnBlockPlaced OnBlockPlaced;

        private int machineBlockCount = 1;

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
                    BesiegeConsoleController.ShowMessage("keyMapperOpen");
                }

                if (BlockMapper.CurrentInstance.Block != _lastBlock)
                {
                    OnKeymapperOpen();
                    _lastBlock = BlockMapper.CurrentInstance.Block;
                }

            }
            else
            {
                _keyMapperOpen = false;
            }


            if (!StatMaster.levelSimulating)
            {

                if (Machine.Active())
                {
                    int currentCount = Machine.Active().BuildingBlocks.Count;
                    if (currentCount > machineBlockCount)
                    {
                        if(Controller.OnBlockPlaced != null)
                        {
                            Controller.OnBlockPlaced(Machine.Active().BuildingBlocks[currentCount-1].transform);
                        }
#if DEBUG
                        BesiegeConsoleController.ShowMessage("on place");
#endif
                    }
                    machineBlockCount = currentCount;
                }
            }
        }

        /// <summary>
        /// 获取菜单类型
        /// </summary>
        public static FieldInfo MapperTypesField = typeof(SaveableDataHolder).GetField("mapperTypes", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>是否有进阶属性</summary>
        public static bool HasEnhancement(BlockBehaviour block)
        {
            return block.MapperTypes.Exists(match => match.Key == "Enhancement");
        }

        /// <summary>
        /// 对所有具有进阶属性的零件添加进阶属性控件
        /// </summary>
        public static void AddAllSliders()
        {
            foreach (BlockBehaviour block in Machine.Active().BuildingBlocks.FindAll(block => !HasEnhancement(block)))
            {
                AddSliders(block);
            }
        }

        /// <summary>
        /// 对有进阶属性的零件添加进阶属性控件 
        /// </summary>
        /// <param name="block"></param>
        private static void AddSliders(Transform block)
        {
            BlockBehaviour blockbehaviour = block.GetComponent<BlockBehaviour>();
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }

        public static IEnumerator RefreshSliders()
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
            Debug.Log("Refresh");
        }

        /// <summary>
        /// 添加进阶属性
        /// </summary>
        /// <param name="block"></param>
        public static void AddSliders(BlockBehaviour block)
        {
#if DEBUG
            BesiegeConsoleController.ShowMessage(block.BlockID.ToString());
#endif
            if (block.BlockID == (int)BlockType.Cannon)
            {
                if(block.GetComponent<CannonScript>()== null)
                block.gameObject.AddComponent<CannonScript>();
                //new Cannon(block);
            }

            if (block.BlockID == (int)BlockType.BallJoint)
            {
                if (block.GetComponent<BallJointScript>() == null)
                    block.gameObject.AddComponent<BallJointScript>();
                //new BallJoint(block);
            }

            if (WheelScript.IsWheel(block.BlockID))
            {
                if (block.GetComponent<WheelScript>() == null)
                    block.gameObject.AddComponent<WheelScript>();
                //new Wheel(block);
            }

            if (block.BlockID == (int)BlockType.GripPad)
            {
                if (block.GetComponent<GripPadScript>() == null)
                    block.gameObject.AddComponent<GripPadScript>();
                //new GripPad(block);

            }

            if (block.BlockID == (int)BlockType.Suspension)
            {
                if (block.GetComponent<SuspensionScript>() == null)
                    block.gameObject.AddComponent<SuspensionScript>();
                //new Suspension(block);

            }

            //if (block.GetBlockID() == (int)BlockType.SteeringHinge)
            //{

            //    new SteeringHinge(block);

            //}

            if (block.BlockID == (int)BlockType.Decoupler)
            {
                if (block.GetComponent<DecouplerScript>() == null)
                    block.gameObject.AddComponent<DecouplerScript>();
                //new Decoupler(block);
            }

            if (block.BlockID == (int)BlockType.SmallWheel)
            {
                if (block.GetComponent<SmallwheelScript>() == null)
                    block.gameObject.AddComponent<SmallwheelScript>();
                //new Smallwheel(block);
            }

            if (block.BlockID == (int)BlockType.Slider)
            {
                if (block.GetComponent<SliderScript>() == null)
                    block.gameObject.AddComponent<SliderScript>();
                //new Blocks.Slider(block);
            }

            if (block.BlockID == (int)BlockType.Piston)
            {
                if (block.GetComponent<PistonScript>() == null)
                    block.gameObject.AddComponent<PistonScript>();
                //new Blocks.Piston(block);
            }

            if (block.BlockID == (int)BlockType.SpinningBlock)
            {
                if (block.GetComponent<SpinningScript>() == null)
                    block.gameObject.AddComponent<SpinningScript>();
                //new Blocks.Spinning(block);
            }

            if (block.BlockID == (int)BlockType.Spring)
            {
                if (block.GetComponent<SpringScript>() == null)
                    block.gameObject.AddComponent<SpringScript>();
                //new Blocks.Spring(block);
            }

            if (PropellerScript.IsPropeller(block.BlockID))
            {
                if (block.GetComponent<PropellerScript>() == null)
                    block.gameObject.AddComponent<PropellerScript>();
                //new Blocks.Propeller(block);
            }

            //if (Cog.IsCog(block.BlockID))
            //{

            //    new Blocks.Cog(block);
            //}

        }




        public virtual void LoadConfiguration(MachineInfo mi)
        {

#if DEBUG
            BesiegeConsoleController.ShowMessage("载入存档");
#endif
            MI = mi;

            Refresh = true;

            StartCoroutine(RefreshSliders());

            //load?.Invoke();

        }


        public delegate void SaveConfigurationHandler(MachineInfo mi);

        public static event SaveConfigurationHandler Save;

        public virtual void SaveConfiguration(MachineInfo mi)
        {
            Save(mi);
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
