using Modding;
using UnityEngine;
using Modding.Levels;
using System.Collections.Generic;
using System;
using System.Collections;

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
        }
    }

    public class Configuration
    {
        internal static ArrayList Propertises { get; private set; } = new ArrayList()
        {
            new Propertise<bool>("Enhance More",  false ),
            new Propertise<bool>("ShowUI",true ),
            new Propertise<bool>("Friction",  false ),
            new Propertise<bool>("Display Warning", true ),
            new Propertise<bool>("Mark Target", true ),
            new Propertise<bool>("Display Rocket Count", true ),

            new Propertise<float>("GuideControl P Factor",  1.25f),
            new Propertise<float>("GuideControl I Factor",  10f),
            new Propertise<float>("GuideControl D Factor",0f),
            new Propertise<float>("Rocket Smoke Emission Constant",80f),
            new Propertise<float>("Rocket Smoke Lifetime",1f),
            new Propertise<float>("Rocket Smoke Size",3.5f),

            new Propertise<int>("Radar Fequency",20),
        };

        //public bool EnhanceMore = false;
        //public bool ShowUI = true;
        //public bool Friction = false;
        //public bool DisplayWaring = true;
        //public bool MarkTarget = true;
        //public bool DisplayRocketCount = true;

        //public float GuideControlPFactor = 1.25f;
        //public float GuideControlIFactor = 10f;
        //public float GuideControlDFactor = 0f;

        //public int RadarFequency = 20;
        public class Propertise<T>
        {
            public string Key = "";
            public T Value = default;

            public Propertise(string key, T value) { Key = key; Value = value; }
            public override string ToString()
            {
                return Key + " - " + Value.ToString();
            }
        }

        public T GetValue<T>(string key)
        {
            T value = default;

            foreach (var pro in Propertises)
            {
                if (pro is Propertise<T>)
                {
                    var _pro = pro as Propertise<T>;
                    if (_pro.Key == key)
                    {
                        value = _pro.Value;
                        break;
                    }
                }
            }
            return value;
        }

        public void SetValue<T>(string key, T value)
        {
            var exist = false;

            foreach (var pro in Propertises)
            {
                if (pro is Propertise<T>)
                {
                    var _pro = pro as Propertise<T>;
                    if (_pro.Key == key)
                    {
                        _pro.Value = value;
                        exist = true;
                        break;
                    }
                }
            }

            if (!exist)
            {
                Propertises.Add(new Propertise<T>(key, value));
            }

            Modding.Configuration.GetData().Write(key, value);
        }

        ~Configuration()
        {
            Modding.Configuration.Save();
        }

        public static Configuration FormatXDataToConfig(Configuration config = null)
        {
            XDataHolder xDataHolder = Modding.Configuration.GetData();
            bool reWrite = true;
            bool needWrite = false;

            if (config == null)
            {
                reWrite = false;
                needWrite = true;
                config = new Configuration();
            }

            for (int i = 0; i < Propertises.Count; i++)
            {
                var value = Propertises[i];

                if (value is Propertise<int>)
                {
                    value = getValue(value as Propertise<int>);
                }
                else if (value is Propertise<bool>)
                {
                    value = getValue(value as Propertise<bool>);
                }
                else if (value is Propertise<float>)
                {
                    value = getValue(value as Propertise<float>);
                }
                else if (value is Propertise<string>)
                {
                    value = getValue(value as Propertise<string>);
                }
                else if (value is Propertise<Vector3>)
                {
                    value = getValue(value as Propertise<Vector3>);
                }
                Propertises[i] = value;
            }

            if (needWrite) Modding.Configuration.Save();

            return config;

            Propertise<T> getValue<T>(Propertise<T> propertise)
            {
                var key = propertise.Key;
                var defaultValue = propertise.Value;

                if (xDataHolder.HasKey(key) && !reWrite)
                {
                    defaultValue = (T)Convert.ChangeType(typeSpecialAction[typeof(T)](xDataHolder, key), typeof(T));
                }
                else
                {
                    xDataHolder.Write(key, defaultValue);
                    needWrite = true;
                }

                return new Propertise<T>(key, defaultValue);
            }
        }
        private static Dictionary<Type, Func<XDataHolder, string, object>> typeSpecialAction = new Dictionary<Type, Func<XDataHolder, string, object>>
        {
            { typeof(int), (xDataHolder,key)=>xDataHolder.ReadInt(key)},
            { typeof(bool), (xDataHolder,key)=>xDataHolder.ReadBool(key)},
            { typeof(float), (xDataHolder,key)=>xDataHolder.ReadFloat(key)},
            { typeof(string), (xDataHolder,key)=>xDataHolder.ReadString(key)},
            { typeof(Vector3), (xDataHolder,key)=>xDataHolder.ReadVector3(key)},
        };
    }
}
