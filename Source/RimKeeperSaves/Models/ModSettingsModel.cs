using Verse;

namespace Keepercraft.RimKeeperSaves.Models
{
    public class RimKeeperSavesModSettings : ModSettings
    {
        public static bool DebugLog = false;
        public static bool SaveCompressionActive = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DebugLog, nameof(DebugLog), false);
            Scribe_Values.Look(ref SaveCompressionActive, nameof(SaveCompressionActive), true);
        }
    }
}