using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace HealingSpell
{
    public class HealingSpellLevelModule : LevelModule
    {
        // GENERAL SETTINGS
        [Tooltip("Sets which type of Healing Spell mode to use.")]
        public HealType healTypeEnum;
        [Tooltip("Turns on/off the angelic ring AOE effect.")]
        public bool useAOEfx;
        [Tooltip("Determines how much the player gets healed when using the Smash, Crush, or SmashAndCrush mode.")]
        [Range(0, 100)]
        public float healAmount;
        [Tooltip("Determines how much you need to charge the spell before you can heal using the Smash, Crush, or SmashAndCrush mode.")]
        [Range(0, 1)]
        public float minimumChargeForHeal;
        [Tooltip("Determines how much you heal upon killing an enemy with a weapon imbued with the Healing Spell.")]
        [Range(0, 100)]
        public float imbueHealOnKill;

        // CRUSH SETTINGS
        [Tooltip("Determines how much grip is required to register a Crush.")]
        [Range(0, 1)]
        public float gripThreshold;

        // SMASH SETTINGS
        [Tooltip("Determines how much distance should be between the charge and the player for a Smash.")]
        [Range(0, 1)]
        public float smashDistance;
        [Tooltip("Determines how much velocty a charge should have for a Smash.")]
        [Range(0, 1)]
        public float smashVelocity;

        // CONSTANT SETTINGS
        [Tooltip("Determines how much health the player should gain per second using Constant.")]
        [Range(0, 100)]
        public float healthPerSecond;
        [Tooltip("Determines how much mana the player should lose per second using Constant.")]
        [Range(0, 100)]
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