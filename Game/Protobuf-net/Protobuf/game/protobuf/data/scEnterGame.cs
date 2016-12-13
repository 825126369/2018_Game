namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;

    [Serializable, ProtoContract(Name="scEnterGame")]
    public class scEnterGame : IExtensible
    {
        private int _ServerId = 0;
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [ProtoMember(1, IsRequired=false, Name="ServerId", DataFormat=DataFormat.TwosComplement), DefaultValue(0)]
        public int ServerId
        {
            get
            {
                return this._ServerId;
            }
            set
            {
                this._ServerId = value;
            }
        }
    }
}

