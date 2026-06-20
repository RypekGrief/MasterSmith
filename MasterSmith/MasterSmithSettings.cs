using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace MasterSmith
{
    /// <summary>
    /// MCM settings for the MasterSmith mod.
    /// 
    /// Settings are saved in JSON format (json2).
    /// Organized into four groups:
    ///   1. Fine Quality — weapon/armor price ranges for Fine quality
    ///   2. Masterwork Quality — weapon/armor price ranges for Masterwork quality
    ///   3. Legendary Quality — weapon/armor price ranges for Legendary quality
    ///   4. General — crafting durations and equipment stat multiplier
    /// 
    /// All price settings are defined as float due to MCM's FloatingInteger attribute requirement.
    /// They are cast to int when used.
    /// 
    /// NOTE: MCM attribute text (HintText, DisplayName) is compile-time only and cannot be
    /// localized via the module string XML file. This is an MCM API limitation.
    /// </summary>
    public class MasterSmithSettings : AttributeGlobalSettings<MasterSmithSettings>
    {
        // MCM system identifiers
        public override string Id => "MasterSmith_v1";
        public override string DisplayName => "Master Smith";
        public override string FolderName => "MasterSmith";
        public override string FormatType => "json2";

        // ========================================
        // FINE QUALITY PRICE SETTINGS (Group 1)
        // ========================================

        /// <summary>Fine quality weapon minimum price (denars). Default: 10,000</summary>
        [SettingPropertyFloatingInteger("Fine Weapon Min Price", 0f, 1000000f, "0", Order = 1, RequireRestart = false, HintText = "Minimum price for Fine quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineWeaponMinPrice { get; set; } = 10000f;

        /// <summary>Fine quality weapon maximum price (denars). Default: 20,000</summary>
        [SettingPropertyFloatingInteger("Fine Weapon Max Price", 0f, 1000000f, "0", Order = 2, RequireRestart = false, HintText = "Maximum price for Fine quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineWeaponMaxPrice { get; set; } = 20000f;

        /// <summary>Fine quality armor minimum price (denars). Default: 20,000</summary>
        [SettingPropertyFloatingInteger("Fine Armor Min Price", 0f, 1000000f, "0", Order = 3, RequireRestart = false, HintText = "Minimum price for Fine quality armors.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineArmorMinPrice { get; set; } = 20000f;

        /// <summary>Fine quality armor maximum price (denars). Default: 100,000</summary>
        [SettingPropertyFloatingInteger("Fine Armor Max Price", 0f, 1000000f, "0", Order = 4, RequireRestart = false, HintText = "Maximum price for Fine quality armors.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineArmorMaxPrice { get; set; } = 100000f;

        // ========================================
        // MASTERWORK QUALITY PRICE SETTINGS (Group 2)
        // ========================================

        /// <summary>Masterwork quality weapon minimum price (denars). Default: 50,000</summary>
        [SettingPropertyFloatingInteger("Masterwork Weapon Min Price", 0f, 1000000f, "0", Order = 5, RequireRestart = false, HintText = "Minimum price for Masterwork quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkWeaponMinPrice { get; set; } = 50000f;

        /// <summary>Masterwork quality weapon maximum price (denars). Default: 100,000</summary>
        [SettingPropertyFloatingInteger("Masterwork Weapon Max Price", 0f, 1000000f, "0", Order = 6, RequireRestart = false, HintText = "Maximum price for Masterwork quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkWeaponMaxPrice { get; set; } = 100000f;

        /// <summary>Masterwork quality armor minimum price (denars). Default: 100,000</summary>
        [SettingPropertyFloatingInteger("Masterwork Armor Min Price", 0f, 1000000f, "0", Order = 7, RequireRestart = false, HintText = "Minimum price for Masterwork quality armors.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkArmorMinPrice { get; set; } = 100000f;

        /// <summary>Masterwork quality armor maximum price (denars). Default: 300,000</summary>
        [SettingPropertyFloatingInteger("Masterwork Armor Max Price", 0f, 1000000f, "0", Order = 8, RequireRestart = false, HintText = "Maximum price for Masterwork quality armors.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkArmorMaxPrice { get; set; } = 300000f;

        // ========================================
        // LEGENDARY QUALITY PRICE SETTINGS (Group 3)
        // ========================================

        /// <summary>Legendary quality weapon minimum price (denars). Default: 200,000</summary>
        [SettingPropertyFloatingInteger("Legendary Weapon Min Price", 0f, 1000000f, "0", Order = 9, RequireRestart = false, HintText = "Minimum price for Legendary quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryWeaponMinPrice { get; set; } = 200000f;

        /// <summary>Legendary quality weapon maximum price (denars). Default: 500,000</summary>
        [SettingPropertyFloatingInteger("Legendary Weapon Max Price", 0f, 1000000f, "0", Order = 10, RequireRestart = false, HintText = "Maximum price for Legendary quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryWeaponMaxPrice { get; set; } = 500000f;

        /// <summary>Legendary quality armor minimum price (denars). Default: 500,000</summary>
        [SettingPropertyFloatingInteger("Legendary Armor Min Price", 0f, 1000000f, "0", Order = 11, RequireRestart = false, HintText = "Minimum price for Legendary quality armors.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryArmorMinPrice { get; set; } = 500000f;

        /// <summary>Legendary quality armor maximum price (denars). Default: 1,000,000</summary>
        [SettingPropertyFloatingInteger("Legendary Armor Max Price", 0f, 1000000f, "0", Order = 12, RequireRestart = false, HintText = "Maximum price for Legendary quality armors.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryArmorMaxPrice { get; set; } = 1000000f;

        // ========================================
        // GENERAL SETTINGS (Group 4)
        // ========================================

        /// <summary>
        /// Equipment stat multiplier.
        /// Higher values make better equipment more expensive.
        /// Formula: statBonus = basePrice * (equipmentFactor - 1.0) * (statMultiplier / 10.0)
        /// Default: 2.0
        /// </summary>
        [SettingPropertyFloatingInteger("Equipment Stat Multiplier", 1f, 10f, "0.0", Order = 13, RequireRestart = false, HintText = "Multiplier applied to the equipment's stats (armor/damage) to adjust the final price. Higher values make better equipment more expensive.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float EquipmentStatMultiplier { get; set; } = 2.0f;

        /// <summary>Fine quality crafting time (days). Default: 7</summary>
        [SettingPropertyFloatingInteger("Fine Crafting Days", 1f, 365f, "0", Order = 14, RequireRestart = false, HintText = "Days required to craft a Fine quality item.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float FineCraftingDays { get; set; } = 7f;

        /// <summary>Masterwork quality crafting time (days). Default: 14</summary>
        [SettingPropertyFloatingInteger("Masterwork Crafting Days", 1f, 365f, "0", Order = 15, RequireRestart = false, HintText = "Days required to craft a Masterwork quality item.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float MasterworkCraftingDays { get; set; } = 14f;

        /// <summary>Legendary quality crafting time (days). Default: 30</summary>
        [SettingPropertyFloatingInteger("Legendary Crafting Days", 1f, 365f, "0", Order = 16, RequireRestart = false, HintText = "Days required to craft a Legendary quality item.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float LegendaryCraftingDays { get; set; } = 30f;
    }
}