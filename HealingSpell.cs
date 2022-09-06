using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using UnityEngine.VFX;

namespace HealingSpell
{
    public enum HealType
    {
        Constant,
        Crush,
        Smash,
        SmashAndCrush,
    }

    public class HealingSpellData : MonoBehaviour
    {
        // GENERAL SETTINGS
        public HealType healTypeEnum;
        public bool useAOEfx;
        public float healAmount;
        public float minimumChargeForHeal;

        // CRUSH SETTINGS
        public float gripThreshold;

        // SMASH SETTINGS
        public float smashDistance;
        public float smashVelocity;

        // CONSTANT SETTINGS
        public float healthPerSecond;
        public float manaDrainPerSecond;
    }

    public class HealingSpell : SpellCastCharge
    {
        public static HealingSpellData data;
        private GameObject vfxOrb;
        private GameObject vfxAoe;
        private bool healSuccess;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
            var source = QuickHealSpellUtils.LoadResources<GameObject>(new string[2] { "healing_orb.prefab", "healing_aoe.prefab" }, "healingsfx_assets_all");
            QuickHealSpellUtils.healingOrb = source.First(x => x.name == "healing_orb");
            QuickHealSpellUtils.healingAoe = source.First(x => x.name == "healing_aoe");
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();

            if (!spellCaster.isFiring) currentCharge = 0.0f;
            if (currentCharge < data.minimumChargeForHeal) return;

            bool isGripping = PlayerControl.GetHand(spellCaster.ragdollHand.side).gripPressed 
                && PlayerControl.GetHand(spellCaster.ragdollHand.side).GetAverageCurlNoThumb() > data.gripThreshold 
                && Player.currentCreature.equipment.GetHeldItem(spellCaster.ragdollHand.side) == null;
            if (spellCaster.mana.currentMana > 0.0 
                && (data.healTypeEnum == HealType.Constant 
                || (data.healTypeEnum == HealType.Crush && isGripping) 
                || (data.healTypeEnum == HealType.SmashAndCrush && isGripping)))
            {
                    HealSelf();
            }
            else if (data.healTypeEnum == HealType.Smash || data.healTypeEnum == HealType.SmashAndCrush)
            {
                Vector3 spellPos = spellCaster.magicSource.position;
                Vector3 chestPos = Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Chest).position;
                float distance = Vector3.Distance(spellPos, chestPos);
                Vector3 dir = chestPos - spellPos;

                if (distance < data.smashDistance 
                    && Vector3.Dot(Player.local.transform.rotation * PlayerControl.GetHand(spellCaster.ragdollHand.side).GetHandVelocity(), dir) > data.smashVelocity)
                    HealSelf();
            }
        }

        private void HealSelf()
        {
            if (data.healTypeEnum == HealType.Crush 
                || data.healTypeEnum == HealType.Smash 
                || data.healTypeEnum == HealType.SmashAndCrush)
            {
                Player.currentCreature.Heal(data.healAmount, Player.currentCreature);
                healSuccess = true;
                Fire(false);
            } else {
                // Constantly heals the player every frame
                Player.currentCreature.Heal(Time.deltaTime * data.healthPerSecond, Player.currentCreature);

                // Constantly drains mana every frame if they aren't max health
                if (Player.currentCreature.currentHealth != Player.currentCreature.maxHealth)
                    spellCaster.mana.ConsumeMana(Time.deltaTime * data.manaDrainPerSecond);

                // If the player has run out of mana or has no health to heal, stop firing
                if (spellCaster.mana.currentMana <= 0)
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
                if (data.useAOEfx)
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
                if (data.useAOEfx)
                {
                    Player.currentCreature.StartCoroutine(LerpVfx(0.2f, vfxAoe, localScale, localScale * 2f));
                }
                PlayerControl.GetHand(spellCaster.ragdollHand.side).HapticPlayClip(Catalog.gameData.haptics.telekinesisThrow, 2f);
                healSuccess = false;
            }
            else
            {
                Player.currentCreature.StartCoroutine(LerpVfx(0.2f, vfxOrb, vfxOrb.transform.localScale, Vector3.zero));
                if (data.useAOEfx)
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
            UnityEngine.Object.Destroy(vfx);
        }
    }
}
