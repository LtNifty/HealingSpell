using System.Collections;
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
    }

    public class HealingSpell : SpellCastCharge
    {
        private GameObject vfxOrb;
        private GameObject vfxAoe;
        private bool healSuccess;

        public static Vector3 ringLoc;
        public static bool isRingOn = false;
        bool crushHealReady = false;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
            var source = QuickHealSpellUtils.LoadResources<GameObject>(new string[2] { "healing_orb.prefab", "healing_aoe.prefab" }, "healingfx_assets_all");
            QuickHealSpellUtils.healingOrb = source.First(x => x.name == "healing_orb");
            QuickHealSpellUtils.healingAoe = source.First(x => x.name == "healing_aoe");
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();

            if (crushHealReady &&
                PlayerControl.GetHand(spellCaster.ragdollHand.side).gripPressed &&
                PlayerControl.GetHand(spellCaster.ragdollHand.side).GetAverageCurlNoThumb() > HealingSpellScript.crushGripThreshold &&
                spellCaster.mana.creature.equipment.GetHeldItem(spellCaster.ragdollHand.side) == null)
            {
                spellCaster.mana.ConsumeMana(HealingSpellScript.crushManaCost);
                spellCaster.mana.creature.Heal(HealingSpellScript.crushHealAmt, spellCaster.mana.creature);
            }

            // Forcing it to be false every time, the player *must*
            // crush the spell while they are holding it, otherwise on the next UpdateCaster()
            // this gets set to false so they can't crush
            crushHealReady = false;

            if (!spellCaster.isFiring) 
                return;

            switch (HealingSpellScript.healType)
            {
                case HealType.Constant:
                    if (currentCharge >= HealingSpellScript.constantMinCharge && spellCaster.mana.CanConsumeMana(HealingSpellScript.constantMPS))
                    {
                        // If the player has run out of mana or has no health to heal, stop firing
                        if (spellCaster.mana.currentMana <= 0)
                            Fire(false);

                        // Constantly drains mana every frame if they aren't max health
                        if (Player.currentCreature.currentHealth != Player.currentCreature.maxHealth)
                            spellCaster.mana.ConsumeMana(Time.deltaTime * HealingSpellScript.constantMPS);

                        // Constantly heals the player every frame
                        spellCaster.mana.creature.Heal(Time.deltaTime * HealingSpellScript.constantHPS, Player.currentCreature);
                    }
                    break;
                case HealType.Crush:
                    crushHealReady = currentCharge >= HealingSpellScript.crushMinCharge && spellCaster.mana.CanConsumeMana(HealingSpellScript.crushManaCost);
                    break;
                case HealType.Smash:
                    Vector3 spellPos = spellCaster.magicSource.position;
                    Vector3 chestPos = Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Chest).position;
                    float distance = Vector3.Distance(spellPos, chestPos);
                    Vector3 dir = chestPos - spellPos;

                    if (currentCharge >= HealingSpellScript.smashMinCharge && 
                        spellCaster.mana.CanConsumeMana(HealingSpellScript.smashManaCost) &&
                        Vector3.Dot(Player.local.transform.rotation * PlayerControl.GetHand(spellCaster.ragdollHand.side).GetHandVelocity(), dir) > HealingSpellScript.smashVelocity &&
                        distance < HealingSpellScript.smashDistance)
                    {
                        spellCaster.mana.ConsumeMana(HealingSpellScript.smashManaCost);
                        spellCaster.mana.creature.Heal(HealingSpellScript.smashHealAmt, spellCaster.mana.creature);
                        healSuccess = true;
                        Fire(false);
                    }
                    break;
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
                if (HealingSpellScript.useAOEfx)
                {
                    vfxAoe = Object.Instantiate(QuickHealSpellUtils.healingAoe, spellCaster.magicSource);
                    vfxAoe.transform.localPosition = Vector3.zero;
                    vfxAoe.transform.localScale /= 11f;
                }
            }
            else
            {
                if (healSuccess)
                {
                    GameManager.local.StartCoroutine(LerpVfx(0.1f, 0.1f, vfxOrb, vfxOrb.transform.localScale, vfxOrb.transform.localScale * 2f));

                    if (HealingSpellScript.useAOEfx)
                    {
                        GameManager.local.StartCoroutine(LerpVfx(0.1f, 0.1f, vfxAoe, vfxAoe.transform.localScale, vfxAoe.transform.localScale * 2f));
                    }

                    PlayerControl.GetHand(spellCaster.ragdollHand.side).HapticPlayClip(Catalog.gameData.haptics.telekinesisThrow, 2f);
                    healSuccess = false;
                }
                else
                {
                    GameManager.local.StartCoroutine(LerpVfx(0.1f, 0.1f, vfxOrb, vfxOrb.transform.localScale, Vector3.zero));

                    if (HealingSpellScript.useAOEfx)
                    {
                        GameManager.local.StartCoroutine(LerpVfx(0.1f, 0.1f, vfxAoe, vfxAoe.transform.localScale, Vector3.zero));
                    }
                }

                spellCaster.isFiring = false;
                spellCaster.grabbedFire = false;
                currentCharge = 0.0f;
                spellCaster.telekinesis.TryRelease(false);
            }
        }

        public static IEnumerator<float> LerpVfx(float seconds, float delay, GameObject vfx, Vector3 startScale, Vector3 endScale)
        {
            if (vfx == null) yield break;
            
            vfx.transform.SetParent(null);
            vfx.GetComponent<VisualEffect>().playRate = 4f;
            vfx.GetComponent<VisualEffect>().Stop();

            var time = 0.0f;
            while (time < seconds)
            {
                time += Time.fixedDeltaTime;
                vfx.transform.localScale = Vector3.Lerp(startScale, endScale, time / seconds);
                yield return Time.fixedDeltaTime;
            }
            time = 0.0f;
            while (time < delay)
            {
                time += Time.fixedDeltaTime;
                yield return Time.fixedDeltaTime;
            }
            Object.Destroy(vfx);
        }

        public override bool OnCrystalUse(RagdollHand hand, bool active)
        {
            base.OnCrystalUse(hand, active);
            return true;
        }

        public override bool OnCrystalSlam(CollisionInstance collisionInstance)
        {
            base.OnCrystalSlam(collisionInstance);
            // Player.currentCreature.StartCoroutine(HealSlamCoroutine(collisionInstance));
            return true;
        }

        private IEnumerator HealSlamCoroutine(CollisionInstance collisionInstance)
        {
            if (!isRingOn)
            {
                isRingOn = true;
                ringLoc = collisionInstance.contactPoint;

                GameObject slamRingAOE = Object.Instantiate(QuickHealSpellUtils.healingAoe,
                                                collisionInstance.contactPoint + new Vector3(0, 0.1f, 0),
                                                Quaternion.Euler(90f, 0f, 0f));

                slamRingAOE.transform.localScale *= HealingSpellScript.slamRingMult;

                AudioSource source = slamRingAOE.AddComponent<AudioSource>();
                source.clip = HealingSpellScript.ringClips[0];
                source.maxDistance = 2.3394f * HealingSpellScript.slamRingMult;
                source.Play();

                yield return new WaitForSeconds(HealingSpellScript.slamDuration);

                GameManager.local.StartCoroutine(LerpVfx(0.1f, 
                                                         0.1f,
                                                         slamRingAOE,
                                                         slamRingAOE.transform.localScale,
                                                         Vector3.zero));

                source.Stop();
                source.clip = HealingSpellScript.ringClips[1];
                source.Play();

                isRingOn = false;
            }
        }
    }
}