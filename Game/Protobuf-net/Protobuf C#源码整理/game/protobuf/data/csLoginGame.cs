namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;

    [Serializable, ProtoContract(Name="csLoginGame")]
    public class csLoginGame : IExtensible
    {
        private string _accountName = "";
        private string _password = "";
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [DefaultValue(""), ProtoMember(1, IsRequired=false, Name="accountName", DataFormat=DataFormat.Default)]
        public string accountName
        {
            get
            {
                return this._accountName;
            }
            set
            {
                this._accountName = value;
            }
        }

        [DefaultValue(""), ProtoMember(2, IsRequired=false, Name="password", DataFormat=DataFormat.Default)]
        public string password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }
    }
}

