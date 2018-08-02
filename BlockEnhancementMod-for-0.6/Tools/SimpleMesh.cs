//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using UnityEngine;

//namespace BlockEnhancementMod.Tools
//{
//    class SimpleMesh
//    {

//        /// <summary>
//        /// 加载网格信息
//        /// </summary>
//        /// <param name="ObjPath">OBJ文件路径</param>
//        /// <returns>网格数据</returns>
//        public static Mesh MeshFromObj(string ObjPath)
//        {
//            List<Vector3> Normals = new List<Vector3>();
//            List<Vector2> UV = new List<Vector2>();
//            List<Vector3> newVertices = new List<Vector3>();
//            List<Vector3> Vertices = new List<Vector3>();
//            List<Vector2> newUV = new List<Vector2>();
//            List<int> triangleslist = new List<int>();
//            List<Vector3> newNormals = new List<Vector3>();
//            Mesh mesh = new Mesh();
//            StreamReader srd;

//            if (!File.Exists(ObjPath))
//            {
//                return new AssetBundle().LoadAsset<Mesh>(ObjPath);
//            }
//            try
//            {
//                srd = File.OpenText(ObjPath);
//                while (srd.Peek() != -1)
//                {
//                    string str = srd.ReadLine();
//                    string[] chara = str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
//                    if (chara.Length > 2)
//                    {
//                        if (chara[0] == "v")
//                        {
//                            Vector3 v1 = new Vector3(
//                              Convert.ToSingle(chara[1]),
//                              Convert.ToSingle(chara[2]),
//                              Convert.ToSingle(chara[3]));
//                            Vertices.Add(v1);
//                        }
//                        else if (chara[0] == "vt")
//                        {
//                            Vector2 uv1 = new Vector2(
//                              Convert.ToSingle(chara[1]),
//                              Convert.ToSingle(chara[2]));

//                            UV.Add(uv1);
//                        }
//                        else if (chara[0] == "vn")
//                        {
//                            Vector3 v2 = new Vector3(
//                              Convert.ToSingle(chara[1]),
//                              Convert.ToSingle(chara[2]),
//                              Convert.ToSingle(chara[3]));

//                            Normals.Add(v2);
//                        }
//                        else if (chara[0] == "f")
//                        {
//                            if (chara.Length == 4)
//                            {
//                                int a = Convert.ToInt32(chara[1].Split('/')[0]);
//                                int b = Convert.ToInt32(chara[2].Split('/')[0]);
//                                int c = Convert.ToInt32(chara[3].Split('/')[0]);
//                                triangleslist.Add(newVertices.Count);
//                                triangleslist.Add(newVertices.Count + 1);
//                                triangleslist.Add(newVertices.Count + 2);
//                                newVertices.Add(Vertices[a - 1]);
//                                newVertices.Add(Vertices[b - 1]);
//                                newVertices.Add(Vertices[c - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[1].Split('/')[2]) - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[2].Split('/')[2]) - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[3].Split('/')[2]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[1].Split('/')[1]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[2].Split('/')[1]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[3].Split('/')[1]) - 1]);
//                            }
//                            if (chara.Length == 5)
//                            {
//                                int a = Convert.ToInt32(chara[1].Split('/')[0]);
//                                int b = Convert.ToInt32(chara[2].Split('/')[0]);
//                                int c = Convert.ToInt32(chara[3].Split('/')[0]);
//                                int d = Convert.ToInt32(chara[4].Split('/')[0]);
//                                triangleslist.Add(newVertices.Count);
//                                triangleslist.Add(newVertices.Count + 1);
//                                triangleslist.Add(newVertices.Count + 2);
//                                triangleslist.Add(newVertices.Count);
//                                triangleslist.Add(newVertices.Count + 2);
//                                triangleslist.Add(newVertices.Count + 3);
//                                newVertices.Add(Vertices[a - 1]);
//                                newVertices.Add(Vertices[b - 1]);
//                                newVertices.Add(Vertices[c - 1]);
//                                newVertices.Add(Vertices[d - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[1].Split('/')[2]) - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[2].Split('/')[2]) - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[3].Split('/')[2]) - 1]);
//                                newNormals.Add(Normals[Convert.ToInt32(chara[4].Split('/')[2]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[1].Split('/')[1]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[2].Split('/')[1]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[3].Split('/')[1]) - 1]);
//                                newUV.Add(UV[Convert.ToInt32(chara[4].Split('/')[1]) - 1]);
//                            }
//                        }
//                    }
//                }
//                mesh.vertices = newVertices.ToArray();
//                mesh.uv = newUV.ToArray();
//                mesh.triangles = triangleslist.ToArray();
//                mesh.normals = newNormals.ToArray();
//#if DEBUG
//                Debug.Log("ReadFile " + ObjPath + " Completed!" + "Vertices:" + newVertices.Count.ToString());
//#endif
//                srd.Close();
//                mesh.RecalculateBounds();
//                mesh.RecalculateNormals();
//                mesh.Optimize();
//            }
//            catch (Exception ex)
//            {
//                Debug.Log("Obj model " + ObjPath + " error!");
//                Debug.Log("newUV==>" + newUV.Count.ToString());
//                Debug.Log("triangleslist==>" + triangleslist.Count.ToString());
//                Debug.Log("newNormals==>" + newNormals.Count.ToString());
//                Debug.Log(ex.ToString());
//            }
//            return mesh;
//        }

//        public static Mesh MeshFormBundle(string ObjPath)
//        {
//            Mesh mesh = new AssetBundle().LoadAsset<Mesh>(ObjPath);
//            return mesh;
//        }


//    }
//}
