using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace MasterSmith
{
    /// <summary>
    /// MasterSmith modunun MCM (Mod Configuration Menu) ayarları.
    /// 
    /// Ayarlar JSON formatında kaydedilir (json2).
    /// Dört grup halinde düzenlenmiştir:
    ///   1. Fine Quality — Fine kalite için silah/zırh fiyat aralıkları
    ///   2. Masterwork Quality — Masterwork kalite için silah/zırh fiyat aralıkları
    ///   3. Legendary Quality — Legendary kalite için silah/zırh fiyat aralıkları
    ///   4. General — Yapım süreleri ve ekipman stat çarpanı
    /// 
    /// Tüm fiyat ayarları float olarak tanımlanır, kullanımda int'e cast edilir.
    /// Bunun sebebi MCM FloatingInteger attribute'unun float gerektirmesidir.
    /// </summary>
    public class MasterSmithSettings : AttributeGlobalSettings<MasterSmithSettings>
    {
        // MCM sistem tanımlamaları
        public override string Id => "MasterSmith_v1.0.0";
        public override string DisplayName => "Master Smith";
        public override string FolderName => "MasterSmith";
        public override string FormatType => "json2";

        // ========================================
        // FINE QUALITY FİYAT AYARLARI (Grup 1)
        // ========================================

        /// <summary>Fine kalite silah minimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Fine Weapon Min Price", 0f, 1000000f, "0", Order = 1, RequireRestart = false, HintText = "Minimum price for Fine quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineWeaponMinPrice { get; set; } = 10000f;

        /// <summary>Fine kalite silah maksimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Fine Weapon Max Price", 0f, 1000000f, "0", Order = 2, RequireRestart = false, HintText = "Maximum price for Fine quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineWeaponMaxPrice { get; set; } = 20000f;

        /// <summary>Fine kalite zırh minimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Fine Armor Min Price", 0f, 1000000f, "0", Order = 3, RequireRestart = false, HintText = "Minimum price for Fine quality armors.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineArmorMinPrice { get; set; } = 20000f;

        /// <summary>Fine kalite zırh maksimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Fine Armor Max Price", 0f, 1000000f, "0", Order = 4, RequireRestart = false, HintText = "Maximum price for Fine quality armors.")]
        [SettingPropertyGroup("Fine Quality", GroupOrder = 1)]
        public float FineArmorMaxPrice { get; set; } = 100000f;

        // ========================================
        // MASTERWORK QUALITY FİYAT AYARLARI (Grup 2)
        // ========================================

        /// <summary>Masterwork kalite silah minimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Masterwork Weapon Min Price", 0f, 1000000f, "0", Order = 5, RequireRestart = false, HintText = "Minimum price for Masterwork quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkWeaponMinPrice { get; set; } = 50000f;

        /// <summary>Masterwork kalite silah maksimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Masterwork Weapon Max Price", 0f, 1000000f, "0", Order = 6, RequireRestart = false, HintText = "Maximum price for Masterwork quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkWeaponMaxPrice { get; set; } = 100000f;

        /// <summary>Masterwork kalite zırh minimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Masterwork Armor Min Price", 0f, 1000000f, "0", Order = 7, RequireRestart = false, HintText = "Minimum price for Masterwork quality armors.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkArmorMinPrice { get; set; } = 100000f;

        /// <summary>Masterwork kalite zırh maksimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Masterwork Armor Max Price", 0f, 1000000f, "0", Order = 8, RequireRestart = false, HintText = "Maximum price for Masterwork quality armors.")]
        [SettingPropertyGroup("Masterwork Quality", GroupOrder = 2)]
        public float MasterworkArmorMaxPrice { get; set; } = 300000f;

        // ========================================
        // LEGENDARY QUALITY FİYAT AYARLARI (Grup 3)
        // ========================================

        /// <summary>Legendary kalite silah minimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Legendary Weapon Min Price", 0f, 1000000f, "0", Order = 9, RequireRestart = false, HintText = "Minimum price for Legendary quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryWeaponMinPrice { get; set; } = 200000f;

        /// <summary>Legendary kalite silah maksimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Legendary Weapon Max Price", 0f, 1000000f, "0", Order = 10, RequireRestart = false, HintText = "Maximum price for Legendary quality weapons, shields, and bows.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryWeaponMaxPrice { get; set; } = 500000f;

        /// <summary>Legendary kalite zırh minimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Legendary Armor Min Price", 0f, 1000000f, "0", Order = 11, RequireRestart = false, HintText = "Minimum price for Legendary quality armors.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryArmorMinPrice { get; set; } = 500000f;

        /// <summary>Legendary kalite zırh maksimum fiyatı (denar).</summary>
        [SettingPropertyFloatingInteger("Legendary Armor Max Price", 0f, 1000000f, "0", Order = 12, RequireRestart = false, HintText = "Maximum price for Legendary quality armors.")]
        [SettingPropertyGroup("Legendary Quality", GroupOrder = 3)]
        public float LegendaryArmorMaxPrice { get; set; } = 1000000f;

        // ========================================
        // GENEL AYARLAR (Grup 4)
        // ========================================

        /// <summary>
        /// Ekipman stat çarpanı.
        /// Daha yüksek değer = daha iyi ekipmanlar daha pahalı olur.
        /// Formül: statBonus = basePrice * (equipmentFactor - 1.0) * (statMultiplier / 10.0)
        /// Varsayılan: 2.0
        /// </summary>
        [SettingPropertyFloatingInteger("Equipment Stat Multiplier", 1f, 10f, "0.0", Order = 13, RequireRestart = false, HintText = "Multiplier applied to the equipment's stats (armor/damage) to adjust the final price. Higher values make better equipment more expensive.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float EquipmentStatMultiplier { get; set; } = 2.0f;

        /// <summary>Fine kalite yapım süresi (gün). Varsayılan: 7</summary>
        [SettingPropertyFloatingInteger("Fine Crafting Days", 1f, 365f, "0", Order = 14, RequireRestart = false, HintText = "Days required to craft a Fine quality item.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float FineCraftingDays { get; set; } = 7f;

        /// <summary>Masterwork kalite yapım süresi (gün). Varsayılan: 14</summary>
        [SettingPropertyFloatingInteger("Masterwork Crafting Days", 1f, 365f, "0", Order = 15, RequireRestart = false, HintText = "Days required to craft a Masterwork quality item.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float MasterworkCraftingDays { get; set; } = 14f;

        /// <summary>Legendary kalite yapım süresi (gün). Varsayılan: 30</summary>
        [SettingPropertyFloatingInteger("Legendary Crafting Days", 1f, 365f, "0", Order = 16, RequireRestart = false, HintText = "Days required to craft a Legendary quality item.")]
        [SettingPropertyGroup("General", GroupOrder = 4)]
        public float LegendaryCraftingDays { get; set; } = 30f;
    }
}