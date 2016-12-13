package com.xk.project;

import com.unity3d.player.UnityPlayerActivity;
import com.xk.sharesdk.UnityShareManager;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

public class MainActivity extends UnityPlayerActivity
{
	private final String tag=this.getClass().getName();
    @Override
    protected void onCreate(Bundle savedInstanceState) 
    {
        super.onCreate(savedInstanceState); 
        Log.d(tag,"onCreate");
    }
    
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) 
	{
		Log.d(tag,"onActivityResult");
		UnityShareManager.single.onActivityResult(requestCode, resultCode, data);
	}
}
