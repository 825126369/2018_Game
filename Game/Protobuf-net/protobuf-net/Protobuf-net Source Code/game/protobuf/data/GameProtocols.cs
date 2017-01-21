using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections;
namespace game.protobuf.data
{
    public sealed class GameProtocols : TypeModel
    {
        //
        // Static Fields
        //
        private static readonly Type[] knownTypes = new Type[]
        {
            typeof(ClientSendData),
            typeof(csLoginGame),
            typeof(csRegisterAccount),
            typeof(ProtoCommand),
            typeof(scLoginGame),
            typeof(scRegisterAccount),
            typeof(ServerSendData)
        };

        //
        // Static Methods
        //
        static object _3(object obj, ProtoReader protoReader)
        {
            if (obj != null)
            {
                return GameProtocols.ReadProtoCommand((ProtoCommand)obj, protoReader);
            }
            return GameProtocols.ReadProtoCommand((ProtoCommand)0, protoReader);
        }

        private static ClientSendData ReadClientSendData(ClientSendData clientSendData, ProtoReader protoReader)
        {
            int num;
            while ((num = protoReader.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    if (num != 2)
                    {
                        if (clientSendData == null)
                        {
                            ClientSendData expr_76 = new ClientSendData();
                            ProtoReader.NoteObject(expr_76, protoReader);
                            clientSendData = expr_76;
                        }
                        protoReader.AppendExtensionData(clientSendData);
                    }
                    else
                    {
                        if (clientSendData == null)
                        {
                            ClientSendData expr_4C = new ClientSendData();
                            ProtoReader.NoteObject(expr_4C, protoReader);
                            clientSendData = expr_4C;
                        }
                        string text = protoReader.ReadString();
                        if (text != null)
                        {
                            clientSendData.TalkMsg = text;
                        }
                    }
                }
                else
                {
                    if (clientSendData == null)
                    {
                        CreateNewObjectCout++;
                        Console.WriteLine("client Send Data is NUll");
                        ClientSendData expr_19 = new ClientSendData();
                        ProtoReader.NoteObject(expr_19, protoReader);
                        clientSendData = expr_19;
                    }
                    string text = protoReader.ReadString();
                    if (text != null)
                    {
                        clientSendData.SenderName = text;
                    }
                }
            }
            if (clientSendData == null)
            {
                ClientSendData expr_9F = new ClientSendData();
                ProtoReader.NoteObject(expr_9F, protoReader);
                clientSendData = expr_9F;
            }
            return clientSendData;
        }

        public static int CreateNewObjectCout=0;
        private static csLoginGame ReadcsLoginGame(csLoginGame csLoginGame, ProtoReader protoReader)
        {
            int num;
            while ((num = protoReader.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    if (num != 2)
                    {
                        if (csLoginGame == null)
                        {
                            csLoginGame expr_76 = new csLoginGame();
                            ProtoReader.NoteObject(expr_76, protoReader);
                            csLoginGame = expr_76;
                        }
                        protoReader.AppendExtensionData(csLoginGame);
                    }
                    else
                    {
                        if (csLoginGame == null)
                        {
                            csLoginGame expr_4C = new csLoginGame();
                            ProtoReader.NoteObject(expr_4C, protoReader);
                            csLoginGame = expr_4C;
                        }
                        string text = protoReader.ReadString();
                        if (text != null)
                        {
                            csLoginGame.password = text;
                        }
                    }
                }
                else
                {
                    if (csLoginGame == null)
                    {
                        csLoginGame expr_19 = new csLoginGame();
                        ProtoReader.NoteObject(expr_19, protoReader);
                        csLoginGame = expr_19;
                    }
                    string text = protoReader.ReadString();
                    if (text != null)
                    {
                        csLoginGame.accountName = text;
                    }
                }
            }
            if (csLoginGame == null)
            {
                csLoginGame expr_9F = new csLoginGame();
                ProtoReader.NoteObject(expr_9F, protoReader);
                csLoginGame = expr_9F;
            }
            return csLoginGame;
        }

        private static csRegisterAccount ReadcsRegisterAccount(csRegisterAccount csRegisterAccount, ProtoReader protoReader)
        {
            int num;
            while ((num = protoReader.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    if (num != 2)
                    {
                        if (num != 3)
                        {
                            if (csRegisterAccount == null)
                            {
                                csRegisterAccount expr_A9 = new csRegisterAccount();
                                ProtoReader.NoteObject(expr_A9, protoReader);
                                csRegisterAccount = expr_A9;
                            }
                            protoReader.AppendExtensionData(csRegisterAccount);
                        }
                        else
                        {
                            if (csRegisterAccount == null)
                            {
                                csRegisterAccount expr_7F = new csRegisterAccount();
                                ProtoReader.NoteObject(expr_7F, protoReader);
                                csRegisterAccount = expr_7F;
                            }
                            string text = protoReader.ReadString();
                            if (text != null)
                            {
                                csRegisterAccount.repeatPassword = text;
                            }
                        }
                    }
                    else
                    {
                        if (csRegisterAccount == null)
                        {
                            csRegisterAccount expr_4C = new csRegisterAccount();
                            ProtoReader.NoteObject(expr_4C, protoReader);
                            csRegisterAccount = expr_4C;
                        }
                        string text = protoReader.ReadString();
                        if (text != null)
                        {
                            csRegisterAccount.password = text;
                        }
                    }
                }
                else
                {
                    if (csRegisterAccount == null)
                    {
                        csRegisterAccount expr_19 = new csRegisterAccount();
                        ProtoReader.NoteObject(expr_19, protoReader);
                        csRegisterAccount = expr_19;
                    }
                    string text = protoReader.ReadString();
                    if (text != null)
                    {
                        csRegisterAccount.accountName = text;
                    }
                }
            }
            if (csRegisterAccount == null)
            {
                csRegisterAccount expr_D2 = new csRegisterAccount();
                ProtoReader.NoteObject(expr_D2, protoReader);
                csRegisterAccount = expr_D2;
            }
            return csRegisterAccount;
        }

        private static ProtoCommand ReadProtoCommand(ProtoCommand protoCommand, ProtoReader protoReader)
        {
            int num = protoReader.ReadInt32();
            ProtoCommand result=ProtoCommand.Chat;
            if (num != 1100)
            {
                if (num != 1101)
                {
                    if (num != 1102)
                    {
                        protoReader.ThrowEnumException(typeof(ProtoCommand), num);
                    }
                    else
                    {
                        result = ProtoCommand.Login;
                    }
                }
                else
                {
                    result = ProtoCommand.RegisterAccount;
                }
            }
            else
            {
                result = ProtoCommand.Chat;
            }
            return result;
        }

        private static scLoginGame ReadscLoginGame(scLoginGame scLoginGame, ProtoReader protoReader)
        {
            int num;
            while ((num = protoReader.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    if (scLoginGame == null)
                    {
                        scLoginGame expr_40 = new scLoginGame();
                        ProtoReader.NoteObject(expr_40, protoReader);
                        scLoginGame = expr_40;
                    }
                    protoReader.AppendExtensionData(scLoginGame);
                }
                else
                {
                    if (scLoginGame == null)
                    {
                        scLoginGame expr_19 = new scLoginGame();
                        ProtoReader.NoteObject(expr_19, protoReader);
                        scLoginGame = expr_19;
                    }
                    bool result = protoReader.ReadBoolean();
                    scLoginGame.result = result;
                }
            }
            if (scLoginGame == null)
            {
                scLoginGame expr_69 = new scLoginGame();
                ProtoReader.NoteObject(expr_69, protoReader);
                scLoginGame = expr_69;
            }
            return scLoginGame;
        }

        private static scRegisterAccount ReadscRegisterAccount(scRegisterAccount scRegisterAccount, ProtoReader protoReader)
        {
            int num;
            while ((num = protoReader.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    if (scRegisterAccount == null)
                    {
                        scRegisterAccount expr_40 = new scRegisterAccount();
                        ProtoReader.NoteObject(expr_40, protoReader);
                        scRegisterAccount = expr_40;
                    }
                    protoReader.AppendExtensionData(scRegisterAccount);
                }
                else
                {
                    if (scRegisterAccount == null)
                    {
                        scRegisterAccount expr_19 = new scRegisterAccount();
                        ProtoReader.NoteObject(expr_19, protoReader);
                        scRegisterAccount = expr_19;
                    }
                    bool result = protoReader.ReadBoolean();
                    scRegisterAccount.result = result;
                }
            }
            if (scRegisterAccount == null)
            {
                scRegisterAccount expr_69 = new scRegisterAccount();
                ProtoReader.NoteObject(expr_69, protoReader);
                scRegisterAccount = expr_69;
            }
            return scRegisterAccount;
        }

        private static ServerSendData ReadServerSendData(ServerSendData serverSendData, ProtoReader protoReader)
        {
            int num;
            while ((num = protoReader.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    if (num != 2)
                    {
                        if (num != 3)
                        {
                            if (num != 4)
                            {
                                if (serverSendData == null)
                                {
                                    ServerSendData expr_DC = new ServerSendData();
                                    ProtoReader.NoteObject(expr_DC, protoReader);
                                    serverSendData = expr_DC;
                                }
                                protoReader.AppendExtensionData(serverSendData);
                            }
                            else
                            {
                                if (serverSendData == null)
                                {
                                    ServerSendData expr_B2 = new ServerSendData();
                                    ProtoReader.NoteObject(expr_B2, protoReader);
                                    serverSendData = expr_B2;
                                }
                                string text = protoReader.ReadString();
                                if (text != null)
                                {
                                    serverSendData.TalkTime = text;
                                }
                            }
                        }
                        else
                        {
                            if (serverSendData == null)
                            {
                                ServerSendData expr_7F = new ServerSendData();
                                ProtoReader.NoteObject(expr_7F, protoReader);
                                serverSendData = expr_7F;
                            }
                            string text = protoReader.ReadString();
                            if (text != null)
                            {
                                serverSendData.TalkMsg = text;
                            }
                        }
                    }
                    else
                    {
                        if (serverSendData == null)
                        {
                            ServerSendData expr_4C = new ServerSendData();
                            ProtoReader.NoteObject(expr_4C, protoReader);
                            serverSendData = expr_4C;
                        }
                        string text = protoReader.ReadString();
                        if (text != null)
                        {
                            serverSendData.NickName = text;
                        }
                    }
                }
                else
                {
                    if (serverSendData == null)
                    {
                        ServerSendData expr_19 = new ServerSendData();
                        ProtoReader.NoteObject(expr_19, protoReader);
                        serverSendData = expr_19;
                    }
                    string text = protoReader.ReadString();
                    if (text != null)
                    {
                        serverSendData.Result = text;
                    }
                }
            }
            if (serverSendData == null)
            {
                ServerSendData expr_105 = new ServerSendData();
                ProtoReader.NoteObject(expr_105, protoReader);
                serverSendData = expr_105;
            }
            return serverSendData;
        }

        private static void WriteClientSendData(ClientSendData clientSendData, ProtoWriter writer)
        {
            if (clientSendData.GetType() != typeof(ClientSendData))
            {
                TypeModel.ThrowUnexpectedSubtype(typeof(ClientSendData), clientSendData.GetType());
            }
            string expr_2D = clientSendData.SenderName;
            if (expr_2D != null)
            {
                if (!(expr_2D == ""))
                {
                    ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
                    ProtoWriter.WriteString(expr_2D, writer);
                }
            }
            string expr_5D = clientSendData.TalkMsg;
            if (expr_5D != null)
            {
                if (!(expr_5D == ""))
                {
                    ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                    ProtoWriter.WriteString(expr_5D, writer);
                }
            }
            ProtoWriter.AppendExtensionData(clientSendData, writer);
        }

        private static void WritecsLoginGame(csLoginGame csLoginGame, ProtoWriter writer)
        {
            if (csLoginGame.GetType() != typeof(csLoginGame))
            {
                TypeModel.ThrowUnexpectedSubtype(typeof(csLoginGame), csLoginGame.GetType());
            }
            string expr_2D = csLoginGame.accountName;
            if (expr_2D != null)
            {
                if (!(expr_2D == ""))
                {
                    ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
                    ProtoWriter.WriteString(expr_2D, writer);
                }
            }
            string expr_5D = csLoginGame.password;
            if (expr_5D != null)
            {
                if (!(expr_5D == ""))
                {
                    ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                    ProtoWriter.WriteString(expr_5D, writer);
                }
            }
            ProtoWriter.AppendExtensionData(csLoginGame, writer);
        }

        private static void WritecsRegisterAccount(csRegisterAccount csRegisterAccount, ProtoWriter writer)
        {
            if (csRegisterAccount.GetType() != typeof(csRegisterAccount))
            {
                TypeModel.ThrowUnexpectedSubtype(typeof(csRegisterAccount), csRegisterAccount.GetType());
            }
            string expr_2D = csRegisterAccount.accountName;
            if (expr_2D != null)
            {
                if (!(expr_2D == ""))
                {
                    ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
                    ProtoWriter.WriteString(expr_2D, writer);
                }
            }
            string expr_5D = csRegisterAccount.password;
            if (expr_5D != null)
            {
                if (!(expr_5D == ""))
                {
                    ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                    ProtoWriter.WriteString(expr_5D, writer);
                }
            }
            string expr_8D = csRegisterAccount.repeatPassword;
            if (expr_8D != null)
            {
                if (!(expr_8D == ""))
                {
                    ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
                    ProtoWriter.WriteString(expr_8D, writer);
                }
            }
            ProtoWriter.AppendExtensionData(csRegisterAccount, writer);
        }

        private static void WriteProtoCommand(ProtoCommand protoCommand, ProtoWriter writer)
        {
            ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
            if (protoCommand != ProtoCommand.Chat)
            {
                if (protoCommand != ProtoCommand.RegisterAccount)
                {
                    if (protoCommand != ProtoCommand.Login)
                    {
                        ProtoWriter.ThrowEnumException(writer, protoCommand);
                    }
                    else
                    {
                        ProtoWriter.WriteInt32(1102, writer);
                    }
                }
                else
                {
                    ProtoWriter.WriteInt32(1101, writer);
                }
            }
            else
            {
                ProtoWriter.WriteInt32(1100, writer);
            }
        }

        private static void WritescLoginGame(scLoginGame scLoginGame, ProtoWriter writer)
        {
            if (scLoginGame.GetType() != typeof(scLoginGame))
            {
                TypeModel.ThrowUnexpectedSubtype(typeof(scLoginGame), scLoginGame.GetType());
            }
            bool expr_2D = scLoginGame.result;
            if (expr_2D)
            {
                ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
                ProtoWriter.WriteBoolean(expr_2D, writer);
            }
            ProtoWriter.AppendExtensionData(scLoginGame, writer);
        }

        private static void WritescRegisterAccount(scRegisterAccount scRegisterAccount, ProtoWriter writer)
        {
            if (scRegisterAccount.GetType() != typeof(scRegisterAccount))
            {
                TypeModel.ThrowUnexpectedSubtype(typeof(scRegisterAccount), scRegisterAccount.GetType());
            }
            bool expr_2D = scRegisterAccount.result;
            if (expr_2D)
            {
                ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
                ProtoWriter.WriteBoolean(expr_2D, writer);
            }
            ProtoWriter.AppendExtensionData(scRegisterAccount, writer);
        }

        private static void WriteServerSendData(ServerSendData serverSendData, ProtoWriter writer)
        {
            if (serverSendData.GetType() != typeof(ServerSendData))
            {
                TypeModel.ThrowUnexpectedSubtype(typeof(ServerSendData), serverSendData.GetType());
            }
            string expr_2D = serverSendData.Result;
            if (expr_2D != null)
            {
                if (!(expr_2D == ""))
                {
                    ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
                    ProtoWriter.WriteString(expr_2D, writer);
                }
            }
            string expr_5D = serverSendData.NickName;
            if (expr_5D != null)
            {
                if (!(expr_5D == ""))
                {
                    ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                    ProtoWriter.WriteString(expr_5D, writer);
                }
            }
            string expr_8D = serverSendData.TalkMsg;
            if (expr_8D != null)
            {
                if (!(expr_8D == ""))
                {
                    ProtoWriter.WriteFieldHeader(3, WireType.String, writer);
                    ProtoWriter.WriteString(expr_8D, writer);
                }
            }
            string expr_BD = serverSendData.TalkTime;
            if (expr_BD != null)
            {
                if (!(expr_BD == ""))
                {
                    ProtoWriter.WriteFieldHeader(4, WireType.String, writer);
                    ProtoWriter.WriteString(expr_BD, writer);
                }
            }
            ProtoWriter.AppendExtensionData(serverSendData, writer);
        }

        //
        // Methods
        //
        protected override  object Deserialize(int num, object obj, ProtoReader protoReader)
        {
            switch (num)
            {
                case 0:
                    return GameProtocols.ReadClientSendData((ClientSendData)obj, protoReader);
                case 1:
                    return GameProtocols.ReadcsLoginGame((csLoginGame)obj, protoReader);
                case 2:
                    return GameProtocols.ReadcsRegisterAccount((csRegisterAccount)obj, protoReader);
                case 3:
                    return GameProtocols._3(obj, protoReader);
                case 4:
                    return GameProtocols.ReadscLoginGame((scLoginGame)obj, protoReader);
                case 5:
                    return GameProtocols.ReadscRegisterAccount((scRegisterAccount)obj, protoReader);
                case 6:
                    return GameProtocols.ReadServerSendData((ServerSendData)obj, protoReader);
                default:
                    return null;
            }
        }

        protected sealed override int GetKeyImpl(Type value)
        {
            Console.WriteLine("GameProtoCols: GetKey Impl: "+value);
            return ((IList)GameProtocols.knownTypes).IndexOf(value);
        }

        protected  override void Serialize(int num, object obj, ProtoWriter protoWriter)
        {
            switch (num)
            {
                case 0:
                    GameProtocols.WriteClientSendData((ClientSendData)obj, protoWriter);
                    return;
                case 1:
                    GameProtocols.WritecsLoginGame((csLoginGame)obj, protoWriter);
                    return;
                case 2:
                    GameProtocols.WritecsRegisterAccount((csRegisterAccount)obj, protoWriter);
                    return;
                case 3:
                    GameProtocols.WriteProtoCommand((ProtoCommand)obj, protoWriter);
                    return;
                case 4:
                    GameProtocols.WritescLoginGame((scLoginGame)obj, protoWriter);
                    return;
                case 5:
                    GameProtocols.WritescRegisterAccount((scRegisterAccount)obj, protoWriter);
                    return;
                case 6:
                    GameProtocols.WriteServerSendData((ServerSendData)obj, protoWriter);
                    return;
                default:
                    return;
            }
        }
    }
}
