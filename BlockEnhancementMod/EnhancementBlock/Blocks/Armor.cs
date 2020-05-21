using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockEnhancementMod
{
    class ArmorScript : EnhancementBlock
    {
        Camera c;

        GameObject go;

        GameObject screen;
        public override void SafeAwake()
        {




#if DEBUG
            ConsoleController.ShowMessage("盔甲添加进阶属性");
#endif
        }

        MeshRenderer mr;
        RenderTexture rt;
        FixedCameraController fcc;
        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();
            Debug.Log("??");


            screen = GameObject.CreatePrimitive(PrimitiveType.Plane);
            screen.name = "Screen";
            Destroy(screen.GetComponent<MeshCollider>());
            screen.transform.SetParent(transform);
            screen.transform.position = transform.position;
            screen.transform.rotation = transform.rotation;
            screen.transform.localPosition = Vector3.forward * 0.25f;
            screen.transform.localEulerAngles = new Vector3(90, 0, 0);
            screen.transform.localScale = Vector3.one * 0.07f;


            go = new GameObject("camera");
            go.transform.SetParent(transform);
            c = go.AddComponent<Camera>();
            c.CopyFrom(Camera.main);

            fcc = GameObject.FindObjectOfType<FixedCameraController>();
            mr =screen.transform.GetComponent<MeshRenderer>();
            mr.material.shader = Shader.Find("Diffuse");
            rt = new RenderTexture(800, 800,0);

            c.targetTexture = rt;

            if (fcc != null)
            {
                var target = fcc.cameras[0];
                var tran = target.CompositeTracker2;
                go.transform.SetParent(target.CompositeTracker);
                go.transform.position = tran.transform.position;
                go.transform.rotation = tran.transform.rotation;
                go.transform.localEulerAngles = tran.localEulerAngles;

            }

            if (mr != null)
            {
                mr.material.mainTexture = rt;
            }
        }
        Texture2D screenShot;
        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            base.SimulateUpdateAlways_EnhancementEnable();

            //if (fcc != null)
            //{
            //    Debug.Log(fcc.cameras[0]. targetPosition);
            //}



            //RenderTexture.active = rt;
            //screenShot = new Texture2D((int)800, (int)800, TextureFormat.RGB24, false);
            //screenShot.ReadPixels(new Rect(100, 100, 800, 800), 0, 0);
            //screenShot.Apply();

            //Camera.main.targetTexture = null;
            //RenderTexture.active = null;

            //Destroy(rt);




            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("...");

                Debug.Log(Camera.allCamerasCount);
            }
        }

        public override void SimulateLateUpdate_EnhancementEnabled()
        {
            base.SimulateLateUpdate_EnhancementEnabled();






        }

    }
}
