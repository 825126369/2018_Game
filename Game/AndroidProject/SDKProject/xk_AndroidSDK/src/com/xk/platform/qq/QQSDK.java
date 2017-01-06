package com.xk.platform.qq;

import java.util.ArrayList;

import java.util.Map;

import com.tencent.connect.share.QQShare;
import com.tencent.connect.share.QzonePublish;
import com.tencent.connect.share.QzoneShare;
import com.tencent.tauth.IUiListener;
import com.tencent.tauth.Tencent;
import com.tencent.tauth.UiError;
import com.unity3d.player.UnityPlayer;
import com.xk.help.JsonManager;
import com.xk.help.ThreadManager;
import com.xk.sharesdk.UnityShareManager;
import com.xk.sharesdk.SharePlatformManager;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

class QQPlatformInfo {
	private static final String hash_key_appId = "AppId";
	private static final String hash_key_appKey = "AppKey";
	public String appId;
	public String appKey;

	public QQPlatformInfo(String appId, String appKey) {
		this.appId = appId;
		this.appKey = appKey;
	}

	public QQPlatformInfo(Map<String, Object> mHash) {
		this.appId = mHash.get(hash_key_appId).toString();
		this.appKey = mHash.get(hash_key_appKey).toString();
	}
}

public class QQSDK {
	private final String tag = this.getClass().getName();
	private static QQPlatformInfo mQQPlatformInfo;

	public Activity mActivity;
	public Tencent mTencent;

	private int CurrentSharePlatform = SharePlatformManager.QQ_Share_Platform;
	private static QQSDK single;

	public static QQSDK getSingle() {
		if (single == null) {
			single = new QQSDK();
		}
		return single;
	}

	public void SetPlatformInfo(Map<String, Object> mHash) {
		mQQPlatformInfo = new QQPlatformInfo(mHash);
		mActivity = UnityPlayer.currentActivity;
		mTencent = Tencent.createInstance(mQQPlatformInfo.appId, UnityPlayer.currentActivity.getApplicationContext());
		Log.d(tag, "设置QQ平台信息：" + mQQPlatformInfo.appId);
	}

	public void ShareToQQ(final String jsonStr) {

		// QQ分享要在主线程做
		ThreadManager.getMainHandler().post(new Runnable() {
			@Override
			public void run() {
				if (null != mTencent) {
					CurrentSharePlatform = SharePlatformManager.QQ_Share_Platform;
					QQShareContent mQQShare = new QQShareContent();
					Bundle params = mQQShare.ShareQQ(jsonStr);
					mTencent.shareToQQ(mActivity, params, qqShareListener);
				}
			}
		});

	}

	public void ShareToQZone(final String jsonStr) {
		ThreadManager.getMainHandler().post(new Runnable() {
			@Override
			public void run() {
				if (null != mTencent) {
					CurrentSharePlatform = SharePlatformManager.Qzone_Share_Platform;
					QzoneShareContent mQzoneShareContent = new QzoneShareContent();
					Bundle params = mQzoneShareContent.ShareQzone(jsonStr);
					if (mQzoneShareContent.OrToSharePublic()) {
						mTencent.publishToQzone(mActivity, params, qqShareListener);
					} else {
						mTencent.shareToQzone(mActivity, params, qqShareListener);
					}
				}
			}
		});
	}

	IUiListener qqShareListener = new IUiListener() {
		@Override
		public void onCancel() {
			UnityShareManager.single.onShareResult(CurrentSharePlatform, UnityShareManager.Share_result_Cancel,
					"Cancel");
		}

		@Override
		public void onComplete(Object response) {
			UnityShareManager.single.onShareResult(CurrentSharePlatform, UnityShareManager.Share_result_Success,
					response.toString());
		}

		@Override
		public void onError(UiError e) {
			UnityShareManager.single.onShareResult(CurrentSharePlatform, UnityShareManager.Share_result_Fail,
					e.errorMessage);
		}
	};

	/// 经测试必须填，否则分享成功后，没有回掉信息
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		Tencent.onActivityResultData(requestCode, resultCode, data, qqShareListener);
	}

}

class QQShareContent {
	private final String tag = this.getClass().getName();

	// 必填， 分享的类型
	private int shareType;
	// 必填， 分享的标题, 最长QQ_SHARE_TITLE_MAX_LENGTH
	private String title;
	// 必填， 这条分享消息被好友点击后的跳转URL
	private String TargetUrl;
	// 分享的消息摘要，QQ_SHARE_SUMMARY_MAX_LENGTH
	private String summary;
	// 分享图片的URL或者本地路径
	private String imageUrl;
	// 分享音乐的路径
	private String musicUrl;
	// 可选，手Q客户端顶部，替换“返回”按钮文字，如果为空，用返回代替
	private String AppName;
	// 可选， 默认是不隐藏分享到QZone按钮且不自动打开分享到QZone的对话,
	// Tencent.SHARE_TO_QQ_FLAG_QZONE_AUTO_OPEN，分享时自动打 开分享到QZone的对话框。
	// Tencent.SHARE_TO_QQ_FLAG_QZONE_ITEM_HIDE，分享时隐藏分享 到QZone按钮
	private int OrHideQzoneBtn;

	private final String hash_key_sharetype = "sharetype";
	private final String hash_key_title = "title";
	private final String hash_key_targetUrl = "targeturl";
	private final String hash_key_summary = "summary";
	private final String hash_key_imageUrl = "imageUrl";
	private final String hash_key_musicUrl = "musicUrl";
	private final String hash_key_AppName = "appName";
	private final String hash_key_OrHideQzoneBtn = "orhideQzoneBtn";

	private static final int sharetype_DEFAULT = 1;
	private static final int sharetype_IMAGE = 2;
	private static final int sharetype_AUDIO = 3;
	private static final int sharetype_APP = 4;

	private static final int QzoneBtnState_DEFAULT = 1;
	private static final int QzoneBtnState_Show = 2;
	private static final int QzoneBtnState_Hide = 3;

	private int ParseQQShareType(int mType) {
		switch (mType) {
		case sharetype_DEFAULT:
			return QQShare.SHARE_TO_QQ_TYPE_DEFAULT;
		case sharetype_IMAGE:
			return QQShare.SHARE_TO_QQ_TYPE_IMAGE;
		case sharetype_AUDIO:
			return QQShare.SHARE_TO_QQ_TYPE_AUDIO;
		case sharetype_APP:
			return QQShare.SHARE_TO_QQ_TYPE_APP;
		}
		return QQShare.SHARE_TO_QQ_TYPE_DEFAULT;
	}

	private int ParseQzoneBtnState(int state) {
		switch (state) {
		case QzoneBtnState_DEFAULT:
			return QQShare.SHARE_TO_QQ_FLAG_QZONE_AUTO_OPEN + QQShare.SHARE_TO_QQ_FLAG_QZONE_ITEM_HIDE;
		case QzoneBtnState_Show:
			return QQShare.SHARE_TO_QQ_FLAG_QZONE_AUTO_OPEN;
		case QzoneBtnState_Hide:
			return QQShare.SHARE_TO_QQ_FLAG_QZONE_ITEM_HIDE;
		}
		return -1;
	}

	private Bundle get_ShareQQ_bundle() {
		final Bundle mBundle = new Bundle();
		switch (shareType) {
		case QQShare.SHARE_TO_QQ_TYPE_IMAGE:// 纯图片分享
		case QQShare.SHARE_TO_QQ_TYPE_DEFAULT:// 图文分享
		case QQShare.SHARE_TO_QQ_TYPE_AUDIO:// 音乐分享
		case QQShare.SHARE_TO_QQ_TYPE_APP:// app分享
			mBundle.putInt(QQShare.SHARE_TO_QQ_KEY_TYPE, shareType);
			mBundle.putString(QQShare.SHARE_TO_QQ_TITLE, title);
			mBundle.putString(QQShare.SHARE_TO_QQ_SUMMARY, summary);
			mBundle.putString(QQShare.SHARE_TO_QQ_TARGET_URL, TargetUrl);
			mBundle.putString(QQShare.SHARE_TO_QQ_IMAGE_URL, imageUrl);
			mBundle.putString(QQShare.SHARE_TO_QQ_AUDIO_URL, musicUrl);
			mBundle.putString(QQShare.SHARE_TO_QQ_APP_NAME, AppName);
			mBundle.putInt(QQShare.SHARE_TO_QQ_EXT_INT, OrHideQzoneBtn);
			break;
		default:
			// System.out.printIn("QQ分享类型错误");
			Log.e(this.getClass().getName(), "QQ分享类型错误");
			break;
		}
		Log.d(tag, "得到分享参数");
		return mBundle;
	}

	public Bundle ShareQQ(String mJsonStr) {
		try {
			Log.d(tag, "开始解析字符串");
			Map<String, Object> mHash = JsonManager.parseJsonFromStr(mJsonStr);
			Log.d(tag, "解析字符串完毕");
			int mshareType = Integer.parseInt(mHash.get(hash_key_sharetype).toString());
			shareType = ParseQQShareType(mshareType);
			title = mHash.get(hash_key_title).toString();
			TargetUrl = mHash.get(hash_key_targetUrl).toString();
			summary = mHash.get(hash_key_summary).toString();
			if (mHash.containsKey(hash_key_musicUrl)) {
				musicUrl = mHash.get(hash_key_musicUrl).toString();
			}
			if (mHash.containsKey(hash_key_imageUrl)) {
				imageUrl = mHash.get(hash_key_imageUrl).toString();
			}
			if (mHash.containsKey(hash_key_AppName)) {
				AppName = mHash.get(hash_key_AppName).toString();
			}
			if (mHash.containsKey(hash_key_OrHideQzoneBtn)) {
				int QzoneBtnState = Integer.parseInt(mHash.get(hash_key_OrHideQzoneBtn).toString());
				OrHideQzoneBtn = ParseQzoneBtnState(QzoneBtnState);
			}
			return get_ShareQQ_bundle();
		} catch (Exception e) {
			Log.e(tag, e.getMessage());
		}
		return null;
	}
}

class QzoneShareContent {
	private final String tag = this.getClass().getName();

	// 必填， 分享的类型
	public int shareType;
	// 必填， 分享的标题, 最长QQ_SHARE_TITLE_MAX_LENGTH
	public String title;
	// 必填， 这条分享消息被好友点击后的跳转URL
	public String TargetUrl;
	// 分享的消息摘要，QQ_SHARE_SUMMARY_MAX_LENGTH
	public String summary;
	// 分享图片的URL或者本地路径
	public ArrayList<String> imageUrlList = new ArrayList<String>();
	// 分享音乐的路径
	public String musicUrl;
	// 视频分享地址
	public String videoUrl;
	// 可选，手Q客户端顶部，替换“返回”按钮文字，如果为空，用返回代替
	private String AppName;

	private final String hash_key_sharetype = "sharetype";
	private final String hash_key_title = "title";
	private final String hash_key_targetUrl = "targeturl";
	private final String hash_key_summary = "summary";
	private final String hash_key_imageUrlList = "imageUrlList";
	private final String hash_key_musicUrl = "musicUrl";
	private final String hash_key_videoUrl = "videoUrl";
	private final String hash_key_AppName = "appName";

	private static final int sharetype_IMAGE_TEXT = 1;
	private static final int sharetype_IMAGE = 2;
	private static final int sharetype_APP = 3;
	private static final int sharetype_NOTYPE = 4;

	private static final int sharetype_VEDIO = 5;
	private static final int sharetype_MOOD = 6;

	private int ParseQzoneShareType(int mType) {
		switch (mType) {
		case sharetype_IMAGE_TEXT:
			return QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE_TEXT;
		case sharetype_IMAGE:
			return QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE;
		case sharetype_APP:
			return QzoneShare.SHARE_TO_QZONE_TYPE_APP;
		case sharetype_NOTYPE:
			return QzoneShare.SHARE_TO_QZONE_TYPE_NO_TYPE;
		case sharetype_VEDIO:
			return QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHVIDEO;
		case sharetype_MOOD:
			return QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHMOOD;
		}
		return QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE_TEXT;
	}

	private Bundle GetBundle() {
		Bundle params = new Bundle();
		switch (shareType) {
		case QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE_TEXT:// 图文分享
		case QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE:// 上传图片
		case QzoneShare.SHARE_TO_QZONE_TYPE_APP:// APP分享
		case QzoneShare.SHARE_TO_QZONE_TYPE_NO_TYPE:// 没有类型
		case QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHVIDEO:// 视频分享
		case QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHMOOD:// 心情分享
			params.putInt(QzoneShare.SHARE_TO_QZONE_KEY_TYPE, shareType);// 必填
			params.putString(QzoneShare.SHARE_TO_QQ_TITLE, title);// 必填
			params.putString(QzoneShare.SHARE_TO_QQ_TARGET_URL, TargetUrl);// 必填
			if (summary != null && !summary.isEmpty()) {
				params.putString(QzoneShare.SHARE_TO_QQ_SUMMARY, summary);// 选填
			}
			if (imageUrlList != null) {
				params.putStringArrayList(QzoneShare.SHARE_TO_QQ_IMAGE_URL, imageUrlList);
			}
			if (musicUrl != null && !musicUrl.isEmpty()) {
				params.putString(QzoneShare.SHARE_TO_QQ_AUDIO_URL, musicUrl);
			}
			if (videoUrl != null && !videoUrl.isEmpty()) {
				params.putString(QzonePublish.PUBLISH_TO_QZONE_VIDEO_PATH, videoUrl);
			}
			if (AppName != null && !AppName.isEmpty()) {
				params.putString(QzoneShare.SHARE_TO_QQ_APP_NAME, AppName);
			}
			break;
		}
		return params;

	}

	@SuppressWarnings("unchecked")
	public Bundle ShareQzone(String mJsonStr) {
		try {
			Log.d(tag, "开始解析字符串");
			Map<String, Object> mHash = JsonManager.parseJsonFromStr(mJsonStr);
			Log.d(tag, "解析字符串完毕");
			int mshareType = Integer.parseInt(mHash.get(hash_key_sharetype).toString());
			shareType = ParseQzoneShareType(mshareType);
			title = mHash.get(hash_key_title).toString();
			TargetUrl = mHash.get(hash_key_targetUrl).toString();
			if (mHash.containsKey(hash_key_summary)) {
				summary = mHash.get(hash_key_summary).toString();
			}
			if (mHash.containsKey(hash_key_musicUrl)) {
				musicUrl = mHash.get(hash_key_musicUrl).toString();
			}
			if (mHash.containsKey(hash_key_imageUrlList)) {
				String imageStr = mHash.get(hash_key_imageUrlList).toString();

				String[] imageList = imageStr.split("\\|");
				for (String s : imageList) {
					imageUrlList.add(s);
				}
				Log.d(tag, "解析后的图片Url： " + imageUrlList.get(0));
			}
			if (mHash.containsKey(hash_key_AppName)) {
				AppName = mHash.get(hash_key_AppName).toString();
			}
			if (mHash.containsKey(hash_key_videoUrl)) {
				videoUrl = mHash.get(hash_key_videoUrl).toString();
			}
			return GetBundle();
		} catch (Exception e) {
			Log.e(tag, e.getMessage());
		}
		return null;
	}

	public boolean OrToSharePublic() {
		switch (shareType) {
		case QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE_TEXT:
		case QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE:
		case QzoneShare.SHARE_TO_QZONE_TYPE_APP:
		case QzoneShare.SHARE_TO_QZONE_TYPE_NO_TYPE:
			return false;
		case QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHVIDEO:
		case QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHMOOD:
			return true;
		}
		return false;
	}

}