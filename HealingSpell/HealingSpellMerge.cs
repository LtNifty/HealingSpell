using ThunderRoad;
using UnityEngine;

namespace HealingSpell
{
    public class HealingSpellMerge : SpellMergeData
    {
        public override void Merge(bool active)
        {
            base.Merge(active);
/*            if (active)
            {
                GameObject vfxOrb = Object.Instantiate(QuickHealSpellUtils.healingOrb, HealingSpell.playerCaster.magicSource);
                vfxOrb.transform.localPosition = Vector3.zero;
                vfxOrb.transform.localScale /= 11f;
                // var localScale = vfxOrb.transform.localScale;
                // Player.currentCreature.StartCoroutine(HealingSpell.LerpVfx(0.2f, vfxOrb, localScale, localScale * 2f));
            } else {
                Vector3 leftVelocity = Player.local.transform.rotation * PlayerControl.GetHand(Side.Left).GetHandVelocity();
                Vector3 rightVelocity = Player.local.transform.rotation * PlayerControl.GetHand(Side.Left).GetHandVelocity();

                if (leftVelocity.magnitude > HealingSpellScript.minHandVelocity && rightVelocity.magnitude > HealingSpellScript.minHandVelocity && currentCharge > HealingSpellScript.minCharge)
                {
                    Player.currentCreature.Heal(50f, Player.currentCreature);
                }
            }*/
        }
    }
}
