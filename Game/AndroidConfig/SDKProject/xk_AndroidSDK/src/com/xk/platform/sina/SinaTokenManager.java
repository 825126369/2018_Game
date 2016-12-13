package com.xk.platform.sina;

import java.text.SimpleDateFormat;
import java.util.Date;

import com.sina.weibo.sdk.auth.Oauth2AccessToken;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.util.Log;

public class SinaTokenManager
{
	private static final String tag = SinaTokenManager.class.getName();
	private static final String PREFERENCES_NAME = "com_xk_project_sina_token";

	private static final String KEY_UID = "uid";
	private static final String KEY_ACCESS_TOKEN = "access_token";
	private static final String KEY_EXPIRES_IN = "expires_in"; 
	private static final String KEY_REFRESH_TOKEN = "refresh_token";

	public static void writeAccessToken(Context context, Oauth2AccessToken token) {
		if (null == context || null == token) {
			Log.e(tag, "context is null Or token is null");
			return;
		}

		SharedPreferences pref = context.getSharedPreferences(PREFERENCES_NAME, Context.MODE_APPEND);
		Editor editor = pref.edit();
		editor.putString(KEY_UID, token.getUid());
		editor.putString(KEY_ACCESS_TOKEN, token.getToken());
		editor.putString(KEY_REFRESH_TOKEN, token.getRefreshToken());
		editor.putLong(KEY_EXPIRES_IN, token.getExpiresTime());
		editor.commit();
	}

	public static Oauth2AccessToken readAccessToken(Context context) {
		if (null == context) {
			return null;
		}
		Oauth2AccessToken token = new Oauth2AccessToken();
		SharedPreferences pref = context.getSharedPreferences(PREFERENCES_NAME, Context.MODE_APPEND);
		token.setUid(pref.getString(KEY_UID, ""));
		token.setToken(pref.getString(KEY_ACCESS_TOKEN, ""));
		token.setRefreshToken(pref.getString(KEY_REFRESH_TOKEN, ""));
		token.setExpiresTime(pref.getLong(KEY_EXPIRES_IN, 0));

		GetTokenLog(token);
		return token;
	}

	public static void clear(Context context) {
		if (null == context) {
			return;
		}

		SharedPreferences pref = context.getSharedPreferences(PREFERENCES_NAME, Context.MODE_APPEND);
		Editor editor = pref.edit();
		editor.clear();
		editor.commit();
	}

	public  static void GetTokenLog(Oauth2AccessToken mAccessToken) {
		if (mAccessToken == null) {
			Log.e(tag, "token is null");
			return;
		}
		String date = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss").format(new Date(mAccessToken.getExpiresTime()));

		Log.d(tag, "token info: ");
		Log.d(tag, "uid: " + mAccessToken.getUid());
		Log.d(tag, "token: " + mAccessToken.getToken());
		Log.d(tag, "refresh token: " + mAccessToken.getRefreshToken());
		Log.d(tag, "expiresTime: " + mAccessToken.getExpiresTime() + " | " + date);

	}

}
