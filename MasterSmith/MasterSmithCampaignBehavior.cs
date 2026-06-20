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
    /// Main CampaignBehavior for MasterSmith.
    /// 
    /// Weekly: Refreshes prices in all master smith cities.
    /// Daily: Decrements remaining days on active orders, marks expired ones as ready.
    /// SettlementEntered: Auto-delivers ready orders when the player enters the city.
    /// OnGameLoaded: Shows usage hint message when a game is loaded.
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
        /// Shows a usage hint when a game is loaded (new game or save).
        /// </summary>
        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            TextObject msg = new TextObject("{=MS_LOADED}[MasterSmith] Mod loaded. Press CTRL + H at the smithy to open the master smith order menu.", null);
            InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFF00FF00)));
        }

        /// <summary>
        /// Called when the player enters a settlement.
        /// Only triggers for the main hero and only for town/castle settlements.
        /// Auto-delivers any ready orders for that city.
        /// </summary>
        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (hero != Hero.MainHero) return;
            if (!settlement.IsTown && !settlement.IsFortification) return;

            var readyOrders = MasterSmithData.ActiveOrders
                .Select(csv => SmithingOrder.FromCsv(csv))
                .Where(o => o != null && o.IsReady && o.GetTown()?.Settlement == settlement)
                .ToList();

            foreach (var order in readyOrders)
                DeliverOrder(order);
        }

        /// <summary>
        /// Weekly tick: Randomly regenerates prices in all master smith cities from MCM ranges.
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
                MasterSmithData.SetPricesForTown(settlementId, combined);
            }
        }

        /// <summary>
        /// Daily tick: Decrements remaining days on all active orders by 1.
        /// Marks orders as ready when DaysRemaining reaches 0.
        /// Writes changes back as CSV strings.
        /// </summary>
        private void OnDailyTick()
        {
            var orders = MasterSmithData.ActiveOrders
                .Select(csv => SmithingOrder.FromCsv(csv))
                .Where(o => o != null)
                .ToList();

            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                if (!order.IsReady)
                {
                    order.DaysRemaining -= 1;
                    if (order.DaysRemaining <= 0)
                    {
                        order.IsReady = true;
                        NotifyOrderReady(order);
                    }
                    MasterSmithData.ActiveOrders[i] = order.ToCsv();
                }
            }
        }

        /// <summary>
        /// Sends a notification when an order is ready.
        /// If the player is currently in the city, delivers immediately.
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
                TextObject msg = new TextObject("{=MS_READY}{QUALITY} {ITEM} is ready at {TOWN}'s Master Smith!", null);
                msg.SetTextVariable("QUALITY", order.RequestedQuality.ToString());
                msg.SetTextVariable("ITEM", originalItem.Item.Name.ToString());
                msg.SetTextVariable("TOWN", town.Name.ToString());
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0x00FF00)));
            }
        }

        /// <summary>
        /// Delivers the order: finds the matching ItemModifier, adds the upgraded item to inventory.
        /// If no modifier is found, returns the original item.
        /// Removes the delivered order from the active list.
        /// </summary>
        private void DeliverOrder(SmithingOrder order)
        {
            EquipmentElement originalItem = order.GetOriginalItem();
            ItemModifier modifier = MasterSmithOrderLogic.GetItemModifierForQuality(originalItem.Item, order.RequestedQuality);
            if (modifier != null)
            {
                EquipmentElement upgradedElement = new EquipmentElement(originalItem.Item, modifier, null, false);
                MobileParty.MainParty.ItemRoster.AddToCounts(upgradedElement, 1);

                TextObject msg = new TextObject("{=MS_DELIVERED}{MODIFIER} {ITEM} has been delivered to your inventory.", null);
                msg.SetTextVariable("MODIFIER", modifier.Name.ToString());
                msg.SetTextVariable("ITEM", originalItem.Item.Name.ToString());
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0x00FF00)));
            }
            else
            {
                MobileParty.MainParty.ItemRoster.AddToCounts(originalItem, 1);
                TextObject msg = new TextObject("{=MS_DELIVERY_FAILED}[MasterSmith] Could not apply quality modifier. Original item returned.", null);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFFFF0000)));
            }

            string csvToRemove = order.ToCsv();
            MasterSmithData.ActiveOrders.Remove(csvToRemove);
        }

        /// <summary>
        /// Writes/reads prices and active orders to/from the save file.
        /// Both are string-based collections, ensuring problem-free serialization.
        /// </summary>
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("MS_Prices", ref MasterSmithData.CurrentPrices);
            dataStore.SyncData("MS_Orders", ref MasterSmithData.ActiveOrders);
        }
    }
}