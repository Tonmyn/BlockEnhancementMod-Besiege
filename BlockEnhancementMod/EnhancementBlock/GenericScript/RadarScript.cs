using Modding;
using Modding.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod
{
    class RadarScript : MonoBehaviour
    {
        public static int CollisionLayer = 10;
        public BlockBehaviour parentBlock;
        public bool ShowRadar { get; set; } = false;
        public float SearchRadius { get; set; } = 2000f;
        public float SafetyRadius { get; set; } = 30f;
        public float SearchAngle { get; set; } = 0f;

        public Vector3 ForwardDirection { get { return parentBlock.BlockID == (int)BlockType.Rocket ? parentBlock.transform.up : parentBlock.transform.forward; } }
        public Vector3 TargetPosition { get { return target.collider.bounds.center - transform.position; } }
        /// <summary>
        /// Distance of StartPoint to Target
        /// </summary>
        /// <returns>Distance value</returns>
        public float TargetDistance { get { return target == null ? Mathf.Infinity : Vector3.Distance(transform.position, target.transform.position); } }
        /// <summary>
        /// Angle of StartPoint to Target
        /// </summary>
        /// <returns>Angle value</returns>
        public float TargetAngle { get { return target == null ? Mathf.Infinity : Vector3.Angle(TargetPosition, ForwardDirection); } }

        public MeshCollider meshCollider;
        public MeshRenderer meshRenderer;

        public static bool MarkTarget { get { return BlockEnhancementMod.Configuration.GetValue<bool>("Mark Target"); } internal set { BlockEnhancementMod.Configuration.SetValue("Mark Target", value); } }
        public static int RadarFrequency { get; } = BlockEnhancementMod.Configuration./*RadarFequency*/GetValue<int>("Radar Frequency");
        private Texture2D redSquareAim;

        public bool Switch { get; set; } = false;
        bool lastSwitchState = false;
        public SearchModes SearchMode { get; set; } = SearchModes.Auto;
        public Target target { get; private set; }

        public event Action<Target> OnTarget;

        private HashSet<BlockBehaviour> blockList = new HashSet<BlockBehaviour>();
        private HashSet<BlockBehaviour> lastBlockList = new HashSet<BlockBehaviour>();
        private bool isChoosingBlock = false;

        public bool receivedRayFromClient = false;
        public Ray rayFromClient;

        public enum SearchModes
        {
            Auto = 0,
            Manual = 1
        }

        private void Awake()
        {
            gameObject.layer = CollisionLayer;
            redSquareAim = RocketsController.redSquareAim;

        }
        private void Update()
        {
            if (lastSwitchState != Switch)
            {
                lastSwitchState = Switch;
                if (Switch)
                {
                    if (SearchMode == SearchModes.Auto)
                    {
                        ActivateDetectionZone();
                    }
                }
                else
                {
                    DeactivateDetectionZone();
                }
            }

            if (!Switch || SearchMode == SearchModes.Manual) return;

            if (Switch && target != null)
            {
                if (!InRadarRange(target))
                {
                    ClearTarget();
                }
            }

            if (blockList.Count > 0 && (!blockList.SetEquals(lastBlockList) || target == null))
            {
#if DEBUG
                Debug.Log(blockList.Count);
#endif
                if (!isChoosingBlock)
                {
#if DEBUG
                    Debug.Log("choose target");
#endif
                    isChoosingBlock = true;
                    lastBlockList = new HashSet<BlockBehaviour>(blockList);

                    var tempTarget = target ?? new Target(Target.warningLevel.dummyValue);
                    var removeBlockList = new HashSet<BlockBehaviour>();
                    int chooseTargetIndex = 0;

                    //StartCoroutine(chooseTargetInTargetList(new List<BlockBehaviour>(lastBlockList)));
                    StartCoroutine(chooseTargetInTargetList(lastBlockList));

                    //IEnumerator chooseTargetInTargetList(List<BlockBehaviour> blocks)
                    IEnumerator chooseTargetInTargetList(HashSet<BlockBehaviour> blocks)
                    {
                        foreach (var itemBlock in blocks)
                        {
                            var itemTarget = ProcessTarget(itemBlock);

                            if (!InRadarRange(itemTarget))
                            {
                                try { removeBlockList.Add(itemBlock); } catch (Exception e) { Debug.Log(e.Message); }
                            }
                            else if (itemTarget.WarningLevel > tempTarget.WarningLevel)
                            {
                                tempTarget = itemTarget;
                            }
                            else if (itemTarget.WarningLevel == tempTarget.WarningLevel)
                            {
                                float itemTargetDistance = Vector3.Distance(itemTarget.transform.position, parentBlock.transform.position);
                                float tempTargetDistance = Vector3.Distance(tempTarget.transform.position, parentBlock.transform.position);

                                if (itemTargetDistance < tempTargetDistance)
                                {
                                    tempTarget = itemTarget;
                                }
                            }


                            if (chooseTargetIndex++ >= RadarFrequency)
                            {
#if DEBUG
                                Debug.Log("frequency" + "  target count:" + blocks.Count);
#endif
                                chooseTargetIndex = 0;
                                yield return 0;
                            }
                        }

                        if (tempTarget != null && tempTarget.transform != null)
                        {
                            SetTarget(tempTarget);
                        }

                        try
                        {
                            if (removeBlockList.Count > 0)
                            {
                                lastBlockList.ExceptWith(removeBlockList);
                                blockList.ExceptWith(removeBlockList);
                            }

                        }
                        catch (Exception e) { Debug.Log(e.Message); }

                        isChoosingBlock = false;

                        yield break;
                    }
                }
            }

        }
        private void OnTriggerEnter(Collider collider)
        {
            if ((SearchMode != SearchModes.Auto) || !Switch) return;
            if (!isQualifiedCollider(collider)) return;
            //var block = collider.gameObject.GetComponent<BlockBehaviour>() ?? collider.gameObject.GetComponentInParent<BlockBehaviour>() ?? collider.transform.parent.gameObject.GetComponent<BlockBehaviour>();
            var block = collider.gameObject.GetComponentInParent<BlockBehaviour>();

            if (!isQualifiedBlock(block)) return;
            blockList.Add(block);
        }
        private void OnGUI()
        {
            if (StatMaster.isMP && StatMaster.isHosting)
            {
                if (transform.parent.gameObject.GetComponent<BlockBehaviour>().ParentMachine.PlayerID != 0)
                {
                    return;
                }
            }
            //if (!Switch) return;
            DrawTargetRedSquare();

            void DrawTargetRedSquare()
            {
                if (MarkTarget)
                {
                    if (target != null)
                    {
                        Vector3 markerPosition = target.collider/*.bounds*/ != null ? target.collider.bounds.center : target.transform.position;
                        if (Vector3.Dot(Camera.main.transform.forward, markerPosition - Camera.main.transform.position) > 0)
                        {
                            int squareWidth = 16;
                            Vector3 itemScreenPosition = Camera.main.WorldToScreenPoint(markerPosition);
                            GUI.DrawTexture(new Rect(itemScreenPosition.x - squareWidth * 0.5f, Camera.main.pixelHeight - itemScreenPosition.y - squareWidth * 0.5f, squareWidth, squareWidth), redSquareAim);
                        }
                    }
                }
            }
        }
        private void OnDestroy()
        {
            Switch = false;
            ClearTarget();
            blockList.Clear();
        }

        public void Setup(BlockBehaviour parentBlock, float searchRadius, float searchAngle, int searchMode, bool showRadar, float safetyRadius = 30f)
        {
            this.parentBlock = parentBlock;
            this.SearchAngle = searchAngle;
            this.ShowRadar = showRadar;
            this.SearchRadius = searchRadius;
            this.SafetyRadius = safetyRadius;
            this.SearchMode = (SearchModes)searchMode;
            CreateFrustumCone(safetyRadius, searchRadius);
            //targetList.Clear();
            blockList.Clear();

            void CreateFrustumCone(float topRadius, float bottomRadius)
            {
                float topHeight = topRadius;
                float height = bottomRadius - topHeight;

                float radiusTop = Mathf.Tan(SearchAngle * 0.5f * Mathf.Deg2Rad) * topHeight + 0.5f;
                float radiusBottom = Mathf.Tan(SearchAngle * 0.5f * Mathf.Deg2Rad) * bottomRadius;

                //越高越精细
                int numVertices = 5 + 10;

                Vector3 myTopCenter = Vector3.up * topHeight - Vector3.up;
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

                Physics.IgnoreLayerCollision(CollisionLayer, CollisionLayer);
                Physics.IgnoreLayerCollision(CollisionLayer, 29);
                //Physics.IgnoreLayerCollision(CollisionLayer, 0);

                var mr = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
                Material material = new Material(Shader.Find("Transparent/Diffuse"));
                material.color = new Color(0, 1, 0, 0.1f);
                mr.material = material;
                meshRenderer = mr;
                meshRenderer.enabled = false;
            }
        }

        public void SetTarget(Target tempTarget)
        {
            if (tempTarget == null) return;

            target = tempTarget;
            //targetList.Add(target);
            blockList.Add(tempTarget.block);
            if (target.collider != null) target.initialDistance = Vector3.Distance(target.collider.bounds.center, transform.position);


            if (receivedRayFromClient) SendTargetToClient();
            receivedRayFromClient = false;

            OnTarget?.Invoke(target);
        }
        public void SetTargetManual()
        {
            if (SearchMode == SearchModes.Manual)
            {
                ClearTarget();
                //SetTarget(ProcessTarget(GetTargetManual()));
                SetTarget(getTargetManual());
            }

            Collider GetTargetManual()
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
                    Collider tempCollider = new Collider();

                    if (Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit, Mathf.Infinity, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        var collider = rayHit.collider;
                        if (collider.gameObject.layer != 29 && collider.enabled && !collider.isTrigger)
                        {
                            LevelEntity levelEntity = rayHit.transform.gameObject.GetComponentInParent<LevelEntity>();
                            BlockBehaviour blockBehaviour = rayHit.transform.gameObject.GetComponentInParent<BlockBehaviour>();
                            if (levelEntity != null || blockBehaviour != null)
                            {
                                tempCollider = collider;
                                //if ((rayHit.transform.position - transform.position).magnitude >= /*minSearchRadiusWhenLaunch*/0)
                                //{
                                //    tempCollider = collider;
                                //}
                            }
                        }
                    }
                    if (tempCollider == null)
                    {
                        float manualSearchRadius = 1.25f;
                        RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);

                        if (hits.Length > 0)
                        {
                            for (int i = 0; i < hits.Length; i++)
                            {
                                var collider = hits[i].collider;
                                if (collider.gameObject.layer == 29 || !collider.enabled || collider.isTrigger) continue;
                                LevelEntity levelEntity = hits[i].transform.gameObject.GetComponentInParent<LevelEntity>();
                                BlockBehaviour blockBehaviour = hits[i].transform.gameObject.GetComponentInParent<BlockBehaviour>();
                                if (levelEntity != null || blockBehaviour != null)
                                {
                                    tempCollider = hits[i].collider;
                                    break;
                                    //if ((hits[i].transform.position - transform.position).magnitude >= /*minSearchRadiusWhenLaunch*/0)
                                    //{
                                    //    tempCollider = hits[i].collider;
                                    //    break;
                                    //}
                                }
                            }
                        }
                    }
                    return tempCollider;
                }
            }

            Target getTargetManual()
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (StatMaster.isClient)
                {
                    SendRayToHost(ray);
                    return null;
                }
                else
                {
                    Target tempTarget = null;

                    if (Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit, SearchRadius, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        tempTarget = ConvertRaycastHitToTarget(rayHit); /*Debug.Log("11- " + (tempTarget == null).ToString());*/
                    }
                    if (tempTarget == null)
                    {
                        float manualSearchRadius = 1.25f;
                        RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, SearchRadius, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);

                        if (hits.Length > 0)
                        {
                            for (int i = 0; i < hits.Length; i++)
                            {
                                tempTarget = ConvertRaycastHitToTarget(hits[i]); /*Debug.Log("22- " + (tempTarget == null).ToString());*/
                                if (tempTarget != null) break;
                            }
                        }
                    }
                    if (tempTarget == null)
                    {
                        tempTarget =new Target(rayHit.point); /*Debug.Log("33- " + (tempTarget == null).ToString());*/
                    }

                    return tempTarget;
                }

                Target ConvertRaycastHitToTarget(RaycastHit raycastHit)
                {
                    if (isQualifiedBlock(raycastHit.transform.GetComponentInParent<BlockBehaviour>()))
                    {
                        return new Target(raycastHit.transform);
                    }
                    else if (isQualifiedEntity(raycastHit.transform.GetComponentInParent<LevelEntity>()))
                    {
                        return new Target(raycastHit.transform);
                    }
                    else if (isQualifiedFireTag(raycastHit.transform.gameObject.GetComponentInParent<FireTag>()))
                    {
                        return new Target(raycastHit.transform);
                    }
                    else if (isQualifiedRigidbody(raycastHit.rigidbody))
                    {
                        return new Target(raycastHit.transform);
                    }
                    else if (isQualifiedCollider(raycastHit.collider))
                    {
                        return new Target(raycastHit.transform);
                    }

                    return null;
                }
            }
        }
        public void ChangeSearchMode()
        {
            //if (!Switch) return;

            ClearTarget();
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
                if (Switch) ActivateDetectionZone();
            }
        }
        public void ClearTarget()
        {
            if (target != null) blockList.Remove(target.block);
            SendClientTargetNull();
            target = null;
#if DEBUG
            Debug.Log("clear target");
#endif
        }
        public void ClearTargetNoRemoval()
        {
            SendClientTargetNull();
            target = null;
        }
        private void ActivateDetectionZone()
        {
            meshRenderer.enabled = ShowRadar;
            StopCoroutine("intervalActivateDetectionZone");
            StartCoroutine(intervalActivateDetectionZone(Time.deltaTime * 10f, Time.deltaTime * 1f));

            IEnumerator intervalActivateDetectionZone(float stopTime, float workTime)
            {
                while (Switch && SearchMode == SearchModes.Auto)
                {
                    meshCollider.enabled = true;
                    yield return new WaitForSeconds(workTime);
                    meshCollider.enabled = false;
                    yield return new WaitForSeconds(stopTime);
                }
                yield break;
            }
        }
        private void DeactivateDetectionZone()
        {
            meshCollider.enabled = false;
            meshRenderer.enabled = false;
        }
        private Target ProcessTarget(Collider collider)
        {
            if (!isQualifiedCollider(collider))
            {
                return null;
            }
            else
            {
                BlockBehaviour block = /*collider.transform.GetComponent<BlockBehaviour>() ?? collider.transform.GetComponentInParent<BlockBehaviour>() ?? collider.transform.parent.GetComponent<BlockBehaviour>()*/ collider.transform.gameObject.GetComponentInParent<BlockBehaviour>();

                return isQualifiedBlock(block) ? ProcessTarget(block) : null;
            }


            //// If not a block
            //if (block == null && SearchMode == SearchModes.Auto) return null;
            //// if not a rocket and have nothing connected to
            //if (block.BlockID != (int)BlockType.Rocket)
            //{
            //    if (block.blockJoint == null)
            //    {
            //        return null;
            //    }
            //    else if (block.blockJoint.connectedBody == null)
            //    {
            //        return null;
            //    }
            //}
            //else
            //{
            //    if (Playerlist.Players.Count < 2 && parentBlock.BlockID == (int)BlockType.Rocket)
            //    {
            //        RocketScript targetRocketScript = block.GetComponent<RocketScript>();
            //        RocketScript selfRocketScript = parentBlock.GetComponent<RocketScript>();
            //        if (!selfRocketScript.SPTeamKey.HasKey(KeyCode.None))
            //        {
            //            if (targetRocketScript.SPTeamKey.GetKey(0) == selfRocketScript.SPTeamKey.GetKey(0)) return null;
            //        }
            //    }
            //}

            //// if is own machine
            //if (block != null)
            //{
            //    if (StatMaster.isMP && !StatMaster.isClient && Playerlist.Players.Count > 1)
            //    {
            //        if (block.Team == MPTeam.None)
            //        {
            //            if (block.ParentMachine.PlayerID == parentBlock.ParentMachine.PlayerID)
            //            {
            //                return null;
            //            }
            //        }
            //        else
            //        {
            //            if (block.Team == parentBlock.Team)
            //            {
            //                return null;
            //            }
            //        }
            //    }
            //}

            //FireTag fireTag = collider.gameObject.GetComponentInParent<FireTag>();
            //Rigidbody rigidbody = collider.gameObject.GetComponentInParent<Rigidbody>();

            //if (rigidbody == null) return null;

            //Target tempTarget = new Target
            //{
            //    collider = collider,
            //    transform = collider.gameObject.transform,
            //    block = block,
            //    rigidbody = rigidbody,
            //    fireTag = fireTag,
            //    hasFireTag = (fireTag != null)
            //};
            //tempTarget.SetTargetWarningLevel();

            //if (tempTarget.hasFireTag)
            //{
            //    if ((tempTarget.fireTag.burning || tempTarget.fireTag.hasBeenBurned) && !tempTarget.isRocket)
            //    {
            //        return null;
            //    }
            //}
            //return tempTarget;
        }
        private Target ProcessTarget(BlockBehaviour block)
        {
            if (!isQualifiedBlock(block) || !isQualifiedCollider(block.GetComponentInChildren<Collider>())) return null;

            Target tempTarget = new Target(block);

            if (tempTarget.hasFireTag)
            {
                if ((tempTarget.fireTag.burning || tempTarget.fireTag.hasBeenBurned) && !tempTarget.isRocket)
                {
                    return null;
                }
            }
            return tempTarget;
        }

        private bool isQualifiedCollider(Collider collider)
        {
            return collider == null ? false : !(collider.isTrigger || !collider.enabled || collider.gameObject.isStatic || collider.gameObject.layer == 29);
        }
        private bool isQualifiedRigidbody(Rigidbody rigidbody)
        {
            return !(rigidbody == null || rigidbody.isKinematic == true);
        }
        private bool isQualifiedFireTag(FireTag fireTag)
        {
            return !(fireTag == null || fireTag.burning || fireTag.hasBeenBurned);
        }
        private bool isQualifiedBlock(BlockBehaviour block)
        {
            var value = true;

            // If not a block
            if (block == null) return false;

            // if not a rocket and have nothing connected to
            if (block.BlockID != (int)BlockType.Rocket)
            {
                if (block.blockJoint == null)
                {
                    return false;
                }
                else if (block.blockJoint.connectedBody == null)
                {
                    return false;
                }
            }
            else
            {
                if (Playerlist.Players.Count < 2 && parentBlock.BlockID == (int)BlockType.Rocket)
                {
                    RocketScript targetRocketScript = block.GetComponent<RocketScript>();
                    RocketScript selfRocketScript = parentBlock.GetComponent<RocketScript>();
                    if (!selfRocketScript.SPTeamKey.HasKey(KeyCode.None))
                    {
                        if (targetRocketScript.SPTeamKey.GetKey(0) == selfRocketScript.SPTeamKey.GetKey(0)) return false;
                    }
                }
            }

            // if is own machine
            if (block != null)
            {
                if (StatMaster.isMP && !StatMaster.isClient && Playerlist.Players.Count > 1)
                {
                    if (block.Team == MPTeam.None)
                    {
                        if (block.ParentMachine.PlayerID == parentBlock.ParentMachine.PlayerID)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (block.Team == parentBlock.Team)
                        {
                            return false;
                        }
                    }
                }

                FireTag fireTag = block.GetComponent<FireTag>() ?? block.gameObject.GetComponentInChildren<FireTag>();
                Rigidbody rigidbody = block.GetComponent<Rigidbody>() ?? block.gameObject.GetComponentInChildren<Rigidbody>();

                if (fireTag == null || fireTag.burning || fireTag.hasBeenBurned) return false;
                if (rigidbody == null || rigidbody.gameObject.layer == 2) return false;
            }

            return value;
        }
        private bool isQualifiedEntity(LevelEntity levelEntity)
        {
            if (levelEntity != null)
            {
                if (levelEntity.isStatic || levelEntity.IsDestroyed) return false;
                if (levelEntity.fireTag.burning || levelEntity.fireTag.hasBeenBurned) return false;
                if (!isQualifiedCollider(levelEntity.GetComponentInParent<Collider>())) return false;
                if (!isQualifiedRigidbody(levelEntity.GetComponentInParent<Rigidbody>())) return false;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InRadarRange(Vector3 positionInWorld)
        {
            if (Vector3.Dot(positionInWorld - transform.position, ForwardDirection) > 0)
            {
                var distance = positionInWorld - transform.position;

                if (distance.magnitude < SearchRadius)
                {
                    if (distance.magnitude > 5f)
                    {
                        if (Vector3.Angle(positionInWorld - transform.position, ForwardDirection) < (SearchAngle / 2f))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool InRadarRange(BlockBehaviour block)
        {
            return block == null ? false : InRadarRange(ProcessTarget(block));
        }
        public bool InRadarRange(Collider collider)
        {
            if (!collider.enabled || collider.isTrigger || collider.gameObject.isStatic) return false;
            return InRadarRange(collider.bounds.center);
        }
        public bool InRadarRange(Target target)
        {
            bool value = false;
            if (target == null) return value;

            if (InRadarRange(target.collider))
            {
                if (target.isRocket)
                {
                    value = !target.rocket.hasExploded;
                }
                else if (target.isBomb)
                {
                    value = !target.bomb.hasExploded;
                }
                else
                {
                    if (target.hasFireTag)
                    {
                        value = !(target.fireTag.burning || target.fireTag.hasBeenBurned);
                    }
                    if (value)
                    {
                        target.block.CheckJoints();
                        value = !(target.block.blockJoint == null);
                    }
                }

                //if (!target.isRocket && !target.isBomb && target.block.blockJoint == null)
                //{
                //    value = false;
                //}
                //else
                //{
                //    value = true;
                //}

                //if (target.hasFireTag && !target.isRocket && !target.isBomb)
                //{
                //    value = !(target.fireTag.burning || target.fireTag.hasBeenBurned);
                //    //if (target.fireTag.burning || target.fireTag.hasBeenBurned)
                //    //{
                //    //    value = false;
                //    //}
                //    //else
                //    //{
                //    //    value = true;
                //    //}
                //}
            }
            return value;
        }

        #region Networking Method
        private void SendRayToHost(Ray ray)
        {
            Message rayToHostMsg = Messages.rocketRayToHostMsg.CreateMessage(ray.origin, ray.direction, /*BB*/transform.parent.GetComponent<BlockBehaviour>());
            ModNetworking.SendToHost(rayToHostMsg);
        }
        private void SendTargetToClient()
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
        private void SendClientTargetNull()
        {
            //Switch = true;
            //target = null;
            //OnTarget?.Invoke(target);

            if (StatMaster.isHosting)
            {
                Message rocketTargetNullMsg = Messages.rocketTargetNullMsg.CreateMessage(parentBlock);
                ModNetworking.SendTo(Player.GetAllPlayers().Find(player => player.NetworkId == parentBlock.ParentMachine.PlayerID), rocketTargetNullMsg);
                ModNetworking.SendToAll(Messages.rocketLostTargetMsg.CreateMessage(parentBlock));
            }
            RocketsController.Instance.RemoveRocketTarget(parentBlock);
        }
        #endregion

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

        //public Vector3 positionDiff = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        //public float angleDiff = 0f;
        //public Vector3 acceleration = Vector3.zero;

        public warningLevel WarningLevel { get; private set; } = 0;

        public enum warningLevel
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

        public Target() { }
        public Target(Vector3 point)
        {
            var go = new GameObject("Target Object");
            go.AddComponent<DestroyIfEditMode>();
            go.transform.position = point;
            transform = go.transform;
            initialDistance = 500f;
            WarningLevel = warningLevel.dummyValue;
        }
        public Target(Transform transform)
        {
            this.transform = transform;
            this.collider = transform.GetComponentInParent<Collider>();
            this.block = transform.GetComponentInParent<BlockBehaviour>();
            this.rigidbody = transform.GetComponentInParent<Rigidbody>();
            this.fireTag = transform.GetComponentInParent<FireTag>();
            this.hasFireTag = (this.fireTag != null);
            initialDistance = 500f;

            WarningLevel = warningLevel.normalBlockValue;
        }
        public Target(warningLevel warningLevel)
        {
            WarningLevel = warningLevel;
        }
        public Target(BlockBehaviour block)
        {
            collider = block.gameObject.GetComponent<Collider>() ?? block.gameObject.GetComponentInChildren<Collider>();
            fireTag = block.gameObject.GetComponent<FireTag>() ?? block.gameObject.GetComponentInChildren<FireTag>();
            rigidbody = block.GetComponent<Rigidbody>() ?? block.gameObject.GetComponentInChildren<Rigidbody>();
            hasFireTag = (fireTag != null);
            transform = block.transform;
            this.block = block;

            SetTargetWarningLevel();
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
                        WarningLevel = warningLevel.normalBlockValue;
                        break;
                    case (int)BlockType.Rocket:
                        WarningLevel = warningLevel.guidedRocketValue;
                        isRocket = true;
                        rocket = collidedObject.GetComponentInParent<TimedRocket>();
                        if (rocket == null)
                        {
                            rocket = collidedObject.GetComponentInChildren<TimedRocket>();
                        }
                        break;
                    case (int)BlockType.Bomb:
                        WarningLevel = warningLevel.bombValue;
                        isBomb = true;
                        bomb = collidedObject.GetComponentInParent<ExplodeOnCollideBlock>();
                        if (bomb == null)
                        {
                            bomb = collidedObject.GetComponentInChildren<ExplodeOnCollideBlock>();
                        }
                        break;
                    case (int)BlockType.WaterCannon:
                        WarningLevel = warningLevel.waterCannonValue;
                        break;
                    case (int)BlockType.FlyingBlock:
                        WarningLevel = warningLevel.flyingBlockValue;
                        break;
                    case (int)BlockType.Flamethrower:
                        WarningLevel = warningLevel.flameThrowerValue;
                        break;
                    case (int)BlockType.CogMediumPowered:
                        WarningLevel = warningLevel.cogMotorValue;
                        break;
                    case (int)BlockType.LargeWheel:
                        WarningLevel = warningLevel.cogMotorValue;
                        break;
                    case (int)BlockType.SmallWheel:
                        WarningLevel = warningLevel.cogMotorValue;
                        break;
                }
            }
            else
            {
                WarningLevel = warningLevel.normalBlockValue;
            }
        }
    }
}
