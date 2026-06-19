using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace MasterSmith
{
    /// <summary>
    /// MasterSmith modunun kalıcı verilerini tutar.
    /// Uzman demirci şehirleri, haftalık fiyatlar ve aktif siparişler burada saklanır.
    /// Kayıt dosyasına yazılıp okunur (SyncData üzerinden).
    /// </summary>
    public static class MasterSmithData
    {
        /// <summary>
        /// Uzman demircilerin bulunduğu şehirler.
        /// Key: Settlement StringId (örn: "town_A1")
        /// Value: Kültür StringId (örn: "aserai")
        /// 
        /// Kültür bilgisi, efsanevi kalite siparişlerde kültür kısıtlaması için kullanılır.
        /// Efsanevi kalite sadece eşya ile aynı kültürdeki demircide yapılabilir.
        /// 
        /// War Sails DLC'si yoksa Nord şehri (town_N3) sessizce atlanır.
        /// </summary>
        public static readonly Dictionary<string, string> SmithCities = new Dictionary<string, string>
        {
            { "town_A1", "aserai" },    // Quyaz
            { "town_B1", "battania" },  // Marunath
            { "town_EW2", "empire" },   // Zeonica
            { "town_ES4", "empire" },   // Lycaron
            { "town_EN2", "empire" },   // Diathma
            { "town_K3", "khuzait" },   // Makeb
            { "town_N3", "nord" },      // Thronderlag (War Sails DLC — yoksa sessizce atlanır)
            { "town_S1", "sturgia" },   // Varcheg
            { "town_V5", "vlandia" }    // Galend
        };

        /// <summary>
        /// Her şehrin haftalık güncel fiyatları.
        /// Key: Settlement StringId
        /// Value: 6 elemanlı int dizisi
        ///   [0] = Fine Weapon, [1] = Fine Armor
        ///   [2] = Masterwork Weapon, [3] = Masterwork Armor
        ///   [4] = Legendary Weapon, [5] = Legendary Armor
        /// 
        /// Haftalık tick'te MCM aralıklarından rastgele yenilenir.
        /// Kayıt dosyasına yazılır.
        /// </summary>
        public static Dictionary<string, int[]> CurrentPrices = new Dictionary<string, int[]>();

        /// <summary>
        /// Tüm aktif ve teslim edilmemiş siparişlerin listesi.
        /// Kayıt dosyasına yazılır.
        /// </summary>
        public static List<SmithingOrder> ActiveOrders = new List<SmithingOrder>();

        /// <summary>
        /// Verilen şehrin uzman demirciye sahip olup olmadığını kontrol eder.
        /// </summary>
        public static bool IsMasterSmithCity(Town town)
        {
            if (town == null) return false;
            return SmithCities.ContainsKey(town.Settlement.StringId);
        }

        /// <summary>
        /// Şehrin uzman demircisinin kültürünü döndürür.
        /// Kültür kısıtlaması için kullanılır. Şehir listede yoksa null döner.
        /// </summary>
        public static CultureObject GetSmithCulture(Town town)
        {
            if (town == null) return null;
            string townId = town.Settlement.StringId;
            if (SmithCities.TryGetValue(townId, out string cultureId))
                return MBObjectManager.Instance.GetObject<CultureObject>(cultureId);
            return null;
        }

        /// <summary>
        /// Verilen kalite için varsayılan fiyat aralığını döndürür.
        /// Şehrin haftalık fiyatı oluşmamışsa yedek olarak kullanılır.
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
    /// Tek bir uzman demirci siparişini temsil eder.
    /// Kayıt dosyasına yazılıp okunabilmesi için public property'ler kullanır.
    /// </summary>
    public class SmithingOrder
    {
        /// <summary>Benzersiz sipariş kimliği (Guid).</summary>
        public string OrderId { get; set; }

        /// <summary>Siparişin verildiği şehir.</summary>
        public Town Town { get; set; }

        /// <summary>Yükseltilecek orijinal eşya (Item + mevcut ItemModifier).</summary>
        public EquipmentElement OriginalItem { get; set; }

        /// <summary>İstenen hedef kalite (Fine, Masterwork, Legendary).</summary>
        public ItemQuality RequestedQuality { get; set; }

        /// <summary>Siparişin toplam fiyatı (denar).</summary>
        public int Price { get; set; }

        /// <summary>Teslime kalan gün sayısı. Her gün 1 azalır, 0 olunca hazır.</summary>
        public int DaysRemaining { get; set; }

        /// <summary>Sipariş teslime hazır mı?</summary>
        public bool IsReady { get; set; }
    }
}