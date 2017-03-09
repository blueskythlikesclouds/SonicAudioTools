using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SonicAudioLib.Archive;
using SonicAudioLib.IO;
using SonicAudioLib.CriMw;
using SonicAudioLib.Module;

namespace SonicAudioLib.Archive
{
    public enum CriAaxEntryFlag
    {
        Intro = 0,
        Loop = 1,
    }

    public class CriAaxEntry : EntryBase
    {
        public CriAaxEntryFlag Flag { get; set; }
    }

    public class CriAaxArchive : ArchiveBase<CriAaxEntry>
    {
        public override void Read(Stream source)
        {
            using (CriTableReader reader = CriTableReader.Create(source))
            {
                if (reader.TableName != "AAX")
                {
                    throw new Exception("Unknown AAX type. Please report the error with the file.");
                }

                while (reader.Read())
                {
                    CriAaxEntry entry = new CriAaxEntry();
                    entry.Flag = (CriAaxEntryFlag)reader.GetByte("lpflg");
                    entry.Position = reader.GetPosition("data");
                    entry.Length = reader.GetLength("data");
                    entries.Add(entry);
                }
            }
        }

        public override void Write(Stream destination)
        {
            using (CriTableWriter writer = CriTableWriter.Create(destination, CriTableWriterSettings.AdxSettings))
            {
                writer.WriteStartTable("AAX");
                writer.WriteField("data", typeof(byte[]));
                writer.WriteField("lpflg", typeof(byte));

                foreach (CriAaxEntry entry in entries.OrderBy(entry => entry.Flag))
                {
                    writer.WriteRow(true, entry.FilePath, (byte)entry.Flag);
                }

                writer.WriteEndTable();
            }
        }

        public override void Add(CriAaxEntry item)
        {
            if (entries.Count == 2 || entries.Exists(entry => (entry.Flag == item.Flag)))
            {
                return;
            }

            base.Add(item);
        }
    }
}
