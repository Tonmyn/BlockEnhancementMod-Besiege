using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BlockEnhancementMod;
using System.Reflection;
using System;
using Modding;
using Modding.Blocks;


namespace BlockEnhancementMod.Blocks
{
    public class CannonScript :CanonBlock_GenericEnhanceScript
    {
        public MToggle BullerCustomBulletToggle;
        //public MToggle BulletInheritSizeToggle;
        public MSlider BulletMassSlider;
        public MSlider BulletDragSlider;
        public MToggle BulletTrailToggle;
        public MSlider BulletTrailLengthSlider;
        public MSlider BulletDelayCollisionSlider;
        public MColourSlider BulletTrailColorSlider;

        public Bullet bullet;
        private bool lastInfinite = false;
        private bool firstShoot = true;
        private bool isRound = false;

        public class Bullet
        {
            public GameObject bulletObject;
            public Rigidbody rigidbody;
            public float Mass { get { return rigidbody.mass; } set { rigidbody.mass = Mathf.Clamp(value, 0.1f, value); } }
            public float Drag { get { return rigidbody.drag; } set { rigidbody.drag = value; } }
            public float DelayCollision { get; set; }

            private TrailRenderer TrailRenderer;
            public bool TrailEnable { get { return TrailRenderer.enabled; } set { TrailRenderer.enabled = value; } }
            public float TrailLength { get { return TrailRenderer.time; } set { TrailRenderer.time = value; } }
            public Color TrailColor { get { return TrailRenderer.material.color; } set { TrailRenderer.material.SetColor("_TintColor", value); } }

            public bool Custom { get; set; }
            //public bool InheritSize { get; set; }

            internal CanonBlock CB;

            public Bullet(CanonBlock canonBlock)
            {
                CB = canonBlock;

                bulletObject = Instantiate(CB.boltObjects[0].gameObject);
                bulletObject.SetActive(false);

                rigidbody = bulletObject.GetComponent<Rigidbody>();
                rigidbody.detectCollisions = false;
                bulletObject.GetComponent<Collider>().enabled = false;
                TrailRenderer = bulletObject.GetComponent<TrailRenderer>() ?? bulletObject.AddComponent<TrailRenderer>();
                TrailRenderer.autodestruct = false;
                TrailRenderer.receiveShadows = false;
                TrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                TrailRenderer.startWidth = 0.5f * bulletObject.transform.localScale.magnitude;
                TrailRenderer.endWidth = 0.1f;

                TrailRenderer.material = new Material(Shader.Find("Particles/Additive"));

              
            }
            public void CreateCustomBullet()
            {
                Transform cannon = CB.transform;

                bulletObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                CB.boltObjects[0].gameObject.SetActive(false);

                //if (InheritSize)
                //{
                //    Vector3 scaleVector = Vector3.Scale(Vector3.one * Mathf.Min(cannon.localScale.x, cannon.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));

                //    bulletObject.transform.localScale = CB.particles[0].transform.localScale = scaleVector;
                //}
            }
        }

        public override void SafeAwake()
        {
            base.SafeAwake();
            bullet = new Bullet(CB);

            #region 子弹控件初始化

            BullerCustomBulletToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.CustomBullet, "Bullet", false);
            //BulletInheritSizeToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.InheritSize, "InheritSize", false);
            BulletMassSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.BulletMass, "BulletMass", 2f, 0.1f, 2f);
            BulletDragSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.BulletDrag, "BulletDrag", 0.2f, 0.01f, 0.5f);
            BulletDelayCollisionSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.BulletDelayCollision, "Delay Collision", 0.2f, 0f, 0.5f);
            BulletTrailToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.Trail, "Trail", false);
            BulletTrailLengthSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.TrailLength, "trail length", 1f, 0.2f, 2f);
            BulletTrailColorSlider = /*BB.*/AddColourSlider(LanguageManager.Instance.CurrentLanguage.TrailColor, "trail color", Color.yellow, false);

            CB.AmmunitionType.ValueChanged += (value) => 
            {
                isRound = (CanonBlock.AmmoType)value == CanonBlock.AmmoType.Round;

                if (!isRound)
                {
                    EnhancementToggle.SetValue(false);
                }

                EnhancementToggle.DisplayInMapper = isRound;
                DisplayInMapper(EnhancementEnabled && isRound);
            };
            #endregion

#if DEBUG
            ConsoleController.ShowMessage("加农炮添加进阶属性");
#endif
        }
        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);

            
            //var isRound = ((CanonBlock.AmmoType)CB.AmmunitionType.Value == CanonBlock.AmmoType.Round);
            var isSingle = StatMaster.IsLevelEditorOnly || !StatMaster.isMP;
            var isCustomBullet = value && isRound && isSingle && BullerCustomBulletToggle.IsActive ;
            var isTrail = BulletTrailToggle.IsActive && isCustomBullet;

            BullerCustomBulletToggle.DisplayInMapper = value && isSingle && isRound;

            //BulletInheritSizeToggle.DisplayInMapper = value && isCustomBullet;
            BulletMassSlider.DisplayInMapper = isCustomBullet;
            BulletDragSlider.DisplayInMapper = isCustomBullet;
            BulletDelayCollisionSlider.DisplayInMapper = isCustomBullet;
            BulletTrailToggle.DisplayInMapper = isCustomBullet;
            BulletTrailColorSlider.DisplayInMapper = isTrail ;
            BulletTrailLengthSlider.DisplayInMapper = isTrail ;
        }

        public override void OnSimulateStartAlways()
        {
            base.OnSimulateStartAlways();

            lastInfinite = StatMaster.GodTools.InfiniteAmmoMode;

            BulletInit();
            isRound = (CanonBlock.AmmoType)CB.AmmunitionType.Value == CanonBlock.AmmoType.Round;

            if (StatMaster.isMP || !isRound) { bullet.Custom = bullet.TrailEnable = false; }

            //独立自定子弹
            if (bullet.Custom)
            {
                bullet.CreateCustomBullet();
            }

            if (EnhancementEnabled || CB.boltObjects[0].gameObject.activeSelf == false || !isRound)
            {
                CB.randomDelay = 0f;
            }

            void BulletInit()
            {
                bullet.Custom = BullerCustomBulletToggle.IsActive;
                bullet.Mass = BulletMassSlider.Value;
                bullet.Drag = BulletDragSlider.Value;
                bullet.DelayCollision = BulletDelayCollisionSlider.Value;
                //bullet.InheritSize = BulletInheritSizeToggle.IsActive;
                bullet.TrailEnable = BulletTrailToggle.IsActive;
                bullet.TrailLength = BulletTrailLengthSlider.Value;
                bullet.TrailColor = BulletTrailColorSlider.Value;
            }

        }
        public override void SimulateUpdateAlways_EnhancementEnable()
        {
  //虽然是空的但是必须要
        }
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();

            if (StatMaster.isClient || !isRound) return;

            if (CB.ShootKey.IsReleased || CB.ShootKey.EmulationReleased())
            {
                if (StatMaster.GodTools.InfiniteAmmoMode) ShootEnabled = true;
                StopCoroutine(Shoot());
                firstShoot = true;
            }

            if ((CB.ShootKey.IsHeld ||CB.ShootKey.EmulationHeld())&& ShootEnabled)
            {
                StopCoroutine(Shoot());
                StartCoroutine(Shoot());
            }
            else
            {
                StopCoroutine(Shoot());
            }

            if (EnhancementEnabled && lastInfinite != StatMaster.GodTools.InfiniteAmmoMode)
            {
                lastInfinite = StatMaster.GodTools.InfiniteAmmoMode;

                if (lastInfinite) ShootEnabled = true;
            }
        }
        protected override IEnumerator Shoot()
        {
            ShootEnabled = false;
            float randomDelay = 0f;

            if (CB.boltObjects[0].gameObject.activeSelf == false)
            {
                if (EnhancementEnabled)
                {
                    if (bullet.Custom)
                    {
                        StartCoroutine(shoot(bullet.bulletObject));       
                    }
                    else
                    {
                        StartCoroutine(shoot(CB.boltObjects[0].gameObject));
                    }
                }
                else
                {
                    StartCoroutine(shoot(CB.boltObjects[0].gameObject));
                }
            }
            else
            {
                if (EnhancementEnabled)
                {
                    if (bullet.Custom)
                    {
                        StartCoroutine(shoot(bullet.bulletObject));
                    }
                    else
                    {
                        StartCoroutine(shoot());
                    }
                }
            }

            yield return new WaitForSeconds(IntervalSlider.Value + randomDelay);
            if (StatMaster.GodTools.InfiniteAmmoMode && EnhancementEnabled) ShootEnabled = true;
            yield break;

            IEnumerator shoot(GameObject bulletObject = null)
            {
                randomDelay = UnityEngine.Random.Range(0f, RandomDelaySlider.Value);
                yield return new WaitForSeconds(randomDelay);
                if (bulletObject != null)
                {
                    var go = (GameObject)Instantiate(bulletObject, transform.TransformPoint(CB.boltSpawnPos), CB.boltSpawnRot);
                    go.AddComponent<DelayCollision>().Delay = bullet.DelayCollision;
                    go.SetActive(true);
                    go.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * CB.StrengthSlider.Value);
                }
                if (!firstShoot) { CB.Shoot(); }
                firstShoot = false;
                yield break;
            }
        }
    }
}



