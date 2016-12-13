package com.qq.wx.voiceui.demo;



import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;

import com.qq.wx.voice.synthesizer.SpeechSynthesizer;
import com.qq.wx.voice.synthesizer.SpeechSynthesizerResult;
import com.qq.wx.voice.synthesizer.TextSenderListener;
import com.qq.wx.voice.synthesizer.TextSenderResult;
import com.qq.wx.voice.synthesizer.TextSenderState;
import com.qq.wx.voiceui.R;

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.os.Bundle;
import android.os.Environment;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

public class TTSActivity extends Activity implements TextSenderListener,
	Player.PlayerCallback {

	int mInitSucc = 0;
	
	//以下是合成页面的组件变量
	private Button mStartBtn;
	private Button mCancelBtn;
	private EditText mWords;
	private String mSynWords = null;
	private TextView mTitle;
	
	//播放程序
	private Player mPlayer = null;
    
    //结果
    private String resFile = null;
    
    //音频类型
    private int voiceFormat = 0;
    
    //tts状态， 0：未开始， 1：正在合成， 2：正在播放， 3：暂停, 4: 已退出
    private int mState = 0;
    
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		initSynUI();
		preInitTTS();
		mPlayer = new Player(this);
	}
	
	private void preInitTTS() {
		SpeechSynthesizer.shareInstance().setListener(this);
		SpeechSynthesizer.shareInstance().setFormat(0);
		SpeechSynthesizer.shareInstance().setVolume(1.0f);
		
		mInitSucc = SpeechSynthesizer.shareInstance().init(this, MainActivity.screKey);

		if (mInitSucc != 0) {
			Toast.makeText(this, "初始化失败",
					Toast.LENGTH_SHORT).show();
		}
	}
	
	private void initSynUI() {
		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE); 
		setContentView(R.layout.syn_demo);
		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,R.layout.title);
		mTitle = (TextView) findViewById(R.id.toptitle);
		mTitle.setText("语音合成");

		mWords = (EditText) findViewById(R.id.et_words);
		//水平滚动设置为False  
		String testText = "微信语音开放平台，免费给移动开发者提供语音云服务，目前开放的有语音识别、语音合成等。"
				+ "其中语音识别可以让有文字输入的地方用语音输入来代替，准确率达90%以上；"
				+ "语音合成支持把文字合成甜美女声。从此让你的用户与手机自由语音交互，体验别致的移动生活！";
		mWords.setText(testText);
		mStartBtn = (Button) findViewById(R.id.syn_start);
		mStartBtn.setOnClickListener(new View.OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				if (0 == mState) {
					mSynWords = mWords.getText().toString();
					startSyn();
					setCancelBtn(true);
				}
				else if (3 == mState) {
					mPlayer.play();
				}
				else {
					mPlayer.pause();
				}
			}
		});

		mCancelBtn = (Button) findViewById(R.id.cancel);
		mCancelBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				int ret = 0;
				if (1 == mState) {
					ret = SpeechSynthesizer.shareInstance().cancel();
				}
				if (0 != ret) {
					mState = 0;
					mStartBtn.setEnabled(true);
					mStartBtn.setBackgroundResource(R.drawable.synplay);
				}

				if (null != resFile) {
					File voiceFile = new File(resFile);
					if (voiceFile.exists()) {
						if (mPlayer != null) {
							mPlayer.stop();
							setWords(true);
							setCancelBtn(false);
							voiceFile.delete();
						}
					}
					resFile = null;
				}
				setCancelBtn(false);

			}
		});
	}
	
	private void startSyn() {
		if (mInitSucc != 0) {
			mInitSucc = SpeechSynthesizer.shareInstance().init(this, MainActivity.screKey);
		}
		if (mInitSucc != 0) {
			Toast.makeText(this, "初始化失败",
					Toast.LENGTH_SHORT).show();
			return;
		}

		int ret = SpeechSynthesizer.shareInstance().start(mSynWords);
		if (0 == ret) {
			mStartBtn.setBackgroundResource(R.drawable.synplayp);
			mState = 1;
			mStartBtn.setEnabled(false);
			return;
		}
		else if (-402 == ret) {
			Toast.makeText(this, "ErrorCode = " + ret +"; 文本不能为空", Toast.LENGTH_LONG).show();
		}
		else if (-403 == ret) {
			Toast.makeText(this, "ErrorCode = " + ret +"; 字符数超过1024", Toast.LENGTH_LONG).show();
		}
		else {
			Toast.makeText(this, "ErrorCode = " + ret, Toast.LENGTH_LONG).show();
		}
	}
	
	@Override
	public void onGetResult(TextSenderResult arg0) {
		SpeechSynthesizerResult result = (SpeechSynthesizerResult)arg0;
		byte [] speech = result.speech;
		
		String filepath = null;
		boolean sdCardExist = Environment.getExternalStorageState()
				.equals(android.os.Environment.MEDIA_MOUNTED); //判断sd卡是否存在
		if (!sdCardExist) {
			/**
			 * 写手机内部存储
			 */
			@SuppressWarnings("deprecation")
			File mediaFilesDir = getDir("mediaFiles", Context.MODE_WORLD_READABLE); 
			filepath = mediaFilesDir.getPath();
		}
		else {
			filepath = Environment.getExternalStorageDirectory().getPath()
					+ "/Tencent/mm";
			File outputpath = new File(filepath);
			if (!outputpath.exists()) {
				outputpath.mkdirs();
			}
		}

		//**测试用
		Date date = new Date();
		SimpleDateFormat dateFormat = new SimpleDateFormat("yyyyMMddHHmmss");
		String dateStr = dateFormat.format(date);
		String voiceType = ".mp3";
		if (voiceFormat == 0) {
			voiceType = ".mp3";
		}
		else if (voiceFormat == 1) {
			voiceType = ".wav";
		}
		else if (voiceFormat == 2) {
			voiceType = ".amr";
		}
		String voiceFileName = filepath + "/" + dateStr + voiceType;
		File voiceFile = new File(voiceFileName);
		if (!voiceFile.exists()) {
			try {
				voiceFile.createNewFile();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			};
		}
		FileOutputStream voiceOutputStream = null;
		try {
			voiceOutputStream = new FileOutputStream(voiceFile);
		} catch (FileNotFoundException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
		if (speech.length == 0) {
			mStartBtn.setBackgroundResource(R.drawable.synplay);
			return;
		}
		
		try {
			voiceOutputStream.write(speech);
			voiceOutputStream.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		resFile = voiceFileName;
		try {
			if (4 != mState) {
				mPlayer.playFile(resFile);
				mStartBtn.setEnabled(true);
				mState = 2;
				mStartBtn.setBackgroundResource(R.drawable.synpause);
			}
		} catch (IllegalArgumentException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (SecurityException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IllegalStateException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

	}

	@Override
	public void onGetError(int errorCode) {
		// TODO Auto-generated method stub
		Toast.makeText(this, "ErrorCode = " + errorCode, Toast.LENGTH_LONG).show();
		setCancelBtn(false);
		mState = 0;
		mStartBtn.setEnabled(true);
		mStartBtn.setBackgroundResource(R.drawable.synplay);
		setWords(true);
	}

	@Override
	public void onGetVoiceRecordState(TextSenderState state) {
		// TODO Auto-generated method stub
		if (TextSenderState.Canceled == state) {
			setCancelBtn(false);
			mState = 0;
			mStartBtn.setEnabled(true);
			mStartBtn.setBackgroundResource(R.drawable.synplay);
			setWords(true);
		}
		
	}

	//以下代码用于处理音频播放
	@Override
	public void onGetPlayerStatePlaying() {
		// TODO Auto-generated method stub
		mState = 2;
		mStartBtn.setBackgroundResource(R.drawable.synpause);
		
		setWords(false);
	}

	@Override
	public void onGetPlayerStatePause() {
		// TODO Auto-generated method stub
		mState = 3;
		mStartBtn.setBackgroundResource(R.drawable.synplay);
		
	}

	@Override
	public void onGetPlayerStateStop() {
		// TODO Auto-generated method stub
		mState = 0;
		mStartBtn.setBackgroundResource(R.drawable.synplay);
		setWords(true);
		setCancelBtn(false);
	}

	//重置返回按钮
	@Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (keyCode == KeyEvent.KEYCODE_BACK) {
        	mState = 4;
        	if (resFile != null) {
        		File voiceFile = new File(resFile);
				if (mPlayer != null) {
					mPlayer.stop();
					mPlayer.release();
					voiceFile.delete();
				}
        	}
        	finish();
        }
        return super.onKeyDown(keyCode, event);
    }
	
	//以下设置一些按键的特性
	public void setWords(boolean enabled) {
		if (null != mWords) {
			if (true == enabled) {
				mWords.setFocusableInTouchMode(true);
				mWords.setFocusable(true);
				mWords.setTextColor(Color.rgb(255, 255, 255));
			}
			else {
				mWords.setFocusable(false);
				mWords.setTextColor(Color.rgb(200, 200, 200));
			}
		}
	}
	
	public void setCancelBtn(boolean enabled) {
		if (null != mCancelBtn) {
			if (true == enabled) {
				mCancelBtn.setEnabled(true);
				mCancelBtn.setBackgroundResource(R.drawable.recogcancel);
			}
			else {
				mCancelBtn.setEnabled(false);
				mCancelBtn.setBackgroundResource(R.drawable.recogcancelgray);
			}
		}
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		SpeechSynthesizer.shareInstance().destroy();
	}

}