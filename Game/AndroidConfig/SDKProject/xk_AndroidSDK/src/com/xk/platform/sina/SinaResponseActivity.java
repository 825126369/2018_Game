package com.xk.platform.sina;

import com.sina.weibo.sdk.api.share.BaseResponse;
import com.sina.weibo.sdk.api.share.IWeiboHandler.Response;
import com.sina.weibo.sdk.constant.WBConstants;
import com.xk.sharesdk.SharePlatformManager;
import com.xk.sharesdk.UnityShareManager;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

public class SinaResponseActivity extends Activity implements Response
{
	private final String tag=this.getClass().getName();
	
    @Override
    protected void onCreate(Bundle savedInstanceState) 
    {
        super.onCreate(savedInstanceState); 
        Log.d(tag,"onCreate");
        SinaSDK.getSingle().HandleResponse(this.getIntent(),this);
    }

	@Override
	protected void onNewIntent(Intent intent) 
	{     
		super.onNewIntent(intent); 
		setIntent(intent);
		Log.d(tag,"onNewIntent");
		SinaSDK.getSingle().HandleResponse(intent,this);	
	}

	@Override
	public void onResponse(BaseResponse baseResp) 
	{
		finish();
		Log.d(tag, "返回结果错误码："+baseResp.errCode+" | "+baseResp.errMsg+" | "+baseResp.transaction);
		switch (baseResp.errCode) 
		{
		case WBConstants.ErrorCode.ERR_OK:
			UnityShareManager.single.onShareResult(SharePlatformManager.Sina_Share_Platform, UnityShareManager.Share_result_Success, baseResp.errMsg);
			break;
		case WBConstants.ErrorCode.ERR_CANCEL:
			UnityShareManager.single.onShareResult(SharePlatformManager.Sina_Share_Platform, UnityShareManager.Share_result_Cancel, baseResp.errMsg);
			break;
		case WBConstants.ErrorCode.ERR_FAIL:
			Log.e(tag, "回掉结果报错："+baseResp.errMsg);
			SinaTokenManager.clear(this.getApplicationContext());
			UnityShareManager.single.onShareResult(SharePlatformManager.Sina_Share_Platform, UnityShareManager.Share_result_Fail, baseResp.errMsg);
			break;
		}
	}
	

}
