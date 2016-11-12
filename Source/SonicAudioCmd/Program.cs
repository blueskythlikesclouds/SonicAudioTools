using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Collections;

using SonicAudioLib;
using SonicAudioLib.Archive;
using SonicAudioLib.Collections;
using SonicAudioLib.IO;
using SonicAudioLib.CriMw;

using System.Xml;

namespace SonicAudioCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CriTableReader reader = CriTableReader.Create(args[0]))
            {
                reader.Read();

                Stream archiveStream = reader.GetSubstream("AwbFile");

                CriAfs2Archive archive = new CriAfs2Archive();
                archive.Read(archiveStream);

                foreach (CriAfs2Entry entry in archive)
                {
                    FileInfo fileInfo = new FileInfo(
                        Path.Combine(
                            Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]),
                            entry.CueIndex.ToString() + ".hca"));

                    fileInfo.Directory.Create();

                    using (Stream destination = fileInfo.Create(), entryStream = entry.Open(archiveStream))
                    {
                        entryStream.CopyTo(destination);
                    }
                }
            }
        }
    }
}
