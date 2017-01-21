namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public sealed class ProtoEnumAttribute : Attribute
    {
        private int enumValue;
        private bool hasValue;
        private string name;

        public bool HasValue()
        {
            return this.hasValue;
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

        public int Value
        {
            get
            {
                return this.enumValue;
            }
            set
            {
                this.enumValue = value;
                this.hasValue = true;
            }
        }
    }
}

