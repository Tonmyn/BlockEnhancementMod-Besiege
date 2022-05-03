﻿using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class BlockEnhancementMod : ModEntryPoint
    {
        public static GameObject mod;
        public static Configuration Configuration { get; private set; }
        public static ModSetting ModSetting { get; internal set; }
        public override void OnLoad()
        {
            Configuration = Configuration.FormatXDataToConfig();
            ModSetting = new ModSetting();

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
            AssetManager.Instance.transform.SetParent(mod.transform);

            //EnhancementEventsController events = mod.AddComponent<EnhancementEventsController>(); ;
            //ModEvents.RegisterCallback(1, events.OnGroup);

            ModConsole.RegisterCommand("be", new CommandHandler((value) =>
            {
                Dictionary<string, Action<string[]>> commandOfAction = new Dictionary<string, Action<string[]>>
                {

                    { "srsecon",   (args)=>{ ModSetting.RocketSmokeEmissionConstant = float.Parse(args[1]);} },
                    { "srsl",   (args)=>{ ModSetting.RocketSmokeLifetime = float.Parse(args[1]);} },
                    { "srss",   (args)=>{ModSetting.RocketSmokeSize = float.Parse(args[1]);} },
                    { "srssc",   (args)=>{ ModSetting.RocketSmokeStartColor = new Color (float.Parse( args[1]),float.Parse( args[2]),float.Parse( args[3]));} },
                    { "srsec",   (args)=>{ ModSetting.RocketSmokeEndColor = new Color (float.Parse( args[1]),float.Parse( args[2]),float.Parse( args[3]));} },
                    { "srssct",   (args)=>{ ModSetting.RocketSmokeStartColorTime = float.Parse(args[1]);} },
                    { "srsect",   (args)=>{ ModSetting.RocketSmokeEndColorTime = float.Parse(args[1]);} },
                    { "srssa",   (args)=>{ ModSetting.RocketSmokeStartAlpha = float.Parse(args[1]);} },
                    { "srsea",   (args)=>{ ModSetting.RocketSmokeEndAlpha = float.Parse(args[1]);} },
                    { "srssat",   (args)=>{ ModSetting.RocketSmokeStartAlphaTime = float.Parse(args[1]);} },
                    { "srseat",   (args)=>{ ModSetting.RocketSmokeEndAlphaTime = float.Parse(args[1]);} },
                    { "srf",   (args)=>{ ModSetting.RadarFrequency = int.Parse(args[1]);} },
                    { "rfa",    (args)=>{ AssetManager.Instance.RereadAudioClipAsset(); } },
                    { "setgap",     (args)=>{ ArmorRoundScript.GlobleAudioPitchValue = Mathf.Clamp( float.Parse(args[1]),0f,2f); } },
                };
                try
                {
                    if (commandOfAction.ContainsKey(value[0].ToLower()))
                    {
                        commandOfAction[value[0].ToLower()].Invoke(value);
                    }
                    else
                    {
                        Debug.Log(string.Format("Unknown command '{0}', type 'help' for list.", value[0]));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(string.Format("Unknown command '{0}', type 'help' for list.", value[0]));
                }
            }),
          "<color=#FF6347>" +
          "Enhancement Mod Commands\n" +
          "  Usage: be srsecon :  set rocket smoke emission constant. ('be srsecon 80.0' )\n" +
          "  Usage: be srsl :  set rocket smoke lifetime. ('be srsl 1.0' )\n" +
          "  Usage: be srss :  set rocket smoke size. ('be srss 3.5' )\n" +
          "  Usage: be srssc :  set rocket smoke start color. ('be srssc 0.1 0.14 0.15' )\n" +
          "  Usage: be srsec :  set rocket smoke end color. ('be srsec 0.1 0.14 0.15' )\n" +
          "  Usage: be srssct :  set rocket smoke start color time. ('be srssct 0.09' )\n" +
          "  Usage: be srsect :  set rocket smoke end color time. ('be srsect 1' )\n" +
          "  Usage: be srssa :  set rocket smoke start alpha. ('be srssa 1.0' )\n" +
          "  Usage: be srsea :  set rocket smoke end alpha. ('be srsea 1.0' )\n" +
          "  Usage: be srssat :  set rocket smoke start alpha time. ('be srssat 0.076' )\n" +
          "  Usage: be srseat :  set rocket smoke end alpha time. ('be srseat 0.26' )\n" +
          "  Usage: be rfa:  Refresh asset resource.\n" +
          "  Usage: be setgap [value]:  Set Globle Audio Pitch Value(default value:1,interval:[0,2]).\n" +
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

        public static string SetColor(this string str, Color color)
        {
            var _color = ColorUtility.ToHtmlStringRGB(color);
            return string.Format("<color=#{0}>{1}</color>", _color, str);
        }

        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length = 4, bool useNum = true, bool useLow = true, bool useUpp = true, bool useSpe = false, string custom = "")
        {
            byte[] b = new byte[4];
            b[0] = (byte)UnityEngine.Random.Range(0, 255);
            b[1] = (byte)UnityEngine.Random.Range(0, 255);
            b[2] = (byte)UnityEngine.Random.Range(0, 255);
            b[3] = (byte)UnityEngine.Random.Range(0, 255);
            System.Random r = new System.Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        public static void ShowMessageWithColor(string message,Color color = default(Color))
        {
            var _color = ColorUtility.ToHtmlStringRGB(color);
            ConsoleController.ShowMessage(string.Format("<color=#{0}>{1}{2}</color>", _color, message, " -- Form Block Enhancement Mod"));
        }
    }
}

