using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;

namespace BlockEnhancementMod
{
    public class AssetManager:SingleInstance<AssetManager>
    {

        public override string Name { get; } = "Asset Manager";

        public bool Data = true;

        public Dictionary<string, List<string>> AudioClipDic = new Dictionary<string, List<string>>();

        public Action OnReload;

        void Awake()
        {
            if (!ModIO.ExistsDirectory("Audio Clips", Data))
            {
                ModIO.CreateDirectory("Audio Clips", Data);
            }
            //Debug.Log(getExtention("asdasd.mp3"));
            AudioClipDic = readAudioClips();
            
        }

        private Dictionary<string,List<string>> readAudioClips()
        {
            var audioClipDic = new Dictionary<string, List<string>>();

            var extentionType = new List<string>() { /*".mp3",*/ ".ogg", ".wav"/*, ".txt" */};

            var dirs = ModIO.GetDirectories(@"Audio Clips", Data).ToList();
            dirs.Insert(0, @"Audio Clips");
   
            foreach (var dir in dirs)
            {
                var files = new List<string>();
                foreach (var file in ModIO.GetFiles(dir, Data))
                {
                    if (extentionType.Contains(PathHelper.GetExtension(file)))
                    {
                        files.Add(file);
                    }
                }
               audioClipDic.Add(dir, files);
            }
            return audioClipDic;
        }

        public ModAudioClip LoadModAudioClip(string name,string path,bool data = false)
        {
            var ac = ModResource.CreateAudioClipResource(name, path, data);
            try
            {
                return ac;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return ac;
            }
        }

        //private string getExtention(string path,bool data = false)
        //{

        //    try
        //    {
        //        return PathHelper.GetExtension(path);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log(e.Message);
        //        return "123";
        //    }


        //}

    }
}
