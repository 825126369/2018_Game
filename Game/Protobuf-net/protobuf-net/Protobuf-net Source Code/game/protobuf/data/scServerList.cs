namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;

    [Serializable, ProtoContract(Name="scServerList")]
    public class scServerList : IExtensible
    {
        private readonly List<struct_serverList> _ServerList = new List<struct_serverList>();
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [ProtoMember(1, Name="ServerList", DataFormat=DataFormat.Default)]
        public List<struct_serverList> ServerList
        {
            get
            {
                return this._ServerList;
            }
        }
    }
}

