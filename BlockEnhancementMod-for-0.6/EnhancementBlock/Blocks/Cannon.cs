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
        private readonly float intervalMin = No8Workshop ? 0f : 0.1f;
        public float RandomDelay = 0.2f;
        public float KnockBackSpeedZeroOne = 1f;
        private readonly float knockBackSpeedZeroOneMin = No8Workshop ? 0f : 0.25f;
        private readonly float knockBackSpeedZeroOneMax = 1f;
        public float originalKnockBackSpeed = 8000;
        public bool firstShotFired = false;


        public Bullet bullet;
        //public bool customBullet = false;
        //public bool InheritSize = false;
        //public float BulletMass = 2f;
        //public float BulletDrag = 0.2f;
        //public bool Trail = false;
        //public float TrailLength = 1f;
        //public Color TrailColor = Color.yellow;
        //public TrailRenderer myTrailRenderer;


        private float timer;
        private float knockBackSpeed;
        private int BulletNumber = 1;

        //public GameObject BulletObject;
        //private GameObject customBulletObject;

       

        //子弹类
        public class Bullet
        {
            public GameObject gameObject;
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
                gameObject = CB.boltObject.gameObject;
                rigidbody = gameObject.GetComponent<Rigidbody>();
                TrailRenderer = gameObject.GetComponent<TrailRenderer>() ?? gameObject.AddComponent<TrailRenderer>();
                //TrailRenderer.enabled = Trail;
                TrailRenderer.autodestruct = false;
                TrailRenderer.receiveShadows = false;
                TrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                TrailRenderer.startWidth = 0.5f * gameObject.transform.localScale.magnitude;
                TrailRenderer.endWidth = 0.1f;
                //TrailRenderer.time = TrailLength;

                TrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
                //TrailRenderer.material.SetColor("_TintColor", Color.yellow);

                //if (Custom)
                //{
  
                //}
            }
            public void CreateCustomBullet()
            {
                GameObject cannon = CB.gameObject;

                gameObject = (GameObject)Instantiate(gameObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
                gameObject.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(cannon.transform.localScale.x, cannon.transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
                gameObject.SetActive(false);

                if (InheritSize) { CB.particles[0].transform.localScale = gameObject.transform.localScale; }
            }
        }

        //public class CustomBullet : Bullet
        //{
        //    public bool InheritSize;

        //    private GameObject cannon;

        //    public CustomBullet(CanonBlock canonBlock) : base(canonBlock)
        //    {
        //        cannon = CB.gameObject;

        //        gameObject = (GameObject)Instantiate(gameObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
        //        gameObject.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(cannon.transform.localScale.x,cannon.transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
        //        gameObject.SetActive(false);
        //        if (InheritSize) { CB.particles[0].transform.localScale = gameObject.transform.localScale; }
        //    }

        //    //public void InheritBulletSize()
        //    //{

        //    //}
        //}

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
            BullerCustomBulletToggle.Toggled += (bool value) => { BulletDragSlider.DisplayInMapper = BulletMassSlider.DisplayInMapper = BulletInheritSizeToggle.DisplayInMapper = bullet.Custom = value; ChangedProperties(); };

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

            BulletTrailToggle.DisplayInMapper = value;
            BulletTrailColorSlider.DisplayInMapper = /*Trail*/bullet.TrailEnable && !StatMaster.isMP;
            BulletTrailLengthSlider.DisplayInMapper = /*Trail*/bullet.TrailEnable && !StatMaster.isMP;

        }

        public override void ChangeParameter()
        {

            BulletInit();

            if (StatMaster.isMP) { /*customBullet = Trail = false;*/ bullet.Custom = bullet.TrailEnable = false; }

            if (!EnhancementEnabled)
            {
                CB.knockbackSpeed = originalKnockBackSpeed;

                //customBullet = Trail = false;

                bullet.Custom = bullet.TrailEnable = false;
            }

            CB.enabled = !bullet.Custom;
            //CB.enabled = !customBullet;
            //Strength = CB.StrengthSlider.Value;
            //BulletObject = CB.boltObject.gameObject;
            timer = Interval < intervalMin ? intervalMin : Interval;
            knockBackSpeed = Mathf.Clamp(KnockBackSpeedZeroOne, knockBackSpeedZeroOneMin, knockBackSpeedZeroOneMax) * originalKnockBackSpeed;

            if (bullet.Custom)
            {
                bullet.CreateCustomBullet();
            }
            else
            {
                Strength = CB.StrengthSlider.Value;
                CB.randomDelay = RandomDelay < 0 ? 0 : RandomDelay;
                if (Strength <= 20 || No8Workshop || !StatMaster.isMP)
                {
                    CB.knockbackSpeed = knockBackSpeed;
                }
            }

            ////独立自定子弹
            //if (customBullet)
            //{
            //    customBulletObject = (GameObject)Instantiate(BulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
            //    customBulletObject.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(transform.localScale.x, transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
            //    customBulletObject.SetActive(false);
            //    if (InheritSize) { CB.particles[0].transform.localScale = customBulletObject.transform.localScale; }
            //    Rigidbody bulletRigibody = customBulletObject.GetComponent<Rigidbody>();
            //    bulletRigibody.mass = BulletMass < 0.1f ? 0.1f : BulletMass;
            //    bulletRigibody.drag = bulletRigibody.angularDrag = BulletDrag;
            //}
            //else
            //{
            //    CB.randomDelay = RandomDelay < 0 ? 0 : RandomDelay;
            //    if (Strength <= 20 || No8Workshop || !StatMaster.isMP)
            //    {
            //        CB.knockbackSpeed = knockBackSpeed;
            //    }
            //}

            //GameObject bullet = customBullet ? customBulletObject : BulletObject;

            //if (Trail)
            //{
            //    myTrailRenderer = GetComponent<TrailRenderer>() ?? bullet.AddComponent<TrailRenderer>();
            //    myTrailRenderer.enabled = Trail;
            //    myTrailRenderer.autodestruct = false;
            //    myTrailRenderer.receiveShadows = false;
            //    myTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            //    myTrailRenderer.startWidth = 0.5f * bullet.transform.localScale.magnitude;
            //    myTrailRenderer.endWidth = 0.1f;
            //    myTrailRenderer.time = TrailLength;

            //    myTrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
            //    myTrailRenderer.material.SetColor("_TintColor", TrailColor);
            //}
            //else
            //{
            //    myTrailRenderer = bullet.GetComponent<TrailRenderer>();
            //    if (myTrailRenderer)
            //    {
            //        myTrailRenderer.enabled = Trail;
            //    }
            //}

            void BulletInit()
            {
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
            if (StatMaster.isMP)
            {
                if (BulletTrailToggle.DisplayInMapper)
                {
                    BulletTrailToggle.DisplayInMapper = false;
                }
            }
            if (!No8Workshop && StatMaster.isMP)
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

            if (CB.ShootKey.IsDown && /*customBullet*/bullet.Custom)
            {
                CB.StopAllCoroutines();
            }

            if (CB.ShootKey.IsDown && Interval > 0)
            {
                if (timer > Interval)
                {
                    timer = 0;
                    if (/*customBullet*/bullet.Custom)
                    {
                        StartCoroutine(Shoot());
                    }
                    else
                    {
                        if (firstShotFired)
                        {
                            CB.Shoot();
                        }
                        if (!firstShotFired)
                        {
                            firstShotFired = true;
                        }
                    }
                }
                else
                {
                    timer += Time.deltaTime;
                }
            }
            else if (CB.ShootKey.IsReleased)
            {
                timer = Interval;
                firstShotFired = false;
            }

 
        }

        private IEnumerator Shoot()
        {
            if (BulletNumber > 0)
            {
                float randomDelay = UnityEngine.Random.Range(0f, RandomDelay);

                yield return new WaitForSeconds(randomDelay);

                //克隆子弹物体
                var bulletClone = /*(GameObject)Instantiate(customBulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);*/(GameObject)Instantiate(bullet.gameObject, CB.boltSpawnPos);
                bulletClone.SetActive(true);
                //子弹施加推力并且继承炮身速度
                try { bulletClone.GetComponent<Rigidbody>().velocity = CB.Rigidbody.velocity; } catch { }
                bulletClone.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * Strength);
                //炮身施加后坐力
                gameObject.GetComponent<Rigidbody>().AddForce(knockBackSpeed * Strength * Mathf.Min(/*customBulletObject*/bullet.gameObject.transform.localScale.x, /*customBulletObject*/bullet.gameObject.transform.localScale.z) * transform.up);
                //播放开炮音效和特效
                foreach (var particle in CB.particles) { particle.Play(); }
                CB.fuseParticles.Stop();
                AS.Play();
            }

            if (!StatMaster.GodTools.InfiniteAmmoMode)
            {
                BulletNumber--;
            }
            else
            {
                BulletNumber = 1;
            }
        }
    }
}


