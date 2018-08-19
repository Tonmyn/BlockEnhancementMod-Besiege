using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class FlamethrowerScript: EnhancementBlock
    {
        FlamethrowerController flamethrowerController;

        public float ThrustForce = 0f;

        public Color FlameColor = Color.white;

        MSlider thrustForceSlider;

        MColourSlider flameColorSlider;

        Rigidbody rigidbody;

        ParticleSystem particleSystem;

        protected override void SafeAwake()
        {
            flamethrowerController = GetComponent<FlamethrowerController>();
            particleSystem = flamethrowerController.fireParticles;
            rigidbody = GetComponent<Rigidbody>();

            thrustForceSlider = AddSlider(LanguageManager.thrustForce, "Thrust Force", ThrustForce, 0f, 5f, false);
            thrustForceSlider.ValueChanged += (float value) => { ThrustForce = value; ChangedProperties(); };           
            flameColorSlider = AddColorSlider(LanguageManager.flameColor, "Flame Color", FlameColor, false);
            flameColorSlider.ValueChanged += (Color value) => { FlameColor = particleSystem.startColor = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { ThrustForce = thrustForceSlider.Value; FlameColor = flameColorSlider.Value; };
#if DEBUG
            ConsoleController.ShowMessage("转向关节添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            thrustForceSlider.DisplayInMapper = value;
            flameColorSlider.DisplayInMapper = value;
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (ThrustForce != 0 && flamethrowerController.isFlaming)
            {
                rigidbody.AddRelativeForce(-Vector3.forward * ThrustForce * 100f);
            }
        }
    }
}
