using xk_System.Model;
using game.protobuf.data;
using xk_System.Net;
using xk_System.Debug;

public class ChatMessage : NetModel
{
    public DataBind<struct_ChatInfo> mBindServerSendData = new DataBind<struct_ChatInfo>();
    public override void initModel()
    {
        base.initModel();
        addNetListenFun(ProtoCommand.PROTO_CHAT, Receive_ServerSenddata);
        addNetListenFun(ProtoCommand.PROTO_PUSH_CHATINFO, Receive_Push_ChatInfo);
    }

    public override void destroyModel()
    {
        base.destroyModel();
        removeNetListenFun(ProtoCommand.PROTO_CHAT, Receive_ServerSenddata);
        removeNetListenFun(ProtoCommand.PROTO_PUSH_CHATINFO, Receive_Push_ChatInfo);
    }

    public void request_ClientSendData(uint channelId,string sendName, string content)
    {
        csChatData mClientSendData = new  csChatData();
        mClientSendData.channelId = channelId;
        mClientSendData.talkMsg = content;
        sendNetData(ProtoCommand.PROTO_CHAT, mClientSendData);
    }

    private void Receive_ServerSenddata(Package package)
    {
        scChatData mServerSendData = new scChatData();
        package.getData<scChatData>(mServerSendData);
        if (mServerSendData.result==1 && mServerSendData.chatInfo!=null)
        {
            mBindServerSendData.HandleData(mServerSendData.chatInfo);
        }else
        {
            DebugSystem.LogError("chat error:"+mServerSendData.result);
        }
    }

    private void Receive_Push_ChatInfo(Package mPackage)
    {
        pushChatInfo mPushChatInfo = new pushChatInfo();
        mPackage.getData<pushChatInfo>(mPushChatInfo);
        if(mPushChatInfo.result==1 && mPushChatInfo.chatInfo!=null)
        {
            mBindServerSendData.HandleData(mPushChatInfo.chatInfo);
        }
        else
        {
            DebugSystem.LogError("chat error:"+mPushChatInfo.result);
        }
    }

}
