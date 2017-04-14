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
            CriTable table = new CriTable();
            table.Load("test.utf");

            Console.WriteLine(table.Rows[0]["testField"]);
            Console.ReadLine();
        }
    }
}
