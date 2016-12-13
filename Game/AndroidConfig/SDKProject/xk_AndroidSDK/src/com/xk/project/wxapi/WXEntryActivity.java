package com.xk.project.wxapi;

import com.tencent.mm.sdk.constants.ConstantsAPI;
import com.tencent.mm.sdk.modelbase.BaseReq;
import com.tencent.mm.sdk.modelbase.BaseResp;
import com.tencent.mm.sdk.openapi.IWXAPIEventHandler;
import com.unity3d.player.UnityPlayer;
import com.xk.sharesdk.SharePlatformManager;
import com.xk.sharesdk.UnityShareManager;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

public class WXEntryActivity extends Activity implements IWXAPIEventHandler {
	private final String tag = this.getClass().getName();

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		Log.d(tag,"onCreate");
		WeChatSDK.getSingle().HandleIntent(this.getIntent(), this);
	}

	@Override
	protected void onNewIntent(Intent intent) {
		super.onNewIntent(intent);
		setIntent(intent);
		Log.d(tag,"onNewIntent");
		WeChatSDK.getSingle().HandleIntent(intent, this);
	}

	// 微信发送请求到第三方应用时，会回调到该方法
	@Override
	public void onReq(BaseReq req) {
		finish();
		
		Log.e(tag, "onReq 返回结果错误码：" + req.getType());

		switch (req.getType()) {
		case ConstantsAPI.COMMAND_GETMESSAGE_FROM_WX:
			;
			break;
		case ConstantsAPI.COMMAND_SHOWMESSAGE_FROM_WX:
			break;
		default:
			break;
		}
	}

	// 第三方应用发送到微信的请求处理后的响应结果，会回调到该方法
	@Override
	public void onResp(BaseResp arg0) {
		
		finish();
		Log.e(tag, "onResp 返回结果错误码：" + arg0.errCode);
		switch (arg0.errCode) {
		case BaseResp.ErrCode.ERR_OK:
			UnityShareManager.single.onShareResult(SharePlatformManager.WeChat_Share_Platform,
					UnityShareManager.Share_result_Success, arg0.errStr);
			break;
		case BaseResp.ErrCode.ERR_USER_CANCEL:
			UnityShareManager.single.onShareResult(SharePlatformManager.WeChat_Share_Platform,
					UnityShareManager.Share_result_Cancel, arg0.errStr);
			break;
		case BaseResp.ErrCode.ERR_AUTH_DENIED:
			UnityShareManager.single.onShareResult(SharePlatformManager.WeChat_Share_Platform,
					UnityShareManager.Share_result_Fail, arg0.errStr);
			break;
		default:
			Log.e(tag, "返回结果状态：未知");
			break;
		}
	}
}