using System;

namespace Keepercraft.RimKeeperSaves.Extensions
{
    public static class FileExtension
    {
        public static string ToBytesCount(this long bytes)
        {
            int unit = 1024;
            string unitStr = "B";
            if (bytes < unit)
            {
                return string.Format("{0} {1}", bytes, unitStr);
            }
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return string.Format("{0:##.##}{1}{2}", bytes / Math.Pow(unit, exp), "KMGTPEZY"[exp - 1], unitStr);
        }

        public static bool IsSaveFile(this string path)
        {
            return path.EndsWith(".rws");
        }
    }
}
