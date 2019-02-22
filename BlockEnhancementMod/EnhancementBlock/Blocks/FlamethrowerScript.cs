using Modding;
using Modding.Blocks;
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

        MSlider thrustForceSlider;
        MColourSlider flameColorSlider;

        public float ThrustForce = 0f;       
        public Color FlameColor = Color.blue;
        public string FlameShader = "Particles/Additive";
        private Color orginFlameColor = Color.white;
        private string orginShader = "Particles/Alpha Blended";

        Rigidbody rigidbody;

        public override void SafeAwake()
        {

            thrustForceSlider = BB.AddSlider(LanguageManager.thrustForce, "Thrust Force", ThrustForce, 0f, 5f);
            thrustForceSlider.ValueChanged += (float value) => { ThrustForce = value; ChangedProperties(); };           
            flameColorSlider = BB.AddColourSlider(LanguageManager.flameColor, "Flame Color", FlameColor, false);
            flameColorSlider.ValueChanged += (Color value) => { FlameColor = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("喷火器添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            thrustForceSlider.DisplayInMapper = value;
            flameColorSlider.DisplayInMapper = value;
        }

        public override void ChangeParameter()
        {
            flamethrowerController = GetComponent<FlamethrowerController>();
            rigidbody = GetComponent<Rigidbody>();

            if (!EnhancementEnabled)
            {
                FlameColor = orginFlameColor;
                FlameShader = orginShader;             
            }
    
            flamethrowerController.fireParticles.GetComponent<ParticleSystemRenderer>().material.shader = Shader.Find(FlameShader);
            flamethrowerController.fireParticles.startColor = FlameColor;
        }

        public override void SimulateFixedUpdateAlways()
        {
            if (StatMaster.isClient) return;

            if (ThrustForce != 0 && flamethrowerController.isFlaming)
            {
                rigidbody.AddRelativeForce(-Vector3.forward * ThrustForce * 100f);
            }
        }   
    }
}
