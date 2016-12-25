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
        private DateTime updateDateTime;

        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public uint Id { get; set; }
        public string Comment { get; set; }
        public bool IsCompressed { get; set; }

        public DateTime UpdateDateTime
        {
            get
            {
                if (FilePath != null)
                {
                    return FilePath.LastWriteTime;
                }

                return updateDateTime;
            }

            set
            {
                updateDateTime = value;
            }
        }
    }

    public enum CriCpkMode
    {
        None = -1,
        FileNameIdAndGroup = 5,
        IdAndGroup = 4,
        FileNameAndGroup = 3,
        FileNameAndId = 2,
        FileName = 1,
        Id = 0,
    }

    public class CriCpkArchive : ArchiveBase<CriCpkEntry>
    {
        private ushort align = 1;
        private CriCpkMode mode = CriCpkMode.FileName;

        public ushort Align
        {
            get
            {
                return align;
            }

            set
            {
                if (value <= 0)
                {
                    value = 1;
                }

                align = value;
            }
        }

        public CriCpkMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
            }
        }

        public string Comment { get; set; }

        public override void Read(Stream source)
        {
            using (CriTableReader reader = CriCpkSection.Open(source, source.Position))
            {
                reader.Read();

#if DEBUG
                for (int i = 0; i < reader.NumberOfFields; i++)
                {
                    Console.WriteLine("{0} ({1}): {2}", reader.GetFieldName(i), reader.GetFieldFlag(i), reader.GetValue(i));
                }
#endif

                mode = (CriCpkMode)reader.GetUInt32("CpkMode");

                // No need to waste time, stop right there.
                if (mode == CriCpkMode.None)
                {
                    return;
                }

                long tocPosition = (long)reader.GetUInt64("TocOffset");
                long itocPosition = (long)reader.GetUInt64("ItocOffset");
                long etocPosition = (long)reader.GetUInt64("EtocOffset");
                long contentPosition = (long)reader.GetUInt64("ContentOffset");

                align = reader.GetUInt16("Align");

                if (mode == CriCpkMode.FileName || mode == CriCpkMode.FileNameAndId)
                {
                    using (CriTableReader tocReader = CriCpkSection.Open(source, tocPosition))
                    using (CriTableReader etocReader = CriCpkSection.Open(source, etocPosition))
                    {
                        while (tocReader.Read())
                        {
                            CriCpkEntry entry = new CriCpkEntry();
                            entry.DirectoryName = tocReader.GetString("DirName");
                            entry.Name = tocReader.GetString("FileName");
                            entry.Length = tocReader.GetUInt32("FileSize");
                            entry.Position = (long)tocReader.GetUInt64("FileOffset");
                            entry.Id = tocReader.GetUInt32("ID");
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

                            etocReader.MoveToRow(tocReader.CurrentRow);
                            entry.UpdateDateTime = DateTimeFromCpkDateTime(etocReader.GetUInt64("UpdateDateTime"));

                            entries.Add(entry);
                        }
                    }

                    if (mode == CriCpkMode.FileNameAndId)
                    {
                        using (CriTableReader itocReader = CriCpkSection.Open(source, itocPosition))
                        {
                            while (itocReader.Read())
                            {
                                entries[itocReader.GetInt32("TocIndex")].Id = (uint)itocReader.GetInt32("ID");
                            }
                        }
                    }
                }

                else if (mode == CriCpkMode.Id)
                {
                    long currentPosition = contentPosition;

                    using (CriTableReader itocReader = CriCpkSection.Open(source, itocPosition))
                    {
                        while (itocReader.Read())
                        {
                            if (itocReader.GetUInt32("FilesL") > 0)
                            {
                                using (CriTableReader dataReader = itocReader.GetCriTableReader("DataL"))
                                {
                                    while (dataReader.Read())
                                    {
                                        CriCpkEntry entry = new CriCpkEntry();
                                        entry.Id = dataReader.GetUInt16("ID");
                                        entry.Length = dataReader.GetUInt16("FileSize");

                                        if (entry.Length != dataReader.GetUInt16("ExtractSize"))
                                        {
                                            entry.IsCompressed = true;
                                        }

                                        while ((currentPosition % align) != 0)
                                        {
                                            currentPosition++;
                                        }

                                        entry.Position = currentPosition;
                                        entries.Add(entry);

                                        currentPosition += entry.Length;
                                    }
                                }
                            }

                            if (itocReader.GetUInt32("FilesH") > 0)
                            {
                                using (CriTableReader dataReader = itocReader.GetCriTableReader("DataH"))
                                {
                                    while (dataReader.Read())
                                    {
                                        CriCpkEntry entry = new CriCpkEntry();
                                        entry.Id = dataReader.GetUInt16("ID");
                                        entry.Length = dataReader.GetUInt32("FileSize");

                                        if (entry.Length != dataReader.GetUInt32("ExtractSize"))
                                        {
                                            entry.IsCompressed = true;
                                        }

                                        while ((currentPosition % align) != 0)
                                        {
                                            currentPosition++;
                                        }

                                        entry.Position = currentPosition;
                                        entries.Add(entry);

                                        currentPosition += entry.Length;
                                    }
                                }
                            }
                        }
                    }
                }

                else
                {
                    throw new Exception($"This CPK mode ({mode}) needs to be implemented. Please report this error with the file.");
                }

                Comment = reader.GetString("Comment");
            }
        }

        public override void Write(Stream destination)
        {
            // FIXME: Alignment support (ugh same thing for AFS2 archives)
            VldPool vldPool = new VldPool();

            using (CriCpkSection cpkSection = new CriCpkSection(destination, "CPK "))
            {
                cpkSection.Writer.WriteStartTable("CpkHeader");

                cpkSection.Writer.WriteField("UpdateDateTime", typeof(ulong));
                cpkSection.Writer.WriteField("FileSize", typeof(ulong));
                cpkSection.Writer.WriteField("ContentOffset", typeof(ulong));
                cpkSection.Writer.WriteField("ContentSize", typeof(ulong));

                if (mode == CriCpkMode.FileName || mode == CriCpkMode.FileNameAndId)
                {
                    cpkSection.Writer.WriteField("TocOffset", typeof(ulong));
                    cpkSection.Writer.WriteField("TocSize", typeof(ulong));
                    cpkSection.Writer.WriteField("TocCrc", typeof(uint), null);
                    cpkSection.Writer.WriteField("EtocOffset", typeof(ulong));
                    cpkSection.Writer.WriteField("EtocSize", typeof(ulong));
                }

                else
                {
                    cpkSection.Writer.WriteField("TocOffset", typeof(ulong), null);
                    cpkSection.Writer.WriteField("TocSize", typeof(ulong), null);
                    cpkSection.Writer.WriteField("TocCrc", typeof(uint), null);
                    cpkSection.Writer.WriteField("EtocOffset", typeof(ulong), null);
                    cpkSection.Writer.WriteField("EtocSize", typeof(ulong), null);
                }

                if (mode == CriCpkMode.Id || mode == CriCpkMode.FileNameAndId)
                {
                    cpkSection.Writer.WriteField("ItocOffset", typeof(ulong));
                    cpkSection.Writer.WriteField("ItocSize", typeof(ulong));
                    cpkSection.Writer.WriteField("ItocCrc", typeof(uint), null);
                }

                else
                {
                    cpkSection.Writer.WriteField("ItocOffset", typeof(ulong), null);
                    cpkSection.Writer.WriteField("ItocSize", typeof(ulong), null);
                    cpkSection.Writer.WriteField("ItocCrc", typeof(uint), null);
                }

                cpkSection.Writer.WriteField("GtocOffset", typeof(ulong), null);
                cpkSection.Writer.WriteField("GtocSize", typeof(ulong), null);
                cpkSection.Writer.WriteField("GtocCrc", typeof(uint), null);

                cpkSection.Writer.WriteField("EnabledPackedSize", typeof(ulong));
                cpkSection.Writer.WriteField("EnabledDataSize", typeof(ulong));
                cpkSection.Writer.WriteField("TotalDataSize", typeof(ulong), null);

                cpkSection.Writer.WriteField("Tocs", typeof(uint), null);
                cpkSection.Writer.WriteField("Files", typeof(uint));
                cpkSection.Writer.WriteField("Groups", typeof(uint));
                cpkSection.Writer.WriteField("Attrs", typeof(uint));
                cpkSection.Writer.WriteField("TotalFiles", typeof(uint), null);
                cpkSection.Writer.WriteField("Directories", typeof(uint), null);
                cpkSection.Writer.WriteField("Updates", typeof(uint), null);

                cpkSection.Writer.WriteField("Version", typeof(ushort));
                cpkSection.Writer.WriteField("Revision", typeof(ushort));
                cpkSection.Writer.WriteField("Align", typeof(ushort));
                cpkSection.Writer.WriteField("Sorted", typeof(ushort));
                cpkSection.Writer.WriteField("EID", typeof(ushort), null);

                cpkSection.Writer.WriteField("CpkMode", typeof(uint));
                cpkSection.Writer.WriteField("Tvers", typeof(string));

                if (!string.IsNullOrEmpty(Comment))
                {
                    cpkSection.Writer.WriteField("Comment", typeof(string));
                }

                else
                {
                    cpkSection.Writer.WriteField("Comment", typeof(string), null);
                }

                cpkSection.Writer.WriteField("Codec", typeof(uint));
                cpkSection.Writer.WriteField("DpkItoc", typeof(uint));

                MemoryStream tocMemoryStream = null;
                MemoryStream itocMemoryStream = null;
                MemoryStream etocMemoryStream = null;

                if (mode == CriCpkMode.FileName || mode == CriCpkMode.FileNameAndId)
                {
                    tocMemoryStream = new MemoryStream();
                    etocMemoryStream = new MemoryStream();

                    using (CriCpkSection tocSection = new CriCpkSection(tocMemoryStream, "TOC "))
                    using (CriCpkSection etocSection = new CriCpkSection(etocMemoryStream, "ETOC"))
                    {
                        tocSection.Writer.WriteStartTable("CpkTocInfo");

                        tocSection.Writer.WriteField("DirName", typeof(string));
                        tocSection.Writer.WriteField("FileName", typeof(string));
                        tocSection.Writer.WriteField("FileSize", typeof(uint));
                        tocSection.Writer.WriteField("ExtractSize", typeof(uint));
                        tocSection.Writer.WriteField("FileOffset", typeof(ulong));
                        tocSection.Writer.WriteField("ID", typeof(uint));
                        tocSection.Writer.WriteField("UserString", typeof(string));

                        etocSection.Writer.WriteStartTable("CpkEtocInfo");

                        etocSection.Writer.WriteField("UpdateDateTime", typeof(ulong));
                        etocSection.Writer.WriteField("LocalDir", typeof(string));

                        foreach (CriCpkEntry entry in entries)
                        {
                            tocSection.Writer.WriteRow(true, entry.DirectoryName, entry.Name, Convert.ToUInt32(entry.Length), Convert.ToUInt32(entry.Length), Convert.ToUInt64(vldPool.Put(entry.FilePath)), entry.Id, entry.Comment);
                            etocSection.Writer.WriteRow(true, CpkDateTimeFromDateTime(entry.UpdateDateTime), entry.DirectoryName);
                        }

                        tocSection.Writer.WriteEndTable();
                        etocSection.Writer.WriteEndTable();
                    }

                    if (mode == CriCpkMode.FileNameAndId)
                    {
                        itocMemoryStream = new MemoryStream();

                        using (CriCpkSection itocSection = new CriCpkSection(itocMemoryStream, "ITOC"))
                        {
                            itocSection.Writer.WriteStartTable("CpkExtendId");

                            itocSection.Writer.WriteField("ID", typeof(int));
                            itocSection.Writer.WriteField("TocIndex", typeof(int));

                            foreach (CriCpkEntry entry in entries)
                            {
                                itocSection.Writer.WriteRow(true, Convert.ToInt32(entry.Id), entries.IndexOf(entry));
                            }

                            itocSection.Writer.WriteEndTable();
                        }
                    }
                }

                else if (mode == CriCpkMode.Id)
                {
                    itocMemoryStream = new MemoryStream();

                    using (CriCpkSection itocSection = new CriCpkSection(itocMemoryStream, "ITOC"))
                    {
                        itocSection.Writer.WriteStartTable("CpkItocInfo");

                        itocSection.Writer.WriteField("FilesL", typeof(uint));
                        itocSection.Writer.WriteField("FilesH", typeof(uint));
                        itocSection.Writer.WriteField("DataL", typeof(byte[]));
                        itocSection.Writer.WriteField("DataH", typeof(byte[]));

                        List<CriCpkEntry> filesL = entries.Where(entry => entry.Length < ushort.MaxValue).ToList();
                        List<CriCpkEntry> filesH = entries.Where(entry => entry.Length > ushort.MaxValue).ToList();

                        ushort id = 0;

                        itocSection.Writer.WriteStartRow();

                        using (MemoryStream dataMemoryStream = new MemoryStream())
                        using (CriTableWriter dataWriter = CriTableWriter.Create(dataMemoryStream))
                        {
                            dataWriter.WriteStartTable("CpkItocL");

                            dataWriter.WriteField("ID", typeof(ushort));
                            dataWriter.WriteField("FileSize", typeof(ushort));
                            dataWriter.WriteField("ExtractSize", typeof(ushort));

                            foreach (CriCpkEntry entry in filesL)
                            {
                                dataWriter.WriteRow(true, id, Convert.ToUInt16(entry.Length), Convert.ToUInt16(entry.Length));
                                vldPool.Put(entry.FilePath);
                                id++;
                            }

                            dataWriter.WriteEndTable();

                            itocSection.Writer.WriteValue("DataL", dataMemoryStream.ToArray());
                        }

                        using (MemoryStream dataMemoryStream = new MemoryStream())
                        using (CriTableWriter dataWriter = CriTableWriter.Create(dataMemoryStream))
                        {
                            dataWriter.WriteStartTable("CpkItocH");

                            dataWriter.WriteField("ID", typeof(ushort));
                            dataWriter.WriteField("FileSize", typeof(uint));
                            dataWriter.WriteField("ExtractSize", typeof(uint));

                            foreach (CriCpkEntry entry in filesH)
                            {
                                dataWriter.WriteRow(true, id, Convert.ToUInt32(entry.Length), Convert.ToUInt32(entry.Length));
                                vldPool.Put(entry.FilePath);
                                id++;
                            }

                            dataWriter.WriteEndTable();

                            itocSection.Writer.WriteValue("DataH", dataMemoryStream.ToArray());
                        }

                        itocSection.Writer.WriteValue("FilesL", (uint)filesL.Count);
                        itocSection.Writer.WriteValue("FilesH", (uint)filesH.Count);
                        itocSection.Writer.WriteEndRow();
                        itocSection.Writer.WriteEndTable();
                    }
                }

                else
                {
                    throw new Exception($"This CPK mode ({mode}) isn't implemented yet! Please choose another mode.");
                }

                cpkSection.Writer.WriteStartRow();
                cpkSection.Writer.WriteValue("UpdateDateTime", CpkDateTimeFromDateTime(DateTime.Now));

                cpkSection.Writer.WriteValue("ContentOffset", Convert.ToUInt64(2048));
                cpkSection.Writer.WriteValue("ContentSize", Convert.ToUInt64(vldPool.Length));

                if (tocMemoryStream != null)
                {
                    cpkSection.Writer.WriteValue("TocOffset", Convert.ToUInt64(2048 + vldPool.Put(tocMemoryStream)));
                    cpkSection.Writer.WriteValue("TocSize", Convert.ToUInt64(tocMemoryStream.Length));
                }

                if (itocMemoryStream != null)
                {
                    cpkSection.Writer.WriteValue("ItocOffset", Convert.ToUInt64(2048 + vldPool.Put(itocMemoryStream)));
                    cpkSection.Writer.WriteValue("ItocSize", Convert.ToUInt64(itocMemoryStream.Length));
                }

                if (etocMemoryStream != null)
                {
                    cpkSection.Writer.WriteValue("EtocOffset", Convert.ToUInt64(2048 + vldPool.Put(etocMemoryStream)));
                    cpkSection.Writer.WriteValue("EtocSize", Convert.ToUInt64(etocMemoryStream.Length));
                }

                uint totalDataSize = 0;
                foreach (CriCpkEntry entry in entries)
                {
                    totalDataSize += (uint)entry.Length;
                }

                cpkSection.Writer.WriteValue("EnabledPackedSize", totalDataSize);
                cpkSection.Writer.WriteValue("EnabledDataSize", totalDataSize);

                cpkSection.Writer.WriteValue("Files", Convert.ToUInt32(entries.Count));

                cpkSection.Writer.WriteValue("Version", (ushort)7);
                cpkSection.Writer.WriteValue("Revision", (ushort)2);
                cpkSection.Writer.WriteValue("Align", (ushort)1);
                cpkSection.Writer.WriteValue("Sorted", (ushort)1);

                cpkSection.Writer.WriteValue("CpkMode", (uint)mode);
                cpkSection.Writer.WriteValue("Tvers", "SonicAudioLib");
                cpkSection.Writer.WriteValue("Comment", Comment);

                cpkSection.Writer.WriteValue("FileSize", Convert.ToUInt64(2048 + (ulong)vldPool.Length));

                cpkSection.Writer.WriteEndRow();
                cpkSection.Writer.WriteEndTable();
            }

            while ((destination.Position % 2042) != 0)
            {
                destination.WriteByte(0);
            }

            EndianStream.WriteCString(destination, "(c)CRI", 6);

            vldPool.Write(destination);
        }

        private DateTime DateTimeFromCpkDateTime(ulong dateTime)
        {
            if (dateTime == 0)
            {
                return new DateTime();
            }

            return new DateTime(
                                (int)(uint)(dateTime >> 32) >> 16, // year
                                (int)(uint)(dateTime >> 32) >> 8 & byte.MaxValue, // month
                                (int)(uint)(dateTime >> 32) & byte.MaxValue, // day
                                (int)(uint)dateTime >> 24, // hour
                                (int)(uint)dateTime >> 16 & byte.MaxValue, // minute
                                (int)(uint)dateTime >> 8 & byte.MaxValue // second
                                );
        }

        private ulong CpkDateTimeFromDateTime(DateTime dateTime)
        {
            return ((((ulong)dateTime.Year * 0x100 + (uint)dateTime.Month) * 0x100 + (uint)dateTime.Day) * 0x100000000) + 
                ((((ulong)dateTime.Hour * 0x100 + (uint)dateTime.Minute) * 0x100 + (uint)dateTime.Second) * 0x100);
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
                uint length = (uint)(position - (headerPosition) - 16);

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

                try
                {
                    return CriTableReader.Create(new Substream(source, source.Position, tableLength));
                }

                catch
                {
                    throw new Exception("CPK file decryption needs to be implemented or it's an unknown error. Please report this error with the file.");
                }
            }

            public CriCpkSection(Stream destination, string signature)
            {
                this.destination = destination;
                headerPosition = destination.Position;

                EndianStream.WriteCString(destination, signature, 4);
                EndianStream.WriteUInt32(destination, byte.MaxValue);
                destination.Seek(8, SeekOrigin.Current);

                writer = CriTableWriter.Create(destination, new CriTableWriterSettings() { LeaveOpen = true });
            }
        }
    }
}
