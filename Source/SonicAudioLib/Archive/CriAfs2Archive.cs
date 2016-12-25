using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SonicAudioLib.IO;
using SonicAudioLib.Module;

namespace SonicAudioLib.Archive
{
    public class CriAfs2Entry : EntryBase
    {
        public ushort CueIndex { get; set; }
    }

    public class CriAfs2Archive : ArchiveBase<CriAfs2Entry>
    {
        public uint Align { get; set; }
        public uint CueIndexFieldLength { get; set; }
        public uint PositionFieldLength { get; set; }

        public override void Read(Stream source)
        {
            if (EndianStream.ReadCString(source, 4) != "AFS2")
            {
                throw new Exception("No AFS2 signature found.");
            }

            uint information = EndianStream.ReadUInt32(source);

            uint type = information & 0xFF;
            if (type != 1)
            {
                throw new Exception($"Invalid AFS2 type ({type}). Please report the error with the AWB file.");
            }

            CueIndexFieldLength = (information & 0x00FF0000) >> 16;
            PositionFieldLength = (information & 0x0000FF00) >> 8;

            ushort entryCount = (ushort)EndianStream.ReadUInt32(source);
            Align = EndianStream.ReadUInt32(source);

            CriAfs2Entry previousEntry = null;
            for (uint i = 0; i < entryCount; i++)
            {
                CriAfs2Entry afs2Entry = new CriAfs2Entry();

                long cueIndexPosition = 16 + (i * CueIndexFieldLength);
                source.Seek(cueIndexPosition, SeekOrigin.Begin);

                switch (CueIndexFieldLength)
                {
                    case 2:
                        afs2Entry.CueIndex = EndianStream.ReadUInt16(source);
                        break;

                    default:
                        throw new Exception($"Unknown CueIndexFieldLength ({CueIndexFieldLength}). Please report the error with the AWB file.");
                }

                long positionPosition = 16 + (entryCount * CueIndexFieldLength) + (i * PositionFieldLength);
                source.Seek(positionPosition, SeekOrigin.Begin);

                switch (PositionFieldLength)
                {
                    case 2:
                        afs2Entry.Position = EndianStream.ReadUInt16(source);
                        break;

                    case 4:
                        afs2Entry.Position = EndianStream.ReadUInt32(source);
                        break;

                    default:
                        throw new Exception($"Unknown PositionFieldLength ({PositionFieldLength}). Please report the error with the AWB file.");
                }

                if (previousEntry != null)
                {
                    previousEntry.Length = afs2Entry.Position - previousEntry.Position;
                }

                while ((afs2Entry.Position % Align) != 0)
                {
                    afs2Entry.Position++;
                }

                if (i == entryCount - 1)
                {
                    switch (PositionFieldLength)
                    {
                        case 2:
                            afs2Entry.Length = EndianStream.ReadUInt16(source) - afs2Entry.Position;
                            break;

                        case 4:
                            afs2Entry.Length = EndianStream.ReadUInt32(source) - afs2Entry.Position;
                            break;
                    }
                }

                entries.Add(afs2Entry);
                previousEntry = afs2Entry;
            }
        }

        public override void Write(Stream destination)
        {
            uint headerLength = (uint)(16 + (entries.Count * CueIndexFieldLength) + (entries.Count * PositionFieldLength) + PositionFieldLength);

            EndianStream.WriteCString(destination, "AFS2", 4);
            EndianStream.WriteUInt32(destination, 1 | (CueIndexFieldLength << 16) | (PositionFieldLength << 8));
            EndianStream.WriteUInt32(destination, (ushort)entries.Count);
            EndianStream.WriteUInt32(destination, 1);
            
            // FIXME: Alignment support
            VldPool vldPool = new VldPool(1);

            foreach (CriAfs2Entry afs2Entry in entries)
            {
                switch (CueIndexFieldLength)
                {
                    case 2:
                        EndianStream.WriteUInt16(destination, (ushort)afs2Entry.CueIndex);
                        break;

                    default:
                        throw new Exception($"Unknown CueIndexFieldLength ({CueIndexFieldLength}). Please set a valid length.");
                }
            }

            foreach (CriAfs2Entry afs2Entry in entries)
            {
                uint entryPosition = (uint)(headerLength + vldPool.Put(afs2Entry.FilePath));

                switch (PositionFieldLength)
                {
                    case 2:
                        EndianStream.WriteUInt16(destination, (ushort)entryPosition);
                        break;

                    case 4:
                        EndianStream.WriteUInt32(destination, entryPosition);
                        break;

                    default:
                        throw new Exception($"Unknown PositionFieldLength ({PositionFieldLength}). Please set a valid length.");
                }

                afs2Entry.Position = entryPosition;
            }

            EndianStream.WriteUInt32(destination, (uint)(headerLength + vldPool.Length));

            vldPool.Write(destination);
            vldPool.Clear();
        }

        public CriAfs2Entry GetByCueIndex(uint cueIndex)
        {
            return entries.Single(e => e.CueIndex == cueIndex);
        }

        public override long CalculateLength()
        {
            long length = 16 + (entries.Count * CueIndexFieldLength) + (entries.Count * PositionFieldLength) + PositionFieldLength;

            foreach (CriAfs2Entry afs2Entry in entries)
            {
                while ((length % Align) != 0)
                {
                    length++;
                }

                length += afs2Entry.Length;
            }

            return length;
        }

        public void Order()
        {
            entries = entries.OrderBy(entry => entry.CueIndex).ToList();
        }

        public CriAfs2Archive()
        {
            Align = 32;
            CueIndexFieldLength = 2;
            PositionFieldLength = 4;
        }
    }
}
