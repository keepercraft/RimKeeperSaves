using System;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Keepercraft.RimKeeperSaves
{
    public class ZipFileReader : IDisposable
    {
        private FileStream fileStream;
        private GZipStream zipStream;
        private StreamReader readerStream;
        public XmlTextReader XmlReader { get; private set; }

        public ZipFileReader(string path) 
        {
            bool isxml = IsFileXML(path);
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (isxml)
            {
                //Log.Message(string.Format("ZipFileReader StreamReader"));
                readerStream = new StreamReader(fileStream);
                XmlReader = new XmlTextReader(readerStream);
            }
            else
            {
                //Log.Message(string.Format("ZipFileReader GZipStream"));
                zipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                XmlReader = new XmlTextReader(zipStream);
            }
        }

        public void Dispose()
        {
            //Log.Message(string.Format("ZipFileReader Dispose"));
            XmlReader?.Dispose();
            readerStream?.Dispose();
            zipStream?.Dispose();
            fileStream?.Dispose();
            XmlReader = null;
            readerStream = null;
            zipStream = null;
            fileStream = null;
        }

        public static bool IsFileXML(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                char[] buffer = new char[5];
                reader.Read(buffer, 0, 5);
                string startOfFile = new string(buffer);
                return startOfFile.Equals("<?xml");
            }
        }
    }

}
