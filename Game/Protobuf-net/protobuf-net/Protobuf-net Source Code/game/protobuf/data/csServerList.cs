namespace game.protobuf.data
{
    using ProtoBuf;
    using System;

    [Serializable, ProtoContract(Name="csServerList")]
    public class csServerList : IExtensible
    {
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }
    }
}

