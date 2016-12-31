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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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

            if (args[0].EndsWith(".csb"))
            {
                List<CriTableCueSheet> tables = CriTableSerializer.Deserialize<CriTableCueSheet>(args[0]);
                Serialize(args[0] + ".xml", tables);

                foreach (CriTableCueSheet table in tables)
                {
                    DirectoryInfo baseDirectory = new DirectoryInfo(
                        Path.Combine(
                            Path.GetDirectoryName(args[0]), 
                            Path.GetFileNameWithoutExtension(args[0]), 
                            Enum.GetName(typeof(CriTableCueSheet.EnumTableType), table.TableType)));

                    baseDirectory.Create();

                    foreach (CriTableCue cue in table.DataList.OfType<CriTableCue>())
                    {
                        Serialize(Path.Combine(baseDirectory.FullName, cue.Name + ".xml"), cue);
                    }

                    foreach (CriTableSynth synth in table.DataList.OfType<CriTableSynth>())
                    {
                        Directory.CreateDirectory(Path.Combine(baseDirectory.FullName, Path.GetDirectoryName(synth.SynthName)));
                        Serialize(Path.Combine(baseDirectory.FullName, synth.SynthName + ".xml"), synth);
                    }

                    foreach (CriTableSoundElement soundElement in table.DataList.OfType<CriTableSoundElement>())
                    {
                        Directory.CreateDirectory(Path.Combine(baseDirectory.FullName, Path.GetDirectoryName(soundElement.Name)));
                    }
                }
            }

            else if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
            {
            }
        }

        static void Serialize(string path, object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            using (Stream destination = File.Create(path))
            {
                serializer.Serialize(destination, obj);
            }
        }
    }
}
