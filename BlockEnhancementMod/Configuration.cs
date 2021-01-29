using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class Configuration
    {
        internal static ArrayList Properties { get; private set; } = new ArrayList()
        {
            new Property<bool>("Enhance More",  false),
            new Property<bool>("ShowUI", true),
            new Property<bool>("Friction", false),
            new Property<bool>("Display Warning", true),
            new Property<bool>("Mark Target", true),
            new Property<bool>("Display Rocket Count", true),

            new Property<float>("GuideControl P Factor", 1.25f),
            new Property<float>("GuideControl I Factor", 10f),
            new Property<float>("GuideControl D Factor", 0f),

            new Property<float>("Rocket Smoke Emission Constant", 80f),
            new Property<float>("Rocket Smoke Lifetime", 1f),
            new Property<float>("Rocket Smoke Size", 3.5f),
            new Property<Color>("Rocket Smoke Start Color", new Color(0.1f,0.14f,0.15f,1f)),
            new Property<Color>("Rocket Smoke End Color", new Color(0.1f,0.14f,0.15f,1f)),
            new Property<float>("Rocket Smoke Start Color Time", 0.09f),
            new Property<float>("Rocket Smoke End Color Time", 1f),
            new Property<float>("Rocket Smoke Start Alpha", 1f),
            new Property<float>("Rocket Smoke End Alpha", 1f),
            new Property<float>("Rocket Smoke Start Alpha Time", 0.076f),
            new Property<float>("Rocket Smoke End Alpha Time", 0.26f),

            new Property<int>("Radar Frequency", 5),
        };

        public class Property<T>
        {
            public string Key = "";
            public T Value = default;

            public Property(string key, T value) { Key = key; Value = value; }
            public override string ToString()
            {
                return Key + " - " + Value.ToString();
            }
        }

        public T GetValue<T>(string key)
        {
            T value = default;

            foreach (var pro in Properties)
            {
                if (pro is Property<T>)
                {
                    var _pro = pro as Property<T>;
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

            foreach (var pro in Properties)
            {
                if (pro is Property<T>)
                {
                    var _pro = pro as Property<T>;
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
                Properties.Add(new Property<T>(key, value));
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

            for (int i = 0; i < Properties.Count; i++)
            {
                var value = Properties[i];

                if (value is Property<int>)
                {
                    value = getValue(value as Property<int>);
                }
                else if (value is Property<bool>)
                {
                    value = getValue(value as Property<bool>);
                }
                else if (value is Property<float>)
                {
                    value = getValue(value as Property<float>);
                }
                else if (value is Property<string>)
                {
                    value = getValue(value as Property<string>);
                }
                else if (value is Property<Vector3>)
                {
                    value = getValue(value as Property<Vector3>);
                }
                else if (value is Property<Color>)
                {
                    value = getValue(value as Property<Color>);
                }
                Properties[i] = value;
            }

            if (needWrite) Modding.Configuration.Save();
            Debug.Log("????");
            return config;

            Property<T> getValue<T>(Property<T> propertise)
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

                return new Property<T>(key, defaultValue);
            }
        }
        private static Dictionary<Type, Func<XDataHolder, string, object>> typeSpecialAction = new Dictionary<Type, Func<XDataHolder, string, object>>
        {
            { typeof(int), (xDataHolder,key)=>xDataHolder.ReadInt(key)},
            { typeof(bool), (xDataHolder,key)=>xDataHolder.ReadBool(key)},
            { typeof(float), (xDataHolder,key)=>xDataHolder.ReadFloat(key)},
            { typeof(string), (xDataHolder,key)=>xDataHolder.ReadString(key)},
            { typeof(Color),(xDataHolder,key)=>xDataHolder.ReadColor(key) } ,
            { typeof(Vector3), (xDataHolder,key)=>xDataHolder.ReadVector3(key)},
        };
    }
}
