using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class DecouplerScript : EnhancementBlock
    {

        MSlider ExplodeForceSlider;

        MSlider ExplodeTorqueSlider;

        float ExplodeForce = 1000f;

        float ExplodeTorque = 2000f;

        protected override void SafeAwake()
        {

            ExplodeForceSlider = AddSlider(LanguageManager.explodeForce, "ExplodeForce", ExplodeForce, 0, 3000f, false);
            ExplodeForceSlider.ValueChanged += (float value) => { ExplodeForce = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ExplodeForce = ExplodeForceSlider.Value; };

            ExplodeTorqueSlider = AddSlider(LanguageManager.explodeTorque, "ExplodeTorque", ExplodeTorque, 0, 2500f, false);
            ExplodeTorqueSlider.ValueChanged += (float value) => { ExplodeTorque = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ExplodeTorque = ExplodeTorqueSlider.Value; };


#if DEBUG
            //ConsoleController.ShowMessage("分离铰链添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            ExplodeForceSlider.DisplayInMapper = value;
            ExplodeTorqueSlider.DisplayInMapper = value;

        }

        private ExplosiveBolt EB;

        protected override void OnSimulateStart()
        {
            EB = GetComponent<ExplosiveBolt>();
            EB.explodePower = ExplodeForce;
            EB.explodeTorquePower = ExplodeTorque;
        }

    }


}
