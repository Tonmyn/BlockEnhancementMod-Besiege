using Modding;
using Modding.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    class RadarScript : MonoBehaviour
    {
        public static int CollisionLayer = 10;
        public bool showRadar = false;
        public float radius = 2000f;
        public float safetyRadius = 1f;
        public float searchAngle = 20f;
        public float minSearchRadiusWhenLaunch = 30;
        public MeshCollider meshCollider;
        public MeshRenderer meshRenderer;
        private HashSet<BlockBehaviour> blocksInSafetyRange = new HashSet<BlockBehaviour>();
        public static bool MarkTarget { get; internal set; } = true;
        private Texture2D redSquareAim;

        public bool Switch { get; set; } = false;
        bool lastSwitchState = false;
        public SearchModes SearchMode { get; set; } = SearchModes.Auto;
        public Target target { get; private set; }

        public event Action<Target> OnTarget;

        private HashSet<Target> checkedTarget = new HashSet<Target>();
        private Dictionary<BlockBehaviour, Collider> checkedTargetDic = new Dictionary<BlockBehaviour, Collider>();

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
                    target = ProcessTarget(GetTargetManual());
                    OnTarget.Invoke(target);
                }
            }

            if (checkedTargetDic.Count > 0 && Switch)
            {
                if (target != null && !checkedTargetDic.ContainsKey(target.block))
                {
                    checkedTarget.Remove(target);
                    checkedTargetDic.Remove(target.block);
                    SendClientTargetNull();
                }
            }


            if (target != null)
            {
                bool removeFlag = false;
                if (target.hasFireTag)
                {
                    if (target.fireTag.burning || target.fireTag.hasBeenBurned)
                    {
                        removeFlag = true;
                    }
                }
                if (target.isBomb)
                {
                    if (target.bomb.hasExploded)
                    {
                        removeFlag = true;
                    }
                }

                if (target.isRocket)
                {
                    if (target.rocket.hasExploded)
                    {
                        removeFlag = true;
                    }
                }

                if (removeFlag)
                {
                    checkedTarget.Remove(target);
                    SendClientTargetNull();
                }
            }

            if (target == null && checkedTarget.Count > 0)
            {
                float aimDistance = 0f;
                float tempAimdistance = Mathf.Infinity;
                Target dummyTarget = new Target
                {
                    warningLevel = Target.WarningLevel.dummyValue
                };
                foreach (var tempTarget in checkedTarget)
                {
                    if (tempTarget != null)
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
                }
                if (dummyTarget.warningLevel != Target.WarningLevel.dummyValue)
                {
                    target = dummyTarget;
                    OnTarget.Invoke(target);
                }
            }

            //--------------------------------------------------//
            Collider GetTargetManual()
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

            Target triggeredTarget = ProcessTarget(collider);
            if (triggeredTarget == null) return;

            if (checkedTarget.Contains(triggeredTarget)) return;
            checkedTarget.Add(triggeredTarget);
            checkedTargetDic.Add(triggeredTarget.block, triggeredTarget.collider);

            if (target == null)
            {
                target = triggeredTarget;
                OnTarget.Invoke(target);
            }
            else
            {
                if (triggeredTarget.warningLevel > target.warningLevel)
                {
                    target = triggeredTarget;
                    OnTarget.Invoke(target);
                }
                else if (triggeredTarget.warningLevel == target.warningLevel)
                {
                    float aimDistance = Vector3.Distance(triggeredTarget.transform.position, transform.position);
                    float targetDistance = Vector3.Distance(target.transform.position, transform.position);
                    if (aimDistance < targetDistance)
                    {
                        target = triggeredTarget;
                        OnTarget.Invoke(target);
                    }
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (SearchMode != SearchModes.Auto) return;

            Target triggeredTarget = ProcessTarget(collider);
            if (triggeredTarget == null) return;

            if (checkedTargetDic.ContainsKey(triggeredTarget.block))
            {
                checkedTarget.Remove(triggeredTarget);
                checkedTargetDic.Remove(triggeredTarget.block);
                SendClientTargetNull();
            }
        }

        Target ProcessTarget(Collider collider)
        {
            BlockBehaviour block = collider.gameObject.GetComponentInParent<BlockBehaviour>();

            // If not a block
            if (block == null) return null;

            // if is own machine
            if (StatMaster.isMP)
            {
                if (block.ParentMachine.PlayerID == GetComponentInParent<BlockBehaviour>().ParentMachine.PlayerID)
                {
                    return null;
                }
            }
            else
            {
                if (blocksInSafetyRange.Contains(block))
                {
                    return null;
                }
            }

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
                transform = collider.gameObject.transform,
                block = block,
                fireTag = fireTag,
                hasFireTag = (fireTag == null)
            };
            tempTarget.SetTargetWarningLevel();

            return tempTarget;
        }

        public void ClearSavedSets()
        {
            checkedTarget.Clear();
            blocksInSafetyRange.Clear();
        }

        public void ResetTriggerState()
        {
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }

        private void GetBlocksInSafetyRange()
        {
            Collider[] overlappedColliders = Physics.OverlapSphere(transform.position, minSearchRadiusWhenLaunch, Game.BlockEntityLayerMask);
            foreach (var collider in overlappedColliders)
            {
                BlockBehaviour block = collider.gameObject.GetComponentInParent<BlockBehaviour>();
                if (block != null)
                {
                    blocksInSafetyRange.Add(block);
                }
            }
        }

        public void ActivateDetectionZone()
        {
            if (!StatMaster.isMP)
            {
                GetBlocksInSafetyRange();
            }
            meshCollider.enabled = true;
            meshRenderer.enabled = showRadar;
        }

        public void DeactivateDetectionZone()
        {
            meshCollider.enabled = false;
            meshRenderer.enabled = false;
        }

        public void ChangeSearchMode()
        {
            if (SearchMode == SearchModes.Auto)
            {
                SearchMode = SearchModes.Manual;
                //do something...
                DeactivateDetectionZone();
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

            var mr = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Transparent/Diffuse"));
            material.color = new Color(0, 1, 0, 0.1f);
            mr.material = material;
            meshRenderer = mr;
            meshRenderer.enabled = false;
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
        public Transform transform;
        public Collider collider;
        public BlockBehaviour block;
        public FireTag fireTag;
        public bool hasFireTag = false;
        public bool isRocket = false;
        public bool isBomb = false;
        public TimedRocket rocket;
        public ExplodeOnCollideBlock bomb;
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
            dummyValue = -1
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
                        isRocket = true;
                        rocket = block.GetComponentInParent<TimedRocket>();
                        break;
                    case (int)BlockType.Bomb:
                        warningLevel = WarningLevel.bombValue;
                        isBomb = true;
                        bomb = block.GetComponentInParent<ExplodeOnCollideBlock>();
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
