package com.xk.sharesdk;

import java.util.HashMap;

import com.unity3d.player.UnityPlayer;
import com.xk.help.JsonManager;
import com.xk.platform.qq.QQSDK;
import com.xk.platform.sina.SinaSDK;
import com.xk.project.wxapi.WeChatSDK;

import android.content.Context;
import android.content.Intent;
import android.util.Log;

public class UnityShareManager 
{
	private static final String tag=UnityShareManager.class.getName();	
	
	public static final int Share_result_Success=1;
	public static final int Share_result_Fail=2;
	public static final int Share_result_Cancel=3;
	
	public static int CurrentSharePlatform=-1;
	public static UnityShareManager single;	
	private static Context context;
	private static String objName;
	private static String CallBackFunName;
	
	public UnityShareManager(String objName,String CallBack)
	{
		single=this;
		CurrentSharePlatform=-1;
		UnityShareManager.objName=objName;
		UnityShareManager.CallBackFunName=CallBack;
		this.context=UnityPlayer.currentActivity.getApplicationContext();
	}
	
	public void ShareContent(int PlatformId,String Content)
	{
		Log.d(tag, "分享内容Json字符串： "+Content);
		CurrentSharePlatform=PlatformId;
		switch(PlatformId)
		{
		case SharePlatformManager.QQ_Share_Platform:
			QQSDK.getSingle().ShareToQQ(Content);
			break;
		case SharePlatformManager.Qzone_Share_Platform:
			QQSDK.getSingle().ShareToQZone(Content);
			break;
		case SharePlatformManager.WeChat_Share_Platform:
			WeChatSDK.getSingle().ShareContent(Content);
			break;
		case SharePlatformManager.Sina_Share_Platform:
			SinaSDK.getSingle().ShareContent(Content);
			break;			
		}		
	}	

	public void onShareResult(int platformId,int status,String resultStr) 
	{
		HashMap<String, Object> map = new HashMap<String, Object>();
		map.put("platformId", platformId);
		map.put("status", status);
		map.put("resultstr", (resultStr!=null?resultStr:""));
		String str= JsonManager.parseJsonFromHash(map);
		Log.d(tag, "发送分享返回信息："+str);
		UnityPlayer.UnitySendMessage(objName, CallBackFunName, str);
	}
	
	public void onActivityResult(int requestCode, int resultCode, Intent data) 
	{
		switch(CurrentSharePlatform)
		{
			case SharePlatformManager.QQ_Share_Platform:
			case SharePlatformManager.Qzone_Share_Platform:
				QQSDK.getSingle().onActivityResult(requestCode, resultCode, data);	
				break;	
			case SharePlatformManager.Sina_Share_Platform:
				SinaSDK.getSingle().onActivityResult(requestCode, resultCode, data);
		}
	}

}
