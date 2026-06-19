using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MasterSmith
{
    /// <summary>
    /// Crafting ekranında CTRL+H kısayolu ile Uzman Demirci sipariş menüsünü açar.
    /// GauntletCraftingScreen.OnFrameTick'e Harmony Postfix ile bağlanır.
    /// </summary>
    [HarmonyPatch(typeof(SandBox.GauntletUI.GauntletCraftingScreen), "OnFrameTick")]
    internal class MasterSmithInputPatch
    {
        // Tekrarlayan açılmayı engellemek için basit flag
        private static bool _orderScreenOpen = false;

        static void Postfix()
        {
            // CTRL+H: Uzman Demirci sipariş menüsünü açar
            if (Input.IsKeyDown(InputKey.LeftControl) && Input.IsKeyPressed(InputKey.H))
            {
                if (_orderScreenOpen) return;

                var currentSettlement = Hero.MainHero?.CurrentSettlement;
                if (currentSettlement == null || !currentSettlement.IsTown)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        "[MasterSmith] You must be in a town to commission items.",
                        Color.FromUint(0xFFFF0000)));
                    return;
                }

                var town = currentSettlement.Town;
                if (!MasterSmithData.IsMasterSmithCity(town))
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[MasterSmith] {town.Name} does not have a master smith.",
                        Color.FromUint(0xFFFF0000)));
                    return;
                }

                _orderScreenOpen = true;
                MasterSmithOrderLogic.StartOrder(town);
                _orderScreenOpen = false;
            }
        }
    }
}