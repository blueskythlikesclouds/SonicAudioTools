using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

using SonicAudioLib;
using SonicAudioLib.Archive;
using SonicAudioLib.IO;
using SonicAudioLib.CriMw;
using System.Threading.Tasks;

using System.Xml;

namespace SonicAudioCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var extractor = new DataExtractor();
                extractor.ProgressChanged += OnProgressChanged;

                CriCpkArchive archive = new CriCpkArchive();
                archive.Load(args[0]);

                foreach (CriCpkEntry entry in archive)
                {
                    extractor.Add(args[0], Path.Combine(Path.ChangeExtension(args[0], null), entry.DirectoryName, entry.Name), entry.Position, entry.Length, entry.IsCompressed);
                }


                DateTime dateTime = DateTime.Now;
                extractor.Run();

                Console.WriteLine("Elapsed time: {0}", DateTime.Now - dateTime);
                Console.ReadLine();

                /*archive.EnableMask = true;
                archive.Save("test.cpk");*/
            }

            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.ReadLine();
            }
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
