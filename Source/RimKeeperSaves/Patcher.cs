using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Keepercraft.RimKeeperSaves.Extensions;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Keepercraft.RimKeeperSaves
{
    [StaticConstructorOnStartup]
    public static class Patcher
    {
        static Patcher()
        {
            Log.Message("RimKeeperSaves Patching");
            var harmony = new Harmony("Keepercraft.RimKeeperSaves");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(ScribeSaver), "InitSaving")]
        public static class InitSaving_Patch
        {
            static bool Prefix(string filePath, string documentElementName)
            {
                ScribeSaver scribeSaver = Scribe.saver;

                if (Scribe.mode != LoadSaveMode.Inactive)
                {
                    Log.Error("Called InitSaving() but current mode is " + Scribe.mode);
                    Scribe.ForceStop();
                }
                if (scribeSaver.GetPrivateField<string>("curPath") != null)
                {
                    Log.Error("Current path is not null in InitSaving");
                    scribeSaver.SetPrivateField("curPath", null);
                    scribeSaver.GetPrivateField<HashSet<string>>("savedNodes").Clear();
                    scribeSaver.SetPrivateField("nextListElementTemporaryId", 0);
                }
                try
                {
                    //Log.Message(string.Format("RimKeeperSaves InitSaving_Patch Path:{0}", filePath));

                    Scribe.mode = LoadSaveMode.Saving;
                    var saveStream = ZipFileStream.Factor(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    scribeSaver.SetPrivateField("saveStream", saveStream);
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.IndentChars = "\t";
                    var writer = XmlWriter.Create(saveStream, xmlWriterSettings);
                    scribeSaver.SetPrivateField("writer", writer);
                    writer.WriteStartDocument();
                    scribeSaver.EnterNode(documentElementName);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(new object[]
                    {
                    "Exception while init saving file: ",
                    filePath,
                    "\n",
                    ex
                    }));
                    scribeSaver.ForceStop();
                    throw;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ScribeLoader), "InitLoading")]
        public static class InitLoading_Patch
        {
            static bool Prefix(string filePath)
            {
                ScribeLoader scribeloader = Scribe.loader;
                //Log.Message(string.Format("RimKeeperSaves InitLoading_Patch Path:{0}", filePath));

                if (Scribe.mode != LoadSaveMode.Inactive)
                {
                    Log.Error("Called InitLoading() but current mode is " + Scribe.mode);
                    Scribe.ForceStop();
                }
                if (scribeloader.curParent != null)
                {
                    Log.Error("Current parent is not null in InitLoading");
                    scribeloader.curParent = null;
                }
                if (scribeloader.curPathRelToParent != null)
                {
                    Log.Error("Current path relative to parent is not null in InitLoading");
                    scribeloader.curPathRelToParent = null;
                }
                try
                {
                    using (ZipFileReader reader = new ZipFileReader(filePath))
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(reader.XmlReader);
                        scribeloader.curXmlParent = xmlDocument.DocumentElement;
                    }
                    //using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    //{
                    //    StreamReader reader = new StreamReader(stream);
                    //    char[] buffer = new char[5];
                    //    reader.Read(buffer, 0, 5);
                    //    string startOfFile = new string(buffer);
                    //    if (startOfFile.Equals("<?xml"))
                    //    {
                    //        reader.Dispose();
                    //        return true;
                    //    }
                    //    stream.Position = 0;

                    //    using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
                    //    {
                    //        using (XmlTextReader xmlTextReader = new XmlTextReader(zip))
                    //        {
                    //            XmlDocument xmlDocument = new XmlDocument();
                    //            xmlDocument.Load(xmlTextReader);
                    //            scribeloader.curXmlParent = xmlDocument.DocumentElement;
                    //        }
                    //    }
                    //}
                    Scribe.mode = LoadSaveMode.LoadingVars;
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(new object[]
                    {
                    "Exception while init loading file: ",
                    filePath,
                    "\n",
                    ex
                    }));
                    scribeloader.ForceStop();
                    return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ScribeLoader), "InitLoadingMetaHeaderOnly")]
        public static class InitLoadingMetaHeaderOnly_Patch
        {
            static bool Prefix(string filePath)
            {
                ScribeLoader scribeloader = Scribe.loader;
                //Log.Message(string.Format("RimKeeperSaves InitLoadingMetaHeaderOnly Path:{0}", filePath));
                
                if (Scribe.mode != LoadSaveMode.Inactive)
                {
                    Log.Error("Called InitLoadingMetaHeaderOnly() but current mode is " + Scribe.mode);
                    Scribe.ForceStop();
                }
                try
                {
                    using (ZipFileReader reader = new ZipFileReader(filePath))
                    {
                        if (ScribeMetaHeaderUtility.ReadToMetaElement(reader.XmlReader))
                        {
                            using (XmlReader xmlReader = reader.XmlReader.ReadSubtree())
                            {
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load(xmlReader);
                                XmlElement xmlElement = xmlDocument.CreateElement("root");
                                xmlElement.AppendChild(xmlDocument.DocumentElement);
                                scribeloader.curXmlParent = xmlElement;
                            }
                        }
                    }
                    //using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    //{
                    //    StreamReader reader = new StreamReader(stream);
                    //    char[] buffer = new char[5];
                    //    reader.Read(buffer, 0, 5);
                    //    string startOfFile = new string(buffer);
                    //    if (startOfFile.Equals("<?xml"))
                    //    {
                    //        Scribe.mode = LoadSaveMode.LoadingVars;
                    //        reader.Dispose();
                    //        return true;
                    //    }
                    //    stream.Position = 0;

                    //    using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
                    //    {
                    //        using (XmlTextReader xmlTextReader = new XmlTextReader(zip))
                    //        {
                    //            if (ScribeMetaHeaderUtility.ReadToMetaElement(xmlTextReader))
                    //            {
                    //                using (XmlReader xmlReader = xmlTextReader.ReadSubtree())
                    //                {
                    //                    XmlDocument xmlDocument = new XmlDocument();
                    //                    xmlDocument.Load(xmlReader);
                    //                    XmlElement xmlElement = xmlDocument.CreateElement("root");
                    //                    xmlElement.AppendChild(xmlDocument.DocumentElement);
                    //                    scribeloader.curXmlParent = xmlElement;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    Scribe.mode = LoadSaveMode.LoadingVars;
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(new object[]
                    {
                    "Exception while init loading meta header: ",
                    filePath,
                    "\n",
                    ex
                    }));
                    scribeloader.ForceStop();
                    throw;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ScribeMetaHeaderUtility), "GameVersionOf")]
        public static class GameVersionOf_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(FileInfo file, ref string __result)
            {
                //Log.Message(string.Format("RimKeeperSaves GameVersionOf Path:{0}", file.Name));
                if (!file.Exists)
                {
                    throw new ArgumentException();
                }
                try
                {
                    using (ZipFileReader reader = new ZipFileReader(file.FullName))
                    {
                        if (ScribeMetaHeaderUtility.ReadToMetaElement(reader.XmlReader) && reader.XmlReader.ReadToDescendant("gameVersion"))
                        {
                            __result = VersionControl.VersionStringWithoutRev(reader.XmlReader.ReadString());
                            return false;
                        }
                    }
                    //using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                    //{
                    //    StreamReader reader = new StreamReader(stream);
                    //    char[] buffer = new char[5];
                    //    reader.Read(buffer, 0, 5);
                    //    string startOfFile = new string(buffer);
                    //    if (startOfFile.Equals("<?xml"))
                    //    {
                    //        reader.Dispose();
                    //        __result = null;
                    //        return true;
                    //    }
                    //    stream.Position = 0;

                    //    using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
                    //    {
                    //        using (XmlTextReader xmlTextReader = new XmlTextReader(zip))
                    //        {
                    //            if (ScribeMetaHeaderUtility.ReadToMetaElement(xmlTextReader) && xmlTextReader.ReadToDescendant("gameVersion"))
                    //            {
                    //                __result = VersionControl.VersionStringWithoutRev(xmlTextReader.ReadString());
                    //                Log.Message(string.Format("RimKeeperSaves GameVersionOf Path:{0} : {1}", file.Name, __result));
                    //            }
                    //        }
                    //    }

                    //    reader.Dispose();
                    //}
                }
                catch (Exception ex)
                {
                    Log.Error("RimKeeperSaves GameVersionOf Exception " + file.Name + ": " + ex.ToString());
                    //return true;
                }
                __result = null;
                return false;
            }
        }
    }
}