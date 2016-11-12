using System;
using System.IO;
using System.Text;
using System.ComponentModel;

using SonicAudioLib.Collections;
using SonicAudioLib.IO;
using SonicAudioLib.Module;

namespace SonicAudioLib.CriMw
{
    public class CriTableWriter : IDisposable
    {
        public enum Status
        {
            Begin,
            Start,
            FieldCollection,
            Row,
            Idle,
            End,
        }

        private CriTableWriterSettings settings;
        private OrderedDictionary<string, CriTableField> fields;
        private Stream destination;
        private CriTableHeader header;
        private VldPool vldPool;
        private StringPool stringPool;
        private uint headerPosition;
        private uint endPosition;

        private Status status = Status.Begin;

        public Status CurrentStatus
        {
            get
            {
                return status;
            }
        }

        public Stream DestinationStream
        {
            get
            {
                return destination;
            }
        }

        public void WriteStartTable()
        {
            WriteStartTable("(no name)");
        }

        public void WriteStartTable(string tableName)
        {
            if (status != Status.Begin)
            {
                throw new InvalidOperationException("Attempted to start table when the status wasn't Begin");
            }

            status = Status.Start;

            headerPosition = (uint)destination.Position;
            header.TableName = tableName;

            if (settings.PutBlankString)
            {
                stringPool.Put(StringPool.AdxBlankString);
            }

            EndianStream.WriteCString(destination, CriTableHeader.Signature, 4);
            WriteUInt32(uint.MinValue);
            WriteBoolean(false);
            WriteBoolean(false);
            WriteUInt16(ushort.MinValue);
            WriteUInt32(uint.MinValue);
            WriteUInt32(uint.MinValue);
            WriteString(tableName);
            WriteUInt16(ushort.MinValue);
            WriteUInt16(ushort.MinValue);
            WriteUInt32(uint.MinValue);
        }

        public void WriteEndTable()
        {
            if (status == Status.FieldCollection)
            {
                WriteEndFieldCollection();
            }

            if (status == Status.Row)
            {
                WriteEndRow();
            }

            status = Status.End;

            destination.Seek(headerPosition + header.RowsPosition + (header.RowLength * header.NumberOfRows), SeekOrigin.Begin);

            stringPool.Write(destination);
            header.StringPoolPosition = (uint)stringPool.Position - headerPosition;

            while ((destination.Position % vldPool.Align) != 0)
            {
                destination.WriteByte(0);
            }

            vldPool.Write(destination);
            header.DataPoolPosition = (uint)vldPool.Position - headerPosition;

            while ((destination.Position % vldPool.Align) != 0)
            {
                destination.WriteByte(0);
            }

            header.Length = (uint)destination.Position - headerPosition;

            header.FirstBoolean = false;
            header.SecondBoolean = false;

            destination.Position = headerPosition + 4;
            WriteUInt32(header.Length - 8);
            WriteBoolean(header.FirstBoolean);
            WriteBoolean(header.SecondBoolean);
            WriteUInt16((ushort)(header.RowsPosition - 8));
            WriteUInt32(header.StringPoolPosition - 8);
            WriteUInt32(header.DataPoolPosition - 8);
            destination.Seek(4, SeekOrigin.Current);
            WriteUInt16(header.NumberOfFields);
            WriteUInt16(header.RowLength);
            WriteUInt32(header.NumberOfRows);
            destination.Seek(0, SeekOrigin.End);
        }

        public void WriteStartFieldCollection()
        {
            if (status != Status.Start)
            {
                throw new InvalidOperationException("Attempted to start field collection when the status wasn't Start");
            }

            status = Status.FieldCollection;
        }

        public void WriteField(string fieldName, Type fieldType, object defaultValue)
        {
            if (status != Status.FieldCollection)
            {
                WriteStartFieldCollection();
            }

            CriFieldFlag fieldFlag = (CriFieldFlag)Array.IndexOf(CriField.FieldTypes, fieldType);

            if (!string.IsNullOrEmpty(fieldName))
            {
                fieldFlag |= CriFieldFlag.Name;
            }
            
            if (defaultValue != null)
            {
                fieldFlag |= CriFieldFlag.DefaultValue;
            }

            CriTableField field = new CriTableField
            {
                Flag = fieldFlag,
                Name = fieldName,
                Value = defaultValue
            };

            WriteByte((byte)field.Flag);

            if (!string.IsNullOrEmpty(fieldName))
            {
                WriteString(field.Name);
            }

            if (defaultValue != null)
            {
                WriteValue(defaultValue);
            }

            fields.Add(fieldName, field);
            header.NumberOfFields++;
        }

        public void WriteField(string fieldName, Type fieldType)
        {
            if (status != Status.FieldCollection)
            {
                WriteStartFieldCollection();
            }

            CriFieldFlag fieldFlag = (CriFieldFlag)Array.IndexOf(CriField.FieldTypes, fieldType) | CriFieldFlag.RowStorage;

            if (!string.IsNullOrEmpty(fieldName))
            {
                fieldFlag |= CriFieldFlag.Name;
            }

            CriTableField field = new CriTableField
            {
                Flag = fieldFlag,
                Name = fieldName
            };

            WriteByte((byte)field.Flag);

            if (!string.IsNullOrEmpty(fieldName))
            {
                WriteString(field.Name);
            }

            fields.Add(fieldName, field);
            header.NumberOfFields++;
        }

        public void WriteField(CriField criField)
        {
            WriteField(criField.FieldName, criField.FieldType);
        }

        public void WriteEndFieldCollection()
        {
            if (status != Status.FieldCollection)
            {
                throw new InvalidOperationException("Attempted to end field collection when the status wasn't FieldCollection");
            }

            status = Status.Idle;

            header.RowsPosition = (ushort)(destination.Position - headerPosition);
            header.RowLength = CalculateRowLength();
        }

        public void WriteStartRow()
        {
            if (status == Status.FieldCollection)
            {
                WriteEndFieldCollection();
            }

            if (status != Status.Idle)
            {
                throw new InvalidOperationException("Attempted to start row when the status wasn't Idle");
            }

            status = Status.Row;

            header.NumberOfRows++;

            destination.Position = headerPosition + header.RowsPosition + (header.NumberOfRows * header.RowLength);
            byte[] buffer = new byte[header.RowLength];
            destination.Write(buffer, 0, buffer.Length);
        }

        public void WriteValue(int fieldIndex, object rowValue)
        {
            if (!fields[fieldIndex].Flag.HasFlag(CriFieldFlag.RowStorage) || rowValue == null)
            {
                return;
            }

            GoToValue(fieldIndex);
            WriteValue(rowValue);
        }

        public void WriteValue(string fieldName, object rowValue)
        {
            WriteValue(fields.IndexOf(fieldName));
        }

        private void GoToValue(int fieldIndex)
        {
            long position = headerPosition + header.RowsPosition + (header.RowLength * (header.NumberOfRows - 1));

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

            destination.Position = position;
        }

        private ushort CalculateRowLength()
        {
            ushort length = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                if (!fields[i].Flag.HasFlag(CriFieldFlag.RowStorage))
                {
                    continue;
                }

                switch (fields[i].Flag & CriFieldFlag.TypeMask)
                {
                    case CriFieldFlag.Byte:
                    case CriFieldFlag.SByte:
                        length += 1;
                        break;
                    case CriFieldFlag.Int16:
                    case CriFieldFlag.UInt16:
                        length += 2;
                        break;
                    case CriFieldFlag.Int32:
                    case CriFieldFlag.UInt32:
                    case CriFieldFlag.Float:
                    case CriFieldFlag.String:
                        length += 4;
                        break;
                    case CriFieldFlag.Int64:
                    case CriFieldFlag.UInt64:
                    case CriFieldFlag.Double:
                    case CriFieldFlag.Data:
                        length += 8;
                        break;
                }
            }

            return length;
        }

        public void WriteEndRow()
        {
            if (status != Status.Row)
            {
                throw new InvalidOperationException("Attempted to end row when the status wasn't Row");
            }

            status = Status.Idle;
        }

        public void WriteRow(bool close, params object[] rowValues)
        {
            WriteStartRow();

            for (int i = 0; i < Math.Min(rowValues.Length, fields.Count); i++)
            {
                WriteValue(i, rowValues[i]);
            }

            if (close)
            {
                WriteEndRow();
            }
        }

        private void WriteByte(byte value)
        {
            EndianStream.WriteByte(destination, value);
        }

        private void WriteBoolean(bool value)
        {
            EndianStream.WriteBoolean(destination, value);
        }

        private void WriteSByte(sbyte value)
        {
            EndianStream.WriteSByte(destination, value);
        }

        private void WriteUInt16(ushort value)
        {
            EndianStream.WriteUInt16BE(destination, value);
        }

        private void WriteInt16(short value)
        {
            EndianStream.WriteInt16BE(destination, value);
        }

        private void WriteUInt32(uint value)
        {
            EndianStream.WriteUInt32BE(destination, value);
        }

        private void WriteInt32(int value)
        {
            EndianStream.WriteInt32BE(destination, value);
        }

        private void WriteUInt64(ulong value)
        {
            EndianStream.WriteUInt64BE(destination, value);
        }

        private void WriteInt64(long value)
        {
            EndianStream.WriteInt64BE(destination, value);
        }

        private void WriteFloat(float value)
        {
            EndianStream.WriteFloatBE(destination, value);
        }

        private void WriteDouble(double value)
        {
            EndianStream.WriteDoubleBE(destination, value);
        }

        private void WriteString(string value)
        {
            if (settings.RemoveDuplicateStrings && stringPool.ContainsString(value))
            {
                WriteUInt32((uint)stringPool.GetStringPosition(value));
            }

            else
            {
                WriteUInt32((uint)stringPool.Put(value));
            }
        }

        private void WriteData(byte[] data)
        {
            WriteUInt32((uint)vldPool.Put(data));
            WriteUInt32((uint)data.Length);
        }

        private void WriteStream(Stream stream)
        {
            WriteUInt32((uint)vldPool.Put(stream));
            WriteUInt32((uint)stream.Length);
        }

        private void WriteFile(FileInfo fileInfo)
        {
            WriteUInt32((uint)vldPool.Put(fileInfo));
            WriteUInt32((uint)fileInfo.Length);
        }

        private void WriteModule(ModuleBase module)
        {
            WriteUInt32((uint)vldPool.Put(module));
            WriteUInt32((uint)module.CalculateLength());
        }

        private void WriteGuid(Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            destination.Write(buffer, 0, buffer.Length);
        }

        private void WriteValue(object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is byte)
            {
                WriteByte((byte)value);
            }

            else if (value is sbyte)
            {
                WriteSByte((sbyte)value);
            }

            else if (value is ushort)
            {
                WriteUInt16((ushort)value);
            }

            else if (value is short)
            {
                WriteInt16((short)value);
            }

            else if (value is uint)
            {
                WriteUInt32((uint)value);
            }

            else if (value is int)
            {
                WriteInt32((int)value);
            }

            else if (value is ulong)
            {
                WriteUInt64((ulong)value);
            }

            else if (value is long)
            {
                WriteInt64((long)value);
            }

            else if (value is float)
            {
                WriteFloat((float)value);
            }

            else if (value is double)
            {
                WriteDouble((double)value);
            }

            else if (value is string)
            {
                WriteString((string)value);
            }

            else if (value is byte[])
            {
                WriteData((byte[])value);
            }

            else if (value is Stream)
            {
                WriteStream((Stream)value);
            }

            else if (value is FileInfo)
            {
                WriteFile((FileInfo)value);
            }

            else if (value is ModuleBase)
            {
                WriteModule((ModuleBase)value);
            }

            else if (value is Guid)
            {
                WriteGuid((Guid)value);
            }
        }

        public void Dispose()
        {
            fields.Clear();
            stringPool.Clear();
            vldPool.Clear();

            if (!settings.LeaveOpen)
            {
                destination.Close();
            }
        }

        public static CriTableWriter Create(string destinationFileName)
        {
            return Create(destinationFileName, new CriTableWriterSettings());
        }

        public static CriTableWriter Create(string destinationFileName, CriTableWriterSettings settings)
        {
            Stream destination = File.Create(destinationFileName);
            return new CriTableWriter(destination, settings);
        }

        public static CriTableWriter Create(Stream destination)
        {
            return new CriTableWriter(destination, new CriTableWriterSettings());
        }

        public static CriTableWriter Create(Stream destination, CriTableWriterSettings settings)
        {
            return new CriTableWriter(destination, settings);
        }

        private CriTableWriter(Stream destination, CriTableWriterSettings settings)
        {
            this.destination = destination;
            this.settings = settings;

            header = new CriTableHeader();
            fields = new OrderedDictionary<string, CriTableField>();
            stringPool = new StringPool(settings.EncodingType);
            vldPool = new VldPool(settings.Align);
        }
    }

    public class CriTableWriterSettings
    {
        private uint align = 1;
        private bool putBlankString = true;
        private bool leaveOpen = false;
        private Encoding encodingType = Encoding.GetEncoding("shift-jis");
        private bool removeDuplicateStrings = true;

        public uint Align
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

        public bool PutBlankString
        {
            get
            {
                return putBlankString;
            }

            set
            {
                putBlankString = value;
            }
        }

        public bool LeaveOpen
        {
            get
            {
                return leaveOpen;
            }

            set
            {
                leaveOpen = true;
            }
        }

        public Encoding EncodingType
        {
            get
            {
                return encodingType;
            }

            set
            {
                encodingType = value;
            }
        }

        public bool RemoveDuplicateStrings
        {
            get
            {
                return removeDuplicateStrings;
            }

            set
            {
                removeDuplicateStrings = value;
            }
        }

        public static CriTableWriterSettings AdxSettings
        {
            get
            {
                return new CriTableWriterSettings()
                {
                    Align = 4,
                    PutBlankString = true,
                    RemoveDuplicateStrings = true,
                };
            }
        }

        public static CriTableWriterSettings Adx2Settings
        {
            get
            {
                return new CriTableWriterSettings()
                {
                    Align = 32,
                    PutBlankString = false,
                    RemoveDuplicateStrings = false,
                };
            }
        }
    }
}
