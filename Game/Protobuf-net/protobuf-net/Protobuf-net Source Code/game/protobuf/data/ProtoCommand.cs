namespace game.protobuf.data
{
    using ProtoBuf;
    using System;

    [ProtoContract(Name="ProtoCommand")]
    public enum ProtoCommand
    {
        [ProtoEnum(Name="Chat", Value=0x44c)]
        Chat = 0x44c,
        [ProtoEnum(Name="EnterGame", Value=0x450)]
        EnterGame = 0x450,
        [ProtoEnum(Name="Login", Value=0x44e)]
        Login = 0x44e,
        [ProtoEnum(Name="RegisterAccount", Value=0x44d)]
        RegisterAccount = 0x44d,
        [ProtoEnum(Name="SelectServer", Value=0x44f)]
        SelectServer = 0x44f
    }
}

