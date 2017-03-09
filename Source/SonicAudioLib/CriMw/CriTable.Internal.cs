using System;

namespace SonicAudioLib.CriMw
{
    struct CriTableHeader
    {
        public const string Signature = "@UTF";
        public const byte EncodingTypeShiftJis = 0;
        public const byte EncodingTypeUtf8 = 1;

        public uint Length;
        public byte UnknownByte;
        public byte EncodingType;
        public ushort RowsPosition;
        public uint StringPoolPosition;
        public uint DataPoolPosition;
        public string TableName;
        public ushort NumberOfFields;
        public ushort RowLength;
        public uint NumberOfRows;
    }

    [Flags]
    enum CriFieldFlag : byte
    {
        Name = 16,
        DefaultValue = 32,
        RowStorage = 64,
        
        Byte = 0,
        SByte = 1,
        UInt16 = 2,
        Int16 = 3,
        UInt32 = 4,
        Int32 = 5,
        UInt64 = 6,
        Int64 = 7,
        Single = 8,
        Double = 9,
        String = 10,
        Data = 11,
        Guid = 12,

        TypeMask = 15,
    };

    struct CriTableField
    {
        public CriFieldFlag Flag;
        public string Name;
        public uint Position;
        public uint Length;
        public object Value;
    }
}
