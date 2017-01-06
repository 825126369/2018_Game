using UnityEngine;
using System.Collections;
using xk_System.Model;
using game.protobuf.data;
using xk_System.Debug;
using xk_System.Net;

public class LoginMessage : NetModel
{
    public DataBind<bool> mRegisterResult = new DataBind<bool>();
    public DataBind<bool> mLoginResult = new DataBind<bool>();

    public override void initModel()
    {
        base.initModel();
        addNetListenFun(ProtoCommand.PROTO_REGISTERACCOUNT, Receive_RegisterAccountResult);
        addNetListenFun(ProtoCommand.PROTO_LOGIN, receive_LoginGame);
    }

    public override void destroyModel()
    {
        base.destroyModel();
        removeNetListenFun(ProtoCommand.PROTO_REGISTERACCOUNT, Receive_RegisterAccountResult);
        removeNetListenFun(ProtoCommand.PROTO_LOGIN, receive_LoginGame);
    }

    public void Send_RegisterAccount(string aN, string ps, string reps)
    {
        csRegisterAccount mdata = new csRegisterAccount();
        mdata.accountName = aN;
        mdata.password = ps;
        mdata.repeatPassword = reps;
        sendNetData(ProtoCommand.PROTO_REGISTERACCOUNT, mdata);
    }

    public void Receive_RegisterAccountResult(Package mProtobuf)
    {
        scRegisterAccount mscRegisterAccountdata = new scRegisterAccount();
        mProtobuf.getData<scRegisterAccount>(mscRegisterAccountdata);
        if (mscRegisterAccountdata.result == 1)
        {
            mRegisterResult.HandleData(mscRegisterAccountdata.result == 1);
        }else
        {
            DebugSystem.LogError("Register Account Error: "+mscRegisterAccountdata.result);
        }
    }

    public void send_LoginGame(string ac, string ps)
    {
        csLoginGame mdata = new csLoginGame();
        mdata.accountName = ac;
        mdata.password = ps;
        sendNetData(ProtoCommand.PROTO_LOGIN, mdata);
    }

    public void receive_LoginGame(Package mProtobuf)
    {
        scLoginGame mscLoginGame = new scLoginGame();
        mProtobuf.getData<scLoginGame>(mscLoginGame);
        if (mscLoginGame.result == 1)
        {
            mLoginResult.HandleData(mscLoginGame.result == 1);
        }else
        {
            DebugSystem.LogError("Login Account Error: " + mscLoginGame.result);
        }
    }
}
