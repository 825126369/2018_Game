namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;

    [Serializable, ProtoContract(Name="struct_serverList")]
    public class struct_serverList : IExtensible
    {
        private int _serverId = 0;
        private string _serverName = "";
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [ProtoMember(2, IsRequired=false, Name="serverId", DataFormat=DataFormat.TwosComplement), DefaultValue(0)]
        public int serverId
        {
            get
            {
                return this._serverId;
            }
            set
            {
                this._serverId = value;
            }
        }

        [DefaultValue(""), ProtoMember(1, IsRequired=false, Name="serverName", DataFormat=DataFormat.Default)]
        public string serverName
        {
            get
            {
                return this._serverName;
            }
            set
            {
                this._serverName = value;
            }
        }
    }
}

