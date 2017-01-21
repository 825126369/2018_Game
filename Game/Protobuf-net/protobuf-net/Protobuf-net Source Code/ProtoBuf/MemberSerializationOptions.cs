namespace ProtoBuf
{
    using System;

    [Flags]
    public enum MemberSerializationOptions
    {
        AsReference = 4,
        AsReferenceHasValue = 0x20,
        DynamicType = 8,
        None = 0,
        OverwriteList = 0x10,
        Packed = 1,
        Required = 2
    }
}

