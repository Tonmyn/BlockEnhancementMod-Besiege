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
        public bool showGUI = BlockEnhancementMod.Configuration.ShowUI;
        public bool Friction = BlockEnhancementMod.Configuration.Friction;

        public Action<bool> OnFrictionToggle;
        private void FrictionToggle(bool value)
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

        public override void SafeAwake()
        {
            OnFrictionToggle += FrictionToggle;

            windowRect = new Rect(15f, 100f, 180f, 50f + 20f);
            windowName = LanguageManager.Instance.CurrentLanguage.ModSettings + "  Ctrl+F9";
            LanguageManager.Instance.OnLanguageChanged += (value) => { windowName = LanguageManager.Instance.CurrentLanguage.ModSettings + "  Ctrl+F9"; };
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F9))
            {
                BlockEnhancementMod.Configuration.ShowUI = showGUI = !showGUI;
                StartCoroutine(SaveConfig());
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
                    BlockEnhancementMod.Configuration.EnhanceMore = EnhancementBlock.EnhanceMore = AddToggle(EnhancementBlock.EnhanceMore, LanguageManager.Instance.CurrentLanguage.AdditionalFunction);

                    if (Friction != AddToggle (Friction, /*new GUIContent*/(LanguageManager.Instance.CurrentLanguage.UnifiedFriction)))
                    {
                        BlockEnhancementMod.Configuration.Friction = Friction = !Friction;
                        OnFrictionToggle(Friction);
                    }
                }
                BlockEnhancementMod.Configuration.DisplayWaring = RocketsController.DisplayWarning = AddToggle(RocketsController.DisplayWarning, LanguageManager.Instance.CurrentLanguage.DisplayWarning);
                BlockEnhancementMod.Configuration.MarkTarget = RadarScript.MarkTarget = AddToggle(RadarScript.MarkTarget, LanguageManager.Instance.CurrentLanguage.MarkTarget);
                BlockEnhancementMod.Configuration.DisplayRocketCount = RocketsController.DisplayRocketCount = AddToggle(RocketsController.DisplayRocketCount, LanguageManager.Instance.CurrentLanguage.DisplayRocketCount);
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
            StartCoroutine(SaveConfig());
            return value;
        }

        private IEnumerator SaveConfig()
        {
            yield return new WaitForSeconds(0.3f);
            Configuration.FormatXDataToConfig(/*Modding.Configuration.GetData(),*/ BlockEnhancementMod.Configuration);
        }
    }
}
