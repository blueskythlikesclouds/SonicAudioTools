using SonicAudioLib.CriMw;
using SonicAudioLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AcbFinder
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var awbsByHeaderHash = new Dictionary<string, string>();
            var acbsByAwbHeaderHash = new Dictionary<string, string>();
            var lines = new List<string>();

            string DirectoryForWork = "";
            string outputDirectory = "";

            if (args.Count() < 1)
            {
                DirectoryForWork = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                outputDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "output");
            }
            else
            {
                DirectoryForWork = args[0];
                outputDirectory = Path.Combine(args[0], "output");
            }

            StreamWriter LogFile = new StreamWriter(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ACB_FinderLog.txt")));
            Directory.CreateDirectory(outputDirectory);

            var md5 = MD5.Create();

            var buffer = new byte[4];
            var di = Directory.GetFiles(DirectoryForWork, "*", SearchOption.AllDirectories);

            Console.WriteLine("=== ACB Finder (" + DateTime.Now + ") ===");
            LogFile.WriteLine("=== ACB Finder (" + DateTime.Now + ") ===");

            Console.WriteLine("Found " + di.Length + " files in " + DirectoryForWork + "...");
            LogFile.WriteLine("Found " + di.Length + " files in " + DirectoryForWork + "...");
            int AWBsFound = 0;
            foreach (var filePath in di)
            {
                if (filePath.Contains("ACBFinder.exe") || filePath.Contains("SonicAudioLib.dll") || filePath.Contains("ACB_FinderLog.txt")) continue;
                using (var stream = File.OpenRead(filePath))
                {
                    stream.Read(buffer, 0, 4);

                    var signature = Encoding.ASCII.GetString(buffer);
                    if (signature == "AFS2")
                    {
                        Console.WriteLine("Found an AWB file: " + filePath.Replace(DirectoryForWork, "").Replace("\\", ""));
                        LogFile.WriteLine("Found an AWB file: " + filePath.Replace(DirectoryForWork, "").Replace("\\", ""));
                        AWBsFound++;

                        stream.Read(buffer, 0, 4);
                        int entryCount = DataStream.ReadInt32(stream);

                        stream.Seek(4 + (entryCount * buffer[2]), SeekOrigin.Current);
                        int length = (buffer[1] == 2 ? DataStream.ReadInt16(stream) : DataStream.ReadInt32(stream)) + 2;

                        stream.Seek(0, SeekOrigin.Begin);
                        var header = new byte[length];
                        stream.Read(header, 0, length);

                        string hash = Convert.ToBase64String(md5.ComputeHash(header));
                        awbsByHeaderHash[hash] = filePath;
                    }
                    else if (signature == "@UTF")
                    {
                        stream.Seek(0, SeekOrigin.Begin);

                        try
                        {
                            using (var reader = CriTableReader.Create(stream))
                            {
                                reader.Read();

                                string name = (((FileStream)reader.SourceStream).Name).Replace(DirectoryForWork, "").Replace("\\",""); //Properly get the filename using the stream and remove the directory from it
                                name = name.Remove(name.Length - 4, 4); //Remove the extension too, just in case?
                                File.Copy(filePath, Path.Combine(outputDirectory, name + ".acb"), true);

                                Console.WriteLine("Found and copied ACB: {0}", name);
                                LogFile.WriteLine("Found and copied ACB: {0}", name);

                                lines.Add($"{Path.GetFileName(filePath)}={name}.acb");

                                if (reader.GetLength("StreamAwbAfs2Header") != 0)
                                {
                                    //using ( var reader2 = reader.GetTableReader( "StreamAwbAfs2Header" ) )
                                    //{
                                    //    reader2.Read();

                                    var header = reader.GetData("StreamAwbAfs2Header");
                                    
                                    //Sometimes this has 0x44 bytes of extra data. When it happens, we can remove that to fix it.
                                    byte[] headerSignature = new byte[4];
                                    Array.Copy(header, 0, headerSignature, 0, 4);

                                    //Copy subset of array to new header if it's long enough and it's not fine already
                                    if (header.Length > 0x44 && Encoding.ASCII.GetString(headerSignature) != "AFS2")
                                    {
                                        byte[] newHeader = new byte[header.Length - 0x44];
                                        Array.Copy(header, 0x44, newHeader, 0, header.Length - 0x44);
                                        header = newHeader;
                                    }
                                    
                                    var hash = Convert.ToBase64String(md5.ComputeHash(header));

                                    acbsByAwbHeaderHash[hash] = name;
                                    //}
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("File could not be read correctly as an ACB: " + filePath.Replace(DirectoryForWork, "").Replace("\\", ""));
                            LogFile.WriteLine("File could not be read correctly as an ACB: " + filePath.Replace(DirectoryForWork, "").Replace("\\", ""));
                            continue;
                        }
                    }
                }
            }

            foreach (var pair in awbsByHeaderHash)
            {
                if (!acbsByAwbHeaderHash.TryGetValue(pair.Key, out string name))
                {
                    Console.WriteLine("AWB (" + pair.Key + ") with no ACB found.");
                    LogFile.WriteLine("AWB (" + pair.Key + ") with no ACB found.");
                    continue;
                }

                lines.Add($"{Path.GetFileName(pair.Value)}={name}.awb");

                File.Copy(pair.Value, Path.Combine(outputDirectory, name + ".awb"), true);
                Console.WriteLine("Copied awb {0}", name);
                LogFile.WriteLine("Copied awb {0}", name);
            }

            foreach (var pair in acbsByAwbHeaderHash.Where(x => !awbsByHeaderHash.ContainsKey(x.Key)))
            {
                File.AppendAllText(Path.Combine(outputDirectory, "missing_awb.txt"), string.Format("{0}\n", pair.Value));
            }
            File.WriteAllLines(Path.Combine(outputDirectory, "file_list.txt"), lines);

            Console.WriteLine("Process completed - Found " + AWBsFound + " AWB file(s).  Press any key to exit.");
            LogFile.WriteLine("== Process completed (" + DateTime.Now + ") - Found " + AWBsFound + " AWB file(s).  Press any key to exit. ===");
            LogFile.Close();
            Console.ReadKey();
        }
    }
}
