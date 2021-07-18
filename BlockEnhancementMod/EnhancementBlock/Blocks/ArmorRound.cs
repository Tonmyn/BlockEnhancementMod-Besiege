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
        MToggle onCollisionToggle,oneShotToggle,loopToggle;
        MSlider volumeSlider, pitchSlider,distanceSlider,dopplerSlider, spatialBlendSlider;
        MKey addVolumeKey, reduceVolumeKey;
        MKey playKey, muteKey,stopKey;
        MKey nextKey, lastKey;
        

        AudioSource audioSource;

        //List<string> audioClipNames;

        public override void SafeAwake()
        {
            directoryMenu = AddMenu("Directory", 0, formatList(AssetManager.Instance.AudioClipDic.Keys.ToList()));
            fileMenu = AddMenu("File", 0, formatList(AssetManager.Instance.AudioClipDic["Audio Clips"].ToList(), true));
            
            playKey = AddKey(LanguageManager.Instance.CurrentLanguage.Play, "Play", KeyCode.P);
            stopKey = AddKey(LanguageManager.Instance.CurrentLanguage.Stop, "Stop", KeyCode.C);
            muteKey = AddKey(LanguageManager.Instance.CurrentLanguage.Mute, "Mute", KeyCode.M);
            //nextKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Next", KeyCode.N);
            //lastKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Last", KeyCode.L);

            loopToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.Loop, "Loop", false);
            oneShotToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.OneShot, "One Shot", false);
            //onCollisionToggle = AddToggle(LanguageManager.Instance.CurrentLanguage.OnCollision ,"On Collision", false);

            volumeSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.Volume, "Volume", 1f, 0f, 1f);
            pitchSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.Pitch, "Pitch", 1f, 0f, 5f);
            distanceSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.Distance, "Distance", 5f, 0f, 10f);
            dopplerSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.Doppler, "Doppler", 1f, 0f, 5f);
            spatialBlendSlider = AddSlider(LanguageManager.Instance.CurrentLanguage.SpatialBlend, "Spatial Blend", 1f, 0f, 1f);

            audioSource = transform.gameObject.GetComponent<AudioSource>() ?? transform.gameObject.AddComponent<AudioSource>();
            
#if DEBUG
            ConsoleController.ShowMessage("圆盔甲添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            directoryMenu.DisplayInMapper = fileMenu.DisplayInMapper = value;
            playKey.DisplayInMapper = stopKey.DisplayInMapper = muteKey.DisplayInMapper = value;
            //nextKey.DisplayInMapper = lastKey.DisplayInMapper = value;

            oneShotToggle.DisplayInMapper = !loopToggle.IsActive&& value;
            loopToggle.DisplayInMapper = !oneShotToggle.IsActive && value;

            volumeSlider.DisplayInMapper = pitchSlider.DisplayInMapper = value;
            distanceSlider.DisplayInMapper = dopplerSlider.DisplayInMapper = spatialBlendSlider.DisplayInMapper = value;
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
            audioSource.loop = loopToggle.IsActive;
            audioSource.pitch = pitchSlider.Value;
            audioSource.volume = volumeSlider.Value;
            audioSource.spatialBlend = spatialBlendSlider.Value;
            audioSource.minDistance = distanceSlider.Value;
            audioSource.maxDistance = distanceSlider.Value * 3f;
            //audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.dopplerLevel = dopplerSlider.Value;

          
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (playKey.IsPressed|| playKey.EmulationPressed())
            {
                if (!oneShotToggle.IsActive)
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
                }
                else
                {
                    audioSource.PlayOneShot(audioSource.clip);
                }
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

        void OnCollisionEnter(Collision other)
        {
          //  Debug.Log("??" + other.gameObject.name);
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
