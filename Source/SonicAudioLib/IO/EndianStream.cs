using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace SonicAudioLib.IO
{
    public static class EndianStream
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct SingleUnion
        {
            [FieldOffset(0)]
            public float Single;

            [FieldOffset(0)]
            public uint UInt;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleUnion
        {
            [FieldOffset(0)]
            public double Double;

            [FieldOffset(0)]
            public ulong ULong;
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
            byte[] buffer = new byte[length];
            source.Read(buffer, 0, length);
            return buffer;
        }

        public static byte[] ReadBytesAt(Stream source, int length, long position)
        {
            long oldPosition = source.Position;
            source.Position = position;
            var result = ReadBytes(source, length);
            source.Position = oldPosition;

            return result;
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
            return source.ReadByte() > 0;
        }

        public static void WriteBoolean(Stream destination, bool value)
        {
            WriteByte(destination, (byte)(value ? 1 : 0));
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
            return (ushort)(source.ReadByte() | source.ReadByte() << 8);
        }

        public static ushort ReadUInt16BE(Stream source)
        {
            return (ushort)(source.ReadByte() << 8 | source.ReadByte());
        }

        public static void WriteUInt16(Stream destination, ushort value)
        {
            destination.WriteByte((byte)(value));
            destination.WriteByte((byte)(value >> 8));
        }

        public static void WriteUInt16BE(Stream destination, ushort value)
        {
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value));
        }

        public static short ReadInt16(Stream source)
        {
            return (short)(source.ReadByte() | source.ReadByte() << 8);
        }

        public static short ReadInt16BE(Stream source)
        {
            return (short)(source.ReadByte() << 8 | source.ReadByte());
        }

        public static void WriteInt16(Stream destination, short value)
        {
            destination.WriteByte((byte)(value));
            destination.WriteByte((byte)(value >> 8));
        }

        public static void WriteInt16BE(Stream destination, short value)
        {
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value));
        }

        public static uint ReadUInt32(Stream source)
        {
            return (uint)(source.ReadByte() | source.ReadByte() << 8 | source.ReadByte() << 16 | source.ReadByte() << 24);
        }

        public static uint ReadUInt32BE(Stream source)
        {
            return (uint)(source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte());
        }

        public static void WriteUInt32(Stream destination, uint value)
        {
            destination.WriteByte((byte)(value));
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value >> 16));
            destination.WriteByte((byte)(value >> 24));
        }

        public static void WriteUInt32BE(Stream destination, uint value)
        {
            destination.WriteByte((byte)((value >> 24)));
            destination.WriteByte((byte)((value >> 16)));
            destination.WriteByte((byte)((value >> 8)));
            destination.WriteByte((byte)(value));
        }

        public static int ReadInt32(Stream source)
        {
            return source.ReadByte() | source.ReadByte() << 8 | source.ReadByte() << 16 | source.ReadByte() << 24;
        }

        public static int ReadInt32BE(Stream source)
        {
            return source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte();
        }

        public static void WriteInt32(Stream destination, int value)
        {
            destination.WriteByte((byte)(value));
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value >> 16));
            destination.WriteByte((byte)(value >> 24));
        }

        public static void WriteInt32BE(Stream destination, int value)
        {
            destination.WriteByte((byte)((value >> 24)));
            destination.WriteByte((byte)((value >> 16)));
            destination.WriteByte((byte)((value >> 8)));
            destination.WriteByte((byte)(value));
        }

        public static ulong ReadUInt64(Stream source)
        {
            return (uint)(source.ReadByte() | source.ReadByte() << 8 | source.ReadByte() << 16 | source.ReadByte() << 24) |
                ((ulong)(source.ReadByte() | source.ReadByte() << 8 | source.ReadByte() << 16 | source.ReadByte() << 24) << 32);
        }

        public static ulong ReadUInt64BE(Stream source)
        {
            return (uint)(source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte()) |
                ((ulong)(source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte()) << 32);
        }

        public static void WriteUInt64(Stream destination, ulong value)
        {
            destination.WriteByte((byte)(value));
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value >> 16));
            destination.WriteByte((byte)(value >> 24));
            destination.WriteByte((byte)(value >> 32));
            destination.WriteByte((byte)(value >> 40));
            destination.WriteByte((byte)(value >> 48));
            destination.WriteByte((byte)(value >> 56));
        }

        public static void WriteUInt64BE(Stream destination, ulong value)
        {
            destination.WriteByte((byte)((value >> 56)));
            destination.WriteByte((byte)((value >> 48)));
            destination.WriteByte((byte)((value >> 40)));
            destination.WriteByte((byte)((value >> 32)));
            destination.WriteByte((byte)((value >> 24)));
            destination.WriteByte((byte)((value >> 16)));
            destination.WriteByte((byte)((value >> 8)));
            destination.WriteByte((byte)(value));
        }

        public static long ReadInt64(Stream source)
        {
            return (uint)(source.ReadByte() | source.ReadByte() << 8 | source.ReadByte() << 16 | source.ReadByte() << 24) | 
                ((long)(source.ReadByte() | source.ReadByte() << 8 | source.ReadByte() << 16 | source.ReadByte() << 24) << 32);
        }

        public static long ReadInt64BE(Stream source)
        {
            return (uint)(source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte()) | 
                ((long)(source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte()) << 32);
        }

        public static void WriteInt64(Stream destination, long value)
        {
            destination.WriteByte((byte)(value));
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value >> 16));
            destination.WriteByte((byte)(value >> 24));
            destination.WriteByte((byte)(value >> 32));
            destination.WriteByte((byte)(value >> 40));
            destination.WriteByte((byte)(value >> 48));
            destination.WriteByte((byte)(value >> 56));
        }

        public static void WriteInt64BE(Stream destination, long value)
        {
            destination.WriteByte((byte)((value >> 56)));
            destination.WriteByte((byte)((value >> 48)));
            destination.WriteByte((byte)((value >> 40)));
            destination.WriteByte((byte)((value >> 32)));
            destination.WriteByte((byte)((value >> 24)));
            destination.WriteByte((byte)((value >> 16)));
            destination.WriteByte((byte)((value >> 8)));
            destination.WriteByte((byte)(value));
        }

        public static float ReadSingle(Stream source)
        {
            var union = new SingleUnion();
            union.UInt = ReadUInt32(source);

            return union.Single;
        }

        public static float ReadSingleBE(Stream source)
        {
            var union = new SingleUnion();
            union.UInt = ReadUInt32BE(source);

            return union.Single;
        }

        public static void WriteSingle(Stream destination, float value)
        {
            var union = new SingleUnion();
            union.Single = value;

            WriteUInt32(destination, union.UInt);
        }

        public static void WriteSingleBE(Stream destination, float value)
        {
            var union = new SingleUnion();
            union.Single = value;

            WriteUInt32BE(destination, union.UInt);
        }

        public static double ReadDouble(Stream source)
        {
            var union = new DoubleUnion();
            union.ULong = ReadUInt64(source);

            return union.Double;
        }

        public static double ReadDoubleBE(Stream source)
        {
            var union = new DoubleUnion();
            union.ULong = ReadUInt64BE(source);

            return union.Double;
        }

        public static void WriteDouble(Stream destination, double value)
        {
            var union = new DoubleUnion();
            union.Double = value;

            WriteUInt64(destination, union.ULong);
        }

        public static void WriteDoubleBE(Stream destination, double value)
        {
            var union = new DoubleUnion();
            union.Double = value;

            WriteUInt64BE(destination, union.ULong);
        }

        public static string ReadCString(Stream source)
        {
            return ReadCString(source, Encoding.ASCII);
        }

        public static string ReadCString(Stream source, Encoding encoding)
        {
            var characters = new List<byte>();

            byte character = (byte)source.ReadByte();
            while (character != 0)
            {
                characters.Add(character);
                character = (byte)source.ReadByte();
            }

            return encoding.GetString(characters.ToArray());
        }

        public static void WriteCString(Stream destination, string value)
        {
            WriteCString(destination, value, Encoding.ASCII);
        }

        public static void WriteCString(Stream destination, string value, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value);

            destination.Write(buffer, 0, buffer.Length);
            destination.WriteByte(0);
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
