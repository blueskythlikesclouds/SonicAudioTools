using System;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

using AcbEditor.Properties;

using SonicAudioLib;
using SonicAudioLib.CriMw;
using SonicAudioLib.IO;
using SonicAudioLib.Archives;

namespace AcbEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                Settings.Default.Reset();
                Settings.Default.Save();
            }

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
            if (args[0].EndsWith(".acb", StringComparison.OrdinalIgnoreCase))
            {
                var extractor = new DataExtractor();
                extractor.ProgressChanged += OnProgressChanged;

                extractor.BufferSize = Settings.Default.BufferSize;
                extractor.EnableThreading = Settings.Default.EnableThreading;
                extractor.MaxThreads = Settings.Default.MaxThreads;

                string baseDirectory = Path.GetDirectoryName(args[0]);
                string outputDirectoryPath = Path.ChangeExtension(args[0], null);
                string extAfs2ArchivePath = string.Empty;

                Directory.CreateDirectory(outputDirectoryPath);

                using (CriTableReader acbReader = CriTableReader.Create(args[0]))
                {
                    acbReader.Read();

                    CriAfs2Archive afs2Archive = new CriAfs2Archive();
                    CriAfs2Archive extAfs2Archive = new CriAfs2Archive();

                    CriCpkArchive cpkArchive = new CriCpkArchive();
                    CriCpkArchive extCpkArchive = null;

                    extAfs2ArchivePath = outputDirectoryPath + ".awb";
                    bool found = File.Exists(extAfs2ArchivePath);

                    if (!found)
                    {
                        extAfs2ArchivePath = outputDirectoryPath + "_streamfiles.awb";
                        found = File.Exists(extAfs2ArchivePath);
                    }

                    if (!found)
                    {
                        extAfs2ArchivePath = outputDirectoryPath + "_STR.awb";
                        found = File.Exists(extAfs2ArchivePath);
                    }

                    bool cpkMode = true;

                    long awbPosition = acbReader.GetPosition("AwbFile");
                    if (acbReader.GetLength("AwbFile") > 0)
                    {
                        using (SubStream afs2Stream = acbReader.GetSubStream("AwbFile"))
                        {
                            cpkMode = !CheckIfAfs2(afs2Stream);

                            if (cpkMode)
                            {
                                cpkArchive.Read(afs2Stream);
                            }

                            else
                            {
                                afs2Archive.Read(afs2Stream);
                            }
                        }
                    }

                    if (acbReader.GetLength("StreamAwbAfs2Header") > 0)
                    {
                        cpkMode = false;

                        using (SubStream extAfs2Stream = acbReader.GetSubStream("StreamAwbAfs2Header"))
                        {
                            bool utfMode = DataStream.ReadCString(extAfs2Stream, 4) == "@UTF";
                            extAfs2Stream.Seek(0, SeekOrigin.Begin);

                            if (utfMode)
                            {
                                using (CriTableReader utfAfs2HeaderReader = CriTableReader.Create(extAfs2Stream))
                                {
                                    utfAfs2HeaderReader.Read();

                                    using (SubStream extAfs2HeaderStream = utfAfs2HeaderReader.GetSubStream("Header"))
                                    {
                                        extAfs2Archive.Read(extAfs2HeaderStream);
                                    }
                                }
                            }

                            else
                            {
                                extAfs2Archive.Read(extAfs2Stream);
                            }
                        }

                        if (!found)
                        {
                            throw new FileNotFoundException("Cannot find the external .AWB file for this .ACB file. Please ensure that the external .AWB file is stored in the directory where the .ACB file is.");
                        }
                    }

                    using (SubStream waveformTableStream = acbReader.GetSubStream("WaveformTable"))
                    using (CriTableReader waveformReader = CriTableReader.Create(waveformTableStream))
                    {
                        while (waveformReader.Read())
                        {
                            byte encodeType = waveformReader.GetByte("EncodeType");
                            bool streaming = waveformReader.GetBoolean("Streaming");

                            ushort id =
                                waveformReader.ContainsField("MemoryAwbId") ?
                                streaming ? waveformReader.GetUInt16("StreamAwbId") : waveformReader.GetUInt16("MemoryAwbId") :
                                waveformReader.GetUInt16("Id");

                            string outputName = id.ToString("D5");
                            if (streaming)
                            {
                                outputName += "_streaming";
                            }

                            outputName += GetExtension(encodeType);
                            outputName = Path.Combine(outputDirectoryPath, outputName);

                            if (streaming)
                            {
                                if (!found)
                                {
                                    throw new Exception("Cannot find the external .AWB file for this .ACB file. Please ensure that the external .AWB file is stored in the directory where the .ACB file is.");
                                }

                                else if (extCpkArchive == null && cpkMode)
                                {
                                    extCpkArchive = new CriCpkArchive();
                                    extCpkArchive.Load(extAfs2ArchivePath, Settings.Default.BufferSize);
                                }

                                EntryBase afs2Entry = null;

                                if (cpkMode)
                                {
                                    afs2Entry = extCpkArchive.GetById(id);
                                }

                                else
                                {
                                    afs2Entry = extAfs2Archive.GetById(id);
                                }

                                extractor.Add(extAfs2ArchivePath, outputName, afs2Entry.Position, afs2Entry.Length);
                            }

                            else
                            {
                                EntryBase afs2Entry = null;

                                if (cpkMode)
                                {
                                    afs2Entry = cpkArchive.GetById(id);
                                }

                                else
                                {
                                    afs2Entry = afs2Archive.GetById(id);
                                }

                                extractor.Add(args[0], outputName, awbPosition + afs2Entry.Position, afs2Entry.Length);
                            }
                        }
                    }
                }

                extractor.Run();
            }

            else if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
            {
                string baseDirectory = Path.GetDirectoryName(args[0]);
                string acbPath = args[0] + ".acb";

                string awbPath = args[0] + "_streamfiles.awb";
                bool found = File.Exists(awbPath);

                if (!found)
                {
                    awbPath = args[0] + "_STR.awb";
                    found = File.Exists(awbPath);
                }

                if (!found)
                {
                    awbPath = args[0] + ".awb";
                }

                if (!File.Exists(acbPath))
                {
                    throw new FileNotFoundException("Cannot find the .ACB file for this directory. Please ensure that the .ACB file is stored in the directory where this directory is.");
                }

                CriTable acbFile = new CriTable();
                acbFile.Load(acbPath, Settings.Default.BufferSize);

                CriAfs2Archive afs2Archive = new CriAfs2Archive();
                CriAfs2Archive extAfs2Archive = new CriAfs2Archive();

                CriCpkArchive cpkArchive = new CriCpkArchive();
                CriCpkArchive extCpkArchive = new CriCpkArchive();
                cpkArchive.Mode = extCpkArchive.Mode = CriCpkMode.Id;

                afs2Archive.ProgressChanged += OnProgressChanged;
                extAfs2Archive.ProgressChanged += OnProgressChanged;
                cpkArchive.ProgressChanged += OnProgressChanged;
                extCpkArchive.ProgressChanged += OnProgressChanged;

                bool cpkMode = true;

                byte[] awbFile = (byte[])acbFile.Rows[0]["AwbFile"];
                byte[] streamAwbAfs2Header = (byte[])acbFile.Rows[0]["StreamAwbAfs2Header"];

                cpkMode = !(awbFile != null && awbFile.Length >= 4 && Encoding.ASCII.GetString(awbFile, 0, 4) == "AFS2") && (streamAwbAfs2Header == null || streamAwbAfs2Header.Length == 0);

                using (CriTableReader reader = CriTableReader.Create((byte[])acbFile.Rows[0]["WaveformTable"]))
                {
                    while (reader.Read())
                    {
                        byte encodeType = reader.GetByte("EncodeType");
                        bool streaming = reader.GetBoolean("Streaming");

                        ushort id = 
                            reader.ContainsField("MemoryAwbId") ? 
                            streaming ? reader.GetUInt16("StreamAwbId") : reader.GetUInt16("MemoryAwbId") : 
                            reader.GetUInt16("Id");

                        string inputName = id.ToString("D5");
                        if (streaming)
                        {
                            inputName += "_streaming";
                        }

                        inputName += GetExtension(encodeType);
                        inputName = Path.Combine(args[0], inputName);

                        if (!File.Exists(inputName))
                        {
                            throw new FileNotFoundException($"Cannot find audio file with id {id} for replacement.\nPath attempt: {inputName}");
                        }

                        if (cpkMode)
                        {
                            CriCpkEntry entry = new CriCpkEntry();
                            entry.FilePath = new FileInfo(inputName);
                            entry.Id = id;

                            if (streaming)
                            {
                                extCpkArchive.Add(entry);
                            }

                            else
                            {
                                cpkArchive.Add(entry);
                            }
                        }

                        else
                        {
                            CriAfs2Entry entry = new CriAfs2Entry();
                            entry.FilePath = new FileInfo(inputName);
                            entry.Id = id;

                            if (streaming)
                            {
                                extAfs2Archive.Add(entry);
                            }

                            else
                            {
                                afs2Archive.Add(entry);
                            }
                        }
                    }
                }

                acbFile.Rows[0]["AwbFile"] = null;
                acbFile.Rows[0]["StreamAwbAfs2Header"] = null;

                if (afs2Archive.Count > 0 || cpkArchive.Count > 0)
                {
                    Console.WriteLine("Saving internal AWB...");
                    acbFile.Rows[0]["AwbFile"] = cpkMode ? cpkArchive.Save() : afs2Archive.Save();
                    Console.WriteLine();
                }

                if (extAfs2Archive.Count > 0 || extCpkArchive.Count > 0)
                {
                    Console.WriteLine("Saving external AWB...");
                    if (cpkMode)
                    {
                        extCpkArchive.Save(awbPath, Settings.Default.BufferSize);
                    }

                    else
                    {
                        extAfs2Archive.Save(awbPath, Settings.Default.BufferSize);

                        if (Encoding.UTF8.GetString(streamAwbAfs2Header, 0, 4) == "@UTF")
                        {
                            CriTable headerTable = new CriTable();
                            headerTable.Load(streamAwbAfs2Header);

                            headerTable.Rows[0]["Header"] = extAfs2Archive.Header;
                            headerTable.WriterSettings = CriTableWriterSettings.Adx2Settings;
                            acbFile.Rows[0]["StreamAwbAfs2Header"] = headerTable.Save();
                        }

                        else
                        {
                            acbFile.Rows[0]["StreamAwbAfs2Header"] = extAfs2Archive.Header;
                        }
                    }
                }

                acbFile.WriterSettings = CriTableWriterSettings.Adx2Settings;
                acbFile.Save(acbPath, Settings.Default.BufferSize);
            }
#if !DEBUG
            }

            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "ACB Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }

        static string GetExtension(byte encodeType)
        {
            switch (encodeType)
            {
                case 0:
                case 3:
                    return ".adx";
                case 1:
                    return ".ahx";
                case 2:
                    return ".hca";
                case 4:
                    return ".wiiadpcm";
                case 5:
                    return ".dsadpcm";
                case 6:
                    return ".hcamx";
                case 10:
                case 7:
                    return ".vag";
                case 8:
                    return ".at3";
                case 9:
                    return ".bcwav";
                case 18:
                case 11:
                    return ".at9";
                case 12:
                    return ".xma";
                case 13:
                    return ".dsp";
                default:
                    return ".bin";
            }
        }

        static bool CheckIfAfs2(Stream source)
        {
            long oldPosition = source.Position;
            bool result = DataStream.ReadCString(source, 4) == "AFS2";
            source.Seek(oldPosition, SeekOrigin.Begin);

            return result;
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
