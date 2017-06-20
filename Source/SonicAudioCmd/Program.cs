using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

using SonicAudioLib;
using SonicAudioLib.Archives;
using SonicAudioLib.IO;
using SonicAudioLib.CriMw;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;

namespace SonicAudioCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Stream source = File.OpenRead(args[0]))
            {
                CriTableReader reader = CriTableReader.Create(source);
                reader.MoveToRow(3);

                long pos = reader.GetPosition("utf");
                CriTableReader soundElementReader = reader.GetTableReader("utf");

                while (soundElementReader.Read())
                {
                    CriTable table = new CriTable();
                    table.Read(soundElementReader.GetSubStream("data"));

                    using (Stream output = File.Create(Path.GetFileName(soundElementReader.GetString("name") + "_" + table.Rows[0].GetValue<byte>("lpflg") + "_" + ".dsp")))
                    {
                        DataStream.WriteBytes(output, table.Rows[0].GetValue<byte[]>("header"));
                        DataStream.WriteBytes(output, table.Rows[0].GetValue<byte[]>("data"));
                    }
                }
            }
        }
    }
}
