using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class HingeScript : EnhancementBlock
    {
        MToggle springToggle;
        MSlider springSlider,damperSlider;

        ConfigurableJoint hingeJoint;
        public override void SafeAwake()
        {
            base.SafeAwake();
            springToggle = AddToggle("Spring Toggle", "spring toggle", false);
            springSlider = AddSlider("Spring", "spring", 1f, 0f, 10f);
            damperSlider = AddSlider("Damper", "damper", 0f, 0f, 10f);
        }

        public override void DisplayInMapper(bool enhance)
        {
            base.DisplayInMapper(enhance);
            springToggle.DisplayInMapper = enhance;

            springSlider.DisplayInMapper = damperSlider.DisplayInMapper = enhance & springToggle.IsActive;

        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            base.OnSimulateStart_EnhancementEnabled();
            hingeJoint = GetComponent<ConfigurableJoint>();

            if (springToggle.IsActive)
            {
                var joint = hingeJoint.angularXDrive;
                joint.positionSpring = springSlider.Value * 1000f;
                joint.positionDamper = damperSlider.Value * 1000f;
                hingeJoint.angularXDrive = joint;
            }
        }
    }
}
