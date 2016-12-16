using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SonicAudioLib.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.Archive
{
    public class CriCpkEntry : EntryBase
    {
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public uint Index { get; set; }
        public string Comment { get; set; }
        public bool IsCompressed { get; set; }
    }

    public class CriCpkArchive : ArchiveBase<CriCpkEntry>
    {
        public override void Read(Stream source)
        {
            using (CriTableReader reader = CriCpkSection.Open(source, source.Position))
            {
                reader.Read();

                if (reader.GetUInt32("CpkMode") != 1)
                {
                    throw new Exception("Unsupported CPK type! Only TOC CPKs are supported for now.");
                }

                long tocPosition = (long)reader.GetUInt64("TocOffset");
                long contentPosition = (long)reader.GetUInt64("ContentOffset");
                ushort align = reader.GetUInt16("Align");

                using (CriTableReader tocReader = CriCpkSection.Open(source, tocPosition))
                {
                    while (tocReader.Read())
                    {
                        CriCpkEntry entry = new CriCpkEntry();
                        entry.DirectoryName = tocReader.GetString("DirName");
                        entry.Name = tocReader.GetString("FileName");
                        entry.Length = tocReader.GetUInt32("FileSize");
                        entry.Position = (long)tocReader.GetUInt64("FileOffset");
                        entry.Index = tocReader.GetUInt32("ID");
                        entry.Comment = tocReader.GetString("UserString");

                        if (entry.Length != tocReader.GetUInt32("ExtractSize"))
                        {
                            entry.IsCompressed = true;
                        }

                        if (contentPosition < tocPosition)
                        {
                            entry.Position += contentPosition;
                        }

                        else
                        {
                            entry.Position += tocPosition;
                        }

                        while ((entry.Position % align) != 0)
                        {
                            entry.Position++;
                        }

                        entries.Add(entry);

                        Console.WriteLine(Path.Combine(entry.DirectoryName, entry.Name));
                    }
                }
            }
        }

        public override void Write(Stream destination)
        {
            throw new NotImplementedException();
        }

        private class CriCpkSection : IDisposable
        {
            private Stream destination;
            private long headerPosition;

            private CriTableWriter writer;

            public CriTableWriter Writer
            {
                get
                {
                    return writer;
                }
            }

            public void Dispose()
            {
                writer.Dispose();

                long position = destination.Position;
                uint length = (uint)(position - (headerPosition - 8));

                destination.Seek(headerPosition + 8, SeekOrigin.Begin);
                EndianStream.WriteUInt32(destination, length);

                destination.Seek(position, SeekOrigin.Begin);
            }

            public static CriTableReader Open(Stream source, long position)
            {
                source.Seek(position, SeekOrigin.Begin);

                string signature = EndianStream.ReadCString(source, 4);
                uint flag = EndianStream.ReadUInt32(source);
                uint tableLength = EndianStream.ReadUInt32(source);
                uint unknown = EndianStream.ReadUInt32(source);

                return CriTableReader.Create(new Substream(source, source.Position, tableLength));
            }

            public CriCpkSection(Stream destination, string signature)
            {
                this.destination = destination;
                headerPosition = destination.Position;

                EndianStream.WriteCString(destination, signature, 4);
                EndianStream.WriteUInt32(destination, byte.MaxValue);
                destination.Seek(8, SeekOrigin.Begin);

                writer = CriTableWriter.Create(destination);
            }
        }
    }
}
