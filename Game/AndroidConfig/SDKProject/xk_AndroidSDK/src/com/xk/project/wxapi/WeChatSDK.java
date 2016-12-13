package com.xk.project.wxapi;

import java.net.URL;
import java.util.Map;

import com.tencent.mm.sdk.modelmsg.SendMessageToWX;
import com.tencent.mm.sdk.modelmsg.SendMessageToWX.Req;
import com.tencent.mm.sdk.modelmsg.WXImageObject;
import com.tencent.mm.sdk.modelmsg.WXMediaMessage;
import com.tencent.mm.sdk.modelmsg.WXMusicObject;
import com.tencent.mm.sdk.modelmsg.WXTextObject;
import com.tencent.mm.sdk.modelmsg.WXVideoObject;
import com.tencent.mm.sdk.modelmsg.WXWebpageObject;
import com.tencent.mm.sdk.openapi.IWXAPI;
import com.tencent.mm.sdk.openapi.IWXAPIEventHandler;
import com.tencent.mm.sdk.openapi.WXAPIFactory;
import com.unity3d.player.UnityPlayer;
import com.xk.help.JsonManager;
import com.xk.help.Util;

import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Log;

class WeChatPlatformInfo {
	private static final String hash_key_appId = "AppId";
	private static final String hash_key_appKey = "AppKey";
	public String appId;
	public String appKey;

	public WeChatPlatformInfo(String appId, String appKey) {
		this.appId = appId;
		this.appKey = appKey;
	}

	public WeChatPlatformInfo(Map<String, Object> mHash) {
		this.appId = mHash.get(hash_key_appId).toString();
		this.appKey = mHash.get(hash_key_appKey).toString();
	}
}

public class WeChatSDK {
	private static final String tag = WeChatSDK.class.getName();
	private static WeChatPlatformInfo mWeChatPlatformInfo = null;
	private IWXAPI api = null;

	private static WeChatSDK single = null;

	public static WeChatSDK getSingle() {
		if (single == null) {
			single = new WeChatSDK();
		}
		return single;
	}

	public void SetPlatformInfo(Map<String, Object> mHash) {
		mWeChatPlatformInfo = new WeChatPlatformInfo(mHash);
		api = WXAPIFactory.createWXAPI(UnityPlayer.currentActivity.getApplicationContext(), mWeChatPlatformInfo.appId,true);
		api.registerApp(mWeChatPlatformInfo.appId);
		//api.handleIntent(new Intent(UnityPlayer.currentActivity,WXEntryActivity.class), mHandler);
		//api.handleIntent(UnityPlayer.currentActivity.getIntent(), (MainActivity)UnityPlayer.currentActivity);
		Log.d(tag, "设置WeChat平台信息：" + mWeChatPlatformInfo.appId);
	}

	public void ShareContent(final String jsonStr) {
		WeChatShareContent mContent = new WeChatShareContent();
		Req mReq = mContent.ShareContent(jsonStr);
		api.sendReq(mReq);
	}
	
	public void HandleIntent(Intent arg0,IWXAPIEventHandler handler)
	{
		Log.d(tag, "HandleIntent");
		api.handleIntent(arg0,handler);
	}
}

class WeChatShareContent {
	private static final String tag = WeChatShareContent.class.getName();
	// 发送到朋友圈
	public static final int shareSceneType_WXSceneTimeline = 1;
	// 添加到微信收藏
	public static final int shareSceneType_WXSceneFavorite = 2;
	// 发送到聊天界面
	public static final int shareSceneType_WXSceneSession = 3;

	public static final int sharContentType_Text = 1;
	public static final int sharContentType_Image = 2;
	public static final int sharContentType_Music = 3;
	public static final int sharContentType_Video = 4;
	public static final int sharContentType_WebPage = 5;

	public static final String shareType_Text_flag = "text";
	public static final String shareType_Image_flag = "image";
	public static final String shareType_Music_flag = "music";
	public static final String shareType_Video_flag = "video";
	public static final String shareType_WebPage_flag = "webpage";

	private final String hash_key_sharescenetype = "sharescenetype";
	private final String hash_key_sharecontenttype = "sharecontenttype";
	private final String hash_key_title = "title";
	private final String hash_key_description = "description";
	private final String hash_key_text = "text";
	private final String hash_key_thumbUrl = "thumbUrl";
	private final String hash_key_imageUrl = "imageUrl";
	private final String hash_key_musicUrl = "musicUrl";
	private final String hash_key_videoUrl = "videoUrl";
	private final String hash_key_WebPageUrl = "webPageUrl";

	private static final int THUMB_SIZE = 100;

	private int parseShareSceneType(int type) {
		switch (type) {
		case shareSceneType_WXSceneTimeline:// 发送到朋友圈
			return Req.WXSceneTimeline;
		case shareSceneType_WXSceneFavorite:// 添加到微信收藏
			return Req.WXSceneFavorite;
		case shareSceneType_WXSceneSession:// 发送到聊天界面
			return Req.WXSceneSession;
		}
		return -1;
	}

	public Req ShareContent(String mJsonStr) {
		Log.d(tag, "开始解析字符串");
		Map<String, Object> mHash = JsonManager.parseJsonFromStr(mJsonStr);
		Log.d(tag, "解析字符串完毕");

		int shareSceneType = Integer.parseInt(mHash.get(hash_key_sharescenetype).toString());
		shareSceneType = parseShareSceneType(shareSceneType);
		int shareContentType = Integer.parseInt(mHash.get(hash_key_sharecontenttype).toString());
		String title = mHash.get(hash_key_title).toString();
		String description = mHash.get(hash_key_description).toString();

		switch (shareContentType) {
		case sharContentType_Text: {
			WXTextObject textObj = new WXTextObject();
			String mText = mHash.get(hash_key_text).toString();
			textObj.text = mText;

			WXMediaMessage msg = new WXMediaMessage();
			msg.mediaObject = textObj;
			msg.description = description;

			Req req = new Req();
			req.transaction = buildTransaction(shareType_Text_flag); // transaction字段用于唯一标识一个请求
			req.message = msg;
			req.scene = shareSceneType;
			return req;
		}
		case sharContentType_Image: {
			try {
				WXImageObject imgObj = new WXImageObject();
				String imagePath = mHash.get(hash_key_imageUrl).toString();		
				
				Bitmap bmp = BitmapFactory.decodeStream(new URL(imagePath).openStream());
				imgObj=new WXImageObject(bmp);
				WXMediaMessage msg = new WXMediaMessage();
				msg.mediaObject = imgObj;
				msg.title=title;
				msg.description=description;

				Bitmap thumbBmp = Bitmap.createScaledBitmap(bmp, THUMB_SIZE, THUMB_SIZE, true);
				bmp.recycle();
				byte[] mBytes=Util.bmpToByteArray(thumbBmp, true);
				msg.thumbData = mBytes;	
				
				Log.d(tag, "缩略图大小"+mBytes.length);		
				if(mBytes.length>32*1024)
				{
					Log.e(tag, "缩略图大小超过32K");
				}

				Req req = new Req();
				req.transaction = buildTransaction(shareType_Image_flag);
				req.message = msg;
				req.scene = shareSceneType;
				Log.d(tag, "Image 解析完毕");
				return req;
			} catch (Exception e) {
				Log.e(tag, e.getMessage());
			}
			break;
		}
		case sharContentType_Music: {
			try {
				WXMusicObject music = new WXMusicObject();
				String musicPath = mHash.get(hash_key_musicUrl).toString();
				music.musicUrl = musicPath;

				WXMediaMessage msg = new WXMediaMessage();
				msg.mediaObject = music;
				msg.title = title;
				msg.description = description;

				Bitmap bmp = BitmapFactory.decodeStream(new URL(musicPath).openStream());
				msg.thumbData = Util.bmpToByteArray(bmp, true);

				SendMessageToWX.Req req = new SendMessageToWX.Req();
				req.transaction = buildTransaction(shareType_Music_flag);
				req.message = msg;
				req.scene = shareSceneType;
				return req;
			} catch (Exception e) {
				Log.e(tag, e.getMessage());
			}
			break;
		}
		case sharContentType_Video: {
			try {
				WXVideoObject video = new WXVideoObject();
				String videoPath = mHash.get(hash_key_videoUrl).toString();
				video.videoUrl = videoPath;

				WXMediaMessage msg = new WXMediaMessage(video);
				msg.title = title;
				msg.description = description;

				Bitmap thumb = BitmapFactory.decodeStream(new URL(videoPath).openStream());
				msg.thumbData = Util.bmpToByteArray(thumb, true);

				SendMessageToWX.Req req = new SendMessageToWX.Req();
				req.transaction = buildTransaction(shareType_Video_flag);
				req.message = msg;
				req.scene = shareSceneType;
				return req;
			} catch (Exception e) {
				Log.e(tag, e.getMessage());
			}
			break;
		}
		case sharContentType_WebPage: {
			try {
				WXWebpageObject webpage = new WXWebpageObject();
				String webpagePath = mHash.get(hash_key_WebPageUrl).toString();
				webpage.webpageUrl = webpagePath;

				WXMediaMessage msg = new WXMediaMessage();
				msg.mediaObject=webpage;
				msg.title = title;
				msg.description = description;
				
				String imagePath = mHash.get(hash_key_imageUrl).toString();	
				Bitmap thumb = BitmapFactory.decodeStream(new URL(imagePath).openStream());
				Bitmap thumbBmp = Bitmap.createScaledBitmap(thumb, THUMB_SIZE, THUMB_SIZE, true);
				
				byte[]	mBytes=Util.bmpToByteArray(thumbBmp, true);
				msg.thumbData = mBytes;
				
				Log.d(tag, "缩略图大小"+mBytes.length);		
				if(mBytes.length>32*1024)
				{
					Log.e(tag, "缩略图大小超过32K");
				}
				
				SendMessageToWX.Req req = new SendMessageToWX.Req();
				req.transaction = buildTransaction(shareType_WebPage_flag);
				req.message = msg;
				req.scene = shareSceneType;
				return req;
			} catch (Exception e) {
				Log.e(tag, e.getMessage());
			}
			break;
		}
		}
		return null;
	}

	private String buildTransaction(final String type) {
		return (type == null) ? String.valueOf(System.currentTimeMillis()) : type + System.currentTimeMillis();
	}

}
