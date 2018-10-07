using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

using CsbEditor.Properties;

using SonicAudioLib;
using SonicAudioLib.CriMw;
using SonicAudioLib.IO;
using SonicAudioLib.Archives;

using System.Globalization;

namespace CsbEditor
{
    class Program
    {
        static void Main(string[] args)
        {
			Settings.Default.Save();
			
            if (args.Length < 1)
            {
                Console.WriteLine(Resources.Description);
                Console.ReadLine();
                return;
            }

#if !DEBUG
            try
            {
#endif
                if (args[0].EndsWith(".csb", StringComparer.OrdinalIgnoreCase))
                {
                    var extractor = new DataExtractor();
                    extractor.ProgressChanged += OnProgressChanged;

                    extractor.BufferSize = Settings.Default.BufferSize;
                    extractor.EnableThreading = Settings.Default.EnableThreading;
                    extractor.MaxThreads = Settings.Default.MaxThreads;

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
                                long tablePosition = reader.GetPosition("utf");
                                using (CriTableReader sdlReader = CriTableReader.Create(reader.GetSubStream("utf")))
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
                                            cpkArchive.Load(cpkPath, Settings.Default.BufferSize);
                                        }

                                        string sdlName = sdlReader.GetString("name");
                                        DirectoryInfo destinationPath = new DirectoryInfo(Path.Combine(outputDirectoryName, sdlName));
                                        destinationPath.Create();

                                        CriAaxArchive aaxArchive = new CriAaxArchive();

                                        if (streaming)
                                        {
                                            CriCpkEntry cpkEntry = cpkArchive.GetByPath(sdlName);

                                            if (cpkEntry != null)
                                            {
                                                using (Stream cpkSource = File.OpenRead(cpkPath))
                                                using (Stream aaxSource = cpkEntry.Open(cpkSource))
                                                {
                                                    aaxArchive.Read(aaxSource);

                                                    foreach (CriAaxEntry entry in aaxArchive)
                                                    {
                                                        extractor.Add(cpkPath,
                                                            Path.Combine(destinationPath.FullName,
                                                            entry.Flag == CriAaxEntryFlag.Intro ? "Intro.adx" : "Loop.adx"),
                                                            cpkEntry.Position + entry.Position, entry.Length);
                                                    }
                                                }
                                            }
                                        }

                                        else
                                        {
                                            long aaxPosition = sdlReader.GetPosition("data");
                                            using (Stream aaxSource = sdlReader.GetSubStream("data"))
                                            {
                                                aaxArchive.Read(aaxSource);

                                                foreach (CriAaxEntry entry in aaxArchive)
                                                {
                                                    extractor.Add(args[0],
                                                        Path.Combine(destinationPath.FullName,
                                                        entry.Flag == CriAaxEntryFlag.Intro ? "Intro.adx" : "Loop.adx"),
                                                        tablePosition + aaxPosition + entry.Position, entry.Length);
                                                }
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }

                    extractor.Run();
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
                    cpkArchive.ProgressChanged += OnProgressChanged;

                    CriTable csbFile = new CriTable();
                    csbFile.Load(csbPath, Settings.Default.BufferSize);

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
                            entry.Name = Path.GetFileName(sdlName);
                            entry.DirectoryName = Path.GetDirectoryName(sdlName);
                            entry.Id = (uint)cpkArchive.Count;
                            entry.FilePath = new FileInfo(Path.GetTempFileName());
                            junks.Add(entry.FilePath);

                            cpkArchive.Add(entry);
                            aaxArchive.Save(entry.FilePath.FullName, Settings.Default.BufferSize);
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
                    csbFile.Save(args[0] + ".csb", Settings.Default.BufferSize);

                    if (cpkArchive.Count > 0)
                    {
                        cpkArchive.Save(args[0] + ".cpk", Settings.Default.BufferSize);
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
                numberChannels = DataStream.ReadByte(source);
                sampleRate = DataStream.ReadUInt32BE(source);
            }
        }

        private static string buffer = new string(' ', 17);

        private static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            Console.Write(buffer);
            Console.SetCursorPosition(left, top);
            Console.WriteLine("Progress: {0}%", e.Progress);
            Console.SetCursorPosition(left, top);
        }
    }
}
