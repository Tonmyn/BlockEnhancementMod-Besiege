using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {

        MMenu ExplosionTypeMenu;
        MToggle RocketPodToggle;
        MToggle GuidedRocketToggle;

        List<string> explosionTypes;

        public enum ExplosionType
        {
            rocket = 0,
            bomb = 1,
            granade = 2
        }

        public bool rocketPodIsActivated = false;
        public bool guidedRocketIsActivated = false;
        public int noOfRocketsInPod = 18;
        public bool hasFired = false;
        private float force = 1f;
        public Transform target;

        protected override void SafeStart()
        {
            explosionTypes = new List<string> { "Rocket", "Bomb", "Grenade" };
            ExplosionTypeMenu = new MMenu("ExplosionType", (int)RocketScript.ExplosionType.rocket, explosionTypes, false);
            CurrentMapperTypes.Add(ExplosionTypeMenu);
            ExplosionTypeMenu.ValueChanged += (int value) =>
            {
                ExplosionTypeMenu.Value = value;
                ChangedPropertise();
                BesiegeConsoleController.ShowMessage(value.ToString());
            };

            RocketPodToggle = new MToggle("火箭巢 Rocket Pod", "RocketPod", rocketPodIsActivated);
            RocketPodToggle.Toggled += (bool value) => { rocketPodIsActivated = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(RocketPodToggle);

            GuidedRocketToggle = new MToggle("追踪目标 Tracking Target", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) => { guidedRocketIsActivated = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(GuidedRocketToggle);

#if DEBUG
            BesiegeConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            RocketPodToggle.DisplayInMapper = value;
            ExplosionTypeMenu.DisplayInMapper = value;
            GuidedRocketToggle.DisplayInMapper = value;
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + ExplosionTypeMenu.Key))
                    {
                        ExplosionTypeMenu.Value = bd.ReadInt("bmt-" + ExplosionTypeMenu.Key);
                    }
                    if (bd.HasKey("bmt-" + RocketPodToggle.Key))
                    {
                        RocketPodToggle.IsActive = rocketPodIsActivated = bd.ReadBool("bmt-" + RocketPodToggle.Key);
                    }
                    if (bd.HasKey("bmt-" + GuidedRocketToggle.Key))
                    {
                        GuidedRocketToggle.IsActive = guidedRocketIsActivated = bd.ReadBool("bmt-" + GuidedRocketToggle.Key);
                    }
                    break;
                }

            }
        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            base.SaveConfiguration(mi);

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    blockinfo.BlockData.Write("bmt-" + ExplosionTypeMenu.Key, ExplosionTypeMenu.Value);
                    blockinfo.BlockData.Write("bmt-" + RocketPodToggle.Key, RocketPodToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + GuidedRocketToggle.Key, GuidedRocketToggle.IsActive);
                    break;
                }

            }
        }

        protected override void OnSimulateStart()
        {

        }

        protected override void OnSimulateFixedUpdate()
        {
            if (Input.GetMouseButtonDown(2))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.collider.transform;
                }

            }
            if (guidedRocketIsActivated && gameObject.GetComponent<TimedRocket>().hasFired)
            {
                if (target != null)
                {
                    //BesiegeConsoleController.ShowMessage("start guiding");
                    transform.rotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(target.position - transform.position), 0.8f);
                    //Vector3 targetDelta = target.position - transform.position;
                    //float angleDiff = Vector3.Angle(transform.forward, targetDelta);
                    //Vector3 cross = Vector3.Cross(transform.forward, targetDelta);
                    //transform.GetComponent<Rigidbody>().AddTorque(-cross * angleDiff * 100000f);
                }

            }
        }
    }
}


