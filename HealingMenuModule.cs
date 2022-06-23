using ThunderRoad;
using UnityEngine.UI;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace HealingSpell
{
    class HealingMenuModule : MenuModule
    {
        private const string OPTIONS_FILE_PATH = "\\Mods\\HealingSpell\\HealingOptions.opt";

        private Toggle aoeFXtoggle;
        private Dropdown typeDropdown;
        private Button saveButton;

        private Button healAmtBigMinus, healAmtMinus, healAmtPlus, healAmtBigPlus;
        private Button minChargeMinus, minChargePlus;
        private Button imbueHealBigMinus, imbueHealMinus, imbueHealPlus, imbueHealBigPlus;
        private Button gripThreshMinus, gripThreshPlus;
        private Button smashDistMinus, smashDistPlus;
        private Button smashVelMinus, smashVelPlus;
        private Button hpsBigMinus, hpsMinus, hpsPlus, hpsBigPlus;
        private Button mdpsBigMinus, mdpsMinus, mdpsPlus, mdpsBigPlus;

        private Text healAmtText;
        private Text minChargeText;
        private Text imbueHealText;
        private Text gripThreshText;
        private Text smashDistText;
        private Text smashVelText;
        private Text hpsText;
        private Text mdpsText;

        // Runs a single time after the game loads the Master level
        public override void Init(MenuData menuData, Menu menu)
        {
            base.Init(menuData, menu);
            try
            {
                HealingSpell.healingOptions = JsonConvert.DeserializeObject<HealingOptions>(File.ReadAllText(Application.streamingAssetsPath + OPTIONS_FILE_PATH));
            } catch
            {
                Debug.LogError("Missing HealingOptions.opt. The mod will not work properly!");
            }

            // Heal Type Dropdown
            typeDropdown = menu.GetCustomReference("HealTypeDropdown").GetComponent<Dropdown>();
            typeDropdown.onValueChanged.AddListener(HealTypeChange);

            // AOE FX Toggle
            aoeFXtoggle = menu.GetCustomReference("AOEfxButton").GetComponent<Toggle>();
            aoeFXtoggle.onValueChanged.AddListener(ToggleClick);

            // Save settings Button
            saveButton = menu.GetCustomReference("SaveSettingsButton").GetComponent<Button>();
            saveButton.onClick.AddListener(SaveData);

            // Heal amount
            healAmtBigMinus = menu.GetCustomReference("HealAmtBigMinus").GetComponent<Button>();
            healAmtMinus = menu.GetCustomReference("HealAmtMinus").GetComponent<Button>();
            healAmtPlus = menu.GetCustomReference("HealAmtPlus").GetComponent<Button>();
            healAmtBigPlus = menu.GetCustomReference("HealAmtBigPlus").GetComponent<Button>();
            healAmtText = menu.GetCustomReference("HealAmtText").GetComponent<Text>();
            healAmtBigMinus.onClick.AddListener(delegate { SubOne(ref HealingSpell.healingOptions.healAmount, healAmtText); });
            healAmtMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.healAmount, healAmtText); });
            healAmtPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.healAmount, healAmtText); });
            healAmtBigPlus.onClick.AddListener(delegate { AddOne(ref HealingSpell.healingOptions.healAmount, healAmtText); });


            // Minimum charge
            minChargeMinus = menu.GetCustomReference("MinChargeMinus").GetComponent<Button>();
            minChargePlus = menu.GetCustomReference("MinChargePlus").GetComponent<Button>();
            minChargeText = menu.GetCustomReference("MinChargeText").GetComponent<Text>();
            minChargeMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.minimumChargeForHeal, minChargeText); });
            minChargePlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.minimumChargeForHeal, minChargeText); });

            // Imbue Heal
            imbueHealBigMinus = menu.GetCustomReference("ImbueHealBigMinus").GetComponent<Button>();
            imbueHealMinus = menu.GetCustomReference("ImbueHealMinus").GetComponent<Button>();
            imbueHealPlus = menu.GetCustomReference("ImbueHealPlus").GetComponent<Button>();
            imbueHealBigPlus = menu.GetCustomReference("ImbueHealBigPlus").GetComponent<Button>();
            imbueHealText = menu.GetCustomReference("ImbueHealText").GetComponent<Text>();
            imbueHealBigMinus.onClick.AddListener(delegate { SubOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
            imbueHealMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
            imbueHealPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
            imbueHealBigPlus.onClick.AddListener(delegate { AddOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });

            // Grip Threshold
            gripThreshMinus = menu.GetCustomReference("GripThreshMinus").GetComponent<Button>();
            gripThreshPlus = menu.GetCustomReference("GripThreshPlus").GetComponent<Button>();
            gripThreshText = menu.GetCustomReference("GripThreshText").GetComponent<Text>();
            gripThreshMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.gripThreshold, gripThreshText); });
            gripThreshPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.gripThreshold, gripThreshText); });

            // Smash Distance
            smashDistMinus = menu.GetCustomReference("SmashDistMinus").GetComponent<Button>();
            smashDistPlus = menu.GetCustomReference("SmashDistPlus").GetComponent<Button>();
            smashDistText = menu.GetCustomReference("SmashDistText").GetComponent<Text>();
            smashDistMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.smashDistance, smashDistText); });
            smashDistPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.smashDistance, smashDistText); });

            // Smash Velocity
            smashVelMinus = menu.GetCustomReference("SmashVelMinus").GetComponent<Button>();
            smashVelPlus = menu.GetCustomReference("SmashVelPlus").GetComponent<Button>();
            smashVelText = menu.GetCustomReference("SmashVelText").GetComponent<Text>();
            smashVelMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.smashVelocity, smashVelText); });
            smashVelPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.smashVelocity, smashVelText); });

            // Health per Second
            hpsBigMinus = menu.GetCustomReference("HPSBigMinus").GetComponent<Button>();
            hpsMinus = menu.GetCustomReference("HPSMinus").GetComponent<Button>();
            hpsPlus = menu.GetCustomReference("HPSPlus").GetComponent<Button>();
            hpsBigPlus = menu.GetCustomReference("HPSBigPlus").GetComponent<Button>();
            hpsText = menu.GetCustomReference("HPSText").GetComponent<Text>();
            hpsBigMinus.onClick.AddListener(delegate { SubOne(ref HealingSpell.healingOptions.healthPerSecond, hpsText); });
            hpsMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.healthPerSecond, hpsText); });
            hpsPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.healthPerSecond, hpsText); });
            hpsBigPlus.onClick.AddListener(delegate { AddOne(ref HealingSpell.healingOptions.healthPerSecond, hpsText); });

            // Mana Drain per Second
            mdpsBigMinus = menu.GetCustomReference("MDPSBigMinus").GetComponent<Button>();
            mdpsMinus = menu.GetCustomReference("MDPSMinus").GetComponent<Button>();
            mdpsPlus = menu.GetCustomReference("MDPSPlus").GetComponent<Button>();
            mdpsBigPlus = menu.GetCustomReference("MDPSBigPlus").GetComponent<Button>();
            mdpsText = menu.GetCustomReference("MDPSText").GetComponent<Text>();
            imbueHealBigMinus.onClick.AddListener(delegate { SubOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
            imbueHealMinus.onClick.AddListener(delegate { SubPointOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
            imbueHealPlus.onClick.AddListener(delegate { AddPointOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
            imbueHealBigPlus.onClick.AddListener(delegate { AddOne(ref HealingSpell.healingOptions.imbueHealOnKill, imbueHealText); });
        }

        public override void OnShow(bool show)
        {
            base.OnShow(show);
            if (show)
                UpdateMenu();
        }

        private void UpdateMenu()
        {
            typeDropdown.value = (int) HealingSpell.healingOptions.healTypeEnum;
            aoeFXtoggle.isOn = HealingSpell.healingOptions.useAOEfx;
            healAmtText.text = HealingSpell.healingOptions.healAmount.ToString("0.0#");
            minChargeText.text = HealingSpell.healingOptions.minimumChargeForHeal.ToString("0.0#");
            imbueHealText.text = HealingSpell.healingOptions.imbueHealOnKill.ToString("0.0#");
            gripThreshText.text = HealingSpell.healingOptions.gripThreshold.ToString("0.0#");
            smashDistText.text = HealingSpell.healingOptions.smashDistance.ToString("0.0#");
            smashVelText.text = HealingSpell.healingOptions.smashVelocity.ToString("0.0#");
            hpsText.text = HealingSpell.healingOptions.healthPerSecond.ToString("0.0#");
            mdpsText.text = HealingSpell.healingOptions.manaDrainPerSecond.ToString("0.0#");
        }

        /* ======================
         *       UI METHODS
         * ====================== */

        private void HealTypeChange(int newType)
        {
            HealingSpell.healingOptions.healTypeEnum = (HealType) newType;
        }

        private void ToggleClick(bool isOn)
        {
            HealingSpell.healingOptions.useAOEfx = isOn;
        }

        private void SaveData()
        {
            File.WriteAllText(Application.streamingAssetsPath + OPTIONS_FILE_PATH, JsonConvert.SerializeObject(HealingSpell.healingOptions));
            Debug.Log("Saving healing spell settings!");
        }

        /*
         * Using the keyword "ref" because I want to
         * pass by reference the entry in HealingOptions,
         * thereby affecting that value instead of the
         * value you'd get if you did the usual pass by 
         * value, which is simply just the value on
         * the stack
         */
        private void SubOne(ref float option, Text text)
        {
            option--;
            text.text = (float.Parse(text.text) - 1.0f).ToString("0.0#");
        }

        private void SubPointOne(ref float option, Text text)
        {
            option -= 0.1f;
            text.text = (float.Parse(text.text) - 0.1f).ToString("0.0#");
        }

        private void AddPointOne(ref float option, Text text)
        {
            option += 0.1f;
            text.text = (float.Parse(text.text) + 0.1f).ToString("0.0#");
        }

        private void AddOne(ref float option, Text text)
        {
            option++;
            text.text = (float.Parse(text.text) + 1.0f).ToString("0.0#");
        }
    }
}
