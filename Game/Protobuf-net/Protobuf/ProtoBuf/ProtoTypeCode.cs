namespace ProtoBuf
{
    using System;

    internal enum ProtoTypeCode
    {
        Boolean = 3,
        Byte = 6,
        ByteArray = 0x65,
        Char = 4,
        DateTime = 0x10,
        Decimal = 15,
        Double = 14,
        Empty = 0,
        Guid = 0x66,
        Int16 = 7,
        Int32 = 9,
        Int64 = 11,
        SByte = 5,
        Single = 13,
        String = 0x12,
        TimeSpan = 100,
        Type = 0x68,
        UInt16 = 8,
        UInt32 = 10,
        UInt64 = 12,
        Unknown = 1,
        Uri = 0x67
    }
}

