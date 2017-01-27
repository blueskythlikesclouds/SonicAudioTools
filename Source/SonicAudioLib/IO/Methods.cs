using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SonicAudioLib.IO
{
    public class Methods
    {
        public static long Align(long value, long alignment)
        {
            while ((value % alignment) != 0)
            {
                value++;
            }

            return value;
        }

        // This one masks the source to destination
        public static void MaskCriTable(Stream source, Stream destination, long length)
        {
            uint currentXor = 25951;
            long currentPosition = source.Position;

            while (source.Position < currentPosition + length)
            {
                byte maskedByte = (byte)(EndianStream.ReadByte(source) ^ currentXor);
                currentXor *= 16661;

                EndianStream.WriteByte(destination, maskedByte);
            }
        }

        // This one masks the source to itself
        public static void MaskCriTable(Stream source, long length)
        {
            if (source.CanRead && source.CanWrite)
            {
                uint currentXor = 25951;
                long currentPosition = source.Position;

                while (source.Position < currentPosition + length)
                {
                    byte maskedByte = (byte)(EndianStream.ReadByte(source) ^ currentXor);
                    currentXor *= 16661;

                    EndianStream.WriteByteAt(source, maskedByte, source.Position - 1);
                }
            }
        }
    }
}
