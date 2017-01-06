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
		Log.d(tag, "����QQƽ̨��Ϣ��" + mQQPlatformInfo.appId);
	}

	public void ShareToQQ(final String jsonStr) {

		// QQ����Ҫ�����߳���
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

	/// �����Ա�����������ɹ���û�лص���Ϣ
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		Tencent.onActivityResultData(requestCode, resultCode, data, qqShareListener);
	}

}

class QQShareContent {
	private final String tag = this.getClass().getName();

	// ��� ���������
	private int shareType;
	// ��� ����ı���, �QQ_SHARE_TITLE_MAX_LENGTH
	private String title;
	// ��� ����������Ϣ�����ѵ�������תURL
	private String TargetUrl;
	// �������ϢժҪ��QQ_SHARE_SUMMARY_MAX_LENGTH
	private String summary;
	// ����ͼƬ��URL���߱���·��
	private String imageUrl;
	// �������ֵ�·��
	private String musicUrl;
	// ��ѡ����Q�ͻ��˶������滻�����ء���ť���֣����Ϊ�գ��÷��ش���
	private String AppName;
	// ��ѡ�� Ĭ���ǲ����ط���QZone��ť�Ҳ��Զ��򿪷���QZone�ĶԻ�,
	// Tencent.SHARE_TO_QQ_FLAG_QZONE_AUTO_OPEN������ʱ�Զ��� ������QZone�ĶԻ���
	// Tencent.SHARE_TO_QQ_FLAG_QZONE_ITEM_HIDE������ʱ���ط��� ��QZone��ť
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
		case QQShare.SHARE_TO_QQ_TYPE_IMAGE:// ��ͼƬ����
		case QQShare.SHARE_TO_QQ_TYPE_DEFAULT:// ͼ�ķ���
		case QQShare.SHARE_TO_QQ_TYPE_AUDIO:// ���ַ���
		case QQShare.SHARE_TO_QQ_TYPE_APP:// app����
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
			// System.out.printIn("QQ�������ʹ���");
			Log.e(this.getClass().getName(), "QQ�������ʹ���");
			break;
		}
		Log.d(tag, "�õ��������");
		return mBundle;
	}

	public Bundle ShareQQ(String mJsonStr) {
		try {
			Log.d(tag, "��ʼ�����ַ���");
			Map<String, Object> mHash = JsonManager.parseJsonFromStr(mJsonStr);
			Log.d(tag, "�����ַ������");
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

	// ��� ���������
	public int shareType;
	// ��� ����ı���, �QQ_SHARE_TITLE_MAX_LENGTH
	public String title;
	// ��� ����������Ϣ�����ѵ�������תURL
	public String TargetUrl;
	// �������ϢժҪ��QQ_SHARE_SUMMARY_MAX_LENGTH
	public String summary;
	// ����ͼƬ��URL���߱���·��
	public ArrayList<String> imageUrlList = new ArrayList<String>();
	// �������ֵ�·��
	public String musicUrl;
	// ��Ƶ�����ַ
	public String videoUrl;
	// ��ѡ����Q�ͻ��˶������滻�����ء���ť���֣����Ϊ�գ��÷��ش���
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
		case QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE_TEXT:// ͼ�ķ���
		case QzoneShare.SHARE_TO_QZONE_TYPE_IMAGE:// �ϴ�ͼƬ
		case QzoneShare.SHARE_TO_QZONE_TYPE_APP:// APP����
		case QzoneShare.SHARE_TO_QZONE_TYPE_NO_TYPE:// û������
		case QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHVIDEO:// ��Ƶ����
		case QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHMOOD:// �������
			params.putInt(QzoneShare.SHARE_TO_QZONE_KEY_TYPE, shareType);// ����
			params.putString(QzoneShare.SHARE_TO_QQ_TITLE, title);// ����
			params.putString(QzoneShare.SHARE_TO_QQ_TARGET_URL, TargetUrl);// ����
			if (summary != null && !summary.isEmpty()) {
				params.putString(QzoneShare.SHARE_TO_QQ_SUMMARY, summary);// ѡ��
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
			Log.d(tag, "��ʼ�����ַ���");
			Map<String, Object> mHash = JsonManager.parseJsonFromStr(mJsonStr);
			Log.d(tag, "�����ַ������");
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
				Log.d(tag, "�������ͼƬUrl�� " + imageUrlList.get(0));
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