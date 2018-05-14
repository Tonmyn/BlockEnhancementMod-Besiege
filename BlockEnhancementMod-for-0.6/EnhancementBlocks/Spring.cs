using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class Spring :Block
    {
        SpringScript SS;

        MSlider DragSlider;

        float Drag = 2;

        public Spring(BlockBehaviour block) : base(block)
        {

            if (BB.GetComponent<SpringScript>() == null)
            {

                SS = BB.gameObject.AddComponent<SpringScript>();

                DragSlider = new MSlider("阻力", "Drag", Drag, 0f, 3f, false);
                DragSlider.ValueChanged += (float value) => { Drag = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(DragSlider);

            }
            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("皮筋添加进阶属性");
#endif
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + DragSlider.Key)) { DragSlider.Value = Drag = bd.ReadFloat("bmt-" + DragSlider.Key); }

                    break;
                }

            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {

                    blockinfo.BlockData.Write("bmt-" + DragSlider.Key, DragSlider.Value);

                    break;
                }

            }
        }

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            SS.Drag = Drag;
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            DragSlider.DisplayInMapper = value;
        }

        class SpringScript : BlockScript
        {

            Rigidbody A,B;

            public float Drag;

            private void Start()
            {
                A = GameObject.Find("A").GetComponent<Rigidbody>();
                B = GameObject.Find("B").GetComponent<Rigidbody>();

                A.drag = B.drag = Drag;
            }

        }
    }
}
