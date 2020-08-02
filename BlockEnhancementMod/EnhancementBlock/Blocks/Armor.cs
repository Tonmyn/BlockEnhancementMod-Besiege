using Modding;
using Modding.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockEnhancementMod
{
    class ArmorScript : EnhancementBlock
    {
        Camera watchCamera;
        GameObject cameraObject;
        GameObject screenObject;
        int channelIndex=0;
        List<string> channelList = new List<string> { "-1"};

        MeshRenderer mr;
        RenderTexture rt;
        FixedCameraController fcc;

        MMenu channelMenu;
        MValue widthPixelValue;
        MValue heightPixelValue;
        MKey changeChannelKey;
        MKey switchKey;

        public override void SafeAwake()
        {
            changeChannelKey = AddKey(LanguageManager.Instance.CurrentLanguage.ChangeChannel, "Change Channel", KeyCode.C);
            switchKey = AddKey(LanguageManager.Instance.CurrentLanguage.Switch, "Switch", KeyCode.E);

            channelMenu = AddMenu("Channel Menu", 0, channelList);
            widthPixelValue = AddValue(LanguageManager.Instance.CurrentLanguage.WidthPixel, "Width", 800f);
            heightPixelValue = AddValue(LanguageManager.Instance.CurrentLanguage.HeightPixel, "Height", 800f);

            StartCoroutine(initChannelInBuilding());

            Events.OnBlockPlaced += RefreshCameraChannelList;
            Events.OnBlockRemoved += RefreshCameraChannelList;



#if DEBUG
            ConsoleController.ShowMessage("盔甲添加进阶属性");
#endif
        }
        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            channelMenu.DisplayInMapper = value;
            changeChannelKey.DisplayInMapper = value;
            widthPixelValue.DisplayInMapper = value;
            heightPixelValue.DisplayInMapper = value;
            switchKey.DisplayInMapper = value;
        }
        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();
            channelList = BB.BuildingBlock.GetComponent<ArmorScript>().channelList;
            channelIndex = BB.BuildingBlock.GetComponent<ArmorScript>().channelIndex;

            fcc = GameObject.FindObjectOfType<FixedCameraController>();

            if (channelIndex < 0 || fcc == null) return;

            rt = new RenderTexture(Mathf.Clamp((int)widthPixelValue.Value, 0, 1920), Mathf.Clamp((int)heightPixelValue.Value, 0, 1080), 0);

            cameraObject = new GameObject("WatchCamera");
            cameraObject.transform.SetParent(transform);
            watchCamera = cameraObject.AddComponent<Camera>();
            watchCamera.CopyFrom(Camera.main);
            watchCamera.targetTexture = rt;

            screenObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            screenObject.name = "Screen";
            Destroy(screenObject.GetComponent<MeshCollider>());
            screenObject.transform.SetParent(transform);
            screenObject.transform.position = transform.position;
            screenObject.transform.rotation = transform.rotation;
            screenObject.transform.localPosition = Vector3.forward * 0.25f;
            screenObject.transform.localEulerAngles = new Vector3(90, 0, 0);
            screenObject.transform.localScale = Vector3.one * 0.07f;
            mr = screenObject.transform.GetComponent<MeshRenderer>();
            mr.material.shader = Shader.Find("Particles/Alpha Blended");
            mr.material.mainTexture = rt;
            mr.sortingOrder =50;

            stickToCamera(channelIndex);

        }
        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();

            if (channelIndex < 0) return;

            if (changeChannelKey.IsPressed|| changeChannelKey.EmulationPressed())
            {
                if (++channelIndex >channelList.Count - 1)
                {
                    channelIndex = 0;
                }
                stickToCamera(channelIndex);
            }

            if (switchKey.IsPressed || switchKey.EmulationPressed())
            {
                mr.enabled = !mr.enabled;
            }
        }
        private void stickToCamera(int index)
        {
            if (fcc != null)
            {
                index = Mathf.Clamp(index, 0, fcc.cameras.Count - 1);

                var target = fcc.cameras[index];
                var tran = target.CompoundTracker;
                cameraObject.transform.SetParent(target.CompositeTracker);
                cameraObject.transform.position = tran.transform.position;
                cameraObject.transform.rotation = tran.transform.rotation;
                cameraObject.transform.eulerAngles = tran.eulerAngles;

            }
        }
        public void RefreshCameraChannelList(Block  block)
        {
            try
            {
                StartCoroutine(waitThreeFrame());
            }
            catch { }

            IEnumerator waitThreeFrame()
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return 0;
                }

                refreshChannelList();
            }
            void refreshChannelList()
            {
                if (!Machine.Active().isSimulating)
                {
                    channelList = new List<string> { };
                    int index = -1;

                    foreach (var bb in Machine.Active().BuildingBlocks)
                    {
                        if (bb.name == "CameraBlock")
                        {
                            index++;
                            channelList.Add(index.ToString());
                        }
                    }

                    if (index < 0)
                    {
                        channelList = LanguageManager.Instance.CurrentLanguage.NullChannelList;
                        channelIndex = -1;
                    }
                    else
                    {
                        if (channelIndex < 0)
                        {
                            channelIndex = 0;
                        }
                    }

                    channelMenu.Items = channelList;

                    if ((channelIndex > channelList.Count + 1))
                    {
                        channelIndex = channelMenu.Value = 0;
                    }
                    else if (channelIndex < 0)
                    {
                        channelMenu.Value = 0;
                    }
                    else
                    {
                        channelMenu.Value = channelIndex;
                    }
                }
            }
        }

        private IEnumerator initChannelInBuilding()
        {
            if (!Machine.Active().isSimulating)
            {
                int defaultChannelIndex = 0;
                try
                {
                    defaultChannelIndex = EnhancementBlockController.Instance.PMI.Blocks.ToList().Find(match => match.Guid == BB.Guid).Data.ReadInt("bmt-Channel Menu");
                }
                catch
                { 
                
                }

                for (int i = 0; i < 10; i++)
                {
                    yield return 0;
                }
                RefreshCameraChannelList(null);
                if (channelList != null)
                {
                    if (defaultChannelIndex < channelList.Count)
                    {
                        channelMenu.Value = channelIndex = defaultChannelIndex;
                    }
                }
                else
                {
                    channelList = LanguageManager.Instance.CurrentLanguage.NullChannelList;
                    channelMenu.Value = channelIndex = 0;
                }
       
                yield break;
            }
            yield break;
        }
    }
}
