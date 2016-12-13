package com.qq.wx.voice.demo;



import java.io.File;
import java.util.Timer;
import java.util.TimerTask;

import android.os.Handler.Callback;

import com.qq.wx.voice.R;
import com.qq.wx.voice.recognizer.VoiceRecognizerGrammar;
import com.qq.wx.voice.recognizer.VoiceRecognizerListener;
import com.qq.wx.voice.recognizer.VoiceRecognizerResult;
import com.qq.wx.voice.recognizer.VoiceRecognizerResult.Word;
import com.qq.wx.voice.recognizer.VoiceRecordState;

import android.app.Activity;
import android.graphics.Color;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

public class GrammarActivity extends Activity implements 
	VoiceRecognizerListener {
	
	int mRecoInitSucc = 0;
	
	//以下是合成页面的组件变量
	private Button mStartRecoBtn;
	private Button mCancelBtn;
	private EditText mWords;
	private TextView mResText;
	private String mSynWords = null;
	private TextView mTitle;
	
	//播放程序
	private Player mPlayer = null;
    
    //结果
    private String resFile = null;
    
    
    //识别类型
    private int mType = MainActivity.WORDLIST;
    
	//表示目前所处的状态 0:空闲状态，可进行识别； 1：正在进行录音; 2：者处于语音识别; 3：处于取消状态
	private int mRecoState = 0;
    
	private String mRecognizerResult;
	private final int mMicNum = 8;

	//定时器
	private Timer frameTimer;
	private TimerTask frameTask;

	public Handler handler = new Handler(new Callback() {
		@Override
		public boolean handleMessage(Message msg) {
			switch (msg.what) 
			{
				case 0:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait001);
					break;
				case 1:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait002);
					break;
				case 2:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait003);
					break;
				case 3:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait004);
					break;
				case 4:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait005);
					break;
				case 5:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait006);
					break;
				case 6:
					mStartRecoBtn.setBackgroundResource(R.drawable.recowait007);
					break;
				default:
					mStartRecoBtn.setBackgroundResource(R.drawable.recogstart);
			}
			return false;
		}
	});

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
		VoiceRecognizerGrammar.shareInstance().setSilentTime(1000);
		VoiceRecognizerGrammar.shareInstance().setListener(this);
		
		mRecoInitSucc = VoiceRecognizerGrammar.shareInstance().init(this, MainActivity.screKey);
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
				if (0 == mRecoState) {
					if (0 == startRecognizer()) {
						mRecoState = 1;
						mCancelBtn.setBackgroundResource(R.drawable.recogcancel);
						mCancelBtn.setEnabled(true);
					}
				}
				else if (1 == mRecoState) {
					VoiceRecognizerGrammar.shareInstance().stop();
					mStartRecoBtn.setEnabled(false);
				}
			}
		});

		mCancelBtn = (Button) findViewById(R.id.cancel);
		mCancelBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				if (1 == mRecoState || 2 == mRecoState) {
					setCancelBtn(false);
					int ret = VoiceRecognizerGrammar.shareInstance().cancel();
					if (ret != 0) {
						cancelTask();
						mResText.setText("点击开始说话");
						mRecoState = 0;
						setStartBtn(true);
						
					}
				}
			}
		});
	}
	
	
	private int startRecognizer() {
		String text = mWords.getText().toString();
		if (0 == VoiceRecognizerGrammar.shareInstance().start(text, mType)) {
			return 0;
		}
		mResText.setText("启动失败");
		mStartRecoBtn.setEnabled(true);
		return -1;
		
	}
	
	@Override
	public void onGetError(int errorCode) {
		// TODO Auto-generated method stub
		mResText.setText("ErrorCode = " + errorCode);
		setWords(true);
		mRecoState = 0;
		setStartBtn(true);
		setCancelBtn(false);
	}

	//重置返回按钮
	@Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (keyCode == KeyEvent.KEYCODE_BACK) {
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
		VoiceRecognizerGrammar.shareInstance().destroy();
	}

	@Override
	public void onGetResult(VoiceRecognizerResult result) {
		
		
		cancelTask();
		
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
		setCancelBtn(false);
		setStartBtn(true);
		mRecoState = 0;
	}

	@Override
	public void onVolumeChanged(int volume) {

		int index = volume;
		if (index < 0) {
			index = 0;
		} else if (index >= mMicNum) {
			index = mMicNum - 1;
		}
		if (mStartRecoBtn != null && 1 == mRecoState) {
			switch (index) {
			case 0:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog001);
				break;
			case 1:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog002);
				break;
			case 2:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog003);
				break;
			case 3:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog004);
				break;
			case 4:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog005);
				break;
			case 5:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog006);
				break;
			case 6:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog007);
				break;
			case 7:
				mStartRecoBtn.setBackgroundResource(R.drawable.recog008);
				break;
			default:
				mStartRecoBtn.setBackgroundResource(R.drawable.recogstart);
			}
		}
	}


	@Override
	public void onGetVoiceRecordState(VoiceRecordState state) {
		if (state == VoiceRecordState.Start) {
			mResText.setText("语音已开启，请说话…");
		} else if (state == VoiceRecordState.Complete) {
			mStartRecoBtn.setEnabled(false);

			mResText.setText("识别中...");
			mRecoState = 2;
			startTask();
			
		} else if (state == VoiceRecordState.Canceling) {
			mRecoState = 3;
			setStartBtn(false);
			mResText.setText("正在取消");
		} else if (state == VoiceRecordState.Canceled) {
			cancelTask();
			mResText.setText("点击开始说话");
			mRecoState = 0;
			setCancelBtn(false);
			setStartBtn(true);
		}
		
	}
	
	private void setStartBtn(boolean enabled) {
		if (null != mStartRecoBtn) {
			if (true == enabled) {
				mStartRecoBtn.setEnabled(enabled);
				mStartRecoBtn.setBackgroundResource(R.drawable.recogstart);
			}
			else {
				mStartRecoBtn.setEnabled(enabled);
				mStartRecoBtn.setBackgroundResource(R.drawable.recoggray);
			}
		}
	}
	
	public void startTask() {
		frameTimer = new Timer(false);
		frameTask = new TimerTask() {
			int btnIndex = 0;
			@Override
			public void run() {
				int index = (btnIndex++) % 8;
				Message message = new Message();
				message.what = index;
				handler.sendMessage(message);
			}
		};
		frameTimer.schedule(frameTask, 200, 100);
	}
	
	public void cancelTask() {
		if (null != frameTask) {
			frameTask.cancel();
		}
		if (null != frameTimer) {
			frameTimer.cancel();
		}
	}

}