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
            using (Stream source = File.OpenRead(args[0]))
            using (Stream destination = File.Create(args[0] + "-unmask"))
            {
                Methods.MaskCriTable(source, destination, source.Length);
            }
        }
    }
}
