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
        public event Action OnReread;

        void Awake()
        { 
            RereadAudioClipAsset();
        }

        private Dictionary<string,List<string>> readAudioClips()
        {
            var audioClipDic = new Dictionary<string, List<string>>();
            var extentionType = new List<string>() { /*".mp3",*/ ".ogg", ".wav"/*, ".txt" */};
            var dirs = Modding.ModIO.GetDirectories(@"Audio Clips", Data).ToList();
            dirs.Insert(0, @"Audio Clips");
   
            foreach (var dir in dirs)
            {
                var files = new List<string>();
                foreach (var file in Modding.ModIO.GetFiles(dir, Data))
                {
                    if (extentionType.Contains(PathHelper.GetExtension(file)))
                    {
                        files.Add(file);
                    }
                }
                if (files.Count != 0)
                {
                    audioClipDic.Add(dir, files);
                }
            }

            ExtensionMethods.ShowMessageWithColor("Audio Clip Asset Read Complete", Color.green);
            return audioClipDic;
        }

        public ModAudioClip LoadModAudioClip(string name,string path, bool data = false)
        {
            try
            {
                var ac = ModResource.CreateAudioClipResource(name, path, data);
                return ac;
            }
            catch (Exception e)
            {
                ExtensionMethods.ShowMessageWithColor(e.Message, Color.red);
                return null;
            }
        }

        public void RereadAudioClipAsset()
        {
            if (!Modding.ModIO.ExistsDirectory("Audio Clips", Data))
            {
                Modding.ModIO.CreateDirectory("Audio Clips", Data);
            }

            AudioClipDic = readAudioClips();

            OnReread?.Invoke();
        }
    }
}
