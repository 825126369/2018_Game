using xk_System.Model;
using game.protobuf.data;
using xk_System.Net;

public class ChatMessage : NetModel
{
    public DataBind<ServerSendData> mBindServerSendData = new DataBind<ServerSendData>();
    public override void initModel()
    {
        base.initModel();
        addNetListenFun(ProtoCommand.Chat, Receive_ServerSenddata);
    }

    public override void destroyModel()
    {
        base.destroyModel();
        removeNetListenFun(ProtoCommand.Chat, Receive_ServerSenddata);
    }

    public void request_ClientSendData(string sendName, string content)
    {
        ClientSendData mClientSendData = new ClientSendData();
        mClientSendData.SenderName = sendName;
        mClientSendData.TalkMsg = content;
        sendNetData(ProtoCommand.Chat, mClientSendData);
    }

    void Receive_ServerSenddata(Package package)
    {
        ServerSendData mServerSendData = null;
        package.getData<ServerSendData>(mServerSendData);
        mBindServerSendData.HandleData(mServerSendData);
    }

}
