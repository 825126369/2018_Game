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
        addNetListenFun(ProtoCommand.RegisterAccount, Receive_RegisterAccountResult);
        addNetListenFun(ProtoCommand.Login, receive_LoginGame);
    }

    public override void destroyModel()
    {
        base.destroyModel();
        removeNetListenFun(ProtoCommand.RegisterAccount, Receive_RegisterAccountResult);
        removeNetListenFun(ProtoCommand.Login, receive_LoginGame);
    }

    public void Send_RegisterAccount(string aN, string ps, string reps)
    {
        csRegisterAccount mdata = new csRegisterAccount();
        mdata.accountName = aN;
        mdata.password = ps;
        mdata.repeatPassword = reps;
        sendNetData(ProtoCommand.RegisterAccount, mdata);
    }

    public void Receive_RegisterAccountResult(Package mProtobuf)
    {
        scRegisterAccount mscRegisterAccountdata = null;
        mProtobuf.getData<scRegisterAccount>(mscRegisterAccountdata);
        mRegisterResult.HandleData(mscRegisterAccountdata.result);
    }

    public void send_LoginGame(string ac, string ps)
    {
        csLoginGame mdata = new csLoginGame();
        mdata.accountName = ac;
        mdata.password = ps;
        sendNetData(ProtoCommand.Login, mdata);
    }

    public void receive_LoginGame(Package mProtobuf)
    {
        scLoginGame mscLoginGame = null;
        mProtobuf.getData<scLoginGame>(mscLoginGame);
        mLoginResult.HandleData(mscLoginGame.result);
    }
}
