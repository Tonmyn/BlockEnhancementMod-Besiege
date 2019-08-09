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
        public static int CollisionLayer = 10;

        public float radius = 2000f;
        public float safetyRadius = 1f;
        public float searchAngle = 20f;
        public MeshCollider meshCollider;
        public static bool MarkTarget { get; internal set; } = true;
        private Texture2D redSquareAim;

        public bool Switch { get; set; } = false;
        bool lastSwitchState = false;
        public SearchModes SearchMode { get; set; } = SearchModes.Auto;
        public Target target { get; private set; }

        public event Action<Target> OnTarget;

        private HashSet<Target> checkedTarget = new HashSet<Target>();

        public bool receivedRayFromClient = false;
        public Ray rayFromClient;

        public enum SearchModes
        {
            Auto = 0,
            Manual = 1
        }

        void Awake()
        {
            OnTarget += (value) => { };
            gameObject.layer = CollisionLayer;

            //Load aim pic
            redSquareAim = new Texture2D(16, 16);
            redSquareAim.LoadImage(ModIO.ReadAllBytes(@"Resources/Square-Red.png"));
        }

        void FixedUpdate()
        {
            if (target != null)
            {
                target.positionDiff = target.transform.position - transform.position;
#if DEBUG
                Debug.Log(target.positionDiff.magnitude);
#endif
            }
        }

        void Update()
        {
            if (lastSwitchState != Switch)
            {
                lastSwitchState = Switch;
                if (Switch)
                {
                    ActivateDetectionZone();
                }
                else
                {
                    DeactivateDetectionZone();
                }
            }

            if (Switch)
            {
                if (SearchMode == SearchModes.Manual)
                {
                    target = PrepareTarget(GetTarget());
                    OnTarget.Invoke(target);
                }
            }

            if (target == null && checkedTarget.Count > 0)
            {
                float aimDistance = 0f;
                float tempAimdistance = Mathf.Infinity;
                Target dummyTarget = new Target
                {
                    warningLevel = Target.WarningLevel.nullValue
                };
                foreach (var tempTarget in checkedTarget)
                {
                    if (tempTarget.warningLevel > dummyTarget.warningLevel)
                    {
                        dummyTarget = tempTarget;
                        tempAimdistance = Vector3.Distance(tempTarget.transform.position, transform.position);
                    }
                    else if (tempTarget.warningLevel == dummyTarget.warningLevel)
                    {
                        aimDistance = Vector3.Distance(tempTarget.transform.position, transform.position);
                        if (aimDistance < tempAimdistance)
                        {
                            dummyTarget = tempTarget;
                            tempAimdistance = aimDistance;
                        }
                    }
                }
                if (dummyTarget != null)
                {
                    target = dummyTarget;
                    OnTarget.Invoke(target);
                }
            }

            //--------------------------------------------------//
            Collider GetTarget()
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
                    RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity, Game.BlockEntityLayerMask);
                    Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit, Game.BlockEntityLayerMask);
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

            if (target == null)
            {
                target = PrepareTarget(collider);
                if (target == null) return;
                OnTarget.Invoke(target);
                checkedTarget.Add(target);
            }
            else
            {
                Target tempTarget = PrepareTarget(collider);
                if (tempTarget == null) return;

                checkedTarget.Add(target);

                if (tempTarget.warningLevel > target.warningLevel)
                {
                    target = tempTarget;
                    OnTarget.Invoke(target);

                }
                else if (tempTarget.warningLevel == target.warningLevel)
                {
                    float aimDistance = Vector3.Distance(tempTarget.transform.position, transform.position);
                    float targetDistance = Vector3.Distance(target.transform.position, transform.position);
                    if (targetDistance > aimDistance)
                    {
                        target = tempTarget;
                        OnTarget.Invoke(target);
                    }
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (SearchMode != SearchModes.Auto) return;

            Target tempTarget = PrepareTarget(collider);
            if (tempTarget == null) return;

            checkedTarget.Remove(tempTarget);

            if (tempTarget == target)
            {
                SendClientTargetNull();
            }
        }

        Target PrepareTarget(Collider collider)
        {
            BlockBehaviour block = collider.gameObject.GetComponentInParent<BlockBehaviour>();

            if (block == null)
            {
                return null;
            }
            else
            {
                FireTag fireTag = collider.gameObject.GetComponentInParent<FireTag>();
                if (fireTag != null)
                {
                    if (fireTag.burning || fireTag.hasBeenBurned)
                    {
                        return null;
                    }
                }

                Target tempTarget = new Target
                {
                    collider = collider,
                    transform = collider.gameObject.transform
                };
                tempTarget.SetTargetWarningLevel();

                return tempTarget;
            }
        }

        public void ClearSavedSets()
        {
            checkedTarget.Clear();
        }

        public void ActivateDetectionZone()
        {
            ClearSavedSets();
#if DEBUG
            Debug.Log("Detection zone activated");
#endif
            // Enable collider
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
            collider.enabled = true;

#if DEBUG
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.enabled = true;
#endif
        }

        public void DeactivateDetectionZone()
        {
#if DEBUG
            Debug.Log("Detection zone deactivated");
#endif
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
            collider.enabled = false;

#if DEBUG
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.enabled = false;
#endif
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
            SendClientTargetNull();

        }

        public void CreateFrustumCone(float angle, float bottomRadius)
        {
            float topHeight = safetyRadius;
            float height = bottomRadius - topHeight;

            float radiusTop = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * topHeight;
            float radiusBottom = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * bottomRadius;

            //越高越精细
            int numVertices = 5 + 10;

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
            meshCollider = mc;
            meshCollider.enabled = false;
#if DEBUG
            var mr = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Transparent/Diffuse"));
            material.color = new Color(0, 1, 0, 0.1f);
            mr.material = material;
            mr.enabled = false;
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

                    if (target.transform.transform.GetComponent<BlockBehaviour>())
                    {
                        BlockBehaviour targetBB = target.transform.transform.GetComponent<BlockBehaviour>();
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
                    if (target.transform.transform.GetComponent<LevelEntity>())
                    {
                        Message targetEntityMsg = Messages.rocketTargetEntityMsg.CreateMessage(target.transform.transform.GetComponent<LevelEntity>(), BB);
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
            Switch = true;
            target = null;
            ClearSavedSets();

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

        #region Markers
        private void OnGUI()
        {
            if (StatMaster.isMP && StatMaster.isHosting)
            {
                if (transform.parent.gameObject.GetComponent<BlockBehaviour>().ParentMachine.PlayerID != 0)
                {
                    return;
                }
            }
            DrawTargetRedSquare();
        }

        private void DrawTargetRedSquare()
        {
            if (MarkTarget)
            {
                if (target != null)
                {
                    Vector3 markerPosition = target.collider.bounds != null ? target.collider.bounds.center : target.transform.transform.position;
                    if (Vector3.Dot(Camera.main.transform.forward, markerPosition - Camera.main.transform.position) > 0)
                    {
                        int squareWidth = 16;
                        Vector3 itemScreenPosition = Camera.main.WorldToScreenPoint(markerPosition);
                        GUI.DrawTexture(new Rect(itemScreenPosition.x - squareWidth / 2, Camera.main.pixelHeight - itemScreenPosition.y - squareWidth / 2, squareWidth, squareWidth), redSquareAim);
                    }
                }
            }
        }
        #endregion
    }

    class Target
    {
        //Initialise transform will cause NRE
        public Transform transform;
        public Collider collider;
        public bool initialCJOrHJ = false;
        public float initialDistance = 0f;
        public Vector3 positionDiff = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        public Vector3 acceleration = Vector3.zero;

        public WarningLevel warningLevel = 0;

        public enum WarningLevel
        {
            normalBlockValue = 0,
            bombValue = 32,
            guidedRocketValue = 1024,
            waterCannonValue = 16,
            flyingBlockValue = 2,
            flameThrowerValue = 8,
            cogMotorValue = 2,
            nullValue = -1
        }

        public void SetTargetWarningLevel()
        {
            GameObject collidedObject = collider.transform.parent.gameObject;
            BlockBehaviour block = collidedObject.GetComponentInParent<BlockBehaviour>();
            if (block != null)
            {
                switch (block.BlockID)
                {
                    default:
                        warningLevel = WarningLevel.normalBlockValue;
                        break;
                    case (int)BlockType.Rocket:
                        warningLevel = WarningLevel.guidedRocketValue;
                        break;
                    case (int)BlockType.Bomb:
                        warningLevel = WarningLevel.bombValue;
                        break;
                    case (int)BlockType.WaterCannon:
                        warningLevel = WarningLevel.waterCannonValue;
                        break;
                    case (int)BlockType.FlyingBlock:
                        warningLevel = WarningLevel.flyingBlockValue;
                        break;
                    case (int)BlockType.Flamethrower:
                        warningLevel = WarningLevel.flameThrowerValue;
                        break;
                    case (int)BlockType.CogMediumPowered:
                        warningLevel = WarningLevel.cogMotorValue;
                        break;
                    case (int)BlockType.LargeWheel:
                        warningLevel = WarningLevel.cogMotorValue;
                        break;
                    case (int)BlockType.SmallWheel:
                        warningLevel = WarningLevel.cogMotorValue;
                        break;
                }
            }
            else
            {
                warningLevel = WarningLevel.normalBlockValue;
            }
        }
    }
}
