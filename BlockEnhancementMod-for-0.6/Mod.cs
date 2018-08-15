using System;
using UnityEngine;
using Modding;
using Modding.Mapper;

namespace BlockEnhancementMod
{
    public class BlockEnhancementMod : ModEntryPoint
    {

        public static GameObject mod;

        public override void OnLoad()
        {
            mod = new GameObject("Block Enhancement Mod");
            Controller.Instance.transform.SetParent(mod.transform);

        }
    }

}
