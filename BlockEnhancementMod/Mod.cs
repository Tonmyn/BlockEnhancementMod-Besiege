using Modding;
using UnityEngine;
using Modding.Levels;
using System.Collections.Generic;

namespace BlockEnhancementMod
{

    public class BlockEnhancementMod : ModEntryPoint
    {
        public static GameObject mod;

        public override void OnLoad()
        {
            mod = new GameObject("Block Enhancement Mod");
            Object.DontDestroyOnLoad(mod);
            mod.AddComponent<EnhancementBlockController>();
            mod.AddComponent<Zone>();

            //Controller.Instance.transform.SetParent(mod.transform);
            ModSettingUI.Instance.transform.SetParent(mod.transform);
            LanguageManager.Instance.transform.SetParent(mod.transform);
            MessageController.Instance.transform.SetParent(mod.transform);
            RocketsController.Instance.transform.SetParent(mod.transform);

            EnhancementEventsController events = mod.AddComponent<EnhancementEventsController>(); ;
            ModEvents.RegisterCallback(1, events.OnGroup);
        }
    }

}
