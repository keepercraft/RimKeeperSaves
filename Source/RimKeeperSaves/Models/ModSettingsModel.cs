using Keepercraft.RimKeeperSaves.Helpers;
using Verse;

namespace Keepercraft.RimKeeperSaves.Models
{
    public class RimKeeperSavesModSettings : ModSettingsInit
    {
        public static bool DebugLog = false;
        public static bool SaveCompressionActive = true;

        public override void ExposeData()
        {
            base.ExposeData();
            DebugHelper.Active = DebugLog;
            Scribe_Values.Look(ref DebugLog, nameof(DebugLog), false);
            Scribe_Values.Look(ref SaveCompressionActive, nameof(SaveCompressionActive), true);
        }
    }
}