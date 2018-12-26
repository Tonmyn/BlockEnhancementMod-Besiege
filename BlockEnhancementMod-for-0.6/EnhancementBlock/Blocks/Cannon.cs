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
    public class CannonScript : EnhancementBlock
    {

        public MSlider StrengthSlider;
        public MSlider IntervalSlider;
        public MSlider RandomDelaySlider;
        public MSlider KnockBackSpeedSlider;

        public MToggle BullerCustomBulletToggle;
        public MToggle BulletInheritSizeToggle;
        public MSlider BulletMassSlider;
        public MSlider BulletDragSlider;
        public MToggle BulletTrailToggle;
        public MSlider BulletTrailLengthSlider;
        public MColourSlider BulletTrailColorSlider;

        public CanonBlock CB;
        public AudioSource AS;

        public float Strength = 1f;
        public float Interval = 0.25f;
        private readonly float intervalMin = EnhanceMore ? 0f : 0.1f;
        public float RandomDelay = 0.2f;
        private float orginRandomDelay = 0.2f;
        public float KnockBackSpeedZeroOne = 1f;
        private readonly float knockBackSpeedZeroOneMin = EnhanceMore ? 0f : 0.25f;
        private readonly float knockBackSpeedZeroOneMax = 1f;
        public float originalKnockBackSpeed = 8000;
        public bool firstShotFired = true;

        public bool ShootEnabled { get; set; } = true;

        public Bullet bullet;

        private float knockBackSpeed;
        private int BulletNumber = 1;

        //子弹类
        public class Bullet
        {
            public GameObject bulletObject;
            public Rigidbody rigidbody;
            public float Mass { get { return rigidbody.mass; } set { rigidbody.mass = Mathf.Clamp(value, 0.1f, value); } }
            public float Drag { get { return rigidbody.drag; } set { rigidbody.drag = value; } }

            private TrailRenderer TrailRenderer;
            public bool TrailEnable { get { return TrailRenderer.enabled; } set { TrailRenderer.enabled = value; } }
            public float TrailLength { get { return TrailRenderer.time; } set { TrailRenderer.time = value; } }
            public Color TrailColor { get { return TrailRenderer.material.color; } set { TrailRenderer.material.SetColor("_TintColor", value); } }

            public bool Custom { get; set; }
            public bool InheritSize { get; set; }

            internal CanonBlock CB;

            public Bullet(CanonBlock canonBlock)
            {
                CB = canonBlock;

                bulletObject = Instantiate(CB.boltObject.gameObject);
                bulletObject.SetActive(false);

                rigidbody = bulletObject.GetComponent<Rigidbody>();
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

                CB.boltObject.gameObject.SetActive(false);

                if (InheritSize)
                {
                    Vector3 scaleVector = Vector3.Scale(Vector3.one * Mathf.Min(cannon.localScale.x, cannon.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));

                    bulletObject.transform.localScale = CB.particles[0].transform.localScale = scaleVector;
                }
            }
        }

        public override void SafeAwake()
        {
            // Initialise some components and default values
            AS = BB.GetComponent<AudioSource>();
            CB = BB.GetComponent<CanonBlock>();
            bullet = new Bullet(CB);


            IntervalSlider = BB.AddSlider(LanguageManager.fireInterval, "Interval", Interval, intervalMin, 0.5f);
            IntervalSlider.ValueChanged += (float value) => { Interval = value; ChangedProperties(); };

            RandomDelaySlider = BB.AddSlider(LanguageManager.randomDelay, "RandomDelay", RandomDelay, 0f, 0.5f);
            RandomDelaySlider.ValueChanged += (float value) => { RandomDelay = value; ChangedProperties(); };

            KnockBackSpeedSlider = BB.AddSlider(LanguageManager.recoil, "KnockBackSpeed", KnockBackSpeedZeroOne, knockBackSpeedZeroOneMin, knockBackSpeedZeroOneMax);
            KnockBackSpeedSlider.ValueChanged += (float value) => { KnockBackSpeedZeroOne = value; ChangedProperties(); };

            #region 子弹控件初始化

            BullerCustomBulletToggle = BB.AddToggle(LanguageManager.customBullet, "Bullet", false);
            BullerCustomBulletToggle.Toggled += (bool value) => { BulletTrailToggle.DisplayInMapper = BulletDragSlider.DisplayInMapper = BulletMassSlider.DisplayInMapper = BulletInheritSizeToggle.DisplayInMapper = bullet.Custom = value; ChangedProperties(); };

            BulletInheritSizeToggle = BB.AddToggle(LanguageManager.inheritSize, "InheritSize", false);
            BulletInheritSizeToggle.Toggled += (bool value) => { bullet.InheritSize = value; ChangedProperties(); };

            BulletMassSlider = BB.AddSlider(LanguageManager.bulletMass, "BulletMass", 2f, 0.1f, 2f);
            BulletMassSlider.ValueChanged += (float value) => { bullet.Mass = value; ChangedProperties(); };

            BulletDragSlider = BB.AddSlider(LanguageManager.bulletDrag, "BulletDrag", 0.2f, 0.01f, 0.5f);
            BulletDragSlider.ValueChanged += (float value) => { bullet.Drag = value; ChangedProperties(); };

            BulletTrailToggle = BB.AddToggle(LanguageManager.trail, "Trail", false);
            BulletTrailToggle.Toggled += (bool value) => { bullet.TrailEnable = BulletTrailColorSlider.DisplayInMapper = BulletTrailLengthSlider.DisplayInMapper = value; ChangedProperties(); };

            BulletTrailLengthSlider = BB.AddSlider(LanguageManager.trailLength, "trail length", 1f, 0.2f, 2f);
            BulletTrailLengthSlider.ValueChanged += (float value) => { bullet.TrailLength = value; ChangedProperties(); };

            BulletTrailColorSlider = BB.AddColourSlider(LanguageManager.trailColor, "trail color", Color.yellow, false);
            BulletTrailColorSlider.ValueChanged += (Color value) => { bullet.TrailColor = value; ChangedProperties(); };

            #endregion

#if DEBUG
            ConsoleController.ShowMessage("加农炮添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            IntervalSlider.DisplayInMapper = value;
            RandomDelaySlider.DisplayInMapper = value;
            KnockBackSpeedSlider.DisplayInMapper = value;

            BullerCustomBulletToggle.DisplayInMapper = value && !StatMaster.isMP;
            BulletInheritSizeToggle.DisplayInMapper = value && /*customBullet*/bullet.Custom && !StatMaster.isMP;
            BulletMassSlider.DisplayInMapper = value && /*customBullet*/bullet.Custom && !StatMaster.isMP;
            BulletDragSlider.DisplayInMapper = value && /*customBullet*/bullet.Custom && !StatMaster.isMP;
            BulletTrailToggle.DisplayInMapper = value && bullet.Custom && !StatMaster.isMP;
            BulletTrailColorSlider.DisplayInMapper = /*Trail*/bullet.TrailEnable && !StatMaster.isMP;
            BulletTrailLengthSlider.DisplayInMapper = /*Trail*/bullet.TrailEnable && !StatMaster.isMP;

        }

        public override void ChangeParameter()
        {

            BulletInit();

            if (StatMaster.isMP) { bullet.Custom = bullet.TrailEnable = false; }

            if (!EnhancementEnabled)
            {
                CB.knockbackSpeed = originalKnockBackSpeed;
                CB.randomDelay = orginRandomDelay;

                bullet.Custom = bullet.TrailEnable = false;
            }

            firstShotFired = true;
            
            Strength = CB.StrengthSlider.Value;
            RandomDelay = Mathf.Clamp(RandomDelay, 0f, RandomDelay);
            knockBackSpeed = Mathf.Clamp(KnockBackSpeedZeroOne, knockBackSpeedZeroOneMin, knockBackSpeedZeroOneMax) * originalKnockBackSpeed;
            
            //独立自定子弹
            if (bullet.Custom)
            {
                bullet.CreateCustomBullet();    
            }
            //else
            //{
            //    if (Strength <= 20 || EnhanceMore || !StatMaster.isMP)
            //    {
            //        CB.knockbackSpeed = knockBackSpeed;
            //    }
            //}

            CB.randomDelay = 0;
            CB.knockbackSpeed= knockBackSpeed;    

            void BulletInit()
            {
                BulletNumber = 1;
                firstShotFired = true;
                bullet.Custom = BullerCustomBulletToggle.IsActive;
                bullet.Mass = BulletMassSlider.Value;
                bullet.Drag = BulletDragSlider.Value;
                bullet.InheritSize = BulletInheritSizeToggle.IsActive;
                bullet.TrailEnable = BulletTrailToggle.IsActive;
                bullet.TrailLength = BulletTrailLengthSlider.Value;
                bullet.TrailColor = BulletTrailColorSlider.Value;
            }

        }

        public override void BuildingUpdate()
        {
            if (!EnhanceMore && StatMaster.isMP)
            {
                if (CB.StrengthSlider.Value > 20 && KnockBackSpeedSlider.DisplayInMapper)
                {
                    KnockBackSpeedSlider.DisplayInMapper = false;
                }
                if (CB.StrengthSlider.Value <= 20 && !KnockBackSpeedSlider.DisplayInMapper)
                {
                    KnockBackSpeedSlider.DisplayInMapper = true;
                }
            }
            else
            {
                if (!KnockBackSpeedSlider.DisplayInMapper)
                {
                    KnockBackSpeedSlider.DisplayInMapper = true;
                }
            }
        }

        public override void SimulateUpdateAlways()
        {
            if (StatMaster.isClient) return;

            if (CB.ShootKey.IsReleased)
            {
                firstShotFired = true;
                ShootEnabled = true;
            }

            if (CB.ShootKey.IsDown && ShootEnabled)
            {
                StartCoroutine(Shoot());
            }
        }

        private IEnumerator Shoot()
        {
            ShootEnabled = false;

            if (BulletNumber > 0 || StatMaster.GodTools.InfiniteAmmoMode)
            {
                float randomDelay = UnityEngine.Random.Range(0f, RandomDelay);

                yield return new WaitForSeconds(randomDelay);

                if (bullet.Custom)
                {

                    //克隆子弹物体
                    var bulletClone = (GameObject)Instantiate(bullet.bulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
                    bulletClone.SetActive(true);
                    //子弹施加推力并且继承炮身速度
                    //try { bulletClone.GetComponent<Rigidbody>().velocity = CB.Rigidbody.velocity; } catch { }
                    bulletClone.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * Strength);
                    //炮身施加后坐力
                    //gameObject.GetComponent<Rigidbody>().AddForce(knockBackSpeed * Strength * Mathf.Min(bullet.bulletObject.transform.localScale.x, bullet.bulletObject.transform.localScale.z) * transform.up);

                    //if (!firstShotFired)
                    //{
                    //    //播放开炮音效和特效
                    //    foreach (var particle in CB.particles) { particle.Play(); }
                    //    CB.fuseParticles.Stop();
                    //    AS.Play();
                    //}

                }
                else if (CB.boltObject.gameObject.activeSelf == false)
                {

                    GameObject go = (GameObject)Instantiate(CB.boltObject.gameObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
                    go.SetActive(true);
                    go.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * Strength);

                }
            }
            if (!firstShotFired)
            {
                CB.Shoot();
            }

            BulletNumber--;
            firstShotFired = false;

            yield return new WaitForSeconds(Interval);
            if (EnhancementEnabled)
            {
                ShootEnabled = true;
            }
        }
    }
}


