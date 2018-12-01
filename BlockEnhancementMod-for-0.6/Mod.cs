using Modding;
using UnityEngine;

namespace BlockEnhancementMod
{

    public class BlockEnhancementMod : ModEntryPoint
    {
        public static GameObject mod;

        public override void OnLoad()
        {
            mod = new GameObject("Block Enhancement Mod");
            Object.DontDestroyOnLoad(mod);
            Controller.Instance.transform.SetParent(mod.transform);
            MessageController.Instance.transform.SetParent(mod.transform);
        }
    }

}
