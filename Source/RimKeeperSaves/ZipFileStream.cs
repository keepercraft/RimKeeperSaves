using System.IO;
using System.IO.Compression;

namespace Keepercraft.RimKeeperSaves
{
    public class ZipFileStream : GZipStream
    {
        private FileStream fileStream;

        public static ZipFileStream Factor(string path, FileMode mode, FileAccess access, FileShare shere)
        {
            FileStream stream = new FileStream(path, mode, access, shere);
            return new ZipFileStream(stream, CompressionLevel.Optimal);
        }

        public ZipFileStream(FileStream fileStream, CompressionLevel lvl) : base(fileStream, lvl)
        {
            this.fileStream = fileStream;
            //Log.Message("ZipFileStream Create Stream");
        }

        public override void Close()
        {
            base.Close();
            fileStream.Dispose();
            fileStream = null;
        }
    }
}