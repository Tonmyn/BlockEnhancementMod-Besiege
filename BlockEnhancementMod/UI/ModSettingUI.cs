using BlockEnhancementMod.Blocks;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockEnhancementMod
{
    class ModSettingUI:SingleInstance<ModSettingUI>
    {
        public override string Name { get; } = "Mod Setting UI";

        public bool showGUI = true;

        public bool Friction = false;

        private Rect windowRect = new Rect(15f, 100f, 180f, 50f + 20f);

        private readonly int windowID = ModUtility.GetWindowId();


        public Action<bool> OnFrictionToggle;

        private void Awake()
        {
            OnFrictionToggle += FrictionToggle;
        }

        void Update()
        {
            if (AddPiece.Instance.CurrentType == BlockType.SmallPropeller && Input.GetKeyDown(KeyCode.LeftShift))
            {
                AddPiece.Instance.SetBlockType(BlockType.Unused3);
                AddPiece.Instance.clickSound.Play();
            }
        }   

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
   
        private void OnGUI()
        {
            if (showGUI && !StatMaster.levelSimulating && IsBuilding() && !StatMaster.inMenu)
            {
                windowRect = GUILayout.Window(windowID, windowRect, new GUI.WindowFunction(EnhancedEnhancementWindow), LanguageManager.modSettings);
            }
        }

        private void EnhancedEnhancementWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            {
                if (!StatMaster.isClient)
                {
                    EnhancementBlock.EnhanceMore = GUILayout.Toggle(EnhancementBlock.EnhanceMore, LanguageManager.additionalFunction);

                    if (Friction != GUILayout.Toggle(Friction, new GUIContent(LanguageManager.unifiedFriction, "dahjksdhakjsd")))
                    {
                        Friction = !Friction;
                        OnFrictionToggle(Friction);
                    }
                }
                RocketsController.DisplayWarning = GUILayout.Toggle(RocketsController.DisplayWarning, LanguageManager.displayWarning);
                RocketScript.MarkTarget = GUILayout.Toggle(RocketScript.MarkTarget, LanguageManager.markTarget);
                RocketsController.DisplayRocketCount = GUILayout.Toggle(RocketsController.DisplayRocketCount, LanguageManager.displayRocketCount);
            }
            GUILayout.Space(2);
            GUILayout.EndVertical();
            GUILayout.Space(10);
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
