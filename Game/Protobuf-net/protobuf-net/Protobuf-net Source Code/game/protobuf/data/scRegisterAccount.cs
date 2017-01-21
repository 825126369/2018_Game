namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;

    [Serializable, ProtoContract(Name="scRegisterAccount")]
    public class scRegisterAccount : IExtensible
    {
        private bool _result = false;
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [DefaultValue(false), ProtoMember(1, IsRequired=false, Name="result", DataFormat=DataFormat.Default)]
        public bool result
        {
            get
            {
                return this._result;
            }
            set
            {
                this._result = value;
            }
        }
    }
}

