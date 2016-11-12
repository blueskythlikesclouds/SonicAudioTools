using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SonicAudioLib.IO;
using SonicAudioLib.Module;

namespace SonicAudioLib.Archive
{
    public class HeroesPacEntry : EntryBase
    {
        public uint GlobalIndex { get; set; }
    }

    public class HeroesPacArchive : ArchiveBase<HeroesPacEntry>
    {
        public override void Read(Stream source)
        {
            uint entryCount = EndianStream.ReadUInt32(source);
            uint tablePosition = EndianStream.ReadUInt32(source);
            uint vldPoolLength = EndianStream.ReadUInt32(source);
            uint vldPoolPosition = EndianStream.ReadUInt32(source);

            source.Seek(tablePosition, SeekOrigin.Begin);

            HeroesPacEntry previousEntry = null;
            for (uint i = 0; i < entryCount; i++)
            {
                HeroesPacEntry pacEntry = new HeroesPacEntry();

                pacEntry.GlobalIndex = EndianStream.ReadUInt32(source);
                pacEntry.Position = vldPoolPosition + EndianStream.ReadUInt32(source);

                if (previousEntry != null)
                {
                    previousEntry.Length = pacEntry.Position - previousEntry.Position;
                }

                if (i == entryCount - 1)
                {
                    pacEntry.Length = (uint)source.Length - pacEntry.Position;
                }

                entries.Add(pacEntry);
                previousEntry = pacEntry;

                source.Seek(8, SeekOrigin.Current);
            }
        }

        public override void Write(Stream destination)
        {
            long headerPosition = destination.Position;

            uint headerLength = 16;
            for (uint i = 0; i < headerLength; i++)
            {
                destination.WriteByte(0);
            }

            while ((destination.Position % 32) != 0)
            {
                destination.WriteByte(0);
            }

            uint tableLength = (uint)(entries.Count * 16);
            uint tablePosition = (uint)destination.Position;

            VldPool vldPool = new VldPool();

            foreach (HeroesPacEntry pacEntry in entries)
            {
                uint entryPosition = (uint)vldPool.Put(pacEntry.FilePath);

                EndianStream.WriteUInt32(destination, pacEntry.GlobalIndex);
                EndianStream.WriteUInt32(destination, entryPosition);

                while ((destination.Position % 16) != 0)
                {
                    destination.WriteByte(0);
                }

                pacEntry.Position = tablePosition + tableLength + entryPosition;
            }

            vldPool.Write(destination);
            vldPool.Clear();

            destination.Seek(0, SeekOrigin.Begin);
            EndianStream.WriteUInt32(destination, (uint)entries.Count);
            EndianStream.WriteUInt32(destination, tablePosition);
            EndianStream.WriteUInt32(destination, (uint)vldPool.Length);
            EndianStream.WriteUInt32(destination, (uint)vldPool.Position);
        }
    }
}
