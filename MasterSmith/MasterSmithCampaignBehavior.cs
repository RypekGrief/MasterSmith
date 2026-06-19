using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MasterSmith
{
    /// <summary>
    /// MasterSmith'in ana CampaignBehavior'ı.
    /// 
    /// Haftalık: Uzman demirci şehirlerindeki fiyatları günceller.
    /// Günlük: Aktif siparişlerin kalan günlerini azaltır, süresi dolanları teslime hazırlar.
    /// SettlementEntered: Oyuncu şehre girdiğinde hazır siparişleri otomatik teslim eder.
    /// OnGameLoaded: Oyuna girişte kullanım bilgisi mesajı gösterir.
    /// </summary>
    public class MasterSmithCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        /// <summary>
        /// Oyun yüklendiğinde (yeni oyun veya kayıt yükleme) oyuncuya modun nasıl kullanılacağını bildirir.
        /// </summary>
        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            InformationManager.DisplayMessage(new InformationMessage(
                "[MasterSmith] Mod loaded. Press CTRL + H at the smithy to open the master smith order menu.",
                Color.FromUint(0xFF00FF00)));
        }

        /// <summary>
        /// Oyuncu bir yerleşkeye girdiğinde çağrılır.
        /// Sadece ana karakter için ve sadece şehir/kale yerleşkelerinde çalışır.
        /// O şehirde hazır bekleyen siparişleri otomatik teslim eder.
        /// </summary>
        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (hero != Hero.MainHero) return;
            if (!settlement.IsTown && !settlement.IsFortification) return;

            var readyOrders = MasterSmithData.ActiveOrders
                .Where(o => o.IsReady && o.GetTown()?.Settlement == settlement)
                .ToList();

            foreach (var order in readyOrders)
                DeliverOrder(order);
        }

        /// <summary>
        /// Haftalık tick: Tüm uzman demirci şehirlerinde fiyatları MCM aralıklarından rastgele yeniler.
        /// </summary>
        private void OnWeeklyTick()
        {
            foreach (var kvp in MasterSmithData.SmithCities)
            {
                string settlementId = kvp.Key;
                Town town = Town.AllTowns.FirstOrDefault(t => t.Settlement.StringId == settlementId);
                if (town == null) continue;

                var settings = MasterSmithSettings.Instance;
                var combined = new List<int>(6);
                combined.Add(MBRandom.RandomInt((int)settings.FineWeaponMinPrice, (int)settings.FineWeaponMaxPrice + 1));
                combined.Add(MBRandom.RandomInt((int)settings.FineArmorMinPrice, (int)settings.FineArmorMaxPrice + 1));
                combined.Add(MBRandom.RandomInt((int)settings.MasterworkWeaponMinPrice, (int)settings.MasterworkWeaponMaxPrice + 1));
                combined.Add(MBRandom.RandomInt((int)settings.MasterworkArmorMinPrice, (int)settings.MasterworkArmorMaxPrice + 1));
                combined.Add(MBRandom.RandomInt((int)settings.LegendaryWeaponMinPrice, (int)settings.LegendaryWeaponMaxPrice + 1));
                combined.Add(MBRandom.RandomInt((int)settings.LegendaryArmorMinPrice, (int)settings.LegendaryArmorMaxPrice + 1));
                MasterSmithData.CurrentPrices[settlementId] = combined;
            }
        }

        /// <summary>
        /// Günlük tick: Tüm aktif siparişlerin kalan gününü 1 azaltır.
        /// Süresi dolan siparişi teslime hazır olarak işaretler.
        /// </summary>
        private void OnDailyTick()
        {
            foreach (var order in MasterSmithData.ActiveOrders.ToList())
            {
                if (!order.IsReady)
                {
                    order.DaysRemaining -= 1;
                    if (order.DaysRemaining <= 0)
                    {
                        order.IsReady = true;
                        NotifyOrderReady(order);
                    }
                }
            }
        }

        /// <summary>
        /// Sipariş hazır olduğunda oyuncuya bildirim gönderir.
        /// Eğer oyuncu o anda şehirdeyse hemen teslim eder.
        /// </summary>
        private void NotifyOrderReady(SmithingOrder order)
        {
            Town town = order.GetTown();
            if (town != null && Hero.MainHero.CurrentSettlement == town.Settlement)
            {
                DeliverOrder(order);
            }
            else if (town != null)
            {
                EquipmentElement originalItem = order.GetOriginalItem();
                TextObject msg = new TextObject("{=MS_READY}{QUALITY} {ITEM} ekipmanınız {TOWN} Uzman Demircisinde hazır!", null);
                msg.SetTextVariable("QUALITY", order.RequestedQuality.ToString());
                msg.SetTextVariable("ITEM", originalItem.Item.Name.ToString());
                msg.SetTextVariable("TOWN", town.Name.ToString());
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0x00FF00)));
            }
        }

        /// <summary>
        /// Siparişi teslim eder: Uygun ItemModifier'ı bulur, yeni ekipmanı envantere ekler.
        /// Eğer modifier bulunamazsa orijinal eşyayı geri verir.
        /// </summary>
        private void DeliverOrder(SmithingOrder order)
        {
            EquipmentElement originalItem = order.GetOriginalItem();
            ItemModifier modifier = MasterSmithOrderLogic.GetItemModifierForQuality(originalItem.Item, order.RequestedQuality);
            if (modifier != null)
            {
                EquipmentElement upgradedElement = new EquipmentElement(originalItem.Item, modifier, null, false);
                MobileParty.MainParty.ItemRoster.AddToCounts(upgradedElement, 1);

                string modifierName = modifier.Name.ToString();
                string itemName = originalItem.Item.Name.ToString();
                TextObject msg = new TextObject("{=MS_DELIVERED}{MODIFIER} {ITEM} envanterinize teslim edildi", null);
                msg.SetTextVariable("MODIFIER", modifierName);
                msg.SetTextVariable("ITEM", itemName);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0x00FF00)));
            }
            else
            {
                MobileParty.MainParty.ItemRoster.AddToCounts(originalItem, 1);
                InformationManager.DisplayMessage(new InformationMessage(
                    "[MasterSmith] Could not apply quality modifier. Original item returned.",
                    Color.FromUint(0xFFFF0000)));
            }

            MasterSmithData.ActiveOrders.Remove(order);
        }

        /// <summary>
        /// Kayıt dosyasına fiyatları ve aktif siparişleri yazar/okur.
        /// </summary>
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("MS_Prices", ref MasterSmithData.CurrentPrices);
            dataStore.SyncData("MS_Orders", ref MasterSmithData.ActiveOrders);
        }
    }
}