using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;

namespace BlockEnhancementMod
{
    [Obsolete]//这个版本获取所有文件的方法有问题， 下个版本来说
    public class SkinManager
    {

        public List<Mesh> meshes; 
        public List<string> meshNames { get { return ConvertToStrings(meshes); } }

        public SkinManager()
        {
           // ModIO.
        }

        private List<string> ConvertToStrings(List<Mesh> meshes)
        {
            var strs = new List<string>();

            foreach (var mesh in meshes)
            {
                strs.Add(mesh.name);
            }
            return strs;
        }
    }
}
