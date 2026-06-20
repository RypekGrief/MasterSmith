using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MasterSmith
{
    /// <summary>
    /// Opens the Master Smith order menu when CTRL+H is pressed on the smithy screen.
    /// Attached to GauntletCraftingScreen.OnFrameTick via Harmony Postfix.
    /// 
    /// Validates:
    /// - Player must be in a town
    /// - The town must have a master smith
    /// - Prevents multiple menus from opening simultaneously via a simple flag
    /// </summary>
    [HarmonyPatch(typeof(SandBox.GauntletUI.GauntletCraftingScreen), "OnFrameTick")]
    internal class MasterSmithInputPatch
    {
        // Simple flag to prevent re-entry while the menu is open
        private static bool _orderScreenOpen = false;

        static void Postfix()
        {
            // CTRL+H: Opens the Master Smith order menu
            if (Input.IsKeyDown(InputKey.LeftControl) && Input.IsKeyPressed(InputKey.H))
            {
                if (_orderScreenOpen) return;

                var currentSettlement = Hero.MainHero?.CurrentSettlement;
                if (currentSettlement == null || !currentSettlement.IsTown)
                {
                    TextObject msg = new TextObject("{=MS_NOT_IN_TOWN}[MasterSmith] You must be in a town to commission items.", null);
                    InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFFFF0000)));
                    return;
                }

                var town = currentSettlement.Town;
                if (!MasterSmithData.IsMasterSmithCity(town))
                {
                    TextObject msg = new TextObject("{=MS_NO_SMITH}[MasterSmith] {TOWN_NAME} does not have a master smith.", null);
                    msg.SetTextVariable("TOWN_NAME", town.Name.ToString());
                    InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Color.FromUint(0xFFFF0000)));
                    return;
                }

                _orderScreenOpen = true;
                MasterSmithOrderLogic.StartOrder(town);
                _orderScreenOpen = false;
            }
        }
    }
}