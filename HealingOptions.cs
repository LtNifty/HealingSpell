using System;

namespace HealingSpell
{
    [Serializable]
    public class HealingOptions
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
    }
}
