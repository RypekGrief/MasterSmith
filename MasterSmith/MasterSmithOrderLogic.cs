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
    /// Manages the entire order flow UI and business logic.
    /// Flow: Item selection -> Quality selection -> Price confirmation -> Order creation
    /// </summary>
    public static class MasterSmithOrderLogic
    {
        /// <summary>
        /// Equipment types that the master smith can upgrade.
        /// Includes weapons, armors, shields, arrows, horse harnesses, and banners.
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
        /// Starts the order flow. Called by CTRL+H.
        /// Checks if the player has any eligible items first.
        /// </summary>
        public static void StartOrder(Town town)
        {
            var playerItems = GetEligibleItemsForSmithing();
            if (playerItems.Count == 0)
            {
                TextObject msg = new TextObject("{=MS_NO_ITEMS}[MasterSmith] You don't have any upgradable items in your inventory.", null);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFFFF0000)));
                return;
            }
            ShowItemSelection(town, playerItems);
        }

        /// <summary>
        /// Collects upgradable items from the player's inventory.
        /// Filters out Legendary items, ineligible types, and duplicate StringId+Modifier combos.
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
        /// Step 1: Item selection menu.
        /// Lists all eligible items, player picks one.
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
                    titleText: new TextObject("{=MS_SELECT_ITEM_TITLE}Select Item to Upgrade", null).ToString(),
                    descriptionText: new TextObject("{=MS_SELECT_ITEM_DESC}Choose which item you'd like the master smith to work on.", null).ToString(),
                    inquiryElements: inquiryElements,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: new TextObject("{=MS_NEXT}Next", null).ToString(),
                    negativeText: new TextObject("{=MS_CANCEL}Cancel", null).ToString(),
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
        /// Step 2: Quality selection menu.
        /// Checks which qualities are available for the selected item.
        /// Only qualities with a truly compatible modifier are shown.
        /// Legendary quality has a culture restriction.
        /// </summary>
        private static void ShowQualitySelection(Town town, EquipmentElement selectedItem)
        {
            var settings = MasterSmithSettings.Instance;
            var inquiryElements = new List<InquiryElement>();

            bool canBeFine = GetItemModifierForQuality(selectedItem.Item, ItemQuality.Fine) != null;
            bool canBeMasterwork = GetItemModifierForQuality(selectedItem.Item, ItemQuality.Masterwork) != null;
            bool canBeLegendary = GetItemModifierForQuality(selectedItem.Item, ItemQuality.Legendary) != null;

            if (canBeLegendary)
            {
                CultureObject smithCulture = MasterSmithData.GetSmithCulture(town);
                CultureObject itemCulture = selectedItem.Item.Culture as CultureObject;
                if (smithCulture != null && itemCulture != null && smithCulture.StringId != itemCulture.StringId)
                    canBeLegendary = false;
            }

            if (canBeFine)
            {
                TextObject fineText = new TextObject("{=MS_QUALITY_FINE}Fine ({DAYS} days)", null);
                fineText.SetTextVariable("DAYS", (int)settings.FineCraftingDays);
                inquiryElements.Add(new InquiryElement(ItemQuality.Fine, fineText.ToString(), null));
            }
            if (canBeMasterwork)
            {
                TextObject mwText = new TextObject("{=MS_QUALITY_MASTERWORK}Masterwork ({DAYS} days)", null);
                mwText.SetTextVariable("DAYS", (int)settings.MasterworkCraftingDays);
                inquiryElements.Add(new InquiryElement(ItemQuality.Masterwork, mwText.ToString(), null));
            }
            if (canBeLegendary)
            {
                TextObject legText = new TextObject("{=MS_QUALITY_LEGENDARY}Legendary ({DAYS} days)", null);
                legText.SetTextVariable("DAYS", (int)settings.LegendaryCraftingDays);
                inquiryElements.Add(new InquiryElement(ItemQuality.Legendary, legText.ToString(), null));
            }

            if (inquiryElements.Count == 0)
            {
                TextObject msg = new TextObject("{=MS_NO_QUALITY}[MasterSmith] No quality upgrades available for this item.", null);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFFFF0000)));
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    titleText: new TextObject("{=MS_SELECT_QUALITY_TITLE}Select Quality", null).ToString(),
                    descriptionText: new TextObject("{=MS_SELECT_QUALITY_DESC}Choose the desired quality level.", null).ToString(),
                    inquiryElements: inquiryElements,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: new TextObject("{=MS_CALCULATE_PRICE}Calculate Price", null).ToString(),
                    negativeText: new TextObject("{=MS_BACK}Back", null).ToString(),
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
        /// Step 3: Price confirmation menu.
        /// Shows the calculated price and duration, asks the player to confirm.
        /// </summary>
        private static void ShowPriceConfirmation(Town town, EquipmentElement selectedItem, ItemQuality selectedQuality)
        {
            int price = CalculatePrice(town, selectedItem, selectedQuality);
            int days = GetCraftingDays(selectedQuality);

            TextObject descText = new TextObject("{=MS_CONFIRM_ORDER_DESC}Item: {ITEM_NAME}\nQuality: {QUALITY}\nTime: {DAYS} days\nPrice: {PRICE} denars\n\nPlace this order?", null);
            descText.SetTextVariable("ITEM_NAME", selectedItem.Item.Name.ToString());
            descText.SetTextVariable("QUALITY", selectedQuality.ToString());
            descText.SetTextVariable("DAYS", days);
            descText.SetTextVariable("PRICE", price);
            string message = descText.ToString();

            bool canAfford = Hero.MainHero.Gold >= price;

            var inquiryElements = new List<InquiryElement>();
            if (canAfford)
                inquiryElements.Add(new InquiryElement(true, new TextObject("{=MS_PLACE_ORDER}Place Order", null).ToString(), null));
            else
            {
                TextObject poorText = new TextObject("{=MS_NOT_ENOUGH_GOLD}Not enough gold! (Need {PRICE}, have {GOLD})", null);
                poorText.SetTextVariable("PRICE", price);
                poorText.SetTextVariable("GOLD", Hero.MainHero.Gold);
                inquiryElements.Add(new InquiryElement(false, poorText.ToString(), null));
            }
            inquiryElements.Add(new InquiryElement(false, new TextObject("{=MS_CANCEL}Cancel", null).ToString(), null));

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    titleText: new TextObject("{=MS_CONFIRM_ORDER_TITLE}Confirm Order", null).ToString(),
                    descriptionText: message,
                    inquiryElements: inquiryElements,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: new TextObject("{=MS_CONFIRM}Confirm", null).ToString(),
                    negativeText: new TextObject("{=MS_BACK}Back", null).ToString(),
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

        private static int CalculatePrice(Town town, EquipmentElement selectedItem, ItemQuality selectedQuality)
        {
            var settings = MasterSmithSettings.Instance;
            string settlementId = town.Settlement.StringId;

            var prices = MasterSmithData.GetPricesForTown(settlementId);
            if (prices == null)
            {
                GeneratePricesForTown(town);
                prices = MasterSmithData.GetPricesForTown(settlementId);
            }

            if (prices != null)
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

                float statMultiplier = settings.EquipmentStatMultiplier;
                float equipmentFactor = GetEquipmentStatFactor(selectedItem.Item);
                int statBonus = (int)(basePrice * (equipmentFactor - 1.0f) * (statMultiplier / 10.0f));

                ItemQuality currentQuality = GetItemQuality(selectedItem);
                float qualityMultiplier = 1.0f;
                if (currentQuality == ItemQuality.Poor || currentQuality == ItemQuality.Inferior)
                    qualityMultiplier = 1.2f;
                else if (currentQuality == ItemQuality.Masterwork && selectedQuality == ItemQuality.Legendary)
                    qualityMultiplier = 0.85f;

                int finalPrice = (int)((basePrice + statBonus) * qualityMultiplier);
                return Math.Max(1, finalPrice);
            }

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

        private static void GeneratePricesForTown(Town town)
        {
            var settings = MasterSmithSettings.Instance;
            var combined = new List<int>(6);
            combined.Add(MBRandom.RandomInt((int)settings.FineWeaponMinPrice, (int)settings.FineWeaponMaxPrice + 1));
            combined.Add(MBRandom.RandomInt((int)settings.FineArmorMinPrice, (int)settings.FineArmorMaxPrice + 1));
            combined.Add(MBRandom.RandomInt((int)settings.MasterworkWeaponMinPrice, (int)settings.MasterworkWeaponMaxPrice + 1));
            combined.Add(MBRandom.RandomInt((int)settings.MasterworkArmorMinPrice, (int)settings.MasterworkArmorMaxPrice + 1));
            combined.Add(MBRandom.RandomInt((int)settings.LegendaryWeaponMinPrice, (int)settings.LegendaryWeaponMaxPrice + 1));
            combined.Add(MBRandom.RandomInt((int)settings.LegendaryArmorMinPrice, (int)settings.LegendaryArmorMaxPrice + 1));
            MasterSmithData.SetPricesForTown(town.Settlement.StringId, combined);
        }

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

        private static void FinalizeOrder(Town town, EquipmentElement selectedItem, ItemQuality selectedQuality, int price, int days)
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, price, false);
            MobileParty.MainParty.ItemRoster.AddToCounts(selectedItem, -1);

            var order = SmithingOrder.Create(town, selectedItem, selectedQuality, price, days);
            MasterSmithData.ActiveOrders.Add(order.ToCsv());

            TextObject msg = new TextObject("{=MS_ORDER_PLACED}[MasterSmith] Order placed! Your item will be ready in {DAYS} days.", null);
            msg.SetTextVariable("DAYS", days);
            InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFF00FF00)));
        }

        public static ItemModifier GetItemModifierForQuality(ItemObject item, ItemQuality quality)
        {
            var allModifiers = MBObjectManager.Instance.GetObjectTypeList<ItemModifier>();
            string prefix = quality.ToString().ToLower();
            string itemTypeStr = GetItemTypeString(item);

            var patterns = new List<string>
            {
                $"{prefix}_{itemTypeStr}",
                $"{prefix}_cheap",
                (quality == ItemQuality.Legendary) ? $"lordly_{itemTypeStr}" : null,
                (quality == ItemQuality.Legendary) ? "lordly_cheap" : null
            };

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

            foreach (var modifier in allModifiers)
            {
                if (modifier == null) continue;
                if (modifier.StringId.ToLower().Contains(prefix) && modifier.ItemQuality == quality
                    && IsModifierCompatibleWithItem(modifier, item))
                    return modifier;
            }

            return null;
        }

        private static bool IsModifierCompatibleWithItem(ItemModifier modifier, ItemObject item)
        {
            bool isWeapon = IsWeapon(item);
            bool isArmor = item.HasArmorComponent;
            bool isHorseHarness = item.ItemType == ItemObject.ItemTypeEnum.HorseHarness;

            if (modifier.Damage != 0 || modifier.Speed != 0 || modifier.MissileSpeed != 0)
            {
                if (!isWeapon) return false;
                if (modifier.Damage <= 0 && modifier.Speed <= 0 && modifier.MissileSpeed <= 0) return false;
            }

            if (modifier.Armor != 0)
            {
                if (!isArmor) return false;
                if (modifier.Armor <= 0) return false;
            }

            if (modifier.MountSpeed != 0f || modifier.Maneuver != 0f || modifier.ChargeDamage != 0f || modifier.MountHitPoints != 0f)
            {
                if (!isHorseHarness) return false;
                if (modifier.MountSpeed <= 0f && modifier.Maneuver <= 0f && modifier.ChargeDamage <= 0f && modifier.MountHitPoints <= 0f)
                    return false;
            }

            if (!modifier.IsBeneficial())
                return false;

            return true;
        }

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

        private static string GetQualityText(EquipmentElement element)
        {
            if (element.ItemModifier != null)
                return element.ItemModifier.Name.ToString();
            return "Common";
        }

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