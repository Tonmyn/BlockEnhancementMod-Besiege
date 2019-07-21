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
        private int radarLayer = 1;
        public float radius = 2000f;
        public float safetyRadius = 30f;
        public float searchAngle = 20f;

        MeshCollider Collider;

        public bool Switch { get; set; } = false;
        public SearchModes SearchMode { get; set; } = SearchModes.Auto;
        public Target target { get; private set; }

        public event Action<Target> OnTarget;

        private HashSet<GameObject> checkedGameObject = new HashSet<GameObject>();
        private HashSet<Machine.SimCluster> checkedCluster = new HashSet<Machine.SimCluster>();

        #region Networking Setting

        public bool receivedRayFromClient = false;
        public Ray rayFromClient;

        #endregion

        public enum SearchModes
        {
            Auto = 0,
            Manual = 1
        }

        void Awake()
        {
            OnTarget += (value) => { };
            gameObject.layer = radarLayer;
        }

        void Start()
        {
            //CreateFrustumCone(searchAngle, safetyRadius, radius);
        }

        void Update()
        {
            if (SearchMode == SearchModes.Manual)
            {
                PrepareTarget(getTarget());
                OnTarget.Invoke(target);
            }

            Collider getTarget()
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (StatMaster.isClient)
                {
                    SendRayToHost(ray);
                }
                else
                {
                    //Find targets in the manual search mode by casting a sphere along the ray
                    float manualSearchRadius = 1.25f;
                    RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity);
                    Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit);
                    if (hits.Length > 0)
                    {
                        for (int i = 0; i < hits.Length; i++)
                        {
                            if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                            {
                                if ((hits[i].transform.position - transform.position).magnitude >= /*safetyRadiusManual*/safetyRadius)
                                {
                                    target.transform = hits[i].transform;
                                    target.collider = target.transform.GetComponentInChildren<Collider>(true);
                                    target.initialCJOrHJ = target.transform.GetComponent<ConfigurableJoint>() != null || target.transform.GetComponent<HingeJoint>() != null;
                                    //previousVelocity = acceleration = Vector3.zero;
                                    target.acceleration = 0;
                                    //initialDistance = (hits[i].transform.position - transform.position).magnitude;
                                    //targetAquired = true;
                                    break;
                                }
                            }
                        }
                        if (target == null)
                        {
                            for (int i = 0; i < hits.Length; i++)
                            {
                                if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
                                {
                                    if ((hits[i].transform.position - transform.position).magnitude >= /*safetyRadiusManual*/safetyRadius)
                                    {
                                        target.transform = hits[i].transform;
                                        target.collider = target.transform.GetComponentInChildren<Collider>(true);
                                        target.initialCJOrHJ = false;
                                        //previousVelocity = acceleration = Vector3.zero;
                                        //initialDistance = (hits[i].transform.position - rocket.transform.position).magnitude;
                                        //targetAquired = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (target == null && rayHit.transform != null)
                    {
                        if ((rayHit.transform.position - transform.position).magnitude >= /*safetyRadiusManual*/safetyRadius)
                        {
                            target.transform = rayHit.transform;
                            target.collider = target.transform.GetComponentInChildren<Collider>(true);
                            target.initialCJOrHJ = target.transform.GetComponent<ConfigurableJoint>() != null || target.transform.GetComponent<HingeJoint>() != null;
                            //previousVelocity = acceleration = Vector3.zero;
                            //initialDistance = (rayHit.transform.position - rocket.transform.position).magnitude;
                            //targetAquired = true;
                        }

                    }
                    if (receivedRayFromClient)
                    {
                        SendTargetToClient();
                    }
                    receivedRayFromClient = false;
                }

                return target.collider;
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (SearchMode != SearchModes.Auto) return;
            if (collider.gameObject.layer == radarLayer) return;

            if (target == null)
            {
                PrepareTarget(collider);
                OnTarget.Invoke(target);

            }
        }

        void PrepareTarget(Collider collider)
        {

            GameObject collidedObject = collider.transform.parent.gameObject;

            //if (checkedGameObject.Contains(collidedObject)) return;
            BlockBehaviour block = collidedObject.GetComponentInParent<BlockBehaviour>();
            if (block == null)
            {
#if DEBUG
                //Debug.Log("block null");
#endif
                return;
            }
            else
            {
                //Machine.SimCluster cluster = block.ParentMachine.simClusters[block.ClusterIndex];
                //if (checkedCluster.Contains(cluster)) return;
                //checkedCluster.Add(cluster);
#if DEBUG
                Debug.Log("Target aquired");
                Debug.Log(collidedObject.name);
                Debug.Log(collider.transform.gameObject.layer);
#endif
                //Transform target = collidedObject.transform;
                //gameObject.transform.parent.gameObject.GetComponent<RocketScript>().target = target;
                //gameObject.transform.parent.gameObject.GetComponent<RocketScript>().targetCollider = collider;
                //gameObject.transform.parent.gameObject.GetComponent<RocketScript>().targetAquired = true;

                DeactivateDetectionZone();
                checkedGameObject.Add(collidedObject);
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

            //if (collider == null)
            //{
            collider.enabled = true;
#if DEBUG
            renderer.enabled = true;
#endif
            //}
        }

        public void DeactivateDetectionZone()
        {
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
#if DEBUG
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
#endif

            //if (/*collider*/ != null)
            //{
            collider.enabled = false;
#if DEBUG
            renderer.enabled = false;
#endif
            //}
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
            int numVertices = 5 + 5;

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
            Collider = mc;
#if DEBUG
            var mr = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            mr.material.color = Color.green;
#endif
        }

        #region Networking Method
        private void SendRayToHost(Ray ray)
        {
            Message rayToHostMsg = Messages.rocketRayToHostMsg.CreateMessage(ray.origin, ray.direction, /*BB*/transform.parent.GetComponent<BlockBehaviour>());
            ModNetworking.SendToHost(rayToHostMsg);
        }

        public void SendTargetToClient()
        {
            if (StatMaster.isHosting)
            {
                if (target != null)
                {
                    var rocket = gameObject.GetComponent<TimedRocket>();
                    var BB = transform.parent.GetComponent<BlockBehaviour>();

                    if (target.transform.GetComponent<BlockBehaviour>())
                    {
                        BlockBehaviour targetBB = target.transform.GetComponent<BlockBehaviour>();
                        int id = targetBB.ParentMachine.PlayerID;
                        if (rocket.ParentMachine.PlayerID != 0)
                        {
                            Message targetBlockBehaviourMsg = Messages.rocketTargetBlockBehaviourMsg.CreateMessage(targetBB, BB);
                            foreach (var player in Player.GetAllPlayers())
                            {
                                if (player.NetworkId == rocket.ParentMachine.PlayerID)
                                {
                                    ModNetworking.SendTo(player, targetBlockBehaviourMsg);
                                }
                            }
                        }
                        ModNetworking.SendToAll(Messages.rocketLockOnMeMsg.CreateMessage(BB, id));
                        RocketsController.Instance.UpdateRocketTarget(BB, id);
                    }
                    if (target.transform.GetComponent<LevelEntity>())
                    {
                        Message targetEntityMsg = Messages.rocketTargetEntityMsg.CreateMessage(target.transform.GetComponent<LevelEntity>(), BB);
                        foreach (var player in Player.GetAllPlayers())
                        {
                            if (player.NetworkId == rocket.ParentMachine.PlayerID)
                            {
                                ModNetworking.SendTo(player, targetEntityMsg);
                            }
                        }
                        ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(BB));
                        RocketsController.Instance.RemoveRocketTarget(BB);
                    }
                }
            }
        }

        public void SendClientTargetNull()
        {
            BlockBehaviour timedRocket = transform.parent.gameObject.GetComponent<BlockBehaviour>();
            if (StatMaster.isHosting)
            {
                Message rocketTargetNullMsg = Messages.rocketTargetNullMsg.CreateMessage(timedRocket);
                ModNetworking.SendTo(Player.GetAllPlayers().Find(player => player.NetworkId == timedRocket.ParentMachine.PlayerID), rocketTargetNullMsg);
                ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(timedRocket));
            }
            RocketsController.Instance.RemoveRocketTarget(timedRocket);
        }
        #endregion
    }

    class Target/*:Transform*/
    {

        public Transform transform;
        public Collider collider;
        public bool initialCJOrHJ = false;

        public float acceleration = 0;

    }
}
