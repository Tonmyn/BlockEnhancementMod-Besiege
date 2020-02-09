using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class CanonBlock_EnhanceScript : EnhancementBlock
    {
        public MSlider IntervalSlider;
        public MSlider RandomDelaySlider;
        public MSlider KnockBackSpeedSlider;

        public CanonBlock CB;
        public bool firstShotFired { get; protected set; } = true;

        public bool ShootEnabled { get; set; } = true;

        public override void SafeAwake()
        {
            CB = GetComponent<CanonBlock>();

            IntervalSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.FireInterval, "Interval", /*Interval*/0.25f, /*intervalMin*/EnhanceMore ? 0f : 0.1f, 0.5f);
            RandomDelaySlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.RandomDelay, "RandomDelay", /*RandomDelay*/0.2f, 0f, 0.5f);
            KnockBackSpeedSlider = /*BB.*/AddSlider(LanguageManager.Instance.CurrentLanguage.Recoil, "KnockBackSpeed", /*KnockBackSpeedZeroOne*/1f, /*knockBackSpeedZeroOneMin*/EnhanceMore ? 0f : 0.25f, /*knockBackSpeedZeroOneMax*/1f);
            CB.StrengthSlider.ValueChanged += (value) => 
            {
                if (!EnhanceMore && (StatMaster.isMP && Modding.Common.Player.GetAllPlayers().Count > 1))
                {
                    if (value > 20f)
                    {
                        KnockBackSpeedSlider.DisplayInMapper = false;
                        KnockBackSpeedSlider.Value = 1f;
                    }
                    else
                    {
                        KnockBackSpeedSlider.DisplayInMapper = true;
                    }
                }
            };
        }

        public override void OnSimulateStart_EnhancementEnabled()
        {
            if (StatMaster.isClient) return;

            CB.randomDelay = UnityEngine.Random.Range(0f, RandomDelaySlider.Value);
            CB.knockbackSpeed *= Mathf.Clamp(KnockBackSpeedSlider.Value, KnockBackSpeedSlider.Min, KnockBackSpeedSlider.Max);
        }

        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (StatMaster.isClient) return;

            if (CB.ShootKey.IsReleased)
            {
                firstShotFired = true;
                ShootEnabled = true;
                StopCoroutine(Shoot());
            }

            if (CB.ShootKey.IsHeld && ShootEnabled)
            {
                StartCoroutine(Shoot());
            }
        }

        protected virtual IEnumerator Shoot()
        {
            ShootEnabled = false;

            if (StatMaster.GodTools.InfiniteAmmoMode && !firstShotFired)
            {
                CB.Shoot();
            }
            else
            {
                firstShotFired = false;
            }

            yield return new WaitForSeconds(IntervalSlider.Value);
            ShootEnabled = true;
            yield break;
        }
    }
}
