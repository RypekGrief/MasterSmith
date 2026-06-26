using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace MasterSmith
{
    /// <summary>
    /// Holds persistent data for the MasterSmith mod.
    /// Master smith cities, weekly prices, and active orders are stored here.
    /// Written to and read from save files via SyncData.
    /// 
    /// All collections use primitive types for serialization:
    /// - CurrentPrices: string CSV ("10000,20000,50000,...")
    /// - ActiveOrders: List of strings, each a pipe-delimited CSV
    /// </summary>
    public static class MasterSmithData
    {
        /// <summary>
        /// Cities that have a master smith.
        /// Key: Settlement StringId (e.g. "town_A1")
        /// Value: Culture StringId (e.g. "aserai")
        /// 
        /// Culture info is used for the legendary quality culture restriction.
        /// Legendary quality can only be crafted at a smith of the same culture.
        /// 
        /// Nord city (town_N3) is silently skipped if War Sails DLC is missing.
        /// </summary>
        public static readonly Dictionary<string, string> SmithCities = new Dictionary<string, string>
        {
            { "town_A1", "aserai" },    // Quyaz
            { "town_B1", "battania" },  // Marunath
            { "town_EW2", "empire" },   // Zeonica
            { "town_ES4", "empire" },   // Lycaron
            { "town_EN2", "empire" },   // Diathma
            { "town_K3", "khuzait" },   // Makeb
            { "town_N3", "nord" },      // Thronderlag (War Sails DLC — silently skipped if absent)
            { "town_S1", "sturgia" },   // Varcheg
            { "town_V5", "vlandia" }    // Galend
        };

        /// <summary>
        /// Current weekly prices for each city.
        /// Key: Settlement StringId
        /// Value: CSV of 6 prices: FineWeapon,FineArmor,MasterworkWeapon,MasterworkArmor,LegendaryWeapon,LegendaryArmor
        /// 
        /// Randomly regenerated every weekly tick from MCM ranges.
        /// Saved to the save file.
        /// </summary>
        public static Dictionary<string, string> CurrentPrices = new Dictionary<string, string>();

        /// <summary>
        /// All active and undelivered orders.
        /// Each element is a pipe-delimited CSV string created by SmithingOrder.ToCsv().
        /// Saved to the save file.
        /// </summary>
        public static List<string> ActiveOrders = new List<string>();

        /// <summary>
        /// Checks if the given town has a master smith.
        /// When UseSpecificCities is ON: checks the hardcoded vanilla city list.
        /// When OFF: any town qualifies (for modded maps / 1.3.15+ compatibility).
        /// </summary>
        public static bool IsMasterSmithCity(Town town)
        {
            if (town == null) return false;
            if (!MasterSmithSettings.Instance.UseSpecificCities)
                return town.IsTown;
            return SmithCities.ContainsKey(town.Settlement.StringId);
        }

        /// <summary>
        /// Returns the culture of the town's master smith.
        /// When UseSpecificCities is ON: looks up culture from the hardcoded list.
        /// When OFF: returns the town's actual settlement culture directly.
        /// </summary>
        public static CultureObject GetSmithCulture(Town town)
        {
            if (town == null) return null;
            if (!MasterSmithSettings.Instance.UseSpecificCities)
                return town.Settlement?.Culture;

            string townId = town.Settlement.StringId;
            if (SmithCities.TryGetValue(townId, out string cultureId))
                return MBObjectManager.Instance.GetObject<CultureObject>(cultureId);
            return null;
        }

        /// <summary>
        /// Parses the CSV price string into a 6-element int list.
        /// Returns null if the format is invalid.
        /// </summary>
        public static List<int> GetPricesForTown(string settlementId)
        {
            if (CurrentPrices.TryGetValue(settlementId, out string csv))
            {
                var parts = csv.Split(',');
                var list = new List<int>();
                foreach (var p in parts)
                {
                    if (int.TryParse(p, out int val))
                        list.Add(val);
                    else
                        return null;
                }
                if (list.Count == 6) return list;
            }
            return null;
        }

        /// <summary>
        /// Saves a 6-element int list as a CSV string.
        /// </summary>
        public static void SetPricesForTown(string settlementId, List<int> prices)
        {
            if (prices != null && prices.Count == 6)
                CurrentPrices[settlementId] = string.Join(",", prices);
        }

        /// <summary>
        /// Returns the default price range for the given quality.
        /// Used as fallback when a town's weekly prices haven't been generated yet.
        /// </summary>
        public static int[] GetDefaultPricesForQuality(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Fine: return new int[] { 10000, 20000 };
                case ItemQuality.Masterwork: return new int[] { 50000, 100000 };
                case ItemQuality.Legendary: return new int[] { 200000, 500000 };
                default: return new int[] { 0, 0 };
            }
        }
    }

    /// <summary>
    /// Represents a single master smith commission order.
    /// 
    /// Only serializable fields are stored for save compatibility.
    /// Town and EquipmentElement references are replaced with StringIds.
    /// RequestedQuality is stored as int instead of the enum.
    /// 
    /// Converted to/from CSV strings via ToCsv() / FromCsv() for storage in ActiveOrders.
    /// </summary>
    public class SmithingOrder
    {
        /// <summary>Unique order identifier (Guid).</summary>
        public string OrderId { get; set; }

        /// <summary>Settlement StringId of the town where the order was placed.</summary>
        public string TownId { get; set; }

        /// <summary>Item StringId of the item being upgraded.</summary>
        public string ItemId { get; set; }

        /// <summary>ItemModifier StringId of the item's current modifier (null if none).</summary>
        public string CurrentModifierId { get; set; }

        /// <summary>Total price of the order in denars.</summary>
        public int Price { get; set; }

        /// <summary>Days remaining until completion. Decrements by 1 each day.</summary>
        public int DaysRemaining { get; set; }

        /// <summary>Whether the order is ready for delivery.</summary>
        public bool IsReady { get; set; }

        /// <summary>Requested quality stored as int (serializable, cast to ItemQuality).</summary>
        public int RequestedQualityInt { get; set; }

        /// <summary>
        /// Convenience property to get/set the requested quality as ItemQuality.
        /// </summary>
        public ItemQuality RequestedQuality
        {
            get => (ItemQuality)RequestedQualityInt;
            set => RequestedQualityInt = (int)value;
        }

        /// <summary>
        /// Resolves the Town reference from the stored StringId.
        /// </summary>
        public Town GetTown()
        {
            Settlement settlement = Settlement.Find(this.TownId);
            return settlement?.Town;
        }

        /// <summary>
        /// Reconstructs the original EquipmentElement from the stored StringIds.
        /// </summary>
        public EquipmentElement GetOriginalItem()
        {
            ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(this.ItemId);
            if (item == null) return EquipmentElement.Invalid;

            ItemModifier modifier = null;
            if (!string.IsNullOrEmpty(this.CurrentModifierId))
                modifier = MBObjectManager.Instance.GetObject<ItemModifier>(this.CurrentModifierId);

            return new EquipmentElement(item, modifier, null, false);
        }

        /// <summary>
        /// Creates a new order from the given EquipmentElement and Town.
        /// </summary>
        public static SmithingOrder Create(Town town, EquipmentElement item, ItemQuality quality, int price, int days)
        {
            return new SmithingOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                TownId = town.Settlement.StringId,
                ItemId = item.Item.StringId,
                CurrentModifierId = item.ItemModifier?.StringId,
                RequestedQualityInt = (int)quality,
                Price = price,
                DaysRemaining = days,
                IsReady = false
            };
        }

        /// <summary>
        /// Converts the order to a pipe-delimited CSV string.
        /// Format: OrderId|TownId|ItemId|CurrentModifierId|Price|DaysRemaining|IsReady(0/1)|RequestedQualityInt
        /// </summary>
        public string ToCsv()
        {
            return $"{OrderId}|{TownId}|{ItemId}|{CurrentModifierId ?? ""}|{Price}|{DaysRemaining}|{(IsReady ? 1 : 0)}|{RequestedQualityInt}";
        }

        /// <summary>
        /// Creates an order from a pipe-delimited CSV string.
        /// </summary>
        public static SmithingOrder FromCsv(string csv)
        {
            var parts = csv.Split('|');
            if (parts.Length != 8) return null;

            return new SmithingOrder
            {
                OrderId = parts[0],
                TownId = parts[1],
                ItemId = parts[2],
                CurrentModifierId = string.IsNullOrEmpty(parts[3]) ? null : parts[3],
                Price = int.Parse(parts[4]),
                DaysRemaining = int.Parse(parts[5]),
                IsReady = parts[6] == "1",
                RequestedQualityInt = int.Parse(parts[7])
            };
        }
    }
}