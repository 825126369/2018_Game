package com.qq.wx.voiceui.demo;


import com.qq.wx.voiceui.R;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.widget.Button;

public class MainActivity extends Activity {
	
	String TAG = "MainActivity";

	/**
	 * 主页的组件变量
	 */
	private Button recoBtn;
	private Button ttsBtn;
	private Button listBtn;
	private Button graBtn;
	
	/**
	 * 官网申请的KEY值
	 */
	public static final String screKey = "248b63f1ceca9158ca88516bcb338e82a482ecd802cbca12";

	/**
	 * 语法识别变量
	 */
	public static final int ABNF = 0;

	/**
	 * 关键词识别变量
	 */
	public static final int WORDLIST = 1;

	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		initMainUI();
	}
	
	private void initMainUI() {
		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE); 
		setContentView(R.layout.activity_main);
		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,R.layout.title);
		
		recoBtn = (Button) findViewById(R.id.reco);
		recoBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				turnToReco();
			}
		});

		ttsBtn = (Button) findViewById(R.id.tts);
		ttsBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				turnToTTS();
			}
		});

		listBtn = (Button) findViewById(R.id.list);
		listBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				turnToGrammar(WORDLIST);
			}
		});

		graBtn = (Button) findViewById(R.id.abnf);
		graBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				turnToGrammar(ABNF);
			}
		});
	}
	
	private void turnToReco() {
		Intent intent = new Intent();  
        intent.setClass(this, RecoActivityUI.class);  
        startActivity(intent);  
	}

	private void turnToTTS() {
		Intent intent = new Intent();  
        intent.setClass(this, TTSActivity.class);  
        startActivity(intent);  
	}

	private void turnToGrammar(int Type) {
		Bundle bundle = new Bundle();
		bundle.putInt("TYPE", Type);
		Intent intent = new Intent();  
        intent.setClass(this, GrammarActivityUI.class);  
        intent.putExtras(bundle);
        startActivity(intent);  
	}
	
}