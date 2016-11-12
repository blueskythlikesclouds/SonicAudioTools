using System;
using System.IO;
using System.Windows.Forms;

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

            try
            {
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

                        if (acbReader.GetLength("AwbFile") > 0)
                        {
                            using (Substream afs2Stream = acbReader.GetSubstream("AwbFile"))
                            {
                                afs2Archive.Read(afs2Stream);
                            }
                        }

                        if (acbReader.GetLength("StreamAwbAfs2Header") > 0)
                        {
                            using (Substream extAfs2Stream = acbReader.GetSubstream("StreamAwbAfs2Header"))
                            {
                                extAfs2Archive.Read(extAfs2Stream);
                            }

                            // cheatingggggg
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
                                ushort index = waveformReader.GetUInt16("Id");
                                byte encodeType = waveformReader.GetByte("EncodeType");
                                bool streaming = waveformReader.GetBoolean("Streaming");

                                string outputName = index.ToString("D5");
                                if (streaming)
                                {
                                    outputName += "_streaming";
                                }

                                outputName += GetExtension(encodeType);
                                outputName = Path.Combine(outputDirectoryPath, outputName);

                                Console.WriteLine("Extracting {0} file with index {1}...", GetExtension(encodeType).ToUpper(), index);

                                if (streaming)
                                {
                                    CriAfs2Entry afs2Entry = extAfs2Archive.GetByCueIndex(index);

                                    using (Stream extAfs2Stream = File.OpenRead(extAfs2ArchivePath))
                                    using (Stream afs2EntryStream = afs2Entry.Open(extAfs2Stream))
                                    using (Stream afs2EntryDestination = File.Create(outputName))
                                    {
                                        afs2EntryStream.CopyTo(afs2EntryDestination);
                                    }
                                }

                                else
                                {
                                    CriAfs2Entry entry = afs2Archive.GetByCueIndex(index);

                                    using (Substream afs2Stream = acbReader.GetSubstream("AwbFile"))
                                    using (Stream afs2EntryStream = entry.Open(afs2Stream))
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

                    using (CriTableReader reader = CriTableReader.Create((byte[])acbFile.Rows[0]["WaveformTable"]))
                    {
                        while (reader.Read())
                        {
                            ushort index = reader.GetUInt16("Id");
                            byte encodeType = reader.GetByte("EncodeType");
                            bool streaming = reader.GetBoolean("Streaming");

                            string inputName = index.ToString("D5");
                            if (streaming)
                            {
                                inputName += "_streaming";
                            }

                            inputName += GetExtension(encodeType);
                            inputName = Path.Combine(args[0], inputName);

                            if (!File.Exists(inputName))
                            {
                                throw new Exception($"Cannot find audio file with index {index} for replacement.\nPath attempt: {inputName}");
                            }

                            CriAfs2Entry entry = new CriAfs2Entry();
                            entry.CueIndex = index;
                            entry.FilePath = new FileInfo(inputName);

                            Console.WriteLine("Adding {0}...", Path.GetFileName(inputName));

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

                    acbFile.Rows[0]["AwbFile"] = null;
                    acbFile.Rows[0]["StreamAwbAfs2Header"] = null;

                    if (afs2Archive.Count > 0)
                    {
                        afs2Archive.Order();

                        Console.WriteLine("Saving internal AWB...");
                        acbFile.Rows[0]["AwbFile"] = afs2Archive.Save();
                    }

                    if (extAfs2Archive.Count > 0)
                    {
                        extAfs2Archive.Order();

                        Console.WriteLine("Saving external AWB...");
                        extAfs2Archive.Save(awbPath);

                        byte[] afs2Header = new byte[16 +
                            (extAfs2Archive.Count * extAfs2Archive.CueIndexFieldLength) +
                            (extAfs2Archive.Count * extAfs2Archive.PositionFieldLength) +
                            extAfs2Archive.PositionFieldLength];

                        using (FileStream fileStream = File.OpenRead(awbPath))
                        {
                            fileStream.Read(afs2Header, 0, afs2Header.Length);
                        }

                        acbFile.Rows[0]["StreamAwbAfs2Header"] = afs2Header;
                    }

                    acbFile.WriterSettings = CriTableWriterSettings.Adx2Settings;
                    acbFile.Save(acbPath);
                }
            }

            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "ACB Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    return ".3dsadpcm";
                case 18:
                case 11:
                    return ".at9";
                case 12:
                    return ".xma";
                case 13:
                    return ".wiiuadpcm";
                default:
                    return ".bin";
            }
        }
    }
}
