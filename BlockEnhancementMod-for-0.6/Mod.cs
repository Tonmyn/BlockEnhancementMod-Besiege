using System;
using UnityEngine;
using PluginManager.Plugin;

namespace BlockEnhancementMod
{

    // If you need documentation about any of these values or the mod loader
    // in general, take a look at https://spaar.github.io/besiege-modloader.

    [OnGameInit]
    public class BlockEnhancementMod : MonoBehaviour
    {

        public static GameObject mod;

        private void Start()
        {
            BesiegeConsoleController.ShowMessage("Block Enhancement Mod Runing...");
            mod = new GameObject("Block Enhancement Mod");
            var controller = Controller.Instance;
            controller.transform.SetParent(mod.transform);
            UnityEngine.Object.DontDestroyOnLoad(mod);
            DontDestroyOnLoad(controller);
            
        }

        private void Update()
        {
           
        }


        

    }


}
