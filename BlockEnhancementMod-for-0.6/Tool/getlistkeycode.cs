using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public static class Tools
    {

        //public static List<string> Get_List_keycode(MKey mKey)
        //{
        //    List<String> keycode_list = new List<String>();
        //    for (int i = 0; i < mKey.KeysCount; i++)
        //    {
        //        keycode_list.Add(mKey.GetKey(i).ToString().ToUpper());
        //    }
        //    return keycode_list;
        //}

        public static string[] Get_List_keycode(MKey mKey)
        {
            List<String> keycode_list = new List<String>();
            for (int i = 0; i < mKey.KeysCount; i++)
            {
                keycode_list.Add(mKey.GetKey(i).ToString().ToUpper());
            }
            return keycode_list.ToArray();
        }
    }
}
