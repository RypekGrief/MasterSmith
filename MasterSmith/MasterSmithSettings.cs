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
        /// <summary>MCM unique identifier for this settings file. Used to load/store presets.</summary>
        public override string Id => "MasterSmith_v1";
        /// <summary>Display name shown in the MCM settings menu.</summary>
        public override string DisplayName => "Master Smith";
        /// <summary>Folder name where the settings JSON file is stored.</summary>
        public override string FolderName => "MasterSmith";
        /// <summary>Settings serialization format. "json2" supports nested objects and arrays.</summary>
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

        /// <summary>
        /// When ON, master smiths are only available in 9 specific vanilla cities.
        /// When OFF, all towns have master smiths (for modded maps like Realm of Thrones and Bannerlord 1.3.15+).
        /// Default: true (specific cities)
        /// </summary>
        [SettingPropertyBool("Use Specific Smith Cities", Order = 17, RequireRestart = false, HintText = "ON: Only 9 specific vanilla cities have master smiths. OFF: All towns have master smiths (for modded maps like Realm of Thrones).")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public bool UseSpecificCities { get; set; } = true;

        /// <summary>
        /// When ON, Legendary quality upgrades require the item's culture to match the town's master smith culture.
        /// When OFF, any culture can be upgraded to Legendary at any master smith.
        /// Default: true
        /// </summary>
        [SettingPropertyBool("Enable Culture Restriction", Order = 18, RequireRestart = false, HintText = "When ON, Legendary quality requires matching item and town culture. When OFF, any culture can reach Legendary at any master smith.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public bool EnableCultureRestriction { get; set; } = true;

        // ========================================
        // RUSH ORDER SETTINGS (Group 5)
        // ========================================

        /// <summary>
        /// When ON, players can pay extra to halve the crafting time.
        /// When OFF, only standard orders are available.
        /// Default: true
        /// </summary>
        [SettingPropertyBool("Enable Rush Orders", Order = 19, RequireRestart = false, HintText = "When ON, players can pay extra to halve the crafting time. When OFF, only standard orders are available.")]
        [SettingPropertyGroup("Rush Orders", GroupOrder = 5)]
        public bool EnableRushOrders { get; set; } = true;

        /// <summary>Price multiplier for rush orders. 1.5 = 150% of standard price. Default: 1.5, Range: 1.0–5.0</summary>
        [SettingPropertyFloatingInteger("Rush Order Price Multiplier", 1.0f, 5.0f, "0.0", Order = 20, RequireRestart = false, HintText = "Price multiplier for rush orders. 1.5 = 150% of standard price.")]
        [SettingPropertyGroup("Rush Orders", GroupOrder = 5)]
        public float RushOrderPriceMultiplier { get; set; } = 1.5f;

        // ========================================
        // ORDER CANCELLATION SETTINGS (Group 6)
        // ========================================

        /// <summary>
        /// When ON, players can cancel in-progress orders to get the item back with a partial refund.
        /// When OFF, orders cannot be cancelled.
        /// Default: true
        /// </summary>
        [SettingPropertyBool("Enable Order Cancellation", Order = 21, RequireRestart = false, HintText = "When ON, players can cancel in-progress orders and receive a partial refund. When OFF, orders cannot be cancelled.")]
        [SettingPropertyGroup("Cancellation", GroupOrder = 6)]
        public bool EnableOrderCancellation { get; set; } = true;

        /// <summary>Refund multiplier for cancelled orders. 0.5 = 50% refund. Default: 0.5, Range: 0.0–1.0</summary>
        [SettingPropertyFloatingInteger("Cancellation Refund Multiplier", 0.0f, 1.0f, "0.00", Order = 22, RequireRestart = false, HintText = "Percentage of the order price refunded on cancellation. 0.5 = 50%, 1.0 = full refund.")]
        [SettingPropertyGroup("Cancellation", GroupOrder = 6)]
        public float CancellationRefundMultiplier { get; set; } = 0.5f;

        // ========================================
        // BULK ORDER SETTINGS (Group 7)
        // ========================================

        /// <summary>
        /// When ON, players can upgrade multiple items at once (Bulk Order).
        /// When OFF, only single item orders are available.
        /// Default: true
        /// </summary>
        [SettingPropertyBool("Enable Bulk Orders", Order = 23, RequireRestart = false, HintText = "When ON, players can upgrade multiple items at once. When OFF, only single item orders are available.")]
        [SettingPropertyGroup("Bulk Orders", GroupOrder = 7)]
        public bool EnableBulkOrders { get; set; } = true;
    }
}