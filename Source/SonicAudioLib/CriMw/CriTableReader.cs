using SonicAudioLib.IO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SonicAudioLib.CriMw
{
    public class CriTableReader : IDisposable
    {
        private List<CriTableField> fields;
        private Stream source;
        private CriTableHeader header;
        private int rowIndex = -1;
        private uint headerPosition;
        private bool leaveOpen;

        public object this[int fieldIndex]
        {
            get
            {
                return GetValue(fieldIndex);
            }
        }

        public object this[string fieldName]
        {
            get
            {
                return GetValue(fieldName);
            }
        }

        public ushort NumberOfFields
        {
            get
            {
                return header.NumberOfFields;
            }
        }

        public uint NumberOfRows
        {
            get
            {
                return header.NumberOfRows;
            }
        }

        public string TableName
        {
            get
            {
                return header.TableName;
            }
        }

        public int CurrentRow
        {
            get
            {
                return rowIndex;
            }
        }

        public Stream SourceStream
        {
            get
            {
                return source;
            }
        }

        private void ReadTable()
        {
            headerPosition = (uint)source.Position;

            if (EndianStream.ReadCString(source, 4) != CriTableHeader.Signature)
            {
                // try to decrypt (currently only for CPK files since those are the only examples I have)
                source.Seek(-4, SeekOrigin.Current);

                MemoryStream unmaskedSource = new MemoryStream();
                Methods.MaskCriTable(source, unmaskedSource, source.Length);

                // try again
                unmaskedSource.Seek(0, SeekOrigin.Begin);

                if (EndianStream.ReadCString(unmaskedSource, 4) != CriTableHeader.Signature)
                {
                    throw new Exception("No @UTF signature found.");
                }

                // Close the old stream
                if (!leaveOpen)
                {
                    source.Close();
                }

                source = unmaskedSource;
            }

            header.Length = ReadUInt32() + 0x8;
            header.FirstBoolean = ReadBoolean();
            header.SecondBoolean = ReadBoolean();
            header.RowsPosition = (ushort)(ReadUInt16() + 0x8);
            header.StringPoolPosition = ReadUInt32() + 0x8;
            header.DataPoolPosition = ReadUInt32() + 0x8;
            header.TableName = ReadString();
            header.NumberOfFields = ReadUInt16();
            header.RowLength = ReadUInt16();
            header.NumberOfRows = ReadUInt32();

            if (header.FirstBoolean)
            {
                throw new Exception($"Invalid boolean ({header.FirstBoolean}. Please report the error with the file.");
            }

            for (ushort i = 0; i < header.NumberOfFields; i++)
            {
                CriTableField field = new CriTableField();

                field.Flag = (CriFieldFlag)ReadByte();

                if (field.Flag.HasFlag(CriFieldFlag.Name))
                {
                    field.Name = ReadString();
                }

                if (field.Flag.HasFlag(CriFieldFlag.DefaultValue))
                {
                    if (field.Flag.HasFlag(CriFieldFlag.Data))
                    {
                        uint vldPosition;
                        uint vldLength;

                        ReadData(out vldPosition, out vldLength);

                        field.Position = vldPosition;
                        field.Length = vldLength;
                    }

                    else
                    {
                        field.Value = ReadValue(field.Flag);
                    }
                }

                // Not even per row, and not even constant value? Then there's no storage.
                else if (!field.Flag.HasFlag(CriFieldFlag.RowStorage) && !field.Flag.HasFlag(CriFieldFlag.DefaultValue))
                {
                    if (field.Flag.HasFlag(CriFieldFlag.Data))
                    {
                        field.Position = 0;
                        field.Length = 0;
                    }

                    else
                    {
                        field.Value = CriField.NullValues[(byte)field.Flag & 0x0F];
                    }
                }

                fields.Add(field);
            }
        }

        public string GetFieldName(int fieldIndex)
        {
            return fields[fieldIndex].Name;
        }

        public Type GetFieldType(int fieldIndex)
        {
            return CriField.FieldTypes[(byte)fields[fieldIndex].Flag & 0x0F];
        }

        public Type GetFieldType(string fieldName)
        {
            return CriField.FieldTypes[(byte)fields[GetFieldIndex(fieldName)].Flag & 0x0F];
        }

        public object GetFieldValue(int fieldIndex)
        {
            return fields[fieldIndex].Value;
        }

        internal CriFieldFlag GetFieldFlag(string fieldName)
        {
            return fields[GetFieldIndex(fieldName)].Flag;
        }

        internal CriFieldFlag GetFieldFlag(int fieldIndex)
        {
            return fields[fieldIndex].Flag;
        }

        public object GetFieldValue(string fieldName)
        {
            return fields[GetFieldIndex(fieldName)].Value;
        }

        public CriField GetField(int fieldIndex)
        {
            return new CriField(GetFieldName(fieldIndex), GetFieldType(fieldIndex), GetFieldValue(fieldIndex));
        }

        public CriField GetField(string fieldName)
        {
            return new CriField(fieldName, GetFieldType(fieldName), GetFieldValue(fieldName));
        }

        public int GetFieldIndex(string fieldName)
        {
            return fields.FindIndex(field => field.Name == fieldName);
        }

        public bool ContainsField(string fieldName)
        {
            return fields.Exists(field => field.Name == fieldName);
        }
        
        private void GoToValue(int fieldIndex)
        {
            long position = headerPosition + header.RowsPosition + (header.RowLength * rowIndex);

            for (int i = 0; i < fieldIndex; i++)
            {
                if (!fields[i].Flag.HasFlag(CriFieldFlag.RowStorage))
                {
                    continue;
                }

                switch (fields[i].Flag & CriFieldFlag.TypeMask)
                {
                    case CriFieldFlag.Byte:
                    case CriFieldFlag.SByte:
                        position += 1;
                        break;
                    case CriFieldFlag.Int16:
                    case CriFieldFlag.UInt16:
                        position += 2;
                        break;
                    case CriFieldFlag.Int32:
                    case CriFieldFlag.UInt32:
                    case CriFieldFlag.Float:
                    case CriFieldFlag.String:
                        position += 4;
                        break;
                    case CriFieldFlag.Int64:
                    case CriFieldFlag.UInt64:
                    case CriFieldFlag.Double:
                    case CriFieldFlag.Data:
                        position += 8;
                        break;
                }
            }

            source.Position = position;
        }

        public bool Read()
        {
            if (rowIndex + 1 >= header.NumberOfRows)
            {
                return false;
            }

            rowIndex++;
            return true;
        }

        public bool MoveToRow(int rowIndex)
        {
            if (rowIndex >= header.NumberOfRows)
            {
                return false;
            }

            this.rowIndex = rowIndex;
            return true;
        }

        public object[] GetValueArray()
        {
            object[] values = new object[header.NumberOfFields];

            for (int i = 0; i < header.NumberOfFields; i++)
            {
                if (fields[i].Flag.HasFlag(CriFieldFlag.Data))
                {
                    values[i] = GetData(i);
                }

                else
                {
                    values[i] = GetValue(i);
                }
            }

            return values;
        }

        public object GetValue(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= fields.Count)
            {
                return null;
            }

            if (!fields[fieldIndex].Flag.HasFlag(CriFieldFlag.RowStorage))
            {
                if (fields[fieldIndex].Flag.HasFlag(CriFieldFlag.Data))
                {
                    return new Substream(source, 0, 0);
                }

                return fields[fieldIndex].Value;
            }

            GoToValue(fieldIndex);
            return ReadValue(fields[fieldIndex].Flag);
        }

        public object GetValue(string fieldName)
        {
            return GetValue(GetFieldIndex(fieldName));
        }

        public T GetValue<T>(int fieldIndex)
        {
            return (T)GetValue(fieldIndex);
        }

        public T GetValue<T>(string fieldName)
        {
            return (T)GetValue(fieldName);
        }

        public byte GetByte(int fieldIndex)
        {
            return (byte)GetValue(fieldIndex);
        }

        public byte GetByte(string fieldName)
        {
            return (byte)GetValue(fieldName);
        }

        public sbyte GetSByte(int fieldIndex)
        {
            return (sbyte)GetValue(fieldIndex);
        }

        public sbyte GetSByte(string fieldName)
        {
            return (sbyte)GetValue(fieldName);
        }

        public ushort GetUInt16(int fieldIndex)
        {
            return (ushort)GetValue(fieldIndex);
        }

        public ushort GetUInt16(string fieldName)
        {
            return (ushort)GetValue(fieldName);
        }

        public short GetInt16(int fieldIndex)
        {
            return (short)GetValue(fieldIndex);
        }

        public short GetInt16(string fieldName)
        {
            return (short)GetValue(fieldName);
        }

        public uint GetUInt32(int fieldIndex)
        {
            return (uint)GetValue(fieldIndex);
        }

        public uint GetUInt32(string fieldName)
        {
            return (uint)GetValue(fieldName);
        }

        public int GetInt32(int fieldIndex)
        {
            return (int)GetValue(fieldIndex);
        }

        public int GetInt32(string fieldName)
        {
            return (int)GetValue(fieldName);
        }

        public ulong GetUInt64(int fieldIndex)
        {
            return (ulong)GetValue(fieldIndex);
        }

        public ulong GetUInt64(string fieldName)
        {
            return (ulong)GetValue(fieldName);
        }

        public long GetInt64(int fieldIndex)
        {
            return (long)GetValue(fieldIndex);
        }

        public long GetInt64(string fieldName)
        {
            return (long)GetValue(fieldName);
        }

        public float GetFloat(int fieldIndex)
        {
            return (float)GetValue(fieldIndex);
        }

        public float GetFloat(string fieldName)
        {
            return (float)GetValue(fieldName);
        }

        public double GetDouble(int fieldIndex)
        {
            return (double)GetValue(fieldIndex);
        }

        public double GetDouble(string fieldName)
        {
            return (double)GetValue(fieldName);
        }

        public string GetString(int fieldIndex)
        {
            return (string)GetValue(fieldIndex);
        }

        public string GetString(string fieldName)
        {
            return (string)GetValue(fieldName);
        }

        public Substream GetSubstream(int fieldIndex)
        {
            return (Substream)GetValue(fieldIndex);
        }

        public Substream GetSubstream(string fieldName)
        {
            return (Substream)GetValue(fieldName);
        }

        public byte[] GetData(int fieldIndex)
        {
            return GetSubstream(fieldIndex).ToArray();
        }

        public byte[] GetData(string fieldName)
        {
            return GetData(GetFieldIndex(fieldName));
        }

        public CriTableReader GetCriTableReader(string fieldName)
        {
            return new CriTableReader(GetSubstream(fieldName), false);
        }

        public CriTableReader GetCriTableReader(int fieldIndex)
        {
            return new CriTableReader(GetSubstream(fieldIndex), false);
        }

        public uint GetLength(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= fields.Count)
            {
                return 0;
            }

            if (!fields[fieldIndex].Flag.HasFlag(CriFieldFlag.RowStorage))
            {
                return fields[fieldIndex].Length;
            }

            uint vldPosition;
            uint vldLength;

            GoToValue(fieldIndex);
            ReadData(out vldPosition, out vldLength);
            return vldLength;
        }

        public uint GetLength(string fieldName)
        {
            return GetLength(GetFieldIndex(fieldName));
        }

        public uint GetPosition(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= fields.Count)
            {
                return 0;
            }

            if (!fields[fieldIndex].Flag.HasFlag(CriFieldFlag.RowStorage))
            {
                return fields[fieldIndex].Position;
            }

            uint vldPosition;
            uint vldLength;

            GoToValue(fieldIndex);
            ReadData(out vldPosition, out vldLength);
            return (uint)(headerPosition + header.DataPoolPosition + vldPosition);
        }

        public uint GetPosition(string fieldName)
        {
            return GetPosition(GetFieldIndex(fieldName));
        }
        
        public bool GetBoolean(int fieldIndex)
        {
            return (byte)GetValue(fieldIndex) > 0;
        }

        public bool GetBoolean(string fieldName)
        {
            return (byte)GetValue(fieldName) > 0;
        }

        public Guid GetGuid(int fieldIndex)
        {
            return (Guid)GetValue(fieldIndex);
        }

        public Guid GetGuid(string fieldName)
        {
            return (Guid)GetValue(fieldName);
        }

        private byte[] ReadBytes(int length)
        {
            byte[] buff = new byte[length];
            source.Read(buff, 0, length);
            return buff;
        }

        private byte ReadByte()
        {
            return EndianStream.ReadByte(source);
        }

        private bool ReadBoolean()
        {
            return EndianStream.ReadBoolean(source);
        }

        private sbyte ReadSByte()
        {
            return EndianStream.ReadSByte(source);
        }

        private ushort ReadUInt16()
        {
            return EndianStream.ReadUInt16BE(source);
        }

        private short ReadInt16()
        {
            return EndianStream.ReadInt16BE(source);
        }

        private uint ReadUInt32()
        {
            return EndianStream.ReadUInt32BE(source);
        }

        private int ReadInt32()
        {
            return EndianStream.ReadInt32BE(source);
        }

        private ulong ReadUInt64()
        {
            return EndianStream.ReadUInt64BE(source);
        }

        private long ReadInt64()
        {
            return EndianStream.ReadInt64BE(source);
        }

        private float ReadFloat()
        {
            return EndianStream.ReadFloatBE(source);
        }

        private double ReadDouble()
        {
            return EndianStream.ReadDoubleBE(source);
        }

        private string ReadString()
        {
            uint stringPosition = ReadUInt32();

            long previousPosition = source.Position;

            source.Position = headerPosition + header.StringPoolPosition + stringPosition;
            string strResult = EndianStream.ReadCString(source, Encoding.GetEncoding("shift-jis"));
            source.Position = previousPosition;

            if (strResult == "<NULL>" ||
                (strResult == header.TableName && stringPosition == 0))
            {
                return string.Empty;
            }

            return strResult;
        }

        private void ReadData(out uint vldPosition, out uint vldLength)
        {
            vldPosition = ReadUInt32();
            vldLength = ReadUInt32();
        }

        private Guid ReadGuid()
        {
            byte[] buffer = new byte[16];
            source.Read(buffer, 0, buffer.Length);
            return new Guid(buffer);
        }

        private object ReadValue(CriFieldFlag fieldFlag)
        {
            switch (fieldFlag & CriFieldFlag.TypeMask)
            {
                case CriFieldFlag.Byte:
                    return ReadByte();
                case CriFieldFlag.SByte:
                    return ReadSByte();
                case CriFieldFlag.UInt16:
                    return ReadUInt16();
                case CriFieldFlag.Int16:
                    return ReadInt16();
                case CriFieldFlag.UInt32:
                    return ReadUInt32();
                case CriFieldFlag.Int32:
                    return ReadInt32();
                case CriFieldFlag.UInt64:
                    return ReadUInt64();
                case CriFieldFlag.Int64:
                    return ReadInt64();
                case CriFieldFlag.Float:
                    return ReadFloat();
                case CriFieldFlag.Double:
                    return ReadDouble();
                case CriFieldFlag.String:
                    return ReadString();
                case CriFieldFlag.Data:
                    {
                        uint vldPosition;
                        uint vldLength;

                        ReadData(out vldPosition, out vldLength);

                        // SecondBoolean being true, check if utf table
                        if (vldPosition > 0 && vldLength == 0)
                        {
                            source.Position = headerPosition + header.DataPoolPosition + vldPosition;

                            if (Encoding.ASCII.GetString(ReadBytes(4)) == "@UTF")
                            {
                                vldLength = ReadUInt32() + 8;
                            }
                        }

                        return new Substream(source, headerPosition + header.DataPoolPosition + vldPosition, vldLength);
                    }
                case CriFieldFlag.Guid:
                    return ReadGuid();
            }

            return null;
        }

        public void Dispose()
        {
            fields.Clear();

            if (!leaveOpen)
            {
                source.Close();
            }

            GC.SuppressFinalize(this);
        }

        public static CriTableReader Create(byte[] sourceByteArray)
        {
            Stream source = new MemoryStream(sourceByteArray);
            return Create(source);
        }

        public static CriTableReader Create(string sourceFileName)
        {
            Stream source = File.OpenRead(sourceFileName);
            return Create(source);
        }

        public static CriTableReader Create(Stream source)
        {
            return Create(source, false);
        }

        public static CriTableReader Create(Stream source, bool leaveOpen)
        {
            return new CriTableReader(source, leaveOpen);
        }

        private CriTableReader(Stream source, bool leaveOpen)
        {
            this.source = source;
            header = new CriTableHeader();
            fields = new List<CriTableField>();
            this.leaveOpen = leaveOpen;

            ReadTable();
        }   
    }
}
