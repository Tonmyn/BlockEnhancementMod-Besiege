using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace BlockEnhancementMod
{
    //[Obsolete]
    //class LanguageManager : SingleInstance<LanguageManager>
    //{
    //    public override string Name { get; } = "Language Manager";

    //    public enum Language
    //    {
    //        Chinese = 0,
    //        English = 1
    //    }

    //    public Language currentLanguage = Language.Chinese;

    //    public Dictionary<int, string> languageDic;

    //    [Obsolete]
    //    public Language GetLanguage()
    //    {
    //        Language L = new Language();

    //        string configPath = Application.dataPath + "/Config.xml";

    //        XmlDocument xDoc = new XmlDocument();
            
    //        xDoc.Load(configPath);    //加载Xml文件,全路径    

    //        XmlNode xmlNode = xDoc.GetElementsByTagName("Language")[0];

    //        string language = xmlNode.InnerText;

    //        return L;
    //    }



    //}
}
