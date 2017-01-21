namespace ProtoBuf
{
    using System;

    public enum WireType
    {
        EndGroup = 4,
        Fixed32 = 5,
        Fixed64 = 1,
        None = -1,
        SignedVariant = 8,
        StartGroup = 3,
        String = 2,
        Variant = 0
    }
}

