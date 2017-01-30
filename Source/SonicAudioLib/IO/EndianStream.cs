using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SonicAudioLib.IO
{
    /// <summary>
    /// Represents a static class for reading various data types in any endian format from a <see cref="Stream"/>.
    /// </summary>
    public static class EndianStream
    {
        private static byte[] buffer;

        /// <summary>
        /// Fills <see cref="buffer"/> in the given length.
        /// </summary>
        private static void FillBuffer(Stream source, int length)
        {
            buffer = new byte[length];
            source.Read(buffer, 0, length);
        }

        public static void CopyTo(Stream source, Stream destination)
        {
            CopyTo(source, destination, 4096);
        }

        public static void CopyTo(Stream source, Stream destination, int bufferSize)
        {
            int read;
            byte[] buffer = new byte[bufferSize];

            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
            }
        }

        public static byte[] ReadBytes(Stream source, int length)
        {
            FillBuffer(source, length);
            return buffer;
        }

        public static byte[] ReadBytesAt(Stream source, int length, long position)
        {
            long oldPosition = source.Position;
            source.Position = position;
            FillBuffer(source, length);
            source.Position = oldPosition;

            return buffer;
        }

        public static void WriteBytes(Stream destination, byte[] value)
        {
            destination.Write(value, 0, value.Length);
        }

        public static void WriteBytes(Stream destination, byte[] value, int length)
        {
            destination.Write(value, 0, length);
        }

        public static byte ReadByte(Stream source)
        {
            int value = source.ReadByte();
            if (value == -1)
            {
                throw new EndOfStreamException();
            }

            return (byte)value;
        }

        public static byte ReadByteAt(Stream source, long position)
        {
            long oldPosition = source.Position;
            source.Position = position;

            byte value = ReadByte(source);
            source.Position = oldPosition;

            return value;
        }

        public static void WriteByte(Stream destination, byte value)
        {
            destination.WriteByte(value);
        }

        public static void WriteByteAt(Stream destination, byte value, long position)
        {
            long oldPosition = destination.Position;
            destination.Position = position;

            WriteByte(destination, value);
            destination.Position = oldPosition;
        }

        public static bool ReadBoolean(Stream source)
        {
            return ReadByte(source) > 0;
        }

        public static void WriteBoolean(Stream destination, bool value)
        {
            WriteByte(destination, (byte)(value == true ? 1 : 0));
        }

        public static sbyte ReadSByte(Stream source)
        {
            return (sbyte)ReadByte(source);
        }

        public static void WriteSByte(Stream source, sbyte value)
        {
            WriteByte(source, (byte)value);
        }

        public static ushort ReadUInt16(Stream source)
        {
            FillBuffer(source, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public static ushort ReadUInt16BE(Stream source)
        {
            FillBuffer(source, 2);

            Array.Reverse(buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public static void WriteUInt16(Stream destination, ushort value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 2);
        }

        public static void WriteUInt16BE(Stream destination, ushort value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 2);
        }

        public static short ReadInt16(Stream source)
        {
            FillBuffer(source, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static short ReadInt16BE(Stream source)
        {
            FillBuffer(source, 2);

            Array.Reverse(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static void WriteInt16(Stream destination, short value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 2);
        }

        public static void WriteInt16BE(Stream destination, short value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 2);
        }

        public static uint ReadUInt32(Stream source)
        {
            FillBuffer(source, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static uint ReadUInt32BE(Stream source)
        {
            FillBuffer(source, 4);

            Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static void WriteUInt32(Stream destination, uint value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 4);
        }

        public static void WriteUInt32BE(Stream destination, uint value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 4);
        }

        public static int ReadInt32(Stream source)
        {
            FillBuffer(source, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static int ReadInt32BE(Stream source)
        {
            FillBuffer(source, 4);

            Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static void WriteInt32(Stream destination, int value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 4);
        }

        public static void WriteInt32BE(Stream destination, int value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 4);
        }

        public static ulong ReadUInt64(Stream source)
        {
            FillBuffer(source, 8);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong ReadUInt64BE(Stream source)
        {
            FillBuffer(source, 8);

            Array.Reverse(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static void WriteUInt64(Stream destination, ulong value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 8);
        }

        public static void WriteUInt64BE(Stream destination, ulong value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 8);
        }

        public static long ReadInt64(Stream source)
        {
            FillBuffer(source, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static long ReadInt64BE(Stream source)
        {
            FillBuffer(source, 8);

            Array.Reverse(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static void WriteInt64(Stream destination, long value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 8);
        }

        public static void WriteInt64BE(Stream destination, long value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 8);
        }

        public static float ReadFloat(Stream source)
        {
            FillBuffer(source, 4);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static float ReadFloatBE(Stream source)
        {
            FillBuffer(source, 4);

            Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static void WriteFloat(Stream destination, float value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 4);
        }

        public static void WriteFloatBE(Stream destination, float value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 4);
        }

        public static double ReadDouble(Stream source)
        {
            FillBuffer(source, 8);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static double ReadDoubleBE(Stream source)
        {
            FillBuffer(source, 8);

            Array.Reverse(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static void WriteDouble(Stream destination, double value)
        {
            buffer = BitConverter.GetBytes(value);
            destination.Write(buffer, 0, 8);
        }

        public static void WriteDoubleBE(Stream destination, double value)
        {
            buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            destination.Write(buffer, 0, 8);
        }

        public static string ReadCString(Stream source)
        {
            return ReadCString(source, Encoding.ASCII);
        }

        public static string ReadCString(Stream source, Encoding encoding)
        {
            var list = new List<byte>();

            byte buff;
            while ((buff = ReadByte(source)) != 0)
            {
                list.Add(buff);
            }

            return encoding.GetString(list.ToArray());
        }

        public static void WriteCString(Stream destination, string value)
        {
            WriteCString(destination, value, Encoding.ASCII);
        }

        public static void WriteCString(Stream destination, string value, Encoding encoding)
        {
            var buff = encoding.GetBytes(value);

            foreach (byte _buff in buff)
            {
                WriteByte(destination, _buff);
            }

            WriteByte(destination, 0);
        }

        public static string ReadCString(Stream source, int length)
        {
            return ReadCString(source, length, Encoding.ASCII);
        }

        public static string ReadCString(Stream source, int length, Encoding encoding)
        {
            byte[] buffer = new byte[length];
            source.Read(buffer, 0, length);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    return encoding.GetString(buffer, 0, i);
                }
            }

            return encoding.GetString(buffer);
        }

        public static void WriteCString(Stream destination, string value, int length)
        {
            WriteCString(destination, value, length, Encoding.ASCII);
        }

        public static void WriteCString(Stream destination, string value, int length, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value.ToCharArray(), 0, length);
            destination.Write(buffer, 0, length);
        }

        public static void Pad(Stream destination, long alignment)
        {
            while ((destination.Position % alignment) != 0)
            {
                destination.WriteByte(0);
            }
        }
    }
}
