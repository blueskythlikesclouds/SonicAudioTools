using System;
using System.IO;
using System.Windows.Forms;
using System.Text;

using SonicAudioLib.CriMw;
using SonicAudioLib.IO;
using SonicAudioLib.Archive;

namespace AcbEditor
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
                if (args[0].EndsWith(".acb"))
                {
                    string baseDirectory = Path.GetDirectoryName(args[0]);
                    string outputDirectoryPath = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(args[0]));
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

                        if (acbReader.GetLength("AwbFile") > 0)
                        {
                            using (Substream afs2Stream = acbReader.GetSubstream("AwbFile"))
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

                            using (Substream extAfs2Stream = acbReader.GetSubstream("StreamAwbAfs2Header"))
                            {
                                extAfs2Archive.Read(extAfs2Stream);
                            }

                            if (!found)
                            {
                                throw new Exception("Cannot find the external .AWB file for this .ACB file. Please ensure that the external .AWB file is stored in the directory where the .ACB file is.");
                            }
                        }

                        using (Substream waveformTableStream = acbReader.GetSubstream("WaveformTable"))
                        using (CriTableReader waveformReader = CriTableReader.Create(waveformTableStream))
                        {
                            while (waveformReader.Read())
                            {
                                ushort id = waveformReader.GetUInt16("Id");
                                byte encodeType = waveformReader.GetByte("EncodeType");
                                bool streaming = waveformReader.GetBoolean("Streaming");

                                string outputName = id.ToString("D5");
                                if (streaming)
                                {
                                    outputName += "_streaming";
                                }

                                outputName += GetExtension(encodeType);
                                outputName = Path.Combine(outputDirectoryPath, outputName);

                                Console.WriteLine("Extracting {0} file with id {1}...", GetExtension(encodeType).ToUpper(), id);

                                if (streaming)
                                {
                                    if (!found)
                                    {
                                        throw new Exception("Cannot find the external .AWB file for this .ACB file. Please ensure that the external .AWB file is stored in the directory where the .ACB file is.");
                                    }

                                    else if (extCpkArchive == null && cpkMode)
                                    {
                                        extCpkArchive = new CriCpkArchive();
                                        extCpkArchive.Load(extAfs2ArchivePath);
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

                                    using (Stream extAfs2Stream = File.OpenRead(extAfs2ArchivePath))
                                    using (Stream afs2EntryStream = afs2Entry.Open(extAfs2Stream))
                                    using (Stream afs2EntryDestination = File.Create(outputName))
                                    {
                                        afs2EntryStream.CopyTo(afs2EntryDestination);
                                    }
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

                                    using (Substream afs2Stream = acbReader.GetSubstream("AwbFile"))
                                    using (Stream afs2EntryStream = afs2Entry.Open(afs2Stream))
                                    using (Stream afs2EntryDestination = File.Create(outputName))
                                    {
                                        afs2EntryStream.CopyTo(afs2EntryDestination);
                                    }
                                }
                            }
                        }
                    }
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
                        throw new Exception("Cannot find the .ACB file for this directory. Please ensure that the .ACB file is stored in the directory where this directory is.");
                    }

                    CriTable acbFile = new CriTable();
                    acbFile.Load(acbPath);

                    CriAfs2Archive afs2Archive = new CriAfs2Archive();
                    CriAfs2Archive extAfs2Archive = new CriAfs2Archive();

                    CriCpkArchive cpkArchive = new CriCpkArchive();
                    CriCpkArchive extCpkArchive = new CriCpkArchive();
                    cpkArchive.Mode = extCpkArchive.Mode = CriCpkMode.Id;

                    bool cpkMode = true;

                    byte[] awbFile = (byte[])acbFile.Rows[0]["AwbFile"];
                    byte[] streamAwbAfs2Header = (byte[])acbFile.Rows[0]["StreamAwbAfs2Header"];

                    cpkMode = !(awbFile != null && awbFile.Length >= 4 && Encoding.ASCII.GetString(awbFile, 0, 4) == "AFS2") && (streamAwbAfs2Header == null || streamAwbAfs2Header.Length == 0);

                    using (CriTableReader reader = CriTableReader.Create((byte[])acbFile.Rows[0]["WaveformTable"]))
                    {
                        while (reader.Read())
                        {
                            ushort id = reader.GetUInt16("Id");
                            byte encodeType = reader.GetByte("EncodeType");
                            bool streaming = reader.GetBoolean("Streaming");

                            string inputName = id.ToString("D5");
                            if (streaming)
                            {
                                inputName += "_streaming";
                            }

                            inputName += GetExtension(encodeType);
                            inputName = Path.Combine(args[0], inputName);

                            if (!File.Exists(inputName))
                            {
                                throw new Exception($"Cannot find audio file with id {id} for replacement.\nPath attempt: {inputName}");
                            }

                            Console.WriteLine("Adding {0}...", Path.GetFileName(inputName));

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
                    }

                    if (extAfs2Archive.Count > 0 || extCpkArchive.Count > 0)
                    {
                        Console.WriteLine("Saving external AWB...");
                        if (cpkMode)
                        {
                            extCpkArchive.Save(awbPath);
                        }

                        else
                        {
                            extAfs2Archive.Save(awbPath);

                            byte[] afs2Header = new byte[16 +
                                (extAfs2Archive.Count * extAfs2Archive.IdFieldLength) +
                                (extAfs2Archive.Count * extAfs2Archive.PositionFieldLength) +
                                extAfs2Archive.PositionFieldLength];

                            using (FileStream fileStream = File.OpenRead(awbPath))
                            {
                                fileStream.Read(afs2Header, 0, afs2Header.Length);
                            }

                            acbFile.Rows[0]["StreamAwbAfs2Header"] = afs2Header;
                        }
                    }

                    acbFile.WriterSettings = CriTableWriterSettings.Adx2Settings;
                    acbFile.Save(acbPath);
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
            bool result = false;

            result = EndianStream.ReadCString(source, 4) == "AFS2";
            source.Seek(oldPosition, SeekOrigin.Begin);

            return result;
        }
    }
}
