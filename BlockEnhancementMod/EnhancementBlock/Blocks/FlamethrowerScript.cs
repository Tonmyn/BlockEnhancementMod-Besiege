using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class FlamethrowerScript: ChangeSpeedBlock
    {
        FlamethrowerController flamethrowerController;

        MSlider thrustForceSlider;
        MColourSlider flameColorSlider;

        //public float ThrustForce = 0f;       
        //public Color FlameColor = Color.white;
        public string FlameShader = "Particles/Additive";
        //private Color orginFlameColor = Color.white;
        //private string orginShader = "Particles/Alpha Blended";

        Rigidbody rigidbody;

        public override void SafeAwake()
        {

            thrustForceSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.ThrustForce, "Thrust Force", /*ThrustForce*/0f, 0f, 5f);
            //thrustForceSlider.ValueChanged += (float value) => { ThrustForce = value; ChangedProperties(); };           
            flameColorSlider = /*BB.*/AddColourSlider(LanguageManager.Instance.CurrentLanguage.FlameColor, "Flame Color", /*FlameColor*/Color.white, false);
            //flameColorSlider.ValueChanged += (Color value) => { FlameColor = value; ChangedProperties(); };

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("喷火器添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            thrustForceSlider.DisplayInMapper = value;
            flameColorSlider.DisplayInMapper = value;
            base.DisplayInMapper(value);
        }

        public override void OnSimulateStartClient()
        {   
            if (EnhancementEnabled)
            {
                flamethrowerController = GetComponent<FlamethrowerController>();
                rigidbody = GetComponent<Rigidbody>();
                SpeedSlider = thrustForceSlider;

                flamethrowerController.fireParticles.GetComponent<ParticleSystemRenderer>().material.shader = Shader.Find(FlameShader);
                flamethrowerController.fireParticles.startColor = /*FlameColor*/flameColorSlider.Value;
            }
        }
        public override void SimulateFixedUpdate_EnhancementEnabled()
        {
            if (StatMaster.isClient) return;

            if (/*ThrustForce*/thrustForceSlider.Value != 0 && flamethrowerController.isFlaming)
            {
                rigidbody.AddRelativeForce(-Vector3.forward * /*ThrustForce*/thrustForceSlider.Value * 100f);
            }
        }   
    }
}
