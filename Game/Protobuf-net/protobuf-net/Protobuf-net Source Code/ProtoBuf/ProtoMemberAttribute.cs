namespace ProtoBuf
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class ProtoMemberAttribute : Attribute, IComparable, IComparable<ProtoMemberAttribute>
    {
        private ProtoBuf.DataFormat dataFormat;
        internal MemberInfo Member;
        private string name;
        private MemberSerializationOptions options;
        private int tag;
        internal bool TagIsPinned;

        public ProtoMemberAttribute(int tag) : this(tag, false)
        {
        }

        internal ProtoMemberAttribute(int tag, bool forced)
        {
            if ((tag <= 0) && !forced)
            {
                throw new ArgumentOutOfRangeException("tag");
            }
            this.tag = tag;
        }

        public int CompareTo(ProtoMemberAttribute other)
        {
            if (other == null)
            {
                return -1;
            }
            if (this == other)
            {
                return 0;
            }
            int num = this.tag.CompareTo(other.tag);
            if (num == 0)
            {
                num = string.CompareOrdinal(this.name, other.name);
            }
            return num;
        }

        public int CompareTo(object other)
        {
            return this.CompareTo(other as ProtoMemberAttribute);
        }

        internal void Rebase(int tag)
        {
            this.tag = tag;
        }

        public bool AsReference
        {
            get
            {
                return ((this.options & MemberSerializationOptions.AsReference) == MemberSerializationOptions.AsReference);
            }
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.AsReference;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.AsReference;
                }
                this.options |= MemberSerializationOptions.AsReferenceHasValue;
            }
        }

        internal bool AsReferenceHasValue
        {
            get
            {
                return ((this.options & MemberSerializationOptions.AsReferenceHasValue) == MemberSerializationOptions.AsReferenceHasValue);
            }
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.AsReferenceHasValue;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.AsReferenceHasValue;
                }
            }
        }

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

        public bool DynamicType
        {
            get
            {
                return ((this.options & MemberSerializationOptions.DynamicType) == MemberSerializationOptions.DynamicType);
            }
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.DynamicType;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.DynamicType;
                }
            }
        }

        public bool IsPacked
        {
            get
            {
                return ((this.options & MemberSerializationOptions.Packed) == MemberSerializationOptions.Packed);
            }
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.Packed;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.Packed;
                }
            }
        }

        public bool IsRequired
        {
            get
            {
                return ((this.options & MemberSerializationOptions.Required) == MemberSerializationOptions.Required);
            }
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.Required;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.Required;
                }
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

        public MemberSerializationOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
            }
        }

        public bool OverwriteList
        {
            get
            {
                return ((this.options & MemberSerializationOptions.OverwriteList) == MemberSerializationOptions.OverwriteList);
            }
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.OverwriteList;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.OverwriteList;
                }
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

