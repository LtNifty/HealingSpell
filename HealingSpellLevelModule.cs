using System.Collections;
using ThunderRoad;

namespace HealingSpell
{
    public class HealingSpellLevelModule : LevelModule
    {
        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onCreatureKill += OnCreatureKill;
            return base.OnLoadCoroutine();
        }

        public void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
                if (Player.currentCreature && !Player.currentCreature.isKilled && collisionInstance?.sourceColliderGroup?.imbue?.spellCastBase?.id == "Heal")
                    Player.currentCreature.Heal(HealingSpell.healingOptions.imbueHealOnKill, Player.currentCreature);
        }
    }
}