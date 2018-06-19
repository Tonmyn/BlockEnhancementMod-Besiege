using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class SpringScript :EnhancementBlock
    {

        MSlider DragSlider;

        float Drag = 2;

        protected override void SafeAwake()
        {

            //DragSlider = AddSlider("阻力", "Drag", Drag, 0f, 3f, false);
            //DragSlider.ValueChanged += (float value) => { Drag = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Drag = DragSlider.Value; };


#if DEBUG
            ConsoleController.ShowMessage("皮筋添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            DragSlider.DisplayInMapper = value;
        }

        Rigidbody A, B;

        protected override void OnSimulateStart()
        {

            A = GameObject.Find("A").GetComponent<Rigidbody>();
            B = GameObject.Find("B").GetComponent<Rigidbody>();

            A.drag = B.drag = Drag;
        }


    }
}
