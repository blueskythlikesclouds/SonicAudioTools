using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

using SonicAudioLib.CriMw;
using SonicAudioLib.IO;
using SonicAudioLib.Archive;

using System.Globalization;

namespace CsbEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(Properties.Resources.Description);
                Console.ReadLine();
                return;
            }

#if !DEBUG
            try
            {
#endif
                if (args[0].EndsWith(".csb"))
                {
                    string baseDirectory = Path.GetDirectoryName(args[0]);
                    string outputDirectoryName = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(args[0]));

                    CriCpkArchive cpkArchive = null;
                    string cpkPath = outputDirectoryName + ".cpk";
                    bool found = File.Exists(cpkPath);

                    using (CriTableReader reader = CriTableReader.Create(args[0]))
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString("name") == "SOUND_ELEMENT")
                            {
                                using (CriTableReader sdlReader = CriTableReader.Create(reader.GetSubstream("utf")))
                                {
                                    while (sdlReader.Read())
                                    {
                                        if (sdlReader.GetByte("fmt") != 0)
                                        {
                                            throw new Exception("The given CSB file contains an audio file which is not an ADX. Only CSB files with ADXs are supported.");
                                        }

                                        bool streaming = sdlReader.GetBoolean("stmflg");
                                        if (streaming && !found)
                                        {
                                            throw new Exception("Cannot find the external .CPK file for this .CSB file. Please ensure that the external .CPK file is stored in the directory where the .CPK file is.");
                                        }

                                        else if (streaming && found && cpkArchive == null)
                                        {
                                            cpkArchive = new CriCpkArchive();
                                            cpkArchive.Load(cpkPath);
                                        }

                                        string sdlName = sdlReader.GetString("name");
                                        DirectoryInfo destinationPath = new DirectoryInfo(Path.Combine(outputDirectoryName, sdlName));
                                        destinationPath.Create();

                                        Console.WriteLine("Extracting {0}...", sdlName);

                                        CriAaxArchive aaxArchive = new CriAaxArchive();

                                        if (streaming)
                                        {
                                            CriCpkEntry cpkEntry = cpkArchive.GetByPath(sdlName);

                                            using (Stream cpkSource = File.OpenRead(cpkPath))
                                            using (Stream aaxSource = cpkEntry.Open(cpkSource))
                                            {
                                                aaxArchive.Read(aaxSource);

                                                foreach (CriAaxEntry entry in aaxArchive)
                                                {
                                                    using (Stream destination = File.Create(Path.Combine(destinationPath.FullName,
                                                        entry.Flag == CriAaxEntryFlag.Intro ? "Intro.adx" : "Loop.adx")))
                                                    using (Stream entrySource = entry.Open(aaxSource))
                                                    {
                                                        entrySource.CopyTo(destination);
                                                    }
                                                }
                                            }
                                        }

                                        else
                                        {
                                            using (Stream aaxSource = sdlReader.GetSubstream("data"))
                                            {
                                                aaxArchive.Read(aaxSource);

                                                foreach (CriAaxEntry entry in aaxArchive)
                                                {
                                                    using (Stream destination = File.Create(Path.Combine(destinationPath.FullName,
                                                        entry.Flag == CriAaxEntryFlag.Intro ? "Intro.adx" : "Loop.adx")))
                                                    using (Stream entrySource = entry.Open(aaxSource))
                                                    {
                                                        entrySource.CopyTo(destination);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                else if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
                {
                    string baseDirectory = Path.GetDirectoryName(args[0]);
                    string csbPath = args[0] + ".csb";

                    if (!File.Exists(csbPath))
                    {
                        throw new Exception("Cannot find the .CSB file for this directory. Please ensure that the .CSB file is stored in the directory where this directory is.");
                    }

                    CriCpkArchive cpkArchive = new CriCpkArchive();

                    CriTable csbFile = new CriTable();
                    csbFile.Load(csbPath);

                    CriRow soundElementRow = csbFile.Rows.First(row => (string)row["name"] == "SOUND_ELEMENT");

                    CriTable soundElementTable = new CriTable();
                    soundElementTable.Load((byte[])soundElementRow["utf"]);

                    List<FileInfo> junks = new List<FileInfo>();

                    foreach (CriRow sdlRow in soundElementTable.Rows)
                    {
                        string sdlName = (string)sdlRow["name"];

                        DirectoryInfo sdlDirectory = new DirectoryInfo(Path.Combine(args[0], sdlName));

                        if (!sdlDirectory.Exists)
                        {
                            throw new Exception($"Cannot find sound element directory for replacement.\nPath attempt: {sdlDirectory.FullName}");
                        }

                        bool streaming = (byte)sdlRow["stmflg"] != 0;
                        uint sampleRate = (uint)sdlRow["sfreq"];
                        byte numberChannels = (byte)sdlRow["nch"];

                        Console.WriteLine("Adding {0}...", sdlName);

                        CriAaxArchive aaxArchive = new CriAaxArchive();
                        foreach (FileInfo file in sdlDirectory.GetFiles("*.adx"))
                        {
                            CriAaxEntry entry = new CriAaxEntry();
                            if (file.Name.ToLower(CultureInfo.GetCultureInfo("en-US")) == "intro.adx")
                            {
                                entry.Flag = CriAaxEntryFlag.Intro;
                                entry.FilePath = file;
                                aaxArchive.Add(entry);

                                ReadAdx(file, out sampleRate, out numberChannels);
                            }

                            else if (file.Name.ToLower(CultureInfo.GetCultureInfo("en-US")) == "loop.adx")
                            {
                                entry.Flag = CriAaxEntryFlag.Loop;
                                entry.FilePath = file;
                                aaxArchive.Add(entry);

                                ReadAdx(file, out sampleRate, out numberChannels);
                            }
                        }

                        if (streaming)
                        {
                            CriCpkEntry entry = new CriCpkEntry();

                            int lastSlash = sdlName.LastIndexOf('/');
                            if (lastSlash != -1)
                            {
                                entry.Name = sdlName.Substring(lastSlash + 1);
                                entry.DirectoryName = sdlName.Substring(0, lastSlash);
                            }

                            else
                            {
                                entry.Name = sdlName;
                            }

                            entry.Id = (uint)cpkArchive.Count;
                            entry.FilePath = new FileInfo(Path.GetTempFileName());
                            junks.Add(entry.FilePath);

                            cpkArchive.Add(entry);
                            aaxArchive.Save(entry.FilePath.FullName);
                        }

                        else
                        {
                            sdlRow["data"] = aaxArchive.Save();
                        }

                        sdlRow["sfreq"] = sampleRate;
                        sdlRow["nch"] = numberChannels;
                    }

                    soundElementTable.WriterSettings = CriTableWriterSettings.AdxSettings;
                    soundElementRow["utf"] = soundElementTable.Save();

                    csbFile.WriterSettings = CriTableWriterSettings.AdxSettings;
                    csbFile.Save(args[0] + ".csb");

                    if (cpkArchive.Count > 0)
                    {
                        cpkArchive.Save(args[0] + ".cpk");
                    }

                    foreach (FileInfo junk in junks)
                    {
                        junk.Delete();
                    }
                }
#if !DEBUG
            }

            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "CSB Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }

        static void ReadAdx(FileInfo fileInfo, out uint sampleRate, out byte numberChannels)
        {
            using (Stream source = fileInfo.OpenRead())
            {
                source.Seek(7, SeekOrigin.Begin);
                numberChannels = EndianStream.ReadByte(source);
                sampleRate = EndianStream.ReadUInt32BE(source);
            }
        }
    }
}
