namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;

    [Serializable, ProtoContract(Name="ClientSendData")]
    public class ClientSendData : IExtensible
    {
        private string _SenderName = "";
        private string _TalkMsg = ""; 
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [DefaultValue(""), ProtoMember(1, IsRequired=false, Name="SenderName", DataFormat=DataFormat.Default)]
        public string SenderName
        {
            get
            {
                return this._SenderName;
            }
            set
            {
                this._SenderName = value;
            }
        }

        [ProtoMember(2, IsRequired=false, Name="TalkMsg", DataFormat=DataFormat.Default), DefaultValue("")]
        public string TalkMsg
        {
            get
            {
                return this._TalkMsg;
            }
            set
            {
                this._TalkMsg = value;
            }
        }
    }
}

