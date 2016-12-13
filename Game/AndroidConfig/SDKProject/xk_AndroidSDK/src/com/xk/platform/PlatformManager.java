package com.xk.platform;

import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import com.unity3d.player.UnityPlayer;
import com.xk.help.JsonManager;
import com.xk.platform.qq.QQSDK;
import com.xk.platform.sina.SinaSDK;
import com.xk.project.wxapi.WeChatSDK;

import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.util.Log;

public class PlatformManager {

	private final String tag = this.getClass().getName();

	public final static int QQ_Platform = 101;
	public final static int WeChat_Platform = 102;
	public final static int Sina_Platform = 103;
	
	public final static String QQ_Platform_PackageName="com.tencent.mobileqq";
	public final static String WeChat_Platform_PackageName="com.tencent.mm";
	public final static String Sina_Platform_PackageName="com.sina.weibo";
	
	
	private static PlatformManager single;

	public static PlatformManager getSingle() {
		if (single == null) {
			single = new PlatformManager();
		}
		return single;
	}

	public void InitAllPlatform(final String mStr) {
		Thread mThread = new Thread(new Runnable() {
			@Override
			public void run() {
				HandleInfo(mStr);
			}
		});
		mThread.start();
	}

	@SuppressWarnings("unchecked")
	private void HandleInfo(final String mStr) {
		try {
			Map<String, Object> devInfo = JsonManager.parseJsonFromStr(mStr);
			if (devInfo == null) {
				Log.e(tag, "json½âÎö´íÎó");
				return;
			}
			for (Entry<String, Object> entry : devInfo.entrySet()) {
				int PlatformId = Integer.parseInt(entry.getKey());
				Map<String, Object> mMap = JsonManager.parseJsonFromStr(entry.getValue().toString());
				switch (PlatformId) {
				case QQ_Platform:
					QQSDK.getSingle().SetPlatformInfo(mMap);
					break;
				case WeChat_Platform:
					WeChatSDK.getSingle().SetPlatformInfo(mMap);
					break;
				case Sina_Platform:
					SinaSDK.getSingle().SetPlatformInfo(mMap);
					break;
				}
			}
		} catch (Exception e) {
			Log.e(tag, e.getMessage());
		}
	}

	public boolean isClientValid(int Platform) 
	{
		switch(Platform)
		{
		case QQ_Platform:
			return checkApkExist(UnityPlayer.currentActivity.getApplicationContext(),QQ_Platform_PackageName);
		case WeChat_Platform:
			return checkApkExist(UnityPlayer.currentActivity.getApplicationContext(),WeChat_Platform_PackageName);
		case Sina_Platform:
			return checkApkExist(UnityPlayer.currentActivity.getApplicationContext(),Sina_Platform_PackageName);		
		}
		return true;
	}

	private boolean checkApkExist(Context context, String packageName) {
		if (packageName == null || packageName.isEmpty())
			return false;
		try {
			ApplicationInfo info = context.getPackageManager().getApplicationInfo(packageName,PackageManager.GET_UNINSTALLED_PACKAGES);
			if(info!=null)
			{
				return true;
			}else
			{
				return false;
			}
		} catch (NameNotFoundException e) {
			return false;
		}
	}
}
