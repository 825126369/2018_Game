namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public sealed class ProtoIncludeAttribute : Attribute
    {
        private ProtoBuf.DataFormat dataFormat;
        private readonly string knownTypeName;
        private readonly int tag;

        public ProtoIncludeAttribute(int tag, string knownTypeName)
        {
            this.dataFormat = ProtoBuf.DataFormat.Default;
            if (tag <= 0)
            {
                throw new ArgumentOutOfRangeException("tag", "Tags must be positive integers");
            }
            if (Helpers.IsNullOrEmpty(knownTypeName))
            {
                throw new ArgumentNullException("knownTypeName", "Known type cannot be blank");
            }
            this.tag = tag;
            this.knownTypeName = knownTypeName;
        }

        public ProtoIncludeAttribute(int tag, Type knownType) : this(tag, (knownType == null) ? "" : knownType.AssemblyQualifiedName)
        {
        }

        [DefaultValue(0)]
        public ProtoBuf.DataFormat DataFormat
        {
            get
            {
                return this.dataFormat;
            }
            set
            {
                this.dataFormat = value;
            }
        }

        public Type KnownType
        {
            get
            {
                return TypeModel.ResolveKnownType(this.KnownTypeName, null, null);
            }
        }

        public string KnownTypeName
        {
            get
            {
                return this.knownTypeName;
            }
        }

        public int Tag
        {
            get
            {
                return this.tag;
            }
        }
    }
}

