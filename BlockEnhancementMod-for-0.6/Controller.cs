using BlockEnhancementMod.Blocks;
using cakeslice;
using Modding;
using Modding.Blocks;
using Modding.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockEnhancementMod
{

    class Controller : SingleInstance<Controller>
    {
        public override string Name { get; } = "Controller";

        public bool showGUI = true;

        public bool Friction = false;

        public Transform targetSavedInController;

        private Rect windowRect = new Rect(15f, 100f, 180f, 50f+20f);

        private readonly int windowID = ModUtility.GetWindowId();

        [Obsolete]
        /// <summary>存档信息</summary>
        internal PlayerMachineInfo PMI;

        //public event Action<PlayerMachineInfo> OnLoad;
        //public event Action<PlayerMachineInfo> OnSave;
        public Action<bool> OnFrictionToggle;

        private void Awake()
        {
            ////加载配置
            //Events.OnMachineLoaded += LoadConfiguration;
            ////储存配置
            //Events.OnMachineSave += SaveConfiguration;
            //添加零件初始化事件委托
            Events.OnBlockInit += AddSliders;

            OnFrictionToggle += FrictionToggle;

            try
            {
                ModConsole.RegisterCommand("CompleteFunctionMode", args =>
                {
                    try
                    {
                        if (args[0].ToLower() == "no8workshop")
                        {
                            EnhancementBlock.No8Workshop = true;
                        }
                        else
                        {
                            EnhancementBlock.No8Workshop = false;
                        }
                    }
                    catch
                    {
                        EnhancementBlock.No8Workshop = false;
                    }


                }, "help: ");
            }
            catch { }

        }

        void Update()
        {
            if (AddPiece.Instance.CurrentType == BlockType.SmallPropeller && Input.GetKeyDown(KeyCode.LeftShift))
            {
                AddPiece.Instance.SetBlockType(BlockType.Unused3);
                AddPiece.Instance.clickSound.Play();
            }
        }

        /// <summary>是否有进阶属性</summary>
        public static bool HasEnhancement(BlockBehaviour block)
        {
            return block.MapperTypes.Exists(match => match.Key == "Enhancement");
        }

        /// <summary>所有零件添加进阶属性控件</summary>
        public void AddAllSliders()
        {
            foreach (BlockBehaviour block in Machine.Active().BuildingBlocks.FindAll(block => !HasEnhancement(block)))
            {
                AddSliders(block);
            }
        }

        /// <summary>零件添加进阶属性控件 </summary>
        private void AddSliders(Block block)
        {
#if DEBUG
            ConsoleController.ShowMessage("on block init");
#endif
            BlockBehaviour blockbehaviour = block.BuildingBlock.InternalObject;
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }
        /// <summary>零件添加进阶属性控件 </summary>
        private void AddSliders(Transform block)
        {
            BlockBehaviour blockbehaviour = block.GetComponent<BlockBehaviour>();
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }
        /// <summary>零件添加进阶属性控件 </summary>
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

        private void FrictionToggle(bool value)
        {
            PhysicMaterialCombine physicMaterialCombine = value ? PhysicMaterialCombine.Average : PhysicMaterialCombine.Maximum;

            //设置地形的摩擦力合并方式
            foreach (var v in GameObject.Find("Terrain Terraced").GetComponentsInChildren<MeshCollider>())
            {
                v.sharedMaterial.frictionCombine = physicMaterialCombine;
                v.sharedMaterial.bounceCombine = physicMaterialCombine;
                break;
            }
        }

        /// <summary>模块扩展脚本字典   通过字典自动为模块加载扩展脚本</summary>
        public Dictionary<int, Type> dic_EnhancementBlock = new Dictionary<int, Type>
        {
            {(int)BlockType.BallJoint,typeof(BallJointScript) },
            {(int)BlockType.Cannon,typeof(CannonScript) },
            ////{(int)BlockType.CogLargeUnpowered,typeof(cog) },
            ////{(int)BlockType.CogMediumPowered,typeof(CannonScript) },
            ////{(int)BlockType.CogMediumUnpowered,typeof(CannonScript) },
            {(int)BlockType.Decoupler,typeof(DecouplerScript) },
            {(int)BlockType.GripPad,typeof(GripPadScript) },
            {(int)BlockType.Piston,typeof(PistonScript) },
            ////{(int)BlockType.Propeller,typeof(PropellerScript) },
            ////{(int)BlockType.SmallPropeller,typeof(PropellerScript) },
            {(int)BlockType.Slider,typeof(SliderScript) },
            {(int)BlockType.SmallWheel,typeof(SmallwheelScript) },
            ////{(int)BlockType.SpinningBlock,typeof(SpinningScript) },
            {(int)BlockType.Spring,typeof(SpringScript) },
            {(int)BlockType.SteeringHinge,typeof(SteeringHinge) },
            {(int)BlockType.SteeringBlock,typeof(SteeringHinge) },
            {(int)BlockType.Suspension,typeof(SuspensionScript) },
            { (int)BlockType.Flamethrower,typeof(FlamethrowerScript)},
            {(int)BlockType.Wheel,typeof(WheelScript) },
            {(int)BlockType.LargeWheel,typeof(WheelScript) },
            {(int)BlockType.LargeWheelUnpowered,typeof(WheelScript) },
            {(int)BlockType.WheelUnpowered,typeof(WheelScript) },
            {(int)BlockType.Rocket,typeof(RocketScript)},
            {(int)BlockType.CameraBlock,typeof(CameraScript)},
            { (int)BlockType.SingleWoodenBlock,typeof(WoodenScript)},
            { (int)BlockType.DoubleWoodenBlock,typeof(WoodenScript)},
            { (int)BlockType.Log,typeof(WoodenScript)},
            { (int)BlockType.WoodenPanel,typeof(WoodenScript)},
            { (int)BlockType.WoodenPole,typeof(WoodenScript)},
            { (int)BlockType.WaterCannon,typeof(WaterCannonScript) },
        };

        [Obsolete]
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

        private void OnGUI()
        {
            if (showGUI && !StatMaster.levelSimulating && IsBuilding() && !StatMaster.inMenu&& !StatMaster.isClient)
            {
                windowRect = GUILayout.Window(windowID, windowRect, new GUI.WindowFunction(EnhancedEnhancementWindow), LanguageManager.modSettings);
            }
        }

        private void EnhancedEnhancementWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                EnhancementBlock.No8Workshop = GUILayout.Toggle(/*new Rect(10, 20, 70, 40),*/ EnhancementBlock.No8Workshop, LanguageManager.additionalFunction);

                if (Friction != GUILayout.Toggle(/*new Rect(10, 70, 70, 40),*/ Friction, new GUIContent(LanguageManager.unifiedFriction, "dahjksdhakjsd")))
                {
                    Friction = !Friction;
                    OnFrictionToggle(Friction);
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private bool IsBuilding()
        {
            List<string> scene = new List<string> { "INITIALISER", "TITLE SCREEN", "LevelSelect", "LevelSelect1", "LevelSelect2", "LevelSelect3" };

            if (SceneManager.GetActiveScene().isLoaded)
            {

                if (!scene.Exists(match => match == SceneManager.GetActiveScene().name))
                {
                    return true;
                }
                return false;
            }

            return false;

        }
    }

}
