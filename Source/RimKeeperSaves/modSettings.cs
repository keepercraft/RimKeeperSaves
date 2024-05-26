using Keepercraft.RimKeeperSaves.Extensions;
using Keepercraft.RimKeeperSaves.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Keepercraft.RimKeeperSaves
{
    public class RimKeeperFilterHelperMod : Mod
    {
        private ZipFileDirectory zipper;

        public RimKeeperFilterHelperMod(ModContentPack content) : base(content)
        {
            GetSettings<RimKeeperSavesModSettings>();
            zipper = new ZipFileDirectory();
        }

        public override string SettingsCategory() => "RK Saves";

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect newRect = new Rect(inRect.x, inRect.y, inRect.width / 2, inRect.height);
            listingStandard.Begin(newRect);

            listingStandard.CheckboxLabeled("Debug Log", ref RimKeeperSavesModSettings.DebugLog, "Log messages");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Active Save Compression", ref RimKeeperSavesModSettings.SaveCompressionActive, "Compress game saves");
            listingStandard.Gap();

            listingStandard.Gap();

            if (listingStandard.ButtonText("Analize savefiles"))
            {
                zipper.ThreadAnalize();
            }
            listingStandard.Gap();

            if(zipper.analizeXMLCount> 0)
            {
                listingStandard.LabelDouble(
                    string.Format("Gamesave not compressed: {0}", zipper.analizeXMLCount),
                    string.Format("{0}", zipper.analizeXMLSize.ToBytesCount()));
            }
            if(zipper.analizeComressCount > 0)
            {
                listingStandard.LabelDouble(
                    string.Format("Gamesave compressed: {0}", zipper.analizeComressCount),
                    string.Format("{0}", zipper.analizeComressSise.ToBytesCount()));
            }

            listingStandard.End();

            Rect newRectRight = new Rect(inRect.x + (inRect.width / 2) + 20, inRect.y, inRect.width / 2, inRect.height);

            if (Widgets.ButtonText(
                new Rect(newRectRight.x, newRectRight.y, newRectRight.width / 2, 30),
                "Decompress All"))
            {
                zipper.ThreadFileDecompress(CompressionMode.Decompress);
            }
            if (Widgets.ButtonText(
                new Rect(newRectRight.x + (newRectRight.width / 2), newRectRight.y, newRectRight.width / 2, 30),
                "Compress All"))
            {
                zipper.ThreadFileDecompress(CompressionMode.Compress);
            }

            Widgets.FillableBar(
                new Rect(newRectRight.x, newRectRight.y + 40, newRectRight.width, 30),
                zipper.progress);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(
                new Rect(newRectRight.x, newRectRight.y + 40, newRectRight.width, 30),
                string.Format("{1}/{2} [{0}%]",
                    zipper.progress * 100,
                    zipper.gamesaveFolderCountNew,
                    zipper.gamesaveFolderCount));
            Text.Anchor = anchor;

            GUI.Label(
                new Rect(newRectRight.x, newRectRight.y + 80, newRectRight.width, 30), 
                string.Format("File size {1} -> {2} [{0}%]",
                    (zipper.gamesaveFolderSize <= 0 ? 0 : zipper.gamesaveFolderSizeNew / (float)zipper.gamesaveFolderSize) * 100,
                    zipper.gamesaveFolderSize.ToBytesCount(),
                    zipper.gamesaveFolderSizeNew.ToBytesCount()));
            GUI.Label(
                new Rect(newRectRight.x, newRectRight.y + 120f, newRectRight.width, newRectRight.height - 120f),
                string.Format("{0}", zipper.gamesaveActualfile));


            base.DoSettingsWindowContents(inRect);
        }
    }
}