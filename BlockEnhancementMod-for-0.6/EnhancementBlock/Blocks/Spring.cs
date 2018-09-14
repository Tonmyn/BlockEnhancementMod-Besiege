using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;


namespace BlockEnhancementMod.Blocks
{
    class SpringScript :EnhancementBlock
    {

        MSlider DragSlider;

        public float Drag = 2;
        private float orginDrag = 2;

        Rigidbody A, B;

        public override void SafeAwake()
        {
            DragSlider = BB.AddSlider(LanguageManager.drag, "Drag", Drag, 0f, 3f);
            DragSlider.ValueChanged += (float value) => { Drag = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("皮筋添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            DragSlider.DisplayInMapper = value;
        }

        public override void ChangeParameter()
        {

            A = GameObject.Find("A").GetComponent<Rigidbody>();
            B = GameObject.Find("B").GetComponent<Rigidbody>();

            if (!EnhancementEnabled) { Drag = orginDrag; };

            A.drag = B.drag = Drag;
        }
    }
}
