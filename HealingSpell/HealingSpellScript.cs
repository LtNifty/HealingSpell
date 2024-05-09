using ThunderRoad;
using UnityEngine;

namespace HealingSpell
{
    public class HealingSpellScript : ThunderScript
    {
        public static ModOptionFloat[] zeroToOneHundered()
        {
            ModOptionFloat[] options = new ModOptionFloat[101];
            float val = 0;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 1f;
            }
            return options;
        }

        public static ModOptionFloat[] zeroToHundredWithTenths()
        {
            ModOptionFloat[] options = new ModOptionFloat[1001];
            float val = 0;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 0.1f;
            }
            return options;
        }

        public static ModOptionFloat[] zeroToOne()
        {
            ModOptionFloat[] options = new ModOptionFloat[11];
            float val = 0;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 0.1f;
            }
            return options;
        }

        /*
         * GENERAL SETTINGS
         */
        [ModOption(name: "Healing Type", tooltip: "Sets which type of Healing Spell mode to use.", defaultValueIndex = 0, order = 0)]
        public static HealType healType;

        [ModOption(name: "Use Ring Effect", tooltip: "Turns on/off the angelic ring AOE effect.", defaultValueIndex = 0, order = 1)]
        public static bool useAOEfx;

        /*
         * CONSTANT SETTINGS
         */
        [ModOption(name: "Health per Second", tooltip: "Determines how much health the player should gain per second in Constant mode.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 10, category = "Constant", order = 0)]
        public static float constantHPS;

        [ModOption(name: "Mana Drain per Second", tooltip: "Determines how much mana the player should lose per second in Constant mode.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 10, category = "Constant", order = 1)]
        public static float constantMPS;

        [ModOption(name: "Constant Minimum Charge", tooltip: "Determines how much charge the spell needs to start healing in Constant mode.", valueSourceName = nameof(zeroToOne), defaultValueIndex = 9, category = "Constant", order = 2)]
        public static float constantMinCharge;

        /*
         * CRUSH SETTINGS
         */
        [ModOption(name: "Crush Heal Amount", tooltip: "Determines how much health the player should gain in Crush mode.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 20, category = "Crush", order = 0)]
        public static float crushHealAmt;

        [ModOption(name: "Crush Mana Cost", tooltip: "Determines how much mana the player should lose in Crush mode.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 0, category = "Crush", order = 1)]
        public static float crushManaCost;

        [ModOption(name: "Crush Minimum Charge", tooltip: "Determines how much charge the spell needs to heal in Crush mode.", valueSourceName = nameof(zeroToOne), defaultValueIndex = 9, category = "Crush", order = 2)]
        public static float crushMinCharge;

        [ModOption(name: "Grip Threshold", tooltip: "Determines how much grip is required to heal in Crush mode.", valueSourceName = nameof(zeroToOne), defaultValueIndex = 7, category = "Crush", order = 3)]
        public static float crushGripThreshold;

        /*
         * SMASH SETTINGS
         */
        [ModOption(name: "Smash Heal Amount", tooltip: "Determines how much health the player should gain in Smash mode", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 20, category = "Smash", order = 0)]
        public static float smashHealAmt;

        [ModOption(name: "Smash Mana Cost", tooltip: "Determines how much mana the player should lose in Smash mode.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 0, category = "Smash", order = 1)]
        public static float smashManaCost;

        [ModOption(name: "Smash Minimum Charge", tooltip: "Determines how much charge the spell needs to heal in Smash mode.", valueSourceName = nameof(zeroToOne), defaultValueIndex = 9, category = "Smash", order = 2)]
        public static float smashMinCharge;

        [ModOption(name: "Required Distance", tooltip: "Determines how much distance should be between the charge and the player to heal in Smash mode.", valueSourceName = nameof(zeroToOne), defaultValueIndex = 4, category = "Smash", order = 3)]
        public static float smashDistance;

        [ModOption(name: "Required Velocity", tooltip: "Determines how much velocty a charge should have to heal in Smash mode.", valueSourceName = nameof(zeroToOne), defaultValueIndex = 5, category = "Smash", order = 4)]
        public static float smashVelocity;

        /*
         * IMBUE SETTINGS
         */
        [ModOption(name: "Heal on Kill", tooltip: "Determines how much the player gets healed on a kill with an imbued weapon.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 10, category = "Imbue")]
        public static float imbueHealOnKill;

        /*
         * STAFF SETTINGS
         */
        [ModOption(name: "Slam Duration", tooltip: "Determines how long the healing ring should exist in seconds.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 30, category = "Staff", order = 0)]
        public static float slamDuration;

        [ModOption(name: "Health per Second", tooltip: "Determines how much health the player should gain per second while standing in the ring.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 3, category = "Staff", order = 1)]
        public static float slamHPS;

        [ModOption(name: "Ring Size Multiplier", tooltip: "Determines how big the healing ring should be. Keep at 1 for default.", valueSourceName = nameof(zeroToHundredWithTenths), defaultValueIndex = 10, category = "Staff", order = 2)]
        public static float slamRingMult;

        /*
         *  MERGE SETTINGS
         */
        public static float minHandVelocity;
        public static float minCharge;

        public static AudioClip[] ringClips = new AudioClip[2];

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            LoadSFX();
            EventManager.onCreatureKill += OnCreatureKill;
        }

        private void LoadSFX()
        {
            Catalog.LoadAssetAsync<AudioClip>("ChillioX.HealingSpell.RingLoop", value => ringClips[0] = value, "ChillioX");
            Catalog.LoadAssetAsync<AudioClip>("ChillioX.HealingSpell.RingEnd", value => ringClips[1] = value, "ChillioX");

            if (ringClips.Length == 0)
            {
                Debug.LogError("(FromSoftwareParry) Unable to find any parry sound effects!");
            }
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
            if (HealingSpell.isRingOn)
            {
                Debug.LogWarning("distance: " + Vector2.Distance(Player.local.locomotion.rb.position, HealingSpell.ringLoc));
                Debug.LogWarning("in ring: " + (Vector2.Distance(Player.local.locomotion.rb.position, HealingSpell.ringLoc) <= 2.3394 * slamRingMult));
                if (Vector2.Distance(Player.local.locomotion.rb.position, HealingSpell.ringLoc) <= 2.3394 * slamRingMult)
                {
                    Player.currentCreature.Heal(Time.deltaTime * slamHPS, Player.currentCreature);
                }
            }
        }

        public void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
                if (Player.currentCreature && !Player.currentCreature.isKilled && collisionInstance?.sourceColliderGroup?.imbue?.spellCastBase?.id == "Heal")
                    Player.currentCreature.Heal(imbueHealOnKill, Player.currentCreature);
        }
    }
}