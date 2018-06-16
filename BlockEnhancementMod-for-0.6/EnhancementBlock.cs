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

        public static AssetBundle iteratorVariable1;

        internal static List<string> MetalHardness = new List<string>() { "低碳钢", "中碳钢", "高碳钢" };

        internal static List<string> WoodHardness = new List<string>() { "朽木", "桦木", "梨木", "檀木" };

        public delegate void BlockDataLoadHandle(XDataHolder BlockData);

        /// <summary>模块数据加载事件 传入参数类型:XDataHolder</summary>
        public event BlockDataLoadHandle BlockDataLoadEvent;

        public delegate void BlockDataSaveHandle(XDataHolder BlockData);

        /// <summary>模块数据储存事件 传入参数类型:XDataHolder</summary>
        public event BlockDataSaveHandle BlockDataSaveEvent;

        private void Start()
        {

            BB = GetComponent<BlockBehaviour>();

            CurrentMapperTypes = BB.MapperTypes;

            Enhancement = AddToggle("进阶属性", "Enhancement", EnhancementEnable);

            Enhancement.Toggled += (bool value) => { EnhancementEnable = value; DisplayInMapper(value); };

            SafeStart();

            if (!StatMaster.levelSimulating)
            {

                LoadConfiguration();

                ChangedProperties();

                DisplayInMapper(EnhancementEnable);

                Controller.Save += SaveConfiguration;
            }
            Controller.MapperTypesField.SetValue(BB, CurrentMapperTypes);

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
            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

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
        /// 安全开始 模块只需要关心自己要添加什么控件就行了
        /// </summary>
        protected virtual void SafeStart() { }

        /// <summary>
        /// 在模拟开始的第一帧 要做的事
        /// </summary>
        protected virtual void OnSimulateStart() { }

        /// <summary>
        /// 在模拟模式下的Update
        /// </summary>
        protected virtual void OnSimulateUpdate() { }

        protected virtual void OnSimulateFixedUpdate() { }

        protected virtual void OnBuildingUpdate() { }

        protected virtual void LateUpdate() { }

        /// <summary>
        /// 加载网格信息
        /// </summary>
        /// <param name="ObjPath">OBJ文件路径</param>
        /// <returns>网格数据</returns>
        public static Mesh MeshFromObj(string ObjPath)
        {
            List<Vector3> Normals = new List<Vector3>();
            List<Vector2> UV = new List<Vector2>();
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector3> Vertices = new List<Vector3>();
            List<Vector2> newUV = new List<Vector2>();
            List<int> triangleslist = new List<int>();
            List<Vector3> newNormals = new List<Vector3>();
            Mesh mesh = new Mesh();
            StreamReader srd;

            if (!File.Exists(ObjPath))
            {
                //return iteratorVariable1.LoadAsset<Mesh>(ObjPath);
                return new AssetBundle().LoadAsset<Mesh>(ObjPath);
            }
            try
            {
                srd = File.OpenText(ObjPath);
                while (srd.Peek() != -1)
                {
                    string str = srd.ReadLine();
                    string[] chara = str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (chara.Length > 2)
                    {
                        if (chara[0] == "v")
                        {
                            Vector3 v1 = new Vector3(
                              Convert.ToSingle(chara[1]),
                              Convert.ToSingle(chara[2]),
                              Convert.ToSingle(chara[3]));
                            Vertices.Add(v1);
                        }
                        else if (chara[0] == "vt")
                        {
                            Vector2 uv1 = new Vector2(
                              Convert.ToSingle(chara[1]),
                              Convert.ToSingle(chara[2]));

                            UV.Add(uv1);
                        }
                        else if (chara[0] == "vn")
                        {
                            Vector3 v2 = new Vector3(
                              Convert.ToSingle(chara[1]),
                              Convert.ToSingle(chara[2]),
                              Convert.ToSingle(chara[3]));

                            Normals.Add(v2);
                        }
                        else if (chara[0] == "f")
                        {
                            if (chara.Length == 4)
                            {
                                int a = Convert.ToInt32(chara[1].Split('/')[0]);
                                int b = Convert.ToInt32(chara[2].Split('/')[0]);
                                int c = Convert.ToInt32(chara[3].Split('/')[0]);
                                triangleslist.Add(newVertices.Count);
                                triangleslist.Add(newVertices.Count + 1);
                                triangleslist.Add(newVertices.Count + 2);
                                newVertices.Add(Vertices[a - 1]);
                                newVertices.Add(Vertices[b - 1]);
                                newVertices.Add(Vertices[c - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[1].Split('/')[2]) - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[2].Split('/')[2]) - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[3].Split('/')[2]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[1].Split('/')[1]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[2].Split('/')[1]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[3].Split('/')[1]) - 1]);
                            }
                            if (chara.Length == 5)
                            {
                                int a = Convert.ToInt32(chara[1].Split('/')[0]);
                                int b = Convert.ToInt32(chara[2].Split('/')[0]);
                                int c = Convert.ToInt32(chara[3].Split('/')[0]);
                                int d = Convert.ToInt32(chara[4].Split('/')[0]);
                                triangleslist.Add(newVertices.Count);
                                triangleslist.Add(newVertices.Count + 1);
                                triangleslist.Add(newVertices.Count + 2);
                                triangleslist.Add(newVertices.Count);
                                triangleslist.Add(newVertices.Count + 2);
                                triangleslist.Add(newVertices.Count + 3);
                                newVertices.Add(Vertices[a - 1]);
                                newVertices.Add(Vertices[b - 1]);
                                newVertices.Add(Vertices[c - 1]);
                                newVertices.Add(Vertices[d - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[1].Split('/')[2]) - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[2].Split('/')[2]) - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[3].Split('/')[2]) - 1]);
                                newNormals.Add(Normals[Convert.ToInt32(chara[4].Split('/')[2]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[1].Split('/')[1]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[2].Split('/')[1]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[3].Split('/')[1]) - 1]);
                                newUV.Add(UV[Convert.ToInt32(chara[4].Split('/')[1]) - 1]);
                            }
                        }
                    }
                }
                mesh.vertices = newVertices.ToArray();
                mesh.uv = newUV.ToArray();
                mesh.triangles = triangleslist.ToArray();
                mesh.normals = newNormals.ToArray();
#if DEBUG
                Debug.Log("ReadFile " + ObjPath + " Completed!" + "Vertices:" + newVertices.Count.ToString());
#endif
                srd.Close();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.Optimize();
            }
            catch (Exception ex)
            {
                Debug.Log("Obj model " + ObjPath + " error!");
                Debug.Log("newUV==>" + newUV.Count.ToString());
                Debug.Log("triangleslist==>" + triangleslist.Count.ToString());
                Debug.Log("newNormals==>" + newNormals.Count.ToString());
                Debug.Log(ex.ToString());
            }
            return mesh;
        }

        public static Mesh MeshFormBundle(string ObjPath)
        {
            Mesh mesh = iteratorVariable1.LoadAsset<Mesh>(ObjPath);
            return mesh;
        }

        /// <summary>
        /// 根据按键重新构造按键组件
        /// </summary>
        /// <param name="keycode">键值清单</param>
        /// <returns></returns>
        [Obsolete]
        internal static MKey GetKey(List<KeyCode> keycode)
        {
            MKey MK = new MKey("", "", KeyCode.None);
            foreach (KeyCode key in keycode)
            {
                MK.AddOrReplaceKey(Array.IndexOf(keycode.ToArray(), key), key);
            }
            return MK;
        }

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

            //foreach (KeyCode k in keys)
            //{
            //    mKey.AddOrReplaceKey(keys.IndexOf(k), k);
            //}

            //mKey.KeysChanged += () => 
            //{
            //    keys.Clear();

            //    for (int i = 0; i < mKey.KeysCount; i++)
            //    {
            //        keys.Add(mKey.GetKey(i));           
            //    }

            //};

            CurrentMapperTypes.Add(mKey);

            //BlockDataLoadEvent += (XDataHolder value) =>
            // {
            //     if (value.HasKey(MapperType.XDATA_PREFIX + mKey.Key))
            //     {
            //         int index = 0; keys.Clear();
            //         foreach (string str in value.ReadStringArray("bmt-" + mKey.Key))
            //         {
            //             KeyCode kc = (KeyCode)Enum.Parse(typeof(KeyCode), str, true);
            //             mKey.AddOrReplaceKey(index++, kc);
            //             keys.Add(kc);
            //         }
            //     }
            // };

            //BlockDataSaveEvent += (XDataHolder value) => { value.Write("bmt-" + mKey.Key, mKey.Serialize().RawValue); };

            Data_Load_Save_event(mKey);

            return mKey;  

        }

        protected MSlider AddSlider(string displayName,string key,float value,float min,float max,bool disableLimit)
        {
            MSlider mSlider = new MSlider(displayName,key,value,min,max,disableLimit);

            CurrentMapperTypes.Add(mSlider);

            //BlockDataLoadEvent += (XDataHolder data) =>
            //  {
            //      if (data.HasKey("bmt-" + mSlider.Key)) { mSlider.Value = data.ReadFloat("bmt-" + mSlider.Key); }
            //  };

            //BlockDataLoadEvent += (XDataHolder data) =>
            // {
            //     XData xDatum = data.Read(MapperType.XDATA_PREFIX + mSlider.Key);
            //     if (xDatum != null || !StatMaster.isPaste)
            //     {
            //         mSlider.DeSerialize((xDatum == null ? mSlider.defaultData : xDatum));
            //     }
            // };

            //BlockDataSaveEvent += (XDataHolder data) =>
            //{
            //    if (data.HasKey("bmt-" + mSlider.Key)) { mSlider.Value = data.ReadFloat("bmt-" + mSlider.Key); }
            //};

            //BlockDataSaveEvent += (XDataHolder data) => 
            //{
            //    bool flag = (!StatMaster.SavingXML ? false : OptionsMaster.BesiegeConfig.ExcludeDefaultSaveData);
            //    if (!flag || !mSlider.isDefaultValue)
            //    {
            //        data.Write(mSlider.Serialize());
            //    }
            //};

            Data_Load_Save_event(mSlider);

            return mSlider;

        }

        protected MToggle AddToggle(string displayName, string key, bool defaltValue)
        {
            MToggle mToggle = new MToggle(displayName, key, defaltValue);

            CurrentMapperTypes.Add(mToggle);

            //BlockDataLoadEvent += (XDataHolder data) =>
            //{
            //    if (data.HasKey("bmt-" + mToggle.Key)) { mToggle.IsActive = data.ReadBool("bmt-" + mToggle.Key); }
            //};

            //BlockDataSaveEvent += (XDataHolder data) =>
            // {
            //     data.Write("bmt-" + mToggle.Key, mToggle.IsActive);
            // };

            Data_Load_Save_event(mToggle);

            return mToggle;
        }

        protected MMenu AddMenu(string key,int defaultIndex,List<string> items,bool footerMenu)
        {
            MMenu mMenu = new MMenu(key, defaultIndex, items, footerMenu);

            CurrentMapperTypes.Add(mMenu);

            //BlockDataLoadEvent += (XDataHolder data) =>
            // {
            //     if (data.HasKey("bmt-" + mMenu.Key)) { mMenu.Value = data.ReadInt("bmt-" + mMenu.Key); }
            // };

            //BlockDataSaveEvent += (XDataHolder data) =>
            // {
            //     data.Write("bmt-" + mMenu.Key, mMenu.Value);
            // };

            Data_Load_Save_event(mMenu);

            return mMenu;
        }

        protected MColourSlider AddColorSlider(string displayName,string key,Color value,bool snapToClosestColor)
        {
            MColourSlider mColorSlider = new MColourSlider(displayName, key, value, snapToClosestColor);

            CurrentMapperTypes.Add(mColorSlider);

            //BlockDataLoadEvent += (XDataHolder BlockData) =>
            // {
            //     if (BlockData.HasKey("bmt-" + mColorSlider.Key)) { mColorSlider.Value = BlockData.ReadColor("bmt-" + mColorSlider.Key); }
            // };

            //BlockDataSaveEvent += (XDataHolder BlockData) =>
            //{
            //    BlockData.Write("bmt-" + mColorSlider.Key, mColorSlider.Value);
            //};

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

