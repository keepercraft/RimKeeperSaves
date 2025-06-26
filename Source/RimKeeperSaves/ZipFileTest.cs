using Keepercraft.RimKeeperSaves.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using Verse;

namespace Keepercraft.RimKeeperSaves
{
    public static class ZipFileTest
    {
        public static bool GZipSteamTest()
        {
            try
            {
                string testText = "Test GZipStream compatibility";
                byte[] compressed;

                using (var output = new MemoryStream())
                {
                    using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
                    using (var writer = new StreamWriter(gzip))
                    {
                        writer.Write(testText);
                    }
                    compressed = output.ToArray();
                }

                using (var input = new MemoryStream(compressed))
                using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzip))
                {
                    string result = reader.ReadToEnd();
                    return result.Equals(testText);
                }
                
            }
            catch (Exception ex)
            {
                Log.Error("GZipSteamTest ERROR:" + ex.Message);
                return false;
            }
        }
    }
}
