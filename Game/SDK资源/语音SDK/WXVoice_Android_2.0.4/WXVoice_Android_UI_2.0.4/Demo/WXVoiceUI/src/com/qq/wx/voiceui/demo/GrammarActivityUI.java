package com.qq.wx.voiceui.demo;

import com.qq.wx.voice.recognizer.VoiceRecognizerDialog;
import com.qq.wx.voice.recognizer.VoiceRecognizerListener;
import com.qq.wx.voice.recognizer.VoiceRecognizerResult;
import com.qq.wx.voice.recognizer.VoiceRecognizerResult.Word;
import com.qq.wx.voice.recognizer.VoiceRecordState;
import com.qq.wx.voiceui.R;

import android.app.Activity;
import android.graphics.Color;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

public class GrammarActivityUI extends Activity implements 
	VoiceRecognizerListener {
	
	int mRecoInitSucc = 0;
	
	//以下是合成页面的组件变量
	private Button mStartRecoBtn;
	private Button mCancelBtn;
	private EditText mWords;
	private TextView mResText;
	private String mSynWords = null;
	private TextView mTitle;
	
    //识别类型
    private int mType = MainActivity.WORDLIST;
    
	private String mRecognizerResult;
	
	/**
	 * 带UI的语音识别
	 */
	private VoiceRecognizerDialog mVoiceRecognizerDialog;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		Bundle bundle = getIntent().getExtras();
		mType = bundle.getInt("TYPE");
		initSynUI();
		preInit();
	}
	
	private void preInit() {
		//setSilentTime参数单位为微秒：1秒=1000毫秒
		mVoiceRecognizerDialog = new VoiceRecognizerDialog(this, mType);
		mVoiceRecognizerDialog.setSilentTime(1000);
		mVoiceRecognizerDialog.setOnRecognizerResultListener(this);
		
		mRecoInitSucc = mVoiceRecognizerDialog.init(MainActivity.screKey);
		if (mRecoInitSucc != 0) {
			mResText.setText("语音识别引擎初始化失败");
		}
	}
	
	private void initSynUI() {
		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE); 
		setContentView(R.layout.grammar_demo);
		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,R.layout.title);
		mTitle = (TextView) findViewById(R.id.toptitle);

		mWords = (EditText) findViewById(R.id.et_words);
		mResText = (TextView) findViewById(R.id.grammar_res);
		//水平滚动设置为False  
		if (MainActivity.ABNF == mType) {
			mTitle.setText("ABNF语法识别");
			mSynWords = "#ABNF 1.0 UTF-8;\r\n"
					+ "language zh-cn;\r\n"
					+ "mode voice;\r\n"
					+ "root $basicCmd;\r\n"
					+ "public $basicCmd =[查询] $allnames  天气;\r\n"
					+ "$allnames = (北京 | 上海 | 天津 | 广州 | 四川) [地区];";
		}
		else if (MainActivity.WORDLIST == mType) {
			mTitle.setText("词表识别");
			mSynWords = "崔大勺 | 张小萌 | 金秀贤  | 李若曦 | 杨不悔 | 叶宁远 | 丰云 | 刘星;";
		}
		mWords.setText(mSynWords);

		mStartRecoBtn = (Button) findViewById(R.id.gra_reco_start);
		mStartRecoBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				mResText.setText("");
				String text = mWords.getText().toString();
				mVoiceRecognizerDialog.show(text, mType);
			}
		});

		mCancelBtn = (Button) findViewById(R.id.cancel);
		mCancelBtn.setVisibility(View.GONE);
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
	
	@Override
	public void onGetError(int errorCode) {
		// TODO Auto-generated method stub
		setWords(true);
	}

	@Override
	public void onGetResult(VoiceRecognizerResult result) {
		mRecognizerResult = "";
		if (result != null && result.words != null) {
			int wordSize = result.words.size();
			StringBuilder results = new StringBuilder();
			for (int i = 0; i<wordSize; ++i) {
				Word word = (Word) result.words.get(i);
				if (word != null && word.text != null) {
					results.append(word.text.replace(" ", ""));
					results.append("\r\n");
				}
			}
			mRecognizerResult = results.toString();
		}
		mResText.setText("识别结果:" + mRecognizerResult);
	}

	@Override
	public void onVolumeChanged(int volume) {
	}


	@Override
	public void onGetVoiceRecordState(VoiceRecordState state) {
	}
}