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
        SliderBlock sliderBlock;
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
            base.SafeAwake();

            lockToggle = AddToggle("Lock", LanguageManager.Instance.CurrentLanguage.LockTarget, false);
            limitSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Limit, "Limit", /*Limit*/1f, 0f, 2f);
            //LimitSlider.ValueChanged += (float value) => { Limit = value; ChangedProperties(); };
            extendSlider = AddSlider("Extend", LanguageManager.Instance.CurrentLanguage.Extend, 0f, 0f, limitSlider.Value);
            extendSlider.ValueChanged += extendValueChanged;
            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/1, LanguageManager.Instance.CurrentLanguage.WoodenHardness/*, false*/);
            //HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            sliderBlock = GetComponent<SliderBlock>();

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

        public override void OnPaste()
        {
            ////base.OnPaste();
            //Debug.Log("粘贴");

            //extendValueChanged(extendSlider.Value);
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
                var orginPos = Vector3.zero;
                if (sliderBlock.jointTrigger != null)
                {
                    orginPos = sliderBlock.jointTrigger.position;
                }
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
                    //ConfigurableJoint.connectedAnchor -= transform.forward * extendSlider.Value;
                    ConfigurableJoint.connectedAnchor = ConfigurableJoint.connectedBody.transform.InverseTransformPoint(orginPos);
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
            //Debug.Log("??" + BB.PlacementComplete + EnhancementEnabled);
            if (!BB.PlacementComplete) return;
            deltaValue = value - lastValue;
            lastValue = value;
         
            if (EnhancementEnabled == false) return;
            //Debug.Log(deltaValue);
            transform.position += transform.forward * deltaValue;
            sliderBlock.Position += transform.forward * deltaValue;
            var triggerForJoint = sliderBlock.jointTrigger;
            if (triggerForJoint != null)
            {
                triggerForJoint.position -= triggerForJoint.forward * deltaValue;
            }
        }
    }
}
