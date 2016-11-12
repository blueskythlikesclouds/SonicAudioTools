using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;

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

            try
            {
                if (args[0].EndsWith(".csb"))
                {
                    string baseDirectory = Path.GetDirectoryName(args[0]);
                    string outputDirectoryName = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(args[0]));

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
                                        if (sdlReader.GetByte("stmflg") != 0)
                                        {
                                            throw new Exception("The given CSB file contains external audio data. Those kind of CSB files are not supported yet.");
                                        }

                                        if (sdlReader.GetByte("fmt") != 0)
                                        {
                                            throw new Exception("The given CSB file contains an audio file which is not an ADX. Only CSB files with ADXs are supported.");
                                        }

                                        string sdlName = sdlReader.GetString("name");
                                        DirectoryInfo destinationPath = new DirectoryInfo(Path.Combine(outputDirectoryName, sdlName));
                                        destinationPath.Create();

                                        Console.WriteLine("Extracting {0}...", sdlName);
                                        using (CriTableReader aaxReader = CriTableReader.Create(sdlReader.GetSubstream("data")))
                                        {
                                            while (aaxReader.Read())
                                            {
                                                string outputName = Path.Combine(destinationPath.FullName, aaxReader.GetBoolean("lpflg") ? "Loop.adx" : "Intro.adx");

                                                using (Stream source = aaxReader.GetSubstream("data"))
                                                using (Stream destination = File.Create(outputName))
                                                {
                                                    source.CopyTo(destination);
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

                    CriTable csbFile = new CriTable();
                    csbFile.Load(csbPath);

                    CriRow soundElementRow = csbFile.Rows.Single(row => (string)row["name"] == "SOUND_ELEMENT");

                    CriTable soundElementTable = new CriTable();
                    soundElementTable.Load((byte[])soundElementRow["utf"]);

                    foreach (CriRow sdlRow in soundElementTable.Rows)
                    {
                        string sdlName = (string)sdlRow["name"];

                        DirectoryInfo sdlDirectory = new DirectoryInfo(Path.Combine(args[0], sdlName));

                        if (!sdlDirectory.Exists)
                        {
                            throw new Exception($"Cannot find sound element directory for replacement.\nPath attempt: {sdlDirectory.FullName}");
                        }

                        uint sampleRate = (uint)sdlRow["sfreq"];
                        byte numberChannels = (byte)sdlRow["nch"];

                        Console.WriteLine("Adding {0}...", sdlName);

                        using (MemoryStream memoryStream = new MemoryStream())
                        using (CriTableWriter writer = CriTableWriter.Create(memoryStream, CriTableWriterSettings.AdxSettings))
                        {
                            writer.WriteStartTable("AAX");

                            writer.WriteField("data", typeof(byte[]));
                            writer.WriteField("lpflg", typeof(byte));

                            foreach (FileInfo audioFile in sdlDirectory.GetFiles("*.adx"))
                            {
                                // In Turkish, lowercase I is ı so you get the idea
                                if (audioFile.Name.ToLower(CultureInfo.GetCultureInfo("en-US")) == "intro.adx")
                                {
                                    ReadAdx(audioFile, out sampleRate, out numberChannels);
                                    writer.WriteRow(true, audioFile, 0);
                                }

                                else if (audioFile.Name.ToLower() == "loop.adx")
                                {
                                    ReadAdx(audioFile, out sampleRate, out numberChannels);
                                    writer.WriteRow(true, audioFile, 1);
                                }
                            }

                            writer.WriteEndTable();
                            sdlRow["data"] = memoryStream.ToArray();
                        }

                        sdlRow["sfreq"] = sampleRate;
                        sdlRow["nch"] = numberChannels;
                    }

                    soundElementTable.WriterSettings = CriTableWriterSettings.AdxSettings;
                    soundElementRow["utf"] = soundElementTable.Save();

                    csbFile.WriterSettings = CriTableWriterSettings.AdxSettings;
                    csbFile.Save(args[0] + ".csb");
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "CSB Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
