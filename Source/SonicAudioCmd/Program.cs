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
using SonicAudioLib.IO;
using SonicAudioLib.CriMw;

using System.Xml;

namespace SonicAudioCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CriCpkArchive archive = new CriCpkArchive();
                archive.Load(args[0]);

                using (Stream source = File.OpenRead(args[0]))
                {
                    foreach (CriCpkEntry entry in archive)
                    {
                        using (Stream destination = File.Create(entry.Name))
                        {
                            EndianStream.CopyPartTo(source, destination, entry.Position, entry.Length, ushort.MaxValue);
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.ReadLine();
            }
        }
    }
}
