using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {

        //MMenu ExplosionTypeMenu;
        //MToggle RocketPodToggle;
        MToggle GuidedRocketToggle;
        MSlider GuidedRocketTorqueSlider;
        MKey LockTargetKey;

        List<string> explosionTypes;
        

        public enum ExplosionType
        {
            rocket = 0,
            bomb = 1,
            grenade = 2
        }

        public bool rocketPodIsActivated = false;
        public bool guidedRocketIsActivated = false;
        public int noOfRocketsInPod = 18;
        public bool hasFired = false;
        float torque = 100f;
        public Transform target;
        public TimedRocket rocket;
        //public bool exploding = false;
        //public int explosionType = 0;

        protected override void SafeStart()
        {
            //explosionTypes = new List<string> { "Rocket", "Bomb", "Grenade" };
            //ExplosionTypeMenu = new MMenu("ExplosionType", (int)RocketScript.ExplosionType.rocket, explosionTypes, false);
            //CurrentMapperTypes.Add(ExplosionTypeMenu);
            //ExplosionTypeMenu.ValueChanged += (int value) =>
            //{
            //    ExplosionTypeMenu.Value = explosionType = value;
            //    ChangedPropertise();
            //    BesiegeConsoleController.ShowMessage(value.ToString());
            //};

            //RocketPodToggle = new MToggle("火箭巢", "RocketPod", rocketPodIsActivated);
            //RocketPodToggle.Toggled += (bool value) => { rocketPodIsActivated = value; ChangedPropertise(); };
            //CurrentMapperTypes.Add(RocketPodToggle);

            GuidedRocketToggle = new MToggle("追踪目标", "TrackingRocket", guidedRocketIsActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketIsActivated = GuidedRocketTorqueSlider.DisplayInMapper = LockTargetKey.DisplayInMapper = value;
                ChangedPropertise();
            };
            CurrentMapperTypes.Add(GuidedRocketToggle);

            LockTargetKey = new MKey("锁定目标", "lockTarget", KeyCode.Delete);
            LockTargetKey.KeysChanged += ChangedPropertise;
            CurrentMapperTypes.Add(LockTargetKey);

            GuidedRocketTorqueSlider = new MSlider("火箭扭转力度", "torqueOnRocket", torque, 0, 1000, false);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedPropertise(); };
            CurrentMapperTypes.Add(GuidedRocketTorqueSlider);

#if DEBUG
            BesiegeConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            //RocketPodToggle.DisplayInMapper = value;
            //ExplosionTypeMenu.DisplayInMapper = value;
            GuidedRocketToggle.DisplayInMapper = value;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketIsActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketIsActivated;
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

                    //if (bd.HasKey("bmt-" + ExplosionTypeMenu.Key))
                    //{
                    //    ExplosionTypeMenu.Value = bd.ReadInt("bmt-" + ExplosionTypeMenu.Key);
                    //}
                    //if (bd.HasKey("bmt-" + RocketPodToggle.Key))
                    //{
                    //    RocketPodToggle.IsActive = rocketPodIsActivated = bd.ReadBool("bmt-" + RocketPodToggle.Key);
                    //}
                    if (bd.HasKey("bmt-" + GuidedRocketToggle.Key))
                    {
                        GuidedRocketToggle.IsActive = guidedRocketIsActivated = bd.ReadBool("bmt-" + GuidedRocketToggle.Key);
                    }
                    if (bd.HasKey("bmt-" + GuidedRocketTorqueSlider.Key))
                    {
                        GuidedRocketTorqueSlider.Value = torque = bd.ReadFloat("bmt-" + GuidedRocketTorqueSlider.Value);
                    }
                    if (bd.HasKey("bmt-" + LockTargetKey.Key))
                    {
                        int index = 0;
                        foreach (string str in bd.ReadStringArray("bmt-" + LockTargetKey.Key))
                        {
                            LockTargetKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
                        }
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
                    //blockinfo.BlockData.Write("bmt-" + ExplosionTypeMenu.Key, ExplosionTypeMenu.Value);
                    //blockinfo.BlockData.Write("bmt-" + RocketPodToggle.Key, RocketPodToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + GuidedRocketToggle.Key, GuidedRocketToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + GuidedRocketTorqueSlider.Key, GuidedRocketTorqueSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + LockTargetKey.Key, LockTargetKey.Serialize().RawValue);
                    break;
                }

            }
        }

        protected override void OnSimulateStart()
        {
            rocket = gameObject.GetComponent<TimedRocket>();
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (LockTargetKey.IsDown)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.collider.transform;
                }
            }
            if (guidedRocketIsActivated && rocket.hasFired)
            {
                if (target != null)
                {
                    // Calculating the rotating axis
                    Vector3 velocityNormarlised = GetComponent<Rigidbody>().velocity.normalized;
                    Vector3 positionDiff = target.position - transform.position;
                    float angle = Vector3.Angle(positionDiff, velocityNormarlised);
                    Vector3 rotatingAxis = -Vector3.Cross(positionDiff, velocityNormarlised);

                    // if the velocity is more than 90 degree apart from the target direction, use maximum torque
                    // otherwise use proportional torque.
                    if (angle > 90)
                    {
                        transform.GetComponent<Rigidbody>().AddTorque(torque * rotatingAxis);
                    }
                    else
                    {
                        transform.GetComponent<Rigidbody>().AddTorque(torque / 90 * angle * rotatingAxis);
                    }
                }

            }
        }
        //void OnCollisionEnter(Collision collision)
        //{
        //    BesiegeConsoleController.ShowMessage("collision detected");
        //    if (rocket.hasFired)
        //    {
        //        if (!exploding)
        //        {
        //            exploding = true;
        //            StartCoroutine(Explode());
        //        }
        //    }
        //}
        //void OnCollisionStay(Collision coll)
        //{
        //    if (rocket.hasFired)
        //    {
        //        if (!exploding)
        //        {
        //            exploding = true;
        //            StartCoroutine(Explode());
        //        }
        //    }
        //}

        //IEnumerator Explode()
        //{
        //    if (explosionType == (int) ExplosionType.bomb)
        //    {
        //        GameObject explo = (GameObject) Instantiate(PrefabMaster.BlockPrefabs[23].gameObject,transform.position,transform.rotation);
        //        explo.transform.localScale = Vector3.one * 0.01f;
        //        ExplodeOnCollideBlock ac = explo.GetComponent<ExplodeOnCollideBlock>();
        //        ac.radius = 7f;
        //        ac.power = 2100f;
        //        ac.torquePower = 100000;
        //        ac.upPower = 0;
        //        ac.Explodey();
        //        Destroy(this.gameObject);
        //    }
        //    //else if (explosionType == (int)ExplosionType.grenade)
        //    //{
        //    //    GameObject explo = (GameObject)GameObject.Instantiate(PrefabMaster.BlockPrefabs[54].gameObject, this.transform.position, this.transform.rotation);
        //    //    explo.transform.localScale = Vector3.one * 0.01f;
        //    //    ControllableBomb ac = explo.GetComponent<ControllableBomb>();
        //    //    ac.radius = 3 * ECS.RangeMultiplierOfExplosion;
        //    //    ac.power = 1500 * ECS.PowerMultiplierOfExplosion;
        //    //    ac.randomDelay = 0.00001f;
        //    //    ac.upPower = 0f;
        //    //    ac.StartCoroutine_Auto(ac.Explode());
        //    //    explo.AddComponent<TimedSelfDestruct>();
        //    //    Destroy(this.gameObject);
        //    //}
        //    //else if (explosionType == (int)ExplosionType.rocket)
        //    //{
        //    //    GameObject explo = (GameObject)GameObject.Instantiate(PrefabMaster.BlockPrefabs[59].gameObject, this.transform.position, this.transform.rotation);
        //    //    explo.transform.localScale = Vector3.one * 0.01f;
        //    //    TimedRocket ac = explo.GetComponent<TimedRocket>();
        //    //    ac.SetSlip(Color.white);
        //    //    ac.radius = 3 * ECS.RangeMultiplierOfExplosion;
        //    //    ac.power = 1500 * ECS.PowerMultiplierOfExplosion;
        //    //    ac.randomDelay = 0.000001f;
        //    //    ac.upPower = 0;
        //    //    ac.StartCoroutine(ac.Explode(0.01f));
        //    //    explo.AddComponent<TimedSelfDestruct>();
        //    //    Destroy(this.gameObject);
        //    //}
        //    yield break;
        //}
    }
}


