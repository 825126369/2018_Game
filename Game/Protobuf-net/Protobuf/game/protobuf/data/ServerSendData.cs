namespace game.protobuf.data
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;

    [Serializable, ProtoContract(Name="ServerSendData")]
    public class ServerSendData : IExtensible
    {
        private string _NickName = "";
        private string _Result = "";
        private string _TalkMsg = "";
        private string _TalkTime = "";
        private IExtension extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
        }

        [DefaultValue(""), ProtoMember(2, IsRequired=false, Name="NickName", DataFormat=DataFormat.Default)]
        public string NickName
        {
            get
            {
                return this._NickName;
            }
            set
            {
                this._NickName = value;
            }
        }

        [ProtoMember(1, IsRequired=false, Name="Result", DataFormat=DataFormat.Default), DefaultValue("")]
        public string Result
        {
            get
            {
                return this._Result;
            }
            set
            {
                this._Result = value;
            }
        }

        [ProtoMember(3, IsRequired=false, Name="TalkMsg", DataFormat=DataFormat.Default), DefaultValue("")]
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

        [DefaultValue(""), ProtoMember(4, IsRequired=false, Name="TalkTime", DataFormat=DataFormat.Default)]
        public string TalkTime
        {
            get
            {
                return this._TalkTime;
            }
            set
            {
                this._TalkTime = value;
            }
        }
    }
}

