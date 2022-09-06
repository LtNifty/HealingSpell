using System.Collections;
using ThunderRoad;

namespace HealingSpell
{
    public class HealingSpellLevelModule : LevelModule
    {
        // GENERAL SETTINGS
        public HealType healTypeEnum;
        public bool useAOEfx;
        public float healAmount;
        public float minimumChargeForHeal;
        public float imbueHealOnKill;

        // CRUSH SETTINGS
        public float gripThreshold;

        // SMASH SETTINGS
        public float smashDistance;
        public float smashVelocity;

        // CONSTANT SETTINGS
        public float healthPerSecond;
        public float manaDrainPerSecond;

        public override IEnumerator OnLoadCoroutine()
        {
            HealingSpell.data = GameManager.local.gameObject.AddComponent<HealingSpellData>();
            EventManager.onCreatureKill += OnCreatureKill;
            return base.OnLoadCoroutine();
        }

        public override void Update()
        {
            base.Update();
            InitValues();
        }

        private void InitValues()
        {
            HealingSpell.data.healTypeEnum = healTypeEnum;
            HealingSpell.data.useAOEfx = useAOEfx;
            HealingSpell.data.healAmount = healAmount;
            HealingSpell.data.minimumChargeForHeal = minimumChargeForHeal;
            HealingSpell.data.gripThreshold = gripThreshold;
            HealingSpell.data.smashDistance = smashDistance;
            HealingSpell.data.smashVelocity = smashDistance;
            HealingSpell.data.healthPerSecond = healthPerSecond;
            HealingSpell.data.manaDrainPerSecond = manaDrainPerSecond;
        }

        public void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
                if (Player.currentCreature && !Player.currentCreature.isKilled && collisionInstance?.sourceColliderGroup?.imbue?.spellCastBase?.id == "Heal")
                    Player.currentCreature.Heal(imbueHealOnKill, Player.currentCreature);
        }
    }
}