using BlockEnhancementMod.Blocks;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockEnhancementMod
{
    class ModSettingUI : SafeUIBehaviour
    {
        public override bool ShouldShowGUI { get; set; } = true;
        public bool showGUI = BlockEnhancementMod.ModSetting.ShowUI;
        public bool Friction  = BlockEnhancementMod.ModSetting.Friction;
        public bool BuildSurface_Collision_Mass = BlockEnhancementMod.ModSetting.BuildSurface_Collision_Mass;

        public Action<bool> OnFrictionToggle;
        //private void FrictionToggle(bool value)
        //{
        //    PhysicMaterialCombine physicMaterialCombine = value ? PhysicMaterialCombine.Average : PhysicMaterialCombine.Maximum;

        //    //设置地形的摩擦力合并方式
        //    if (GameObject.Find("Terrain Terraced") != null)
        //    {
        //        foreach (var v in GameObject.Find("Terrain Terraced").GetComponentsInChildren<MeshCollider>())
        //        {
        //            v.sharedMaterial.frictionCombine = physicMaterialCombine;
        //            v.sharedMaterial.bounceCombine = physicMaterialCombine;
        //            break;
        //        }
        //    }
        //}

        public override void SafeAwake()
        {
            //OnFrictionToggle += FrictionToggle;

            windowRect = new Rect(15f, 100f, 180f, 50f + 20f);
            windowName = LanguageManager.Instance.CurrentLanguage.ModSettings + "  Ctrl+F9";
            LanguageManager.Instance.OnLanguageChanged += (value) => { windowName = LanguageManager.Instance.CurrentLanguage.ModSettings + "  Ctrl+F9"; };
        }

        public override void OnGUI()
        {
            base.OnGUI();
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F9))
            {
                BlockEnhancementMod.ModSetting.ShowUI = showGUI = !showGUI;
            }

            ShouldShowGUI = showGUI && !StatMaster.levelSimulating && IsBuilding() && !StatMaster.inMenu;
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

        protected override void WindowContent(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            {
                if (!StatMaster.isClient)
                {
                    if (EnhancementBlock.EnhanceMore != AddToggle(EnhancementBlock.EnhanceMore, new GUIContent(LanguageManager.Instance.CurrentLanguage.AdditionalFunction)))
                    {
                        BlockEnhancementMod.ModSetting.EnhanceMore = EnhancementBlock.EnhanceMore = !EnhancementBlock.EnhanceMore;
                    }

                    if (Friction != AddToggle(Friction, new GUIContent(LanguageManager.Instance.CurrentLanguage.UnifiedFriction)))
                    {
                        BlockEnhancementMod.ModSetting.Friction = Friction = !Friction;
                        FrictionToggle(Friction);
                        OnFrictionToggle?.Invoke(Friction);

                        void FrictionToggle(bool value)
                        {
                            PhysicMaterialCombine physicMaterialCombine = value ? PhysicMaterialCombine.Average : PhysicMaterialCombine.Maximum;

                            //设置地形的摩擦力合并方式
                            if (GameObject.Find("Terrain Terraced") != null)
                            {
                                foreach (var v in GameObject.Find("Terrain Terraced").GetComponentsInChildren<MeshCollider>())
                                {
                                    v.sharedMaterial.frictionCombine = physicMaterialCombine;
                                    v.sharedMaterial.bounceCombine = physicMaterialCombine;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (BuildSurface_Collision_Mass != AddToggle(BuildSurface_Collision_Mass, new GUIContent(LanguageManager.Instance.CurrentLanguage.BuildSurface)))
                {
                    BlockEnhancementMod.ModSetting.BuildSurface_Collision_Mass = BuildSurface_Collision_Mass = !BuildSurface_Collision_Mass;
                    BuildSurface.ShowCollisionToggle = BuildSurface.ShowMassSlider = BuildSurface_Collision_Mass;
                }

                if (RocketsController.DisplayWarning != AddToggle(RocketsController.DisplayWarning, new GUIContent(LanguageManager.Instance.CurrentLanguage.DisplayWarning)))
                {
                    BlockEnhancementMod.ModSetting.DisplayWarning = RocketsController.DisplayWarning = !RocketsController.DisplayWarning;
                }

                if (RadarScript.MarkTarget != AddToggle(RadarScript.MarkTarget, new GUIContent(LanguageManager.Instance.CurrentLanguage.MarkTarget)))
                {
                    BlockEnhancementMod.ModSetting.MarkTarget = RadarScript.MarkTarget = !RadarScript.MarkTarget;
                }
                if (RocketsController.DisplayRocketCount != AddToggle(RocketsController.DisplayRocketCount, new GUIContent(LanguageManager.Instance.CurrentLanguage.DisplayRocketCount)))
                {
                    BlockEnhancementMod.ModSetting.DisplayRocketCount = RocketsController.DisplayRocketCount = !RocketsController.DisplayRocketCount;
                }


            }
            GUILayout.Space(2);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private bool AddToggle(bool value,string text)
        {
            value = GUILayout.Toggle(value, text);
            return value;
        }
        private bool AddToggle(bool value, GUIContent content)
        {
            var _value = GUILayout.Toggle(value,content);
            return _value;
        }
    }

    public class ModSetting
    {
        public bool EnhanceMore { get { return getValue<bool>("Enhance More"); } set { Changed("Enhance More", value); } }

        public bool ShowUI { get { return getValue<bool>("ShowUI"); } set { Changed("ShowUI", value); } }
        public bool Friction { get { return getValue<bool>("Friction"); } set { Changed("Friction", value); } }
        public bool DisplayWarning { get { return getValue<bool>("Display Warning"); } set { Changed("Display Warning", value); } }
        public bool MarkTarget { get { return getValue<bool>("Mark Target"); } set { Changed("Mark Target", value); } }
        public bool DisplayRocketCount { get { return getValue<bool>("Display Rocket Count"); } set { Changed("Display Rocket Count", value); } }
        public bool BuildSurface_Collision_Mass { get { return getValue<bool>("BuildSurface"); } set { Changed("BuildSurface", value); } }

        public float GuideControl_P_Factor { get { return getValue<float>("GuideControl P Factor"); } set { GuideController.pFactor = value; Changed("GuideControl P Factor", value); } }
        public float GuideControl_I_Factor { get { return getValue<float>("GuideControl I Factor"); } set { GuideController.iFactor = value;  Changed("GuideControl I Factor", value); } }
        public float GuideControl_D_Factor { get { return getValue<float>("GuideControl D Factor"); } set { GuideController.dFactor = value; Changed("GuideControl D Factor", value); } }

        public float RocketSmokeEmissionConstant { get { return getValue<float>("Rocket Smoke Emission Constant"); } set { RocketScript.trailSmokePropertise.EmissionConstant = value; Changed("Rocket Smoke Emission Constant", value); } }
        public float RocketSmokeLifetime { get { return getValue<float>("Rocket Smoke Lifetime"); } set { RocketScript.trailSmokePropertise.Lifetime= value; Changed("Rocket Smoke Lifetime", value); } }
        public float RocketSmokeSize { get { return getValue<float>("Rocket Smoke Size"); } set { RocketScript.trailSmokePropertise.Size = value; Changed("Rocket Smoke Size", value); } }
        public Color RocketSmokeStartColor { get { return getValue<Color>("Rocket Smoke Start Color"); } set { RocketScript.trailSmokePropertise.StartColor = value; Changed("Rocket Smoke Start Color", value); } }
        public Color RocketSmokeEndColor { get { return getValue<Color>("Rocket Smoke End Color"); } set { RocketScript.trailSmokePropertise.EndColor = value; Changed("Rocket Smoke End Color", value); } }
        public float RocketSmokeStartColorTime { get { return getValue<float>("Rocket Smoke Start Color Time"); } set { RocketScript.trailSmokePropertise.StartColorTime = value; Changed("Rocket Smoke Start Color Time", value); } }
        public float RocketSmokeEndColorTime { get { return getValue<float>("Rocket Smoke End Color Time"); } set { RocketScript.trailSmokePropertise.EndColorTime = value; Changed("Rocket Smoke End Color Time", value); } }
        public float RocketSmokeStartAlpha { get { return getValue<float>("Rocket Smoke Start Alpha"); } set { RocketScript.trailSmokePropertise.StartAlpha = value; Changed("Rocket Smoke Start Alpha", value); } }
        public float RocketSmokeEndAlpha { get { return getValue<float>("Rocket Smoke End Alpha"); } set { RocketScript.trailSmokePropertise.EndAlpha = value; Changed("Rocket Smoke End Alpha", value); } }
        public float RocketSmokeStartAlphaTime { get { return getValue<float>("Rocket Smoke Start Alpha Time"); } set { RocketScript.trailSmokePropertise.StartAlphaTime = value; Changed("Rocket Smoke Start Alpha Time", value); } }
        public float RocketSmokeEndAlphaTime { get { return getValue<float>("Rocket Smoke End Alpha Time"); } set { RocketScript.trailSmokePropertise.EndAlphaTime = value; Changed("Rocket Smoke End Alpha Time", value); } }

        public int RadarFrequency { get { return getValue<int>("Radar Frequency"); } set { RadarScript.RadarFrequency = value; Changed("Radar Frequency", value); } }


        //public ModSetting()
        //{
        //    enhanceMore = BlockEnhancementMod.Configuration.GetValue<bool>("Enhance More");
        //    showUI = BlockEnhancementMod.Configuration.GetValue<bool>("ShowUI");
        //    Friction = BlockEnhancementMod.Configuration.GetValue<bool>("Friction");
        //    DisplayWarning = BlockEnhancementMod.Configuration.GetValue<bool>("Display Warning");
        //    MarkTarget = BlockEnhancementMod.Configuration.GetValue<bool>("Mark Targete");
        //    DisplayRocketCount = BlockEnhancementMod.Configuration.GetValue<bool>("Display Rocket Count");

        //    GuideControl_P_Factor = BlockEnhancementMod.Configuration.GetValue<float>("GuideControl P Factor");
        //    GuideControl_I_Factor = BlockEnhancementMod.Configuration.GetValue<float>("GuideControl I Factor");
        //    GuideControl_D_Factor = BlockEnhancementMod.Configuration.GetValue<float>("GuideControl D Factor");

        //    RocketSmokeEmissionConstant = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Emission Constant");
        //    RocketSmokeLifetime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Lifetime");
        //    RocketSmokeSize = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Size");
        //    RocketSmokeStartColor = BlockEnhancementMod.Configuration.GetValue<Color>("Rocket Smoke Start Color");
        //    RocketSmokeEndColor = BlockEnhancementMod.Configuration.GetValue<Color>("Rocket Smoke End Color");
        //    RocketSmokeStartColorTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Start Color Time");
        //    RocketSmokeEndColorTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke End Color Time");
        //    RocketSmokeStartAlpha = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Start Alpha");
        //    RocketSmokeEndAlpha = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke End Alpha");
        //    RocketSmokeStartAlphaTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke Start Alpha Time");
        //    RocketSmokeEndAlphaTime = BlockEnhancementMod.Configuration.GetValue<float>("Rocket Smoke End Alpha Time");

        //    RadarFrequency = BlockEnhancementMod.Configuration.GetValue<int>("Radar Frequency");
        //    BuildSurface_Collision_Mass = BlockEnhancementMod.Configuration.GetValue<bool>("BuildSurface");
        //}
        private T getValue<T>(string key)
        {
            Debug.Log("get modsetting...");
            return BlockEnhancementMod.Configuration.GetValue<T>(key);
        }
        private void Changed<T>(string key,T value)
        {
            BlockEnhancementMod.Configuration.SetValue(key, value);
        }
    }
}
