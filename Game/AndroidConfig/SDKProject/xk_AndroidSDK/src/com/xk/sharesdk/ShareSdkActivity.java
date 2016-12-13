package com.xk.sharesdk;

import com.xk.platform.qq.QQSDK;

import android.app.Activity;
import android.content.Intent;


public class ShareSdkActivity extends Activity
{	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) 
	{
		QQSDK.getSingle().onActivityResult(requestCode, resultCode, data);
	} 
	
}
