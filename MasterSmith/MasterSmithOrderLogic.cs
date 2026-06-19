using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace MasterSmith
{
    /// <summary>
    /// Sipariş akışının tüm UI ve iş mantığını yönetir.
    /// Akış: Eşya seçimi -> Kalite seçimi -> Fiyat onayı -> Sipariş oluşturma
    /// </summary>
    public static class MasterSmithOrderLogic
    {
        /// <summary>
        /// Uzman demircinin yükseltebileceği ekipman tipleri.
        /// Silahlar, zırhlar, kalkanlar, oklar, binek zırhları ve sancaklar dahil.
        /// </summary>
        private static readonly HashSet<ItemObject.ItemTypeEnum> EquipmentTypes = new HashSet<ItemObject.ItemTypeEnum>
        {
            ItemObject.ItemTypeEnum.OneHandedWeapon,
            ItemObject.ItemTypeEnum.TwoHandedWeapon,
            ItemObject.ItemTypeEnum.Polearm,
            ItemObject.ItemTypeEnum.Arrows,
            ItemObject.ItemTypeEnum.Bolts,
            ItemObject.ItemTypeEnum.SlingStones,
            ItemObject.ItemTypeEnum.Shield,
            ItemObject.ItemTypeEnum.Bow,
            ItemObject.ItemTypeEnum.Crossbow,
            ItemObject.ItemTypeEnum.Sling,
            ItemObject.ItemTypeEnum.Thrown,
            ItemObject.ItemTypeEnum.HeadArmor,
            ItemObject.ItemTypeEnum.BodyArmor,
            ItemObject.ItemTypeEnum.LegArmor,
            ItemObject.ItemTypeEnum.HandArmor,
            ItemObject.ItemTypeEnum.ChestArmor,
            ItemObject.ItemTypeEnum.Cape,
            ItemObject.ItemTypeEnum.HorseHarness,
            ItemObject.ItemTypeEnum.Banner
        };

        /// <summary>
        /// Sipariş akışını başlatır. CTRL+H ile çağrılır.
        /// Önce oyuncunun envanterinde uygun eşya var mı kontrol eder.
        /// </summary>
        public static void StartOrder(Town town)
        {
            var playerItems = GetEligibleItemsForSmithing();
            if (playerItems.Count == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[MasterSmith] You don't have any upgradable items in your inventory.",
                    Color.FromUint(0xFFFF0000)));
                return;
            }
            ShowItemSelection(town, playerItems);
        }

        /// <summary>
        /// Oyuncunun envanterinden yükseltilebilir eşyaları toplar.
        /// Legendary kalitedekileri, uygun tipte olmayanları ve
        /// aynı StringId+Modifier kombinasyonundaki tekrarları eler.
        /// </summary>
        private static List<EquipmentElement> GetEligibleItemsForSmithing()
        {
            var items = new List<EquipmentElement>();
            var playerRoster = MobileParty.MainParty?.ItemRoster;
            if (playerRoster == null) return items;

            foreach (var rosterElement in playerRoster)
            {
                var element = rosterElement.EquipmentElement;
                if (element.Item == null) continue;
                if (!EquipmentTypes.Contains(element.Item.ItemType)) continue;
                if (GetItemQuality(element) == ItemQuality.Legendary) continue;
                if (items.Any(e => e.Item.StringId == element.Item.StringId && e.ItemModifier == element.ItemModifier))
                    continue;
                items.Add(element);
            }
            return items;
        }

        /// <summary>
        /// Adım 1: Eşya seçim menüsü.
        /// Tüm uygun eşyaları listeler, oyuncu birini seçer.
        /// </summary>
        private static void ShowItemSelection(Town town, List<EquipmentElement> items)
        {
            var inquiryElements = new List<InquiryElement>();
            foreach (var element in items)
            {
                string itemName = element.Item.Name.ToString();
                string qualityText = GetQualityText(element);
                inquiryElements.Add(new InquiryElement(element, $"{itemName} ({qualityText})", null));
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    titleText: "Select Item to Upgrade",
                    descriptionText: "Choose which item you'd like the master smith to work on.",
                    inquiryElements: inquiryElements,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: "Next",
                    negativeText: "Cancel",
                    affirmativeAction: (selectedElements) =>
                    {
                        if (selectedElements.Count > 0)
                        {
                            var selectedItem = (EquipmentElement)selectedElements[0].Identifier;
                            ShowQualitySelection(town, selectedItem);
                        }
                    },
                    negativeAction: null
                ));
        }

        /// <summary>
        /// Adım 2: Kalite seçim menüsü.
        /// Seçili eşya için hangi kalitelerin mevcut olduğunu kontrol eder.
        /// Sadece gerçekten uygulanabilir modifier'ı olan kaliteler gösterilir.
        /// Efsanevi kalite için kültür kısıtlaması uygulanır.
        /// </summary>
        private static void ShowQualitySelection(Town town, EquipmentElement selectedItem)
        {
            var settings = MasterSmithSettings.Instance;
            var inquiryElements = new List<InquiryElement>();

            // Her kalite için uygun modifier var mı kontrol et
            bool canBeFine = GetItemModifierForQuality(selectedItem.Item, ItemQuality.Fine) != null;
            bool canBeMasterwork = GetItemModifierForQuality(selectedItem.Item, ItemQuality.Masterwork) != null;
            bool canBeLegendary = GetItemModifierForQuality(selectedItem.Item, ItemQuality.Legendary) != null;

            // Kültür kısıtlaması: Efsanevi kalite sadece eşya ile aynı kültürdeki demircide yapılabilir
            if (canBeLegendary)
            {
                CultureObject smithCulture = MasterSmithData.GetSmithCulture(town);
                CultureObject itemCulture = selectedItem.Item.Culture as CultureObject;
                if (smithCulture != null && itemCulture != null && smithCulture.StringId != itemCulture.StringId)
                    canBeLegendary = false;
            }

            if (canBeFine)
                inquiryElements.Add(new InquiryElement(ItemQuality.Fine, $"Fine ({(int)settings.FineCraftingDays} days)", null));
            if (canBeMasterwork)
                inquiryElements.Add(new InquiryElement(ItemQuality.Masterwork, $"Masterwork ({(int)settings.MasterworkCraftingDays} days)", null));
            if (canBeLegendary)
                inquiryElements.Add(new InquiryElement(ItemQuality.Legendary, $"Legendary ({(int)settings.LegendaryCraftingDays} days)", null));

            if (inquiryElements.Count == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[MasterSmith] No quality upgrades available for this item.",
                    Color.FromUint(0xFFFF0000)));
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    titleText: "Select Quality",
                    descriptionText: "Choose the desired quality level.",
                    inquiryElements: inquiryElements,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: "Calculate Price",
                    negativeText: "Back",
                    affirmativeAction: (selectedQualities) =>
                    {
                        if (selectedQualities.Count > 0)
                        {
                            var selectedQuality = (ItemQuality)selectedQualities[0].Identifier;
                            ShowPriceConfirmation(town, selectedItem, selectedQuality);
                        }
                    },
                    negativeAction: (list) => ShowItemSelection(town, GetEligibleItemsForSmithing())
                ));
        }

        /// <summary>
        /// Adım 3: Fiyat onay menüsü.
        /// Hesaplanan fiyatı ve süreyi gösterir, oyuncudan onay ister.
        /// </summary>
        private static void ShowPriceConfirmation(Town town, EquipmentElement selectedItem, ItemQuality selectedQuality)
        {
            int price = CalculatePrice(town, selectedItem, selectedQuality);
            int days = GetCraftingDays(selectedQuality);

            string message = $"Item: {selectedItem.Item.Name}\nQuality: {selectedQuality}\nTime: {days} days\nPrice: {price} denars\n\nPlace this order?";
            bool canAfford = Hero.MainHero.Gold >= price;

            var inquiryElements = new List<InquiryElement>();
            if (canAfford)
                inquiryElements.Add(new InquiryElement(true, "Place Order", null));
            else
                inquiryElements.Add(new InquiryElement(false, $"Not enough gold! (Need {price}, have {Hero.MainHero.Gold})", null));
            inquiryElements.Add(new InquiryElement(false, "Cancel", null));

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    titleText: "Confirm Order",
                    descriptionText: message,
                    inquiryElements: inquiryElements,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: "Confirm",
                    negativeText: "Back",
                    affirmativeAction: (selectedOptions) =>
                    {
                        if (selectedOptions.Count > 0 && (bool)selectedOptions[0].Identifier)
                        {
                            FinalizeOrder(town, selectedItem, selectedQuality, price, days);
                        }
                    },
                    negativeAction: (list) => ShowQualitySelection(town, selectedItem)
                ));
        }

        /// <summary>
        /// Seçili kalite için MCM'den yapım süresini (gün) döndürür.
        /// </summary>
        private static int GetCraftingDays(ItemQuality quality)
        {
            var settings = MasterSmithSettings.Instance;
            switch (quality)
            {
                case ItemQuality.Fine: return (int)settings.FineCraftingDays;
                case ItemQuality.Masterwork: return (int)settings.MasterworkCraftingDays;
                case ItemQuality.Legendary: return (int)settings.LegendaryCraftingDays;
                default: return 7;
            }
        }

        /// <summary>
        /// Eşyanın silah tipi olup olmadığını kontrol eder.
        /// Kalkan, yay, ok, cirit vb. dahil.
        /// </summary>
        private static bool IsWeapon(ItemObject item)
        {
            return item.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon
                || item.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon
                || item.ItemType == ItemObject.ItemTypeEnum.Polearm
                || item.ItemType == ItemObject.ItemTypeEnum.Bow
                || item.ItemType == ItemObject.ItemTypeEnum.Crossbow
                || item.ItemType == ItemObject.ItemTypeEnum.Sling
                || item.ItemType == ItemObject.ItemTypeEnum.Shield
                || item.ItemType == ItemObject.ItemTypeEnum.Thrown
                || item.ItemType == ItemObject.ItemTypeEnum.Arrows
                || item.ItemType == ItemObject.ItemTypeEnum.Bolts
                || item.ItemType == ItemObject.ItemTypeEnum.SlingStones;
        }

        /// <summary>
        /// Sipariş fiyatını hesaplar.
        /// Haftalık güncellenen base fiyat + ekipman istatistik bonusu + kalite çarpanı.
        /// Eğer şehrin fiyatı henüz oluşmamışsa, hemen oluşturur (ilk hafta beklenmez).
        /// </summary>
        private static int CalculatePrice(Town town, EquipmentElement selectedItem, ItemQuality selectedQuality)
        {
            var settings = MasterSmithSettings.Instance;
            string settlementId = town.Settlement.StringId;

            // Fiyat yoksa anında oluştur - böylece ilk hafta beklenmez ve her seferinde farklı fiyat çıkmaz
            if (!MasterSmithData.CurrentPrices.ContainsKey(settlementId))
            {
                GeneratePricesForTown(town);
            }

            if (MasterSmithData.CurrentPrices.TryGetValue(settlementId, out int[] prices))
            {
                bool isWeapon = IsWeapon(selectedItem.Item);
                int qualityIndex;
                if (selectedQuality == ItemQuality.Fine)
                    qualityIndex = isWeapon ? 0 : 1;
                else if (selectedQuality == ItemQuality.Masterwork)
                    qualityIndex = isWeapon ? 2 : 3;
                else
                    qualityIndex = isWeapon ? 4 : 5;

                int basePrice = prices[qualityIndex];

                // Ekipmanın statlarına göre fiyat bonusu (daha iyi eşya = daha pahalı)
                float statMultiplier = settings.EquipmentStatMultiplier;
                float equipmentFactor = GetEquipmentStatFactor(selectedItem.Item);
                int statBonus = (int)(basePrice * (equipmentFactor - 1.0f) * (statMultiplier / 10.0f));

                // Mevcut kaliteye göre çarpan
                ItemQuality currentQuality = GetItemQuality(selectedItem);
                float qualityMultiplier = 1.0f;
                if (currentQuality == ItemQuality.Poor || currentQuality == ItemQuality.Inferior)
                    qualityMultiplier = 1.2f; // Kötü durumdaysa biraz daha pahalı
                else if (currentQuality == ItemQuality.Masterwork && selectedQuality == ItemQuality.Legendary)
                    qualityMultiplier = 0.85f; // Masterwork'ten Legendary'ye geçişte indirim

                int finalPrice = (int)((basePrice + statBonus) * qualityMultiplier);
                return Math.Max(1, finalPrice);
            }

            // Yedek fiyatlandırma (normalde buraya düşmez, ama güvenlik için)
            int baseMin = 0, baseMax = 0;
            if (IsWeapon(selectedItem.Item))
            {
                switch (selectedQuality)
                {
                    case ItemQuality.Fine: baseMin = (int)settings.FineWeaponMinPrice; baseMax = (int)settings.FineWeaponMaxPrice; break;
                    case ItemQuality.Masterwork: baseMin = (int)settings.MasterworkWeaponMinPrice; baseMax = (int)settings.MasterworkWeaponMaxPrice; break;
                    case ItemQuality.Legendary: baseMin = (int)settings.LegendaryWeaponMinPrice; baseMax = (int)settings.LegendaryWeaponMaxPrice; break;
                }
            }
            else
            {
                switch (selectedQuality)
                {
                    case ItemQuality.Fine: baseMin = (int)settings.FineArmorMinPrice; baseMax = (int)settings.FineArmorMaxPrice; break;
                    case ItemQuality.Masterwork: baseMin = (int)settings.MasterworkArmorMinPrice; baseMax = (int)settings.MasterworkArmorMaxPrice; break;
                    case ItemQuality.Legendary: baseMin = (int)settings.LegendaryArmorMinPrice; baseMax = (int)settings.LegendaryArmorMaxPrice; break;
                }
            }
            return MBRandom.RandomInt(baseMin, baseMax + 1);
        }

        /// <summary>
        /// Şehir için MCM aralıklarından rastgele haftalık fiyatları oluşturur.
        /// Fiyat dizisi: [FineWeapon, FineArmor, MasterworkWeapon, MasterworkArmor, LegendaryWeapon, LegendaryArmor]
        /// </summary>
        private static void GeneratePricesForTown(Town town)
        {
            var settings = MasterSmithSettings.Instance;
            int[] combined = new int[6];
            combined[0] = MBRandom.RandomInt((int)settings.FineWeaponMinPrice, (int)settings.FineWeaponMaxPrice + 1);
            combined[1] = MBRandom.RandomInt((int)settings.FineArmorMinPrice, (int)settings.FineArmorMaxPrice + 1);
            combined[2] = MBRandom.RandomInt((int)settings.MasterworkWeaponMinPrice, (int)settings.MasterworkWeaponMaxPrice + 1);
            combined[3] = MBRandom.RandomInt((int)settings.MasterworkArmorMinPrice, (int)settings.MasterworkArmorMaxPrice + 1);
            combined[4] = MBRandom.RandomInt((int)settings.LegendaryWeaponMinPrice, (int)settings.LegendaryWeaponMaxPrice + 1);
            combined[5] = MBRandom.RandomInt((int)settings.LegendaryArmorMinPrice, (int)settings.LegendaryArmorMaxPrice + 1);
            MasterSmithData.CurrentPrices[town.Settlement.StringId] = combined;
        }

        /// <summary>
        /// Ekipmanın zırh veya silah statlarına göre çarpan faktörü hesaplar.
        /// Daha yüksek zırh değeri veya hasar = daha yüksek faktör.
        /// Aralık: 0.8 - 1.5
        /// </summary>
        private static float GetEquipmentStatFactor(ItemObject item)
        {
            if (item.HasArmorComponent && item.ArmorComponent != null)
            {
                float totalArmor = item.ArmorComponent.HeadArmor + item.ArmorComponent.BodyArmor
                                 + item.ArmorComponent.LegArmor + item.ArmorComponent.ArmArmor;
                return 0.8f + (totalArmor / 100f) * 0.7f;
            }
            if (item.PrimaryWeapon != null)
            {
                int damage = item.PrimaryWeapon.SwingDamage > 0 ? item.PrimaryWeapon.SwingDamage : item.PrimaryWeapon.ThrustDamage;
                return 0.8f + (damage / 100f) * 0.7f;
            }
            return 1.0f;
        }

        /// <summary>
        /// Siparişi oluşturur: parayı alır, eşyayı envanterden çıkarır,
        /// siparişi ActiveOrders listesine ekler.
        /// </summary>
        private static void FinalizeOrder(Town town, EquipmentElement selectedItem, ItemQuality selectedQuality, int price, int days)
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, price, false);
            MobileParty.MainParty.ItemRoster.AddToCounts(selectedItem, -1);

            var order = new SmithingOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                Town = town,
                OriginalItem = selectedItem,
                RequestedQuality = selectedQuality,
                Price = price,
                DaysRemaining = days,
                IsReady = false
            };
            MasterSmithData.ActiveOrders.Add(order);

            InformationManager.DisplayMessage(new InformationMessage(
                $"[MasterSmith] Order placed! Your item will be ready in {days} days.",
                Color.FromUint(0xFF00FF00)));
        }

        /// <summary>
        /// Belirli bir eşya ve kalite için uygun ItemModifier'ı bulur.
        /// 
        /// Arama stratejisi (sırasıyla):
        /// 1. Tam isim eşleşmesi: {kalite}_{eşyaTipi} (örn: fine_sword, masterwork_plate)
        /// 2. Genel eşleşme: {kalite}_cheap (örn: fine_cheap)
        /// 3. Legendary için lordly alternatifi: lordly_{eşyaTipi}, lordly_cheap
        /// 4. Son çare: StringId'sinde kalite adı geçen herhangi bir modifier
        /// 
        /// Her adımda IsModifierCompatibleWithItem ile uyumluluk kontrolü yapılır.
        /// </summary>
        public static ItemModifier GetItemModifierForQuality(ItemObject item, ItemQuality quality)
        {
            var allModifiers = MBObjectManager.Instance.GetObjectTypeList<ItemModifier>();
            string prefix = quality.ToString().ToLower();
            string itemTypeStr = GetItemTypeString(item);

            // Arama pattern'leri (öncelik sırasıyla)
            var patterns = new List<string>
            {
                $"{prefix}_{itemTypeStr}",       // Örn: fine_sword
                $"{prefix}_cheap",               // Örn: fine_cheap (genel)
                (quality == ItemQuality.Legendary) ? $"lordly_{itemTypeStr}" : null, // Sadece Legendary için lordly alternatifi
                (quality == ItemQuality.Legendary) ? "lordly_cheap" : null
            };

            // Pattern'leri sırayla dene, uyumlu ilk modifier'ı döndür
            foreach (var pattern in patterns)
            {
                if (pattern == null) continue;
                foreach (var modifier in allModifiers)
                {
                    if (modifier == null) continue;
                    if (modifier.StringId.ToLower() != pattern) continue;
                    if (modifier.ItemQuality == quality && IsModifierCompatibleWithItem(modifier, item))
                        return modifier;
                }
            }

            // Son çare: StringId'sinde prefix geçen ve uyumlu ilk modifier
            foreach (var modifier in allModifiers)
            {
                if (modifier == null) continue;
                if (modifier.StringId.ToLower().Contains(prefix) && modifier.ItemQuality == quality
                    && IsModifierCompatibleWithItem(modifier, item))
                    return modifier;
            }

            return null;
        }

        /// <summary>
        /// Bir ItemModifier'ın belirli bir eşyaya uygulanabilir olup olmadığını kontrol eder.
        /// 
        /// Kontrol mantığı:
        /// - Modifier'ın Damage/Speed/MissileSpeed bonusu varsa, eşya silah olmalı VE bonus pozitif olmalı
        /// - Modifier'ın Armor bonusu varsa, eşya zırh olmalı VE bonus pozitif olmalı
        /// - Modifier'ın MountSpeed/Maneuver/ChargeDamage/MountHitPoints bonusu varsa, eşya binek zırhı olmalı
        /// - Hiçbir pozitif stat bonusu olmayan modifier'lar elenir (IsBeneficial)
        /// 
        /// Bu sayede masterwork_sword zırha, fine_plate silaha uygulanamaz.
        /// </summary>
        private static bool IsModifierCompatibleWithItem(ItemModifier modifier, ItemObject item)
        {
            bool isWeapon = IsWeapon(item);
            bool isArmor = item.HasArmorComponent;
            bool isHorseHarness = item.ItemType == ItemObject.ItemTypeEnum.HorseHarness;

            // Silah stat bonusları kontrolü
            if (modifier.Damage != 0 || modifier.Speed != 0 || modifier.MissileSpeed != 0)
            {
                if (!isWeapon) return false;
                if (modifier.Damage <= 0 && modifier.Speed <= 0 && modifier.MissileSpeed <= 0) return false;
            }

            // Zırh stat bonusu kontrolü
            if (modifier.Armor != 0)
            {
                if (!isArmor) return false;
                if (modifier.Armor <= 0) return false;
            }

            // Binek stat bonusları kontrolü
            if (modifier.MountSpeed != 0f || modifier.Maneuver != 0f || modifier.ChargeDamage != 0f || modifier.MountHitPoints != 0f)
            {
                if (!isHorseHarness) return false;
                if (modifier.MountSpeed <= 0f && modifier.Maneuver <= 0f && modifier.ChargeDamage <= 0f && modifier.MountHitPoints <= 0f)
                    return false;
            }

            // Hiçbir faydalı stat bonusu yoksa uyumsuz
            if (!modifier.IsBeneficial())
                return false;

            return true;
        }

        /// <summary>
        /// Eşyanın ItemType'ına göre ItemModifier StringId'lerinde kullanılan tip string'ini döndürür.
        /// Örn: OneHandedWeapon -> "sword", HeadArmor + yüksek zırh -> "plate"
        /// </summary>
        private static string GetItemTypeString(ItemObject item)
        {
            switch (item.ItemType)
            {
                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                    return "sword";
                case ItemObject.ItemTypeEnum.Polearm:
                    return "polearm";
                case ItemObject.ItemTypeEnum.Bow:
                    return "bow";
                case ItemObject.ItemTypeEnum.Crossbow:
                    return "crossbow";
                case ItemObject.ItemTypeEnum.Shield:
                    return "shield";
                case ItemObject.ItemTypeEnum.Arrows:
                    return "arrows";
                case ItemObject.ItemTypeEnum.Bolts:
                    return "bolts";
                case ItemObject.ItemTypeEnum.Thrown:
                    return "spear_dart_throwing";
                case ItemObject.ItemTypeEnum.HeadArmor:
                case ItemObject.ItemTypeEnum.BodyArmor:
                case ItemObject.ItemTypeEnum.LegArmor:
                case ItemObject.ItemTypeEnum.HandArmor:
                case ItemObject.ItemTypeEnum.ChestArmor:
                case ItemObject.ItemTypeEnum.Cape:
                    if (item.ArmorComponent != null)
                    {
                        int totalArmor = item.ArmorComponent.HeadArmor + item.ArmorComponent.BodyArmor
                                       + item.ArmorComponent.LegArmor + item.ArmorComponent.ArmArmor;
                        if (totalArmor >= 40) return "plate";
                        if (totalArmor >= 25) return "chain";
                        if (totalArmor >= 15) return "leather";
                    }
                    return "cloth";
                default:
                    return "cheap";
            }
        }

        /// <summary>
        /// Eşyanın mevcut kalitesini okunabilir string olarak döndürür.
        /// </summary>
        private static string GetQualityText(EquipmentElement element)
        {
            if (element.ItemModifier != null)
                return element.ItemModifier.Name.ToString();
            return "Common";
        }

        /// <summary>
        /// Eşyanın ItemModifier'ından ItemQuality enum değerini belirler.
        /// StringId'deki anahtar kelimelere göre eşleştirme yapar.
        /// </summary>
        private static ItemQuality GetItemQuality(EquipmentElement element)
        {
            if (element.ItemModifier == null) return ItemQuality.Common;
            string modifierId = element.ItemModifier.StringId.ToLower();
            if (modifierId.Contains("legendary") || modifierId.Contains("lordly")) return ItemQuality.Legendary;
            if (modifierId.Contains("masterwork")) return ItemQuality.Masterwork;
            if (modifierId.Contains("fine")) return ItemQuality.Fine;
            if (modifierId.Contains("cracked") || modifierId.Contains("rusty") || modifierId.Contains("dull")
                || modifierId.Contains("bent") || modifierId.Contains("splintered") || modifierId.Contains("dented")
                || modifierId.Contains("ripped") || modifierId.Contains("worn")) return ItemQuality.Poor;
            if (modifierId.Contains("battered") || modifierId.Contains("lame") || modifierId.Contains("loose")
                || modifierId.Contains("unbalanced")) return ItemQuality.Inferior;
            return ItemQuality.Common;
        }
    }
}