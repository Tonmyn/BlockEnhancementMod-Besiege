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
        public MToggle CustomBulletToggle;
        public MToggle InheritSizeToggle;
        public MSlider BulletMassSlider;
        public MSlider BulletDragSlider;
        public MToggle TrailToggle;
        public MSlider TrailLengthSlider;
        public MColourSlider TrailColorSlider;

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
        public bool customBullet = false;
        public bool InheritSize = false;
        public float BulletMass = 2f;
        public float BulletDrag = 0.2f;
        public bool Trail = false;
        public float TrailLength = 1f;
        public Color TrailColor = Color.yellow;
        public TrailRenderer myTrailRenderer;
     
        private float timer;
        private float knockBackSpeed;
        private int BulletNumber = 1;

        public GameObject BulletObject;
        private GameObject customBulletObject;

        public override void SafeAwake()
        {

            IntervalSlider = BB.AddSlider(LanguageManager.fireInterval, "Interval", Interval, intervalMin, 0.5f);
            IntervalSlider.ValueChanged += (float value) => { Interval = value; ChangedProperties(); };

            RandomDelaySlider = BB.AddSlider(LanguageManager.randomDelay, "RandomDelay", RandomDelay, 0f, 0.5f);
            RandomDelaySlider.ValueChanged += (float value) => { RandomDelay = value; ChangedProperties(); };

            KnockBackSpeedSlider = BB.AddSlider(LanguageManager.recoil, "KnockBackSpeed", KnockBackSpeedZeroOne, knockBackSpeedZeroOneMin, knockBackSpeedZeroOneMax);
            KnockBackSpeedSlider.ValueChanged += (float value) => { KnockBackSpeedZeroOne = value; ChangedProperties(); };

            CustomBulletToggle = BB.AddToggle(LanguageManager.customBullet, "Bullet", customBullet);
            CustomBulletToggle.Toggled += (bool value) => { BulletDragSlider.DisplayInMapper = BulletMassSlider.DisplayInMapper = InheritSizeToggle.DisplayInMapper = customBullet = value; ChangedProperties(); };

            InheritSizeToggle = BB.AddToggle(LanguageManager.inheritSize, "InheritSize", InheritSize);
            InheritSizeToggle.Toggled += (bool value) => { InheritSize = value; ChangedProperties(); };

            BulletMassSlider = BB.AddSlider(LanguageManager.bulletMass, "BulletMass", BulletMass, 0.1f, 2f);
            BulletMassSlider.ValueChanged += (float value) => { BulletMass = value; ChangedProperties(); };

            BulletDragSlider = BB.AddSlider(LanguageManager.bulletDrag, "BulletDrag", BulletDrag, 0.01f, 0.5f);
            BulletDragSlider.ValueChanged += (float value) => { BulletDrag = value; ChangedProperties(); };

            TrailToggle = BB.AddToggle(LanguageManager.trail, "Trail", Trail);
            TrailToggle.Toggled += (bool value) => { Trail = TrailColorSlider.DisplayInMapper = TrailLengthSlider.DisplayInMapper = value; ChangedProperties(); };

            TrailLengthSlider = BB.AddSlider(LanguageManager.trailLength, "trail length", TrailLength, 0.2f, 2f);
            TrailLengthSlider.ValueChanged += (float value) => { TrailLength = value; ChangedProperties(); };

            TrailColorSlider = BB.AddColourSlider(LanguageManager.trailColor, "trail color", TrailColor,false);
            TrailColorSlider.ValueChanged += (Color value) => { TrailColor = value; ChangedProperties(); };

            // Initialise some components and default values
            AS = BB.GetComponent<AudioSource>();
            CB = BB.GetComponent<CanonBlock>();

#if DEBUG
            ConsoleController.ShowMessage("加农炮添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            IntervalSlider.DisplayInMapper = value;
            RandomDelaySlider.DisplayInMapper = value;
            KnockBackSpeedSlider.DisplayInMapper = value;
            CustomBulletToggle.DisplayInMapper = value && !StatMaster.isMP;
            InheritSizeToggle.DisplayInMapper = value && customBullet && !StatMaster.isMP;
            BulletMassSlider.DisplayInMapper = value && customBullet && !StatMaster.isMP;
            BulletDragSlider.DisplayInMapper = value && customBullet && !StatMaster.isMP;

            TrailToggle.DisplayInMapper = value;
            TrailColorSlider.DisplayInMapper = Trail && !StatMaster.isMP;
            TrailLengthSlider.DisplayInMapper = Trail && !StatMaster.isMP;

        }

        public override void ChangeParameter()
        {
            if (StatMaster.isMP) { customBullet = Trail = false; }

            if (!EnhancementEnabled)
            {
                CB.knockbackSpeed = originalKnockBackSpeed;

                customBullet = Trail = false;
            }

            CB.enabled = !customBullet;
            Strength = CB.StrengthSlider.Value;
            BulletObject = CB.boltObject.gameObject;
            timer = Interval < intervalMin ? intervalMin : Interval;
            knockBackSpeed = Mathf.Clamp(KnockBackSpeedZeroOne, knockBackSpeedZeroOneMin, knockBackSpeedZeroOneMax) * originalKnockBackSpeed;               

            //独立自定子弹
            if (customBullet)
            {
                customBulletObject = (GameObject)Instantiate(BulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
                customBulletObject.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(transform.localScale.x, transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
                customBulletObject.SetActive(false);
                if (InheritSize) { CB.particles[0].transform.localScale = customBulletObject.transform.localScale; }
                Rigidbody bulletRigibody = customBulletObject.GetComponent<Rigidbody>();
                bulletRigibody.mass = BulletMass < 0.1f ? 0.1f : BulletMass;
                bulletRigibody.drag = bulletRigibody.angularDrag = BulletDrag;
            }
            else
            {
                CB.randomDelay = RandomDelay < 0 ? 0 : RandomDelay;
                if (Strength <= 20 || No8Workshop || !StatMaster.isMP)
                {
                    CB.knockbackSpeed = knockBackSpeed;
                }
            }

            GameObject bullet = customBullet ? customBulletObject : BulletObject;

            if (Trail)
            {
                myTrailRenderer = GetComponent<TrailRenderer>() ?? bullet.AddComponent<TrailRenderer>();
                myTrailRenderer.enabled = Trail;
                myTrailRenderer.autodestruct = false;
                myTrailRenderer.receiveShadows = false;
                myTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                myTrailRenderer.startWidth = 0.5f * bullet.transform.localScale.magnitude;
                myTrailRenderer.endWidth = 0.1f;
                myTrailRenderer.time = TrailLength;

                myTrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
                myTrailRenderer.material.SetColor("_TintColor", TrailColor);
            }
            else
            {
                myTrailRenderer = bullet.GetComponent<TrailRenderer>();
                if (myTrailRenderer)
                {
                    myTrailRenderer.enabled = Trail;
                }
            }

        }

        public override void BuildingUpdate()
        {
            if (StatMaster.isMP)
            {
                if (TrailToggle.DisplayInMapper)
                {
                    TrailToggle.DisplayInMapper = false;
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

            if (CB.ShootKey.IsDown && Interval > 0)
            {
                if (timer > Interval)
                {
                    timer = 0;
                    if (customBullet)
                    {
                        StartCoroutine(Shoot());
                    }
                    else
                    {
                        CB.Shoot();
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
            }

            if (CB.ShootKey.IsDown && customBullet)
            {
                CB.StopAllCoroutines();
            }
        }

        private IEnumerator Shoot()
        {
            if (BulletNumber > 0)
            {
                float randomDelay = UnityEngine.Random.Range(0f, RandomDelay);

                yield return new WaitForSeconds(randomDelay);

                var bullet = (GameObject)Instantiate(customBulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);

                bullet.SetActive(true);
                try { bullet.GetComponent<Rigidbody>().velocity = CB.Rigidbody.velocity; } catch { }
                bullet.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * Strength);

                gameObject.GetComponent<Rigidbody>().AddForce(knockBackSpeed * Strength * Mathf.Min(customBulletObject.transform.localScale.x, customBulletObject.transform.localScale.z) * transform.up);

                foreach (var particle in CB.particles)
                {
                    particle.Play();
                }
                AS.Play();
                CB.fuseParticles.Stop();
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


