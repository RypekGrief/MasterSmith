using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MasterSmith
{
    /// <summary>
    /// Entry point for the MasterSmith mod.
    /// Registers Harmony patches on load and attaches the CampaignBehavior on game start.
    /// </summary>
    public class SubModule : MBSubModuleBase
    {
        /// <summary>
        /// Called when the submodule DLL is loaded by the game launcher.
        /// Applies all Harmony patches and initializes MCM settings.
        /// </summary>
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("com.rypekgrief.mastersmith").PatchAll();

            // Start MCM settings
            var settings = MasterSmithSettings.Instance;
        }

        /// <summary>
        /// Called when a game (campaign) is starting.
        /// Registers the MasterSmithCampaignBehavior to receive game events.
        /// </summary>
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter starter = gameStarterObject as CampaignGameStarter;
                starter?.AddBehavior(new MasterSmithCampaignBehavior());
            }
        }
    }
}