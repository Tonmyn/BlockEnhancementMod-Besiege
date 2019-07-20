using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Common;

namespace BlockEnhancementMod
{
    class RadarScript : MonoBehaviour
    {
        public float radius = 2000f;
        public float safetyRadius = 30f;
        public float searchAngle = 20f;

        public bool Switch { get; set; } = false;
        public SearchModes SearchMode { get; set; } = SearchModes.Auto;
        public Target target { get; private set; }

        public event Action<Target> OnTarget;

        private HashSet<GameObject> checkedGameObject = new HashSet<GameObject>();
        private HashSet<Machine.SimCluster> checkedCluster = new HashSet<Machine.SimCluster>();

        public enum SearchModes
        {
            Auto = 0,
            Manual = 1
        }

        void Awake()
        {
            OnTarget += (value) => { };
        }

        void Start()
        {
            //CreateFrustumCone(searchAngle, safetyRadius, radius);
        }

        void Update()
        {
            if (SearchMode == SearchModes.Manual)
            {
                //prepareTarget();
                OnTarget.Invoke(target);
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (SearchMode != SearchModes.Auto) return;

            PrepareTarget(collider);
            //OnTarget.Invoke(target);
        }

        void PrepareTarget(Collider collider)
        {
            GameObject collidedObject = collider.transform.parent.gameObject;

            //if (checkedGameObject.Contains(collidedObject)) return;
            BlockBehaviour block = collidedObject.GetComponentInParent<BlockBehaviour>();
            if (block == null)
            {
#if DEBUG
                Debug.Log("block null");
#endif
                return;
            }
            else
            {
#if DEBUG
                Debug.Log("Target aquired");
                Debug.Log(collidedObject.name);
#endif
                Transform target = collidedObject.transform;
                gameObject.transform.parent.gameObject.GetComponent<RocketScript>().target = target;
                gameObject.transform.parent.gameObject.GetComponent<RocketScript>().targetCollider = collider;
                gameObject.transform.parent.gameObject.GetComponent<RocketScript>().targetAquired = true;

                DeactivateDetectionZone();
                checkedGameObject.Add(collidedObject);

                Machine.SimCluster cluster = block.ParentMachine.simClusters[block.ClusterIndex];
                if (checkedCluster.Contains(cluster)) return;
                checkedCluster.Add(cluster);
            }
        }

        public void ClearSavedSets()
        {
            checkedGameObject.Clear();
            checkedCluster.Clear();
        }

        public void ActivateDetectionZone()
        {
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
#if DEBUG
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
#endif

            if (collider != null)
            {
                collider.enabled = true;
#if DEBUG
                renderer.enabled = true;
#endif
            }
        }

        public void DeactivateDetectionZone()
        {
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
#if DEBUG
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
#endif

            if (collider != null)
            {
                collider.enabled = false;
#if DEBUG
                renderer.enabled = false;
#endif
            }
        }

        public void ChangeSearchMode()
        {
            if (SearchMode == SearchModes.Auto)
            {
                SearchMode = SearchModes.Manual;
                //do something...
            }
            else
            {
                SearchMode = SearchModes.Auto;
                //do something...
            }
            ClearSavedSets();
        }

        public void CreateFrustumCone(float angle, float topRadius, float bottomRadius)
        {
            float topHeight = topRadius;
            float height = bottomRadius - topHeight;

            float radiusTop = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * topHeight;
            float radiusBottom = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * bottomRadius;

            //越高越精细
            int numVertices = 5;

            Vector3 myTopCenter = Vector3.up * topHeight;
            Vector3 myBottomCenter = myTopCenter + Vector3.up * height;
            //构建顶点数组和UV数组
            Vector3[] vertices = new Vector3[numVertices * 2 + 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            //顶圆中心点放第一个，底圆中心点放最后一个
            vertices[0] = myTopCenter;
            vertices[vertices.Length - 1] = myBottomCenter;

            for (int i = 0; i < numVertices; i++)
            {
                float angleStep = 2 * Mathf.PI * i / numVertices;
                float angleSin = Mathf.Sin(angleStep);
                float angleCos = Mathf.Cos(angleStep);

                vertices[i + 1] = new Vector3(radiusTop * angleCos, myTopCenter.magnitude, radiusTop * angleSin);
                vertices[i + 1 + numVertices] = new Vector3(radiusBottom * angleCos, myBottomCenter.magnitude, radiusBottom * angleSin);

                uvs[i] = new Vector2(1.0f * i / numVertices, 1);
                uvs[i + numVertices] = new Vector2(1.0f * i / numVertices, 0);
            }

            int[] tris = new int[numVertices * 6 + numVertices * 6];
            int index = 0;
            //画下圆面
            for (int i = 0; i < numVertices; i++)
            {
                tris[index++] = 0;
                tris[index++] = i + 1;
                tris[index++] = (i == numVertices - 1) ? 1 : i + 2;
            }
            //画斜面
            for (int i = 0; i < numVertices; i++)
            {
                int ip1 = i + 2;
                if (ip1 > numVertices)
                    ip1 = 1;

                tris[index++] = i + 1;
                tris[index++] = i + 1 + numVertices;
                tris[index++] = ip1 + numVertices;

                tris[index++] = i + 1;
                tris[index++] = ip1 + numVertices;
                tris[index++] = ip1;
            }
            //画上圆面
            for (int i = 0; i < numVertices; i++)
            {
                tris[index++] = 2 * numVertices + 1;
                tris[index++] = (i == numVertices - 1) ? numVertices + 1 : numVertices + i + 2;
                tris[index++] = numVertices + i + 1;
            }

            var mf = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            mf.mesh.vertices = vertices;
            mf.mesh.triangles = tris;
            mf.mesh.uv = uvs;
            mf.mesh.RecalculateBounds();
            mf.mesh.RecalculateNormals();

            var mc = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = GetComponent<MeshFilter>().mesh;
            mc.convex = true;
            mc.isTrigger = true;
#if DEBUG
            var mr = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            mr.material.color = Color.green;
#endif
        }
    }

    class Target
    {

    }
}
