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
        public BlockBehaviour parentBlock;
        public bool showRadar = false;
        public float radius = 2000f;
        public float safetyRadius = 1f;
        public float searchAngle = 0f;
        public float minSearchRadiusWhenLaunch = 30;
        public MeshCollider meshCollider;
        public MeshRenderer meshRenderer;
        private HashSet<BlockBehaviour> blocksInSafetyRange = new HashSet<BlockBehaviour>();
        private Vector3 forwardDirection = Vector3.zero;
        public static bool MarkTarget { get; internal set; } = true;
        private Texture2D redSquareAim;

        public bool Switch { get; set; } = false;
        bool lastSwitchState = false;
        public SearchModes SearchMode { get; set; } = SearchModes.Auto;
        public Target target { get; private set; }

        public event Action<Target> OnTarget;

        private HashSet<Target> checkedTarget = new HashSet<Target>();
        private Dictionary<BlockBehaviour, Target> checkedTargetDic = new Dictionary<BlockBehaviour, Target>();

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
            redSquareAim = RocketsController.redSquareAim;

        }

        void FixedUpdate()
        {
            if (forwardDirection == Vector3.zero)
            {
                forwardDirection = parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward;
            }

            if (Switch && target != null)
            {
                bool removeFlag = !target.collider.enabled;
                bool inSight = false;

                target.positionDiff = target.collider.bounds.center - transform.position;
                target.angleDiff = Vector3.Angle(target.positionDiff, forwardDirection);

                if (!removeFlag)
                {
                    if (!target.isRocket && target.block.blockJoint == null)
                    {
                        removeFlag = true;
                    }
                    if (target.hasFireTag)
                    {
                        if ((target.fireTag.burning || target.fireTag.hasBeenBurned) && !target.isRocket)
                        {
                            removeFlag = true;
                        }
                    }
                    bool forward = Vector3.Dot(target.positionDiff, forwardDirection) > 0;
                    inSight = forward && target.angleDiff < searchAngle / 2;
                }

                if (removeFlag || !inSight)
                {
                    if (checkedTargetDic.TryGetValue(target.block, out Target targetInDict))
                    {
                        checkedTargetDic.Remove(target.block);
                        checkedTarget.Remove(targetInDict);
                    }
                    SendClientTargetNull();
                }
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

            if (!Switch || SearchMode == SearchModes.Manual) return;

            if (target == null && checkedTarget.Count > 0)
            {
                target = new Target
                {
                    warningLevel = Target.WarningLevel.dummyValue,
                };
                foreach (var tempTarget in checkedTarget)
                {
                    if (tempTarget != null)
                    {
                        if (tempTarget.warningLevel > target.warningLevel)
                        {
                            SetTarget(tempTarget);
                        }
                        else if (tempTarget.warningLevel == target.warningLevel)
                        {
                            float tempDistance = Vector3.Distance(tempTarget.transform.position, parentBlock.transform.position);
                            float targetDistance = Vector3.Distance(target.transform.position, parentBlock.transform.position);
                            if (tempDistance < targetDistance)
                            {
                                SetTarget(tempTarget);
                            }
                        }
                    }
                }
            }
        }

        public Collider GetTargetManual()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (StatMaster.isClient)
            {
                SendRayToHost(ray);
                return null;
            }
            else
            {
                //Find targets in the manual search mode by casting a sphere along the ray
                float manualSearchRadius = 1.25f;
                Collider tempCollider = new Collider();

                RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);

                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (hits[i].collider.gameObject.layer == 29) continue;
                        LevelEntity levelEntity = hits[i].transform.gameObject.GetComponentInParent<LevelEntity>();
                        BlockBehaviour blockBehaviour = hits[i].transform.gameObject.GetComponentInParent<BlockBehaviour>();
                        if (levelEntity != null || blockBehaviour != null)
                        {
                            if ((hits[i].transform.position - transform.position).magnitude >= minSearchRadiusWhenLaunch)
                            {
                                tempCollider = hits[i].collider;
                                break;
                            }
                        }
                    }
                }
                if (tempCollider == null && Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit, Mathf.Infinity, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (rayHit.collider.gameObject.layer != 29)
                    {
                        LevelEntity levelEntity = rayHit.transform.gameObject.GetComponentInParent<LevelEntity>();
                        BlockBehaviour blockBehaviour = rayHit.transform.gameObject.GetComponentInParent<BlockBehaviour>();
                        if (levelEntity != null || blockBehaviour != null)
                        {
                            if ((rayHit.transform.position - transform.position).magnitude >= minSearchRadiusWhenLaunch)
                            {
                                tempCollider = rayHit.collider;
                            }
                        }
                    }
                }
                return tempCollider;
            }
        }
        public void SetTarget(Target tempTarget)
        {
            if (tempTarget == null) return;

            target = tempTarget;
            target.initialDistance = Vector3.Distance(target.collider.bounds.center, transform.position);

            if (receivedRayFromClient) SendTargetToClient();
            receivedRayFromClient = false;

            OnTarget.Invoke(target);
        }

        void OnTriggerEnter(Collider collider)
        {
            if (SearchMode != SearchModes.Auto) return;
            if (collider.isTrigger) return;

            Target triggeredTarget = ProcessTarget(collider);
            if (triggeredTarget == null) return;

            if (!checkedTargetDic.ContainsKey(triggeredTarget.block))
            {
                checkedTarget.Add(triggeredTarget);
                checkedTargetDic.Add(triggeredTarget.block, triggeredTarget);

                if (target == null)
                {
                    SetTarget(triggeredTarget);
                }
                else
                {
                    if (triggeredTarget.warningLevel > target.warningLevel)
                    {
                        SetTarget(triggeredTarget);
                    }
                    else if (triggeredTarget.warningLevel == target.warningLevel)
                    {
                        float aimDistance = Vector3.Distance(triggeredTarget.transform.position, transform.position);
                        float targetDistance = Vector3.Distance(target.transform.position, transform.position);
                        if (aimDistance < targetDistance)
                        {
                            SetTarget(triggeredTarget);
                        }
                    }
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (SearchMode != SearchModes.Auto) return;
            if (collider.isTrigger) return;

            BlockBehaviour triggeredBB = collider.gameObject.GetComponentInParent<BlockBehaviour>();
            if (triggeredBB == null) return;

            if (checkedTargetDic.TryGetValue(triggeredBB, out Target targetInDict))
            {
                checkedTargetDic.Remove(triggeredBB);
                checkedTarget.Remove(targetInDict);
            }
        }

        public Target ProcessTarget(Collider collider)
        {
            if (collider == null) return null;

            BlockBehaviour block = collider.gameObject.GetComponentInParent<BlockBehaviour>();

            // If not a block
            if (block == null && SearchMode == SearchModes.Auto) return null;

            // if not a rocket and have nothing connected to
            if (block.BlockID != (int)BlockType.Rocket)
            {
                if (block.blockJoint == null)
                {
                    return null;
                }
                else if (block.blockJoint.connectedBody == null)
                {
                    return null;
                }
                
                //if (block.iJointTo == null && block.jointsToMe == null)
                //{
                //    return null;
                //}
                //if (block.iJointTo != null && block.iJointTo.Count == 0)
                //{
                //    return null;
                //}
                //if (block.jointsToMe != null && block.jointsToMe.Count == 0)
                //{
                //    return null;
                //}
            }

            // if is own machine
            if (block != null)
            {
                if (StatMaster.isMP)
                {
                    if (block.Team == MPTeam.None)
                    {
                        if (block.ParentMachine.PlayerID == GetComponentInParent<BlockBehaviour>().ParentMachine.PlayerID)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (block.Team == GetComponentInParent<BlockBehaviour>().Team)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    if (blocksInSafetyRange.Contains(block))
                    {
                        return null;
                    }
                }
            }

            FireTag fireTag = collider.gameObject.GetComponentInParent<FireTag>();
            Rigidbody rigidbody = collider.gameObject.GetComponentInParent<Rigidbody>();
            if (rigidbody == null) return null;

            Target tempTarget = new Target
            {
                collider = collider,
                transform = collider.gameObject.transform,
                block = block,
                rigidbody = rigidbody,
                fireTag = fireTag,
                hasFireTag = (fireTag != null)
            };
            tempTarget.SetTargetWarningLevel();

            if (tempTarget.hasFireTag)
            {
                if ((tempTarget.fireTag.burning || tempTarget.fireTag.hasBeenBurned) && !tempTarget.isRocket)
                {
                    return null;
                }
            }

            return tempTarget;
        }

        public void ClearSavedSets()
        {
            checkedTarget.Clear();
            checkedTargetDic.Clear();
            blocksInSafetyRange.Clear();
        }

        public void GetBlocksInSafetyRange()
        {
            Collider[] overlappedColliders = Physics.OverlapSphere(transform.parent.position, minSearchRadiusWhenLaunch, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);
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

        public void CreateFrustumCone(float bottomRadius)
        {
            float topHeight = safetyRadius;
            float height = bottomRadius - topHeight;

            float radiusTop = Mathf.Tan(searchAngle * 0.5f * Mathf.Deg2Rad) * topHeight;
            float radiusBottom = Mathf.Tan(searchAngle * 0.5f * Mathf.Deg2Rad) * bottomRadius;

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

            Physics.IgnoreLayerCollision(CollisionLayer, 29);

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
                    if (target.transform.transform.GetComponent<BlockBehaviour>())
                    {
                        BlockBehaviour targetBB = target.transform.transform.GetComponent<BlockBehaviour>();
                        int id = targetBB.ParentMachine.PlayerID;
                        if (parentBlock.ParentMachine.PlayerID != 0)
                        {
                            Message targetBlockBehaviourMsg = Messages.rocketTargetBlockBehaviourMsg.CreateMessage(targetBB, parentBlock);
                            foreach (var player in Player.GetAllPlayers())
                            {
                                if (player.NetworkId == parentBlock.ParentMachine.PlayerID)
                                {
                                    ModNetworking.SendTo(player, targetBlockBehaviourMsg);
                                }
                            }
                        }
                        ModNetworking.SendToAll(Messages.rocketLockOnMeMsg.CreateMessage(parentBlock, id));
                        RocketsController.Instance.UpdateRocketTarget(parentBlock, id);
                    }
                    if (target.transform.transform.GetComponent<LevelEntity>())
                    {
                        Message targetEntityMsg = Messages.rocketTargetEntityMsg.CreateMessage(target.transform.transform.GetComponent<LevelEntity>(), parentBlock);
                        foreach (var player in Player.GetAllPlayers())
                        {
                            if (player.NetworkId == parentBlock.ParentMachine.PlayerID)
                            {
                                ModNetworking.SendTo(player, targetEntityMsg);
                            }
                        }
                        ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(parentBlock));
                        RocketsController.Instance.RemoveRocketTarget(parentBlock);
                    }
                }
            }
        }

        public void SendClientTargetNull()
        {
            Switch = true;
            target = null;
            OnTarget.Invoke(target);

            if (StatMaster.isHosting)
            {
                Message rocketTargetNullMsg = Messages.rocketTargetNullMsg.CreateMessage(parentBlock);
                ModNetworking.SendTo(Player.GetAllPlayers().Find(player => player.NetworkId == parentBlock.ParentMachine.PlayerID), rocketTargetNullMsg);
                ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(parentBlock));
            }
            RocketsController.Instance.RemoveRocketTarget(parentBlock);
        }
        #endregion

        private void OnGUI()
        {
            if (StatMaster.isMP && StatMaster.isHosting)
            {
                if (transform.parent.gameObject.GetComponent<BlockBehaviour>().ParentMachine.PlayerID != 0)
                {
                    return;
                }
            }
            if (!Switch) return;
            DrawTargetRedSquare();
        }

        private void DrawTargetRedSquare()
        {
            if (MarkTarget)
            {
                if (target != null)
                {
                    Vector3 markerPosition = target.collider.bounds != null ? target.collider.bounds.center : target.transform.position;
                    if (Vector3.Dot(Camera.main.transform.forward, markerPosition - Camera.main.transform.position) > 0)
                    {
                        int squareWidth = 16;
                        Vector3 itemScreenPosition = Camera.main.WorldToScreenPoint(markerPosition);
                        GUI.DrawTexture(new Rect(itemScreenPosition.x - squareWidth / 2, Camera.main.pixelHeight - itemScreenPosition.y - squareWidth / 2, squareWidth, squareWidth), redSquareAim);
                    }
                }
            }
        }
    }

    class Target
    {
        public Transform transform;
        public Collider collider;
        public BlockBehaviour block;
        public Rigidbody rigidbody;
        public FireTag fireTag;
        public bool hasFireTag = false;
        public bool isRocket = false;
        public bool isBomb = false;
        public TimedRocket rocket;
        public ExplodeOnCollideBlock bomb;
        public float initialDistance = 0f;

        public Vector3 positionDiff = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        public float angleDiff = 0f;
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
                        rocket = collidedObject.GetComponentInParent<TimedRocket>();
                        if (rocket == null)
                        {
                            rocket = collidedObject.GetComponentInChildren<TimedRocket>();
                        }
                        break;
                    case (int)BlockType.Bomb:
                        warningLevel = WarningLevel.bombValue;
                        isBomb = true;
                        bomb = collidedObject.GetComponentInParent<ExplodeOnCollideBlock>();
                        if (bomb == null)
                        {
                            bomb = collidedObject.GetComponentInChildren<ExplodeOnCollideBlock>();
                        }
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
