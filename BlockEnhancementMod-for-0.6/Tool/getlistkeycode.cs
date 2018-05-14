using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public static class Tools
    {

        public static List<KeyCode> Get_List_keycode(MKey mKey)
        {
            List<KeyCode> keycode_list = new List<KeyCode>();
            for (int i = 0; i < mKey.KeysCount; i++)
            {
                keycode_list.Add(mKey.GetKey(i));
            }
            return keycode_list;
        }
    }
}
