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
            CriCpkArchive archive = new CriCpkArchive();
            archive.Load(args[0]);

            foreach (CriCpkEntry entry in archive)
            {
                using (Stream source = File.OpenRead(args[0]), substream = entry.Open(source))
                {
                    FileInfo fileInfo = new FileInfo(Path.Combine(Path.GetFileNameWithoutExtension(args[0]), entry.DirectoryName, entry.Name));
                    fileInfo.Directory.Create();
                    using (Stream destination = fileInfo.Create())
                    {
                        substream.CopyTo(destination);
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
