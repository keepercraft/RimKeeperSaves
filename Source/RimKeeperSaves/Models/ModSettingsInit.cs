using System;
using Verse;

namespace Keepercraft.RimKeeperSaves.Models
{
    public abstract class ModSettingsInit : ModSettings
    {
        private static bool initialized = false;

        public static void DoWindowContents(Action onInit)
        {
            if (!initialized)
            {
                initialized = true;
                onInit();
            }
        }

        public override void ExposeData()
        {
            initialized = false;
            base.ExposeData();
        }
    }
}