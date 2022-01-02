using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;
using System.Collections;

namespace BlockEnhancementMod
{
    class SliderScript : ChangeHardnessBlock
    {

        //MMenu HardnessMenu;

        MSlider limitSlider;
        MSlider extendSlider;
        MToggle lockToggle;
        //public int HardnessIndex = 1;
        //private int orginHardnessIndex = 1;
        //public float Limit = 1;
        //private float orginLimit = 1;

        //ConfigurableJoint ConfigurableJoint;
        private float lastValue = 0f;
        private float deltaValue = 0f;
        public override void SafeAwake()
        {
            lockToggle = AddToggle("Lock", LanguageManager.Instance.CurrentLanguage.LockTarget, false);
            limitSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Limit, "Limit", /*Limit*/1f, 0f, 2f);
            //LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedProperties(); };
            extendSlider = AddSlider("Extend", LanguageManager.Instance.CurrentLanguage.Extend, 0f, 0f, limitSlider.Value);
            extendSlider.ValueChanged += extendValueChanged;
            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/1, LanguageManager.Instance.CurrentLanguage.WoodenHardness/*, false*/);
            //HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            base.SafeAwake();
#if DEBUG
            ConsoleController.ShowMessage("滑块添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            limitSlider.DisplayInMapper = value;
            lockToggle.DisplayInMapper = value;
            extendSlider.DisplayInMapper = value;
            base.DisplayInMapper(value);
        }

        public override void OnPlaced()
        {
            extendValueChanged(extendSlider.Value);
        }
        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                ConfigurableJoint = GetComponent<ConfigurableJoint>();
                hardness = new Hardness(ConfigurableJoint);

                StartCoroutine(wait());
            }    

            IEnumerator wait()
            {
             
                yield return new WaitUntil(() => ConfigurableJoint.connectedBody != null);
                //if (!EnhancementEnabled)
                //{
                //    Limit = orginLimit;
                //    HardnessIndex = orginHardnessIndex;
                //}
                if (lockToggle.IsActive)
                {
                    ConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
                }
                else
                {
                    ConfigurableJoint.autoConfigureConnectedAnchor = false;
                    ConfigurableJoint.connectedAnchor += Vector3.right * extendSlider.Value;
                }

                SoftJointLimit limit = ConfigurableJoint.linearLimit;
                limit.limit = /*Limit =*/ Mathf.Abs(/*Limit*/limitSlider.Value);
                ConfigurableJoint.linearLimit = limit;

                hardness.SwitchWoodHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);
                yield break;
            }
        }

        private void extendValueChanged(float value)
        {
            if (!BB.PlacementComplete) return;
            deltaValue = value - lastValue;
            lastValue = value;

            if (EnhancementEnabled == false) return;

            transform.position += transform.forward * deltaValue;
            var triggerForJoint = transform.FindChild("TriggerForJoint");
            if (triggerForJoint != null)
            {
                triggerForJoint.position -= triggerForJoint.forward * deltaValue;
            }
        }
    }
}
