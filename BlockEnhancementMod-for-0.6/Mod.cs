using System;
using UnityEngine;
using PluginManager.Plugin;

namespace BlockEnhancementMod
{

    [OnGameInit]
    public class BlockEnhancementMod : MonoBehaviour
    {

        public static GameObject mod;

        private void Start()
        {

            DontDestroyOnLoad(mod = new GameObject("Block Enhancement Mod"));
            Controller.Instance.transform.SetParent(mod.transform);
            LanguageManager.Instance.transform.SetParent(mod.transform);
            
        }

    }

}
