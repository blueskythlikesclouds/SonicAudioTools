using System;

namespace SonicAudioLib.CriMw
{
    struct CriTableHeader
    {
        public static readonly string Signature = "@UTF";
        public uint Length { get; set; }
        public bool FirstBoolean { get; set; }
        public bool SecondBoolean { get; set; }
        public ushort RowsPosition { get; set; }
        public uint StringPoolPosition { get; set; }
        public uint DataPoolPosition { get; set; }
        public string TableName { get; set; }
        public ushort NumberOfFields { get; set; }
        public ushort RowLength { get; set; }
        public uint NumberOfRows { get; set; }
    }

    [Flags]
    enum CriFieldFlag
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
        Float = 8,
        Double = 9,
        String = 10,
        Data = 11,
        Guid = 12,

        TypeMask = 15,
    };

    struct CriTableField
    {
        public CriFieldFlag Flag { get; set; }
        public string Name { get; set; }
        public uint Position { get; set; }
        public uint Length { get; set; }
        public object Value { get; set; }
    }
}
