using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class BlockEnhancementMod : ModEntryPoint
    {
        public static GameObject mod;
        public static Configuration Configuration { get; private set; }

        public override void OnLoad()
        {
            Configuration = Configuration.FormatXDataToConfig();

            mod = new GameObject("Block Enhancement Mod");
            UnityEngine.Object.DontDestroyOnLoad(mod);
            mod.AddComponent<EnhancementBlockController>();
            mod.AddComponent<ModSettingUI>();
            //mod.AddComponent<Zone>();
          
            //Controller.Instance.transform.SetParent(mod.transform);
            //ModSettingUI.Instance.transform.SetParent(mod.transform);
            LanguageManager.Instance.transform.SetParent(mod.transform);
            MessageController.Instance.transform.SetParent(mod.transform);
            RocketsController.Instance.transform.SetParent(mod.transform);

            //EnhancementEventsController events = mod.AddComponent<EnhancementEventsController>(); ;
            //ModEvents.RegisterCallback(1, events.OnGroup);

            ModConsole.RegisterCommand("be", new CommandHandler((value) =>
            {
                Dictionary<string, Action<string[]>> commandOfAction = new Dictionary<string, Action<string[]>>
                {
                    { "RefreshConfiguration".ToLower(),   (args)=>{ Debug.Log( Modding.Configuration.GetData().ReadColor("Rocket Smoke Start Color").ToString()); } },
                };

                if (commandOfAction.ContainsKey(value[0].ToLower()))
                {
                    commandOfAction[value[0].ToLower()].Invoke(value);
                }
                else
                {
                    Debug.Log(string.Format("Unknown command '{0}', type 'help' for list.", value[0]));
                }
            }),
          "<color=#FF6347>" +
          "Enhancement Mod Commands\n" +
          "  Usage: be RefreshConfiguration :  Refresh configuration param in config file.\n" +
          "</color>"
          );
        }
    }


    public static  class ExtensionMethods
    {
        /// <summary>
        /// Get component in self,children and parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static T GetComponentInAll<T>(this Component component)
        {
            T _component = component.GetComponentInChildren<T>();
            if (_component == null)
            {
                _component = component.GetComponentInParent<T>();
            }
            return _component;
        }

        /// <summary>
        /// Get component in self,children and parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static T GetComponentInAll<T>(this GameObject gameObject)
        {
            T _component = gameObject.GetComponentInChildren<T>();
            if (_component == null)
            {
                _component = gameObject.GetComponentInParent<T>();
            }
            return _component;
        }
    }
}

