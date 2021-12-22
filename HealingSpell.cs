using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using UnityEngine.VFX;

namespace HealingSpell
{
    internal enum HealType
    {
        Crush,
        Smash,
        Constant,
        SmashAndCrush,
    }
    
    public class HealingSpell : SpellCastCharge
    {
        private static HealType healTypeEnum;
        private GameObject vfxOrb;
        private GameObject vfxAoe;
        private bool healSuccess;

        // GENERAL SETTINGS
        public string healType;
        public bool useAOEFX;
        public float baseHeal;
        public float healChargePercent;
        
        // CRUSH SETTINGS
        public float gripThreshold;

        // SMASH SETTINGS
        public float smashDistance;
        public float smashVelocity;

        // CONSTANT SETTINGS
        public float constantBaseHeal;
        public float constantExchangeRateConsumption;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
            var source = QuickHealSpellUtils.LoadResources<GameObject>(new string[2] {"healing_orb.prefab", "healing_aoe.prefab"}, "healingsfx_assets_all");
            QuickHealSpellUtils.healingOrb = source.First(x => x.name == "healing_orb");
            QuickHealSpellUtils.healingAoe = source.First(x => x.name == "healing_aoe");
            imbueEnabled = false;
            VerifyType();
        }

        private void VerifyType()
        {
            switch(healType.ToLower())
            {
                default:
                    Debug.LogError("No valid type has been assigned to QuickHealSpell.");
                    break;
                case "smash":
                    healTypeEnum = HealType.Smash;
                    break;
                case "crush":
                    healTypeEnum = HealType.Crush;
                    break;
                case "constant":
                    healTypeEnum = HealType.Constant;
                    break;
                case "smashandcrush":
                    healTypeEnum = HealType.SmashAndCrush;
                    break;
            }
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();
            if (!spellCaster.isFiring)
                currentCharge = 0.0f;

            if (currentCharge < healChargePercent / 100.0)
                return;

            bool isGripping = PlayerControl.GetHand(spellCaster.ragdollHand.side).gripPressed &&
                PlayerControl.GetHand(spellCaster.ragdollHand.side).GetAverageCurlNoThumb() > gripThreshold;
            if (healTypeEnum == HealType.Constant || (healTypeEnum == HealType.Crush && isGripping) || (healTypeEnum == HealType.SmashAndCrush && isGripping))
            {
                if (spellCaster.mana.currentMana > 0.0)
                    HealSelf();
            }
            else if (healTypeEnum == HealType.Smash || healTypeEnum == HealType.SmashAndCrush)
            {
                Vector3 spellPos = spellCaster.magicSource.position;
                Vector3 chestPos = Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Chest).position;
                float distance = Vector3.Distance(spellPos, chestPos);
                Vector3 dir = chestPos - spellPos;

                if (distance < smashDistance && Vector3.Dot(Player.local.transform.rotation * PlayerControl.GetHand(spellCaster.ragdollHand.side).GetHandVelocity(), dir) > smashVelocity)
                    HealSelf();
            }
        }

        private void HealSelf()
        {
            if (healTypeEnum == HealType.Crush || healTypeEnum == HealType.Smash || healTypeEnum == HealType.SmashAndCrush)
            {
                Player.currentCreature.Heal(baseHeal, Player.currentCreature);
                healSuccess = true;
                Fire(false);
            }
            else
            {
                // Constantly heals the player every frame
                Player.currentCreature.Heal(Time.deltaTime * baseHeal / constantBaseHeal, Player.currentCreature);

                // Constantly drains mana every frame
                spellCaster.mana.currentMana -= Time.deltaTime * constantExchangeRateConsumption * (baseHeal / constantBaseHeal);

                // If the player has run out of mana or has no health to heal, stop firing
                if (spellCaster.mana.currentMana <= 0 || Player.currentCreature.currentHealth >= Player.currentCreature.maxHealth)
                    Fire(false);
            }
        }

        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active)
            {
                vfxOrb = Object.Instantiate(QuickHealSpellUtils.healingOrb, spellCaster.magicSource);
                vfxOrb.transform.localPosition = Vector3.zero;
                vfxOrb.transform.localScale /= 11f;
                if (useAOEFX)
                {
                    vfxAoe = Object.Instantiate(QuickHealSpellUtils.healingAoe, spellCaster.magicSource);
                    vfxAoe.transform.localPosition = Vector3.zero;
                    vfxAoe.transform.localScale /= 11f;
                }
                return;
            }

            if (healSuccess)
            {
                /* Extract's commented out code. Unsure of what it does.
                 * var temp = this.chargeEffectData;

                Debug.Log(temp.modules.Count);
                foreach (var g in temp.modules.Where(x => x.stepCustomId == "2"))
                {
                    Debug.Log(g.stepCustomId);
                    var effect = g.Spawn(temp, false);
                    effect.Play();

                    Timing.RunCoroutine(DoActionAfter(1f, () => effect.Despawn()));
                }
                 */

                var localScale = vfxOrb.transform.localScale;
                Player.currentCreature.StartCoroutine(LerpVfx(0.2f, vfxOrb, localScale, localScale * 2f));
                if (useAOEFX)
                {
                    Player.currentCreature.StartCoroutine(LerpVfx(0.2f, vfxAoe, localScale, localScale * 2f));
                }
                PlayerControl.GetHand(spellCaster.ragdollHand.side).HapticPlayClip(Catalog.gameData.haptics.telekinesisThrow, 2f);
                healSuccess = false;
            }
            else
            {
                Player.currentCreature.StartCoroutine(LerpVfx(0.2f, vfxOrb, vfxOrb.transform.localScale, Vector3.zero));
                if (useAOEFX)
                {
                    Player.currentCreature.StartCoroutine(LerpVfx(0.2f, vfxAoe, vfxAoe.transform.localScale, Vector3.zero));

                }
            }
            spellCaster.isFiring = false;
            spellCaster.grabbedFire = false;
            currentCharge = 0.0f;
            spellCaster.telekinesis.TryRelease(false);
        }

        private static IEnumerator<float> LerpVfx(float seconds, GameObject vfx, Vector3 startScale, Vector3 endScale)
        {
            if (vfx == null) yield break;
            
            var time = 0.0f;
            vfx.transform.SetParent(null);
            vfx.GetComponent<VisualEffect>().playRate = 4f;
            vfx.GetComponent<VisualEffect>().Stop();
            while (time < 1.0)
            {
                time += Time.fixedDeltaTime / (seconds / 2f);
                vfx.transform.localScale = Vector3.Lerp(startScale, endScale, time);
                yield return Time.fixedDeltaTime;
            }
            time = 0.0f;
            while (time < 1.0)
            {
                time += Time.fixedDeltaTime / (float)(1.0 - seconds / 2.0);
                yield return Time.fixedDeltaTime;
            }
            Object.Destroy(vfx);
        }
    }
}
