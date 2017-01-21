namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class ProtoContractAttribute : Attribute
    {
        private int dataMemberOffset;
        private byte flags;
        private ProtoBuf.ImplicitFields implicitFields;
        private int implicitFirstTag;
        private string name;
        private const byte OPTIONS_AsReferenceDefault = 0x20;
        private const byte OPTIONS_EnumPassthru = 0x40;
        private const byte OPTIONS_EnumPassthruHasValue = 0x80;
        private const byte OPTIONS_IgnoreListHandling = 0x10;
        private const byte OPTIONS_InferTagFromName = 1;
        private const byte OPTIONS_InferTagFromNameHasValue = 2;
        private const byte OPTIONS_SkipConstructor = 8;
        private const byte OPTIONS_UseProtoMembersOnly = 4;

        private bool HasFlag(byte flag)
        {
            return ((this.flags & flag) == flag);
        }

        private void SetFlag(byte flag, bool value)
        {
            if (value)
            {
                this.flags = (byte) (this.flags | flag);
            }
            else
            {
                this.flags = (byte) (this.flags & ~flag);
            }
        }

        public bool AsReferenceDefault
        {
            get
            {
                return this.HasFlag(0x20);
            }
            set
            {
                this.SetFlag(0x20, value);
            }
        }

        public int DataMemberOffset
        {
            get
            {
                return this.dataMemberOffset;
            }
            set
            {
                this.dataMemberOffset = value;
            }
        }

        public bool EnumPassthru
        {
            get
            {
                return this.HasFlag(0x40);
            }
            set
            {
                this.SetFlag(0x40, value);
                this.SetFlag(0x80, true);
            }
        }

        internal bool EnumPassthruHasValue
        {
            get
            {
                return this.HasFlag(0x80);
            }
        }

        public bool IgnoreListHandling
        {
            get
            {
                return this.HasFlag(0x10);
            }
            set
            {
                this.SetFlag(0x10, value);
            }
        }

        public ProtoBuf.ImplicitFields ImplicitFields
        {
            get
            {
                return this.implicitFields;
            }
            set
            {
                this.implicitFields = value;
            }
        }

        public int ImplicitFirstTag
        {
            get
            {
                return this.implicitFirstTag;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("ImplicitFirstTag");
                }
                this.implicitFirstTag = value;
            }
        }

        public bool InferTagFromName
        {
            get
            {
                return this.HasFlag(1);
            }
            set
            {
                this.SetFlag(1, value);
                this.SetFlag(2, true);
            }
        }

        internal bool InferTagFromNameHasValue
        {
            get
            {
                return this.HasFlag(2);
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public bool SkipConstructor
        {
            get
            {
                return this.HasFlag(8);
            }
            set
            {
                this.SetFlag(8, value);
            }
        }

        public bool UseProtoMembersOnly
        {
            get
            {
                return this.HasFlag(4);
            }
            set
            {
                this.SetFlag(4, value);
            }
        }
    }
}

