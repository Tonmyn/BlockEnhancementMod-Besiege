using System;
using UnityEngine;
using Modding;

namespace BlockEnhancementMod
{

    public class BlockEnhancementMod : ModEntryPoint
    {

        public static GameObject mod;

        public override void OnLoad()
        {

            mod = new GameObject("Block Enhancement Mod");
            Controller.Instance.transform.SetParent(mod.transform);
            //LanguageManager.Instance.transform.SetParent(mod.transform);
            
        }

    }

}
