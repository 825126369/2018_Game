package com.xk.platform.sina;

import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Map;

import com.sina.weibo.sdk.api.ImageObject;
import com.sina.weibo.sdk.api.MusicObject;
import com.sina.weibo.sdk.api.TextObject;
import com.sina.weibo.sdk.api.VideoObject;
import com.sina.weibo.sdk.api.VoiceObject;
import com.sina.weibo.sdk.api.WebpageObject;
import com.sina.weibo.sdk.api.WeiboMultiMessage;

import com.sina.weibo.sdk.api.share.IWeiboHandler.Response;
import com.sina.weibo.sdk.auth.AuthInfo;
import com.sina.weibo.sdk.auth.Oauth2AccessToken;
import com.sina.weibo.sdk.auth.WeiboAuthListener;
import com.sina.weibo.sdk.auth.sso.SsoHandler;
import com.sina.weibo.sdk.exception.WeiboException;
import com.sina.weibo.sdk.api.share.IWeiboShareAPI;
import com.sina.weibo.sdk.api.share.SendMultiMessageToWeiboRequest;
import com.sina.weibo.sdk.api.share.WeiboShareSDK;
import com.sina.weibo.sdk.utils.Utility;
import com.unity3d.player.UnityPlayer;
import com.xk.help.JsonManager;
import com.xk.help.Util;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

class SinaPlatformInfo {
	private static final String hash_key_appId = "AppId";
	private static final String hash_key_appKey = "AppKey";
	private static final String hash_key_redirectUrl = "redirectUrl";
	public String appId;
	public String appKey;
	public String redirectUrl;

	public SinaPlatformInfo(String appId, String appKey) {
		this.appId = appId;
		this.appKey = appKey;
	}

	public SinaPlatformInfo(Map<String, Object> mHash) {
		this.appId = mHash.get(hash_key_appId).toString();
		this.appKey = mHash.get(hash_key_appKey).toString();
	}
}

public class SinaSDK {
	private static final String tag = SinaSDK.class.getName();
	static SinaPlatformInfo mSinaPlatformInfo;
	static IWeiboShareAPI api;
	static SsoHandler mSsoHandler;
	private static SinaSDK single;
	private static final Context mContext = UnityPlayer.currentActivity.getApplicationContext();

	public static SinaSDK getSingle() {
		if (single == null) {
			single = new SinaSDK();
		}
		return single;
	}

	public void SetPlatformInfo(Map<String, Object> mHash) {
		mSinaPlatformInfo = new SinaPlatformInfo(mHash);
		api = WeiboShareSDK.createWeiboAPI(mContext, mSinaPlatformInfo.appId);
		api.registerApp(); // 将应用注册到微博客户端
		Log.d(tag, "设置Sina平台信息：" + mSinaPlatformInfo.appId);
	}

	public void ShareContent(String mJsonStr) {
		api = WeiboShareSDK.createWeiboAPI(mContext, mSinaPlatformInfo.appId);
		api.registerApp();
		if (!api.isWeiboAppSupportAPI()) 
		{
			Log.e(tag, "微博客户端不支持 SDK 分享或微博客户端未安装或微博客户端是非官方版本： "+api.getWeiboAppSupportAPI());
		}else
		{
			Log.d(tag,"当前微博支持API版本号: "+api.getWeiboAppSupportAPI());
		}
		SinaShareContent mContent = new SinaShareContent();
		final SendMultiMessageToWeiboRequest request = mContent.GetShareMessage(mJsonStr);
		api.sendRequest(UnityPlayer.currentActivity, request);
		
		return;
		/*AuthInfo authInfo = new AuthInfo(mContext, mSinaPlatformInfo.appId, "http://www.sina.com",null);
		Oauth2AccessToken accessToken =  SinaTokenManager.readAccessToken(mContext);
		if (accessToken.isSessionValid()) 
		{
			api.sendRequest(UnityPlayer.currentActivity, request);
		} else {
			Log.d(tag, "开始认证");
			mSsoHandler = new SsoHandler(UnityPlayer.currentActivity, authInfo);
			mSsoHandler.authorize(new WeiboAuthListener() {
				@Override
				public void onWeiboException(WeiboException arg0) {
					Log.d(tag, "授权异常");
				}

				@Override
				public void onComplete(Bundle bundle) {
					Oauth2AccessToken newToken = Oauth2AccessToken.parseAccessToken(bundle);
					if (newToken.isSessionValid()) {
						SinaTokenManager.writeAccessToken(mContext, newToken);
						Log.d(tag, "授权完成");
						api.registerApp();
						api.sendRequest(UnityPlayer.currentActivity, request);
					} else {
						String code = bundle.getString("code", "");
						Log.e(tag, "错误码：" + code);
					}
				}

				@Override
				public void onCancel() {
					Log.d(tag, "授权取消");
				}

			});
		}*/
	}

	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (mSsoHandler != null) {
			mSsoHandler.authorizeCallBack(requestCode, resultCode, data);
		}
	}

	public void HandleResponse(Intent intent, Response mResponse) {
		Log.d(tag, "HandleResponse");
		api.handleWeiboResponse(intent, mResponse);
	}
}

class SinaShareContent {
	private static final String tag = SinaShareContent.class.getName();
	private static final int THUMB_SIZE = 100;

	private int sharetype;
	private String title;
	private String description;
	private String text;
	private String imageUrl;
	private String musicUrl;
	private String videoUrl;
	private String webpageUrl;
	private String voiceUrl;

	// 文字图片（默认） //视频、音乐,网页，声音
	public static final int sharContentType_Text = 1;
	public static final int sharContentType_Image = 2;
	public static final int sharContentType_Music = 3;
	public static final int sharContentType_Video = 4;
	public static final int sharContentType_WebPage = 5;
	public static final int sharContentType_Voice = 6;

	private final String hash_key_sharecontenttype = "sharecontenttype";
	private final String hash_key_title = "title";
	private final String hash_key_description = "description";
	private final String hash_key_text = "text";
	private final String hash_key_imageUrl = "imageUrl";
	private final String hash_key_musicUrl = "musicUrl";
	private final String hash_key_videoUrl = "videoUrl";
	private final String hash_key_WebPageUrl = "webPageUrl";
	private final String hash_key_voiceUrl = "voiceUrl";

	private Bitmap GetThumbBmp(String urlPath) {
		try {
			Bitmap bmp = BitmapFactory.decodeStream(new URL(urlPath).openStream());
			bmp = Bitmap.createScaledBitmap(bmp, THUMB_SIZE, THUMB_SIZE, true);
			byte[] mBytes = Util.bmpToByteArray(bmp, false);
			Log.d(tag, "缩略图大小" + mBytes.length);
			if (mBytes.length > 32 * 1024) {
				Log.e(tag, "缩略图大小超过32K");
			}
			return bmp;
		} catch (Exception e) {
			Log.e(tag, e.getMessage());
		}
		return null;
	}

	private void ParseJsonStr(String mJsonStr) {
		Log.d(tag, "开始解析字符串");
		Map<String, Object> mHash = JsonManager.parseJsonFromStr(mJsonStr);
		Log.d(tag, "解析字符串完毕");
		if (mHash.containsKey(hash_key_sharecontenttype)) {
			sharetype = Integer.parseInt(mHash.get(hash_key_sharecontenttype).toString());
		}
		if (mHash.containsKey(hash_key_title)) {
			title = mHash.get(hash_key_title).toString();
		}
		if (mHash.containsKey(hash_key_description)) {
			description = mHash.get(hash_key_description).toString();
		}
		if (mHash.containsKey(hash_key_text)) {
			text = mHash.get(hash_key_text).toString();
		}
		if (mHash.containsKey(hash_key_imageUrl)) {
			imageUrl = mHash.get(hash_key_imageUrl).toString();
		}
		if (mHash.containsKey(hash_key_musicUrl)) {
			musicUrl = mHash.get(hash_key_musicUrl).toString();
		}
		if (mHash.containsKey(hash_key_videoUrl)) {
			videoUrl = mHash.get(hash_key_videoUrl).toString();
		}
		if (mHash.containsKey(hash_key_WebPageUrl)) {
			webpageUrl = mHash.get(hash_key_WebPageUrl).toString();
		}
		if (mHash.containsKey(hash_key_voiceUrl)) {
			voiceUrl = mHash.get(hash_key_voiceUrl).toString();
		}
	}

	public SendMultiMessageToWeiboRequest GetShareMessage(String mJsonStr) {
		ParseJsonStr(mJsonStr);
		WeiboMultiMessage weiboMessage = new WeiboMultiMessage();

		switch (sharetype) {
		case sharContentType_Text:
			TextObject textObject = new TextObject();
			textObject.text = text;
			weiboMessage.textObject = textObject;
			break;
		case sharContentType_Image:
			ImageObject imageObject = new ImageObject();
			imageObject.setImageObject(GetThumbBmp(imageUrl));
			weiboMessage.imageObject = imageObject;
			break;
		case sharContentType_WebPage: {
			ImageObject imageObject1 = new ImageObject();
			imageObject1.setImageObject(GetThumbBmp(imageUrl));
			weiboMessage.imageObject = imageObject1;

			WebpageObject mediaObject = new WebpageObject();
			mediaObject.identify = Utility.generateGUID();
			mediaObject.title = title;
			mediaObject.description = description;

			mediaObject.setThumbImage(GetThumbBmp(imageUrl));
			mediaObject.actionUrl = webpageUrl;
			mediaObject.defaultText = text;
			weiboMessage.mediaObject = mediaObject;
			break;
		}
		case sharContentType_Music: {
			MusicObject musicObject = new MusicObject();
			musicObject.identify = Utility.generateGUID();
			musicObject.title = title;
			musicObject.description = description;

			musicObject.setThumbImage(GetThumbBmp(imageUrl));
			musicObject.actionUrl = musicUrl;
			musicObject.dataUrl = "www.weibo.com";
			musicObject.dataHdUrl = "www.weibo.com";
			musicObject.duration = 10;
			musicObject.defaultText = text;
			weiboMessage.mediaObject = musicObject;
			break;
		}
		case sharContentType_Video: {
			VideoObject videoObject = new VideoObject();
			videoObject.identify = Utility.generateGUID();
			videoObject.title = title;
			videoObject.description = description;
			videoObject.setThumbImage(GetThumbBmp(imageUrl));
			videoObject.actionUrl = videoUrl;
			videoObject.dataUrl = "www.weibo.com";
			videoObject.dataHdUrl = "www.weibo.com";
			videoObject.duration = 10;
			videoObject.defaultText = text;
			weiboMessage.mediaObject = videoObject;
			break;
		}
		case sharContentType_Voice: {
			VoiceObject voiceObject = new VoiceObject();
			voiceObject.identify = Utility.generateGUID();
			voiceObject.title = title;
			voiceObject.description = description;
			voiceObject.setThumbImage(GetThumbBmp(imageUrl));
			voiceObject.actionUrl = voiceUrl;
			voiceObject.dataUrl = "www.weibo.com";
			voiceObject.dataHdUrl = "www.weibo.com";
			voiceObject.duration = 10;
			voiceObject.defaultText = "Voice";
			weiboMessage.mediaObject = voiceObject;
			break;
		}
		}

		SendMultiMessageToWeiboRequest request = new SendMultiMessageToWeiboRequest();
		request.transaction = String.valueOf(System.currentTimeMillis());
		request.multiMessage = weiboMessage;
		return request;

	}
}
