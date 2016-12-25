using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;

using SonicAudioLib.CriMw;
using SonicAudioLib.CriMw.Serialization;
using SonicAudioLib.IO;
using SonicAudioLib.Archive;

using System.Globalization;
using System.Xml.Serialization;

namespace CsbBuilder
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
                if (args[0].EndsWith(".csb"))
                {
                    List<CriTableCueSheet> cueSheets = CriTableSerializer.Deserialize<CriTableCueSheet>(args[0]);
                    //CriTableSerializer.Serialize(args[0] + ".new", cueSheets, CriTableWriterSettings.AdxSettings);

                    XmlSerializer serializer = new XmlSerializer(typeof(List<CriTableCueSheet>));
                    using (Stream dest = File.Create(Path.GetFileNameWithoutExtension(args[0]) + ".xml"))
                    {
                        serializer.Serialize(dest, cueSheets);
                    }
                }

                else if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
                {
                }

                else if (args[0].EndsWith(".xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<CriTableCueSheet>));
                    using (Stream dest = File.OpenRead(args[0]))
                    {
                        CriTableSerializer.Serialize(Path.GetFileNameWithoutExtension(args[0]) + ".csb", (List<CriTableCueSheet>)serializer.Deserialize(dest), CriTableWriterSettings.AdxSettings);
                    }
                }
            }

            catch (Exception exception)
            {
                //MessageBox.Show($"{exception.Message}", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
        }

        static void ReadAdx(FileInfo fileInfo, out uint sampleRate, out byte numberChannels)
        {
            using (Stream source = fileInfo.OpenRead())
            {
                source.Seek(7, SeekOrigin.Begin);
                numberChannels = EndianStream.ReadByte(source);
                sampleRate = EndianStream.ReadUInt32BE(source);
            }
        }
    }
}
