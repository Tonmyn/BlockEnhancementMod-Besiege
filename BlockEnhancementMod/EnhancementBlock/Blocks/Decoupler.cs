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
        private float orginExplodeForce = 1000;
        private float orginExplodeTorque = 2000;

        private ExplosiveBolt EB;

        public override void SafeAwake()
        {

            ExplodeForceSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.explodeForce, "ExplodeForce", ExplodeForce, 0, 3000f);
            ExplodeForceSlider.ValueChanged += (float value) => { ExplodeForce = value; ChangedProperties(); };

            ExplodeTorqueSlider = BB.AddSlider(LanguageManager.Instance.CurrentLanguage.explodeTorque, "ExplodeTorque", ExplodeTorque, 0, 2500f);
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

        public override void OnSimulateStart_Client()
        {         
            if (EnhancementEnabled)
            {
                EB = GetComponent<ExplosiveBolt>();

                //if (!EnhancementEnabled)
                //{
                //    ExplodeForce = orginExplodeForce;
                //    ExplodeTorque = orginExplodeTorque;
                //}

                EB.explodePower = ExplodeForce;
                EB.explodeTorquePower = ExplodeTorque;
            }
        }  
    }


}
