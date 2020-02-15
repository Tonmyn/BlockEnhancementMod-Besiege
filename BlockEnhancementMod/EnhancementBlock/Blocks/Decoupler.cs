using Modding;
using Modding.Blocks;
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
        public float ExplodeForce;
        public float ExplodeTorque;

        private ExplosiveBolt EB;

        public override void SafeAwake()
        {

            ExplodeForceSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.ExplodeForce, "ExplodeForce", ExplodeForce, 0, 3000f);
            ExplodeForceSlider.ValueChanged += (float value) => { ExplodeForce = value; ChangedProperties(); };

            ExplodeTorqueSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.ExplodeTorque, "ExplodeTorque", ExplodeTorque, 0, 2500f);
            ExplodeTorqueSlider.ValueChanged += (float value) => { ExplodeTorque = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("分离铰链添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            ExplodeForceSlider.DisplayInMapper = value;
            ExplodeTorqueSlider.DisplayInMapper = value;
        }

        public override void OnSimulateStartClient()
        {         
            if (EnhancementEnabled)
            {
                EB = GetComponent<ExplosiveBolt>();
 
                EB.explodePower = ExplodeForce;
                EB.explodeTorquePower = ExplodeTorque;
            }
        }  
    }


}
