package com.qq.wx.voiceui.demo;

import com.qq.wx.voiceui.R;
import com.qq.wx.voice.recognizer.VoiceRecognizerDialog;
import com.qq.wx.voice.recognizer.VoiceRecognizerListener;
import com.qq.wx.voice.recognizer.VoiceRecognizerResult;
import com.qq.wx.voice.recognizer.VoiceRecordState;
import com.qq.wx.voice.recognizer.VoiceRecognizerResult.Word;

import android.annotation.SuppressLint;
import android.app.ActionBar.LayoutParams;
import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.PopupWindow;
import android.widget.TextView;
import android.widget.Toast;

@SuppressLint("InlinedApi")
public class RecoActivityUI extends Activity  implements VoiceRecognizerListener {

	int mInitSucc = 0;
	
	
	
	//以下是识别页面的组件变量
	private Button mStartBtn;
	
	private Button mCancelBtn;
	
	private TextView mStartRecordTv;
	
	private TextView mTitle;
	
	private String mRecognizerResult;
	
	private PopupWindow mPop;
	private View layout;
	private Button goMailBtn = null;
	
	/**
	 * 带UI的语音识别
	 */
	private VoiceRecognizerDialog mVoiceRecognizerDialog;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		initRecognizerUI();
	}
	
	@Override
	public void  onResume() {
		super.onResume();
		preInitVoiceRecognizer();
	}
	
	private void preInitVoiceRecognizer() {
		mVoiceRecognizerDialog = new VoiceRecognizerDialog(this, -1);
		mVoiceRecognizerDialog.setSilentTime(1000);
		mVoiceRecognizerDialog.setOnRecognizerResultListener(this);
		int ret = mVoiceRecognizerDialog.init(MainActivity.screKey);
		if (0 != ret) {
			Toast.makeText(this, "failed to init", Toast.LENGTH_LONG).show();
		}
	}
	
	private void initRecognizerUI() {
		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE); 
		setContentView(R.layout.voicerecognizer_demo);
		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,R.layout.title);
		mTitle = (TextView) findViewById(R.id.toptitle);
		mTitle.setText("语音识别");

		mStartRecordTv = (TextView) findViewById(R.id.start_record);
		mStartRecordTv.setMovementMethod(ScrollingMovementMethod.getInstance());
		
		layout = View.inflate(this, R.layout.window, null);
		
		mStartBtn = (Button) findViewById(R.id.complete);

		mStartBtn.setOnClickListener(new View.OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				mStartRecordTv.setText("");
				mVoiceRecognizerDialog.show();
			}
		});

		mCancelBtn = (Button) findViewById(R.id.cancel);
		mCancelBtn.setVisibility(View.GONE);
	}
	
	@SuppressLint("InlinedApi")
	private void initPopWindow() {
		if (mPop == null) {
			goMailBtn = (Button) layout.findViewById(R.id.hint3);
			goMailBtn.setOnClickListener(new View.OnClickListener() {
				
				@Override
				public void onClick(View v) {
					// TODO Auto-generated method stub
					Intent data=new Intent(Intent.ACTION_SENDTO);    
					data.setData(Uri.parse("mailto:prteam@tencent.com"));    
					startActivity(data); 
				}
			});
			mPop = new PopupWindow(layout,
					LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
		}
		if (mPop.isShowing()) {
			mPop.dismiss();
		}
	}
	
	@Override
	protected void onDestroy() {
		mVoiceRecognizerDialog.onDestroy();
		super.onDestroy();
	}

	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
	    if(keyCode == KeyEvent.KEYCODE_BACK) {
	        // 监控返回键
	    	if (mVoiceRecognizerDialog.isShowing()) {
	    		mVoiceRecognizerDialog.onDismiss();
	    	}
	    	else {
	    		finish();
	    	}
	    }
	    return super.onKeyDown(keyCode, event);
	}
	
	@SuppressLint("ShowToast")
	@Override
	public void onGetResult(VoiceRecognizerResult result) {
		
		
		mRecognizerResult = "";
		if (result != null && result.words != null) {
			int wordSize = result.words.size();
			StringBuilder results = new StringBuilder();
			for (int i = 0; i<wordSize; ++i) {
				Word word = (Word) result.words.get(i);
				if (word != null && word.text != null) {
					results.append("\r\n");
					results.append(word.text.replace(" ", ""));
				}
			}
			results.append("\r\n");
			mRecognizerResult = results.toString();
		}
		mStartRecordTv.setText(mRecognizerResult);
	}

	@Override
	public void onVolumeChanged(int volume) {
	}

	@Override
	public void onGetError(int errorCode) {
	}

	@Override
	public void onGetVoiceRecordState(VoiceRecordState state) {
	}
	
}