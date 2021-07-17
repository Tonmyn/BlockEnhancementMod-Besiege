using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Modding;
using Modding.Blocks;
using UnityEngine;

namespace BlockEnhancementMod
{
    class ArmorRoundScript : EnhancementBlock
    {
        MMenu directoryMenu, fileMenu;
        MToggle collisionToggle,oneShotToggle,loopToggle;
        MSlider volumeSlider, pitchSlider,distanceSlider,dopplerSlider;
        MKey addVolumeKey, reduceVolumeKey;
        MKey playKey, muteKey,stopKey;
        MKey nextKey, lastKey;

        AudioSource audioSource;

        //List<string> audioClipNames;

        public override void SafeAwake()
        {
            directoryMenu = AddMenu("Directory", 0, formatList(AssetManager.Instance.AudioClipDic.Keys.ToList()));
            fileMenu = AddMenu("File", 0, formatList(AssetManager.Instance.AudioClipDic["Audio Clips"].ToList(), true));
            
            playKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Play", KeyCode.P);
            stopKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Stop", KeyCode.C);
            muteKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Mute", KeyCode.M);
            //nextKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Next", KeyCode.N);
            //lastKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Last", KeyCode.L);

            volumeSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.AddSpeed, "Volume", 1f, 0f, 5f);
            //channelMenu = AddMenu("Channel Menu", 0, channelList);
            //widthPixelValue = AddValue(LanguageManager.Instance.CurrentLanguage.WidthPixel, "Width", 800f);
            //heightPixelValue = AddValue(LanguageManager.Instance.CurrentLanguage.HeightPixel, "Height", 800f);

            audioSource = transform.gameObject.AddComponent<AudioSource>();
          
#if DEBUG
            ConsoleController.ShowMessage("圆盔甲添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            directoryMenu.DisplayInMapper = fileMenu.DisplayInMapper = value;
            playKey.DisplayInMapper = stopKey.DisplayInMapper = muteKey.DisplayInMapper = value;
            //nextKey.DisplayInMapper = lastKey.DisplayInMapper = value;
            volumeSlider.DisplayInMapper = value;
        }

        public override void ChangedProperties(MapperType mapper)
        {
            if (mapper.Key == directoryMenu.Key)
            {
                var key = AssetManager.Instance.AudioClipDic.Keys.ToList()[directoryMenu.Value];
                var list = AssetManager.Instance.AudioClipDic[key];

                fileMenu.Items = formatList(list, true, key);
                fileMenu.Value = 0;
                fileMenu.DisplayInMapper = false;
                fileMenu.DisplayInMapper = true;
            }
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            //audioClipNames = new List<string>();
           var ac = loadAudioClip(directoryMenu.Value, fileMenu.Value);
            ac.OnLoad += () => { audioSource.clip = ac; };
            //audioSource.clip = ModResource.GetAudioClip(audioClipNames[0]);
            audioSource.loop = false;
            audioSource.pitch = 1;
            audioSource.volume = volumeSlider.Value;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 10f;
            audioSource.maxDistance = 11f;
            //audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.dopplerLevel = 5;
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (playKey.IsPressed|| playKey.EmulationPressed())
            {
                if (audioSource.time == 0 && !audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                else if (audioSource.time > 0 && audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
                else if (audioSource.time > 0 && !audioSource.isPlaying)
                {
                    audioSource.UnPause();
                }
                //if (audioSource.isPlaying)
                //{
                //    audioSource.Pause();
                //}
                //else
                //{

                //    audioSource.PlayOneShot(audioSource.clip);
                //}

            }

            if (stopKey.IsPressed || stopKey.EmulationPressed())
            {
                audioSource.Stop();
            }

            if (muteKey.IsPressed || muteKey.EmulationPressed())
            {
                audioSource.mute = !audioSource.mute;
            }


        }

        private ModAudioClip loadAudioClip(int index_directoryMenu,int index_fileMenu)
        {
            var key = AssetManager.Instance.AudioClipDic.Keys.ToList()[index_directoryMenu];
            var list = AssetManager.Instance.AudioClipDic[key];
            var path = list[index_fileMenu];
            var name = fileMenu.Items[index_fileMenu] + ExtensionMethods.GetRandomString();
            //audioClipNames.Add(name);

            return AssetManager.Instance.LoadModAudioClip(name, path, true);
        }

        //cut "audio clips/" in list
        private List<string> formatList(List<string> list, bool extention = false, string path = @"Audio Clips/")
        {
            var strs = new List<string>();
            foreach (var str in list)
            {
                var _str = Regex.Replace(str, path, "");
                if (extention) _str = _str.Substring(1, _str.Length - 4);
                strs.Add(_str);
            }
            return strs;
        }
    }
}
