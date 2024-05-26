using Keepercraft.RimKeeperSaves.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace Keepercraft.RimKeeperSaves
{
    public class ZipFileDirectory
    {
        public float progress = 0f;
        public bool progress_run = false;
        public int gamesaveFolderCount = 0;
        public int gamesaveFolderCountNew = 0;
        public long gamesaveFolderSize = 0;
        public long gamesaveFolderSizeNew = 0;
        public string gamesaveActualfile = "";

        public bool analize_run = false;
        public int analizeComressCount = 0;
        public int analizeXMLCount = 0;
        public long analizeComressSise = 0;
        public long analizeXMLSize = 0;

        public IEnumerable<string> GetSaveFiles()
        {
            string saveLocation = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
            return Directory.EnumerateFiles(saveLocation, "*.rws", SearchOption.AllDirectories);
        }

        public void ThreadAnalize()
        {
            if (!analize_run)
                Task.Run(() =>
                {
                    analize_run = true;
                    analizeComressCount = 0;
                    analizeXMLCount = 0;
                    analizeComressSise = 0;
                    analizeXMLSize = 0;
                    try
                    {
                        foreach (var item in GetSaveFiles())
                        {
                            bool iszip = ZipFileReader.IsFileXML(item);
                            long size = new FileInfo(item).Length;

                            if (iszip)
                            {
                                analizeXMLCount++;
                                analizeXMLSize += size;
                            }
                            else
                            {
                                analizeComressCount++;
                                analizeComressSise += size;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Analize ERROR:" + ex.Message);
                    }
                    finally
                    {
                        analize_run = false;
                    }
                });
        }

        public void ThreadFileDecompress(CompressionMode mode)
        {
            if (!progress_run)
                Task.Run(() =>
                {
                    progress_run = true;
                    progress = 0f;
                    DebugHelper.Message("START");
                    var files = GetSaveFiles().ToList();
                    gamesaveFolderCount = files.Count();
                    gamesaveFolderCountNew = 0;
                    gamesaveFolderSize = 0;
                    gamesaveFolderSizeNew = 0;
                    // gamesaveFolderSize = files.Sum(f => File.(f).).ToBytesCount();
                    //long totalSize = files.Sum(file => new FileInfo(file).Length);

                    try
                    {
                        for (int i = 0; i < gamesaveFolderCount; i++)
                        {
                            //Thread.Sleep(3000);
                            var item = files[i];

                            bool iszip = ZipFileReader.IsFileXML(item);

                            if (!iszip && mode == CompressionMode.Decompress)
                            {
                                DebugHelper.Message("File:" + item);
                                gamesaveActualfile = item;
                                using (var fileStream = new FileStream(item, FileMode.Open, FileAccess.ReadWrite))
                                {
                                    gamesaveFolderSize += fileStream.Length;
                                    using (MemoryStream decompressedStream = new MemoryStream())
                                    {
                                        using (var zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                                        {
                                            zipStream.CopyTo(decompressedStream);
                                            fileStream.Seek(0, SeekOrigin.Begin);
                                            decompressedStream.WriteTo(fileStream);
                                            fileStream.SetLength(fileStream.Position);
                                            gamesaveFolderSizeNew += fileStream.Length;
                                        }
                                    }
                                }
                                gamesaveFolderCountNew++;
                            }
                            else if (iszip && mode == CompressionMode.Compress)
                            {
                                DebugHelper.Message("File:" + item);
                                gamesaveActualfile = item;
                                using (var fileStream = new FileStream(item, FileMode.Open, FileAccess.ReadWrite))
                                {
                                    gamesaveFolderSize += fileStream.Length;
                                    using (MemoryStream compressedStream = new MemoryStream())
                                    {
                                        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                                        {
                                            fileStream.CopyTo(zipStream);
                                            fileStream.Seek(0, SeekOrigin.Begin);
                                            compressedStream.WriteTo(fileStream);
                                            fileStream.SetLength(fileStream.Position);
                                            gamesaveFolderSizeNew += fileStream.Length;
                                        }
                                    }
                                }
                                gamesaveFolderCountNew++;
                            }

                            progress = (float)i / (float)gamesaveFolderCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("ThreadFileDecompress ERROR:" + ex.Message);
                    }
                    finally
                    {
                        gamesaveActualfile = "";
                        progress = 1f;
                        progress_run = false;
                    }
                });
        }
    }
}
