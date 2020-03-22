using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;

namespace BlockEnhancementMod
{
    class UnpoweredCog : EnhancementBlock
    {

        MKey SwitchKey;
        MToggle HeldToggle;

        public bool Switch = false;
        //public bool Held = false;

        private ConfigurableJoint hinge;
        private bool state =  false;
        private bool lastState = false;

        public override void SafeAwake()
        {
            SwitchKey = /*BB.*/AddKey("Switch", "Switch", KeyCode.R);
            SwitchKey.InvokeKeysChanged();

            HeldToggle = /*BB.*/AddToggle("Toggle", "Toggle", /*Held*/false);
            //HeldToggle.Toggled += (value) => { Held = value; ChangedProperties(); };
        }

        //public override void DisplayInMapper(bool value)
        //{
        //    SwitchKey.DisplayInMapper = HeldToggle.DisplayInMapper = value;
        //}

        public override void OnSimulateStartClient()
        {
            if (!EnhancementEnabled) return;
            hinge = GetComponent<ConfigurableJoint>();
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (StatMaster.isClient) return;

            Debug.Log(hinge.targetPosition); Debug.Log(hinge.targetRotation);

            if (SwitchKey.IsPressed || SwitchKey.EmulationPressed())
            {
                state = !state;
                Debug.Log("switch");
               
            }
            if (/*Held*/HeldToggle.IsActive)
            {
                if (SwitchKey.IsReleased)
                {
                    state = !state;
                }
            }
            if (state != lastState)
            {
                lastState = state;

                if (state)
                {
                    MakeCogStatic();
                }
                else
                {
                    MakeCogNormal();
                }
            }
        }

        private void MakeCogNormal()
        {
            SoftJointLimitSpring softJointLimitSpring = this.hinge.angularXLimitSpring;
            softJointLimitSpring.damper = 0f;
            this.hinge.angularXLimitSpring = softJointLimitSpring;
            JointDrive jointDrive = this.hinge.angularXDrive;
            jointDrive.positionSpring = 0f;
            this.hinge.angularXDrive = jointDrive;
        }

        private void MakeCogStatic()
        {
            SoftJointLimitSpring softJointLimitSpring = this.hinge.angularXLimitSpring;
            softJointLimitSpring.damper = 10000f;
            this.hinge.angularXLimitSpring = softJointLimitSpring;
            JointDrive jointDrive = this.hinge.angularXDrive;
            jointDrive.positionSpring = 10000f;
            this.hinge.angularXDrive = jointDrive;
        }

    }
}
