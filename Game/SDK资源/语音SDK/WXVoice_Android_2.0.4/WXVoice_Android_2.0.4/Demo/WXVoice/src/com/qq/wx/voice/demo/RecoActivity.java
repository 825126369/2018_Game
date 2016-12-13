package com.qq.wx.voice.demo;


import java.util.Timer;
import java.util.TimerTask;

import com.qq.wx.voice.R;
import com.qq.wx.voice.recognizer.VoiceRecognizer;
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
import android.os.Handler;
import android.os.Message;
import android.os.Handler.Callback;
import android.text.method.ScrollingMovementMethod;
import android.view.Gravity;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.PopupWindow;
import android.widget.TextView;
import android.widget.Toast;

@SuppressLint("InlinedApi")
public class RecoActivity extends Activity  implements VoiceRecognizerListener {
	int mInitSucc = 0;
	
	//以下是识别页面的组件变量
	private Button mCompleteBtn;
	
	private Button mCancelBtn;
	
	private TextView mStartRecordTv;
	
	private String mRecognizerResult;
	
	private final int mMicNum = 8;

	private TextView mTitle;
	
	//表示目前所处的状态 0:空闲状态，可进行识别； 1：正在进行录音; 2：者处于语音识别; 3：处于取消状态
	private int mRecoState = 0;
	
	//定时器
	private Timer frameTimer;
	private TimerTask frameTask;
	
	private PopupWindow mPop;
	private View layout;
	private Button goMailBtn = null;
	
	public Handler handler = new Handler(new Callback() {
		@Override
		public boolean handleMessage(Message msg) {
			switch (msg.what) 
			{
				case 0:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait001);
					break;
				case 1:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait002);
					break;
				case 2:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait003);
					break;
				case 3:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait004);
					break;
				case 4:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait005);
					break;
				case 5:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait006);
					break;
				case 6:
					mCompleteBtn.setBackgroundResource(R.drawable.recowait007);
					break;
				default:
					mCompleteBtn.setBackgroundResource(R.drawable.recogstart);
			}
			return false;
		}
	});

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		initRecognizerUI();
	}
	
	@Override
	public void  onResume() {
		super.onResume();
		//preInitVoiceRecognizer();
	}
	
	private void preInitVoiceRecognizer() {
		//setSilentTime参数单位为微秒：1秒=1000毫秒
		VoiceRecognizer.shareInstance().setSilentTime(1000);
		VoiceRecognizer.shareInstance().setListener(this);
		//VoiceRecognizer.shareInstance().setDomain("113.108.82.81", 8080);
		//VoiceRecognizer.shareInstance().setDomain("newsso.map.qq.com", 8080, "newsso.map.qq.com:8080");
		//VoiceRecognizer.shareInstance().setUri("/voice/fetchinfo");
		//VoiceRecognizer.shareInstance().setGetPureRes(true);
		
		mInitSucc = VoiceRecognizer.shareInstance().init(this, MainActivity.screKey);
		if (mInitSucc != 0) {
			Toast.makeText(this, "初始化失败",
					Toast.LENGTH_SHORT).show();
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
		
		mCompleteBtn = (Button) findViewById(R.id.complete);
		mCancelBtn = (Button) findViewById(R.id.cancel);
		mCancelBtn.setEnabled(false);

		mCompleteBtn.setOnClickListener(new View.OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				if (0 == mRecoState) {
					preInitVoiceRecognizer();
					if (0 == startRecognizer()) {
						//int ret = VoiceRecognizer.shareInstance().cancel();
						//Log.d(TAG, "cancel ret = " + ret);

						mRecoState = 1;
						mCancelBtn.setBackgroundResource(R.drawable.recogcancel);
						mCancelBtn.setEnabled(true);
					}
				}
				else if (1 == mRecoState) {
					VoiceRecognizer.shareInstance().stop();
					mCompleteBtn.setEnabled(false);
				}
			}
		});

		mCancelBtn.setOnClickListener(new View.OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				setCancelBtn(false);
				setStartBtn(false);
				int ret = 0;
				ret = VoiceRecognizer.shareInstance().cancel();
				if (ret != 0) {
					cancelTask();
					mStartRecordTv.setText("点击开始说话");
					mRecoState = 0;
					setStartBtn(true);
				}
			}
		});
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
	
	private int startRecognizer() {
		if (0 == VoiceRecognizer.shareInstance().start()) {
			mStartRecordTv.setText("语音已开启，请说话…");
			return 0;
		}
		mStartRecordTv.setText("启动失败");
		mCompleteBtn.setEnabled(true);
		return -1;
		
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		VoiceRecognizer.shareInstance().destroy();
	}
	
	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
	    if(keyCode == KeyEvent.KEYCODE_BACK) {
	        // 监控返回键
	    	finish();
	    }
	    return super.onKeyDown(keyCode, event);
	}
	
	@SuppressLint("ShowToast")
	@Override
	public void onGetResult(VoiceRecognizerResult result) {
		cancelTask();

		mRecognizerResult = "";

		//if (1 == result.type) {
		//	Log.d(TAG, "result = " + new String(result.httpRes));
		//}

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
		if (mCompleteBtn != null && 1 == mRecoState) {
			switch (index) {
			case 0:
				mCompleteBtn.setBackgroundResource(R.drawable.recog001);
				break;
			case 1:
				mCompleteBtn.setBackgroundResource(R.drawable.recog002);
				break;
			case 2:
				mCompleteBtn.setBackgroundResource(R.drawable.recog003);
				break;
			case 3:
				mCompleteBtn.setBackgroundResource(R.drawable.recog004);
				break;
			case 4:
				mCompleteBtn.setBackgroundResource(R.drawable.recog005);
				break;
			case 5:
				mCompleteBtn.setBackgroundResource(R.drawable.recog006);
				break;
			case 6:
				mCompleteBtn.setBackgroundResource(R.drawable.recog007);
				break;
			case 7:
				mCompleteBtn.setBackgroundResource(R.drawable.recog008);
				break;
			default:
				mCompleteBtn.setBackgroundResource(R.drawable.recogstart);
			}
		}
	}

	@Override
	public void onGetError(int errorCode) {
		if (2 == mRecoState) {
			cancelTask();
		}
		
		if (-202 == errorCode) {
			initPopWindow();
			findViewById(R.id.start_record).post(new Runnable() {
				public void run() {
				  mPop.showAtLocation(findViewById(R.id.start_record), Gravity.CENTER, 0, 0);
				}
			});
			return;
		}
		mStartRecordTv.setText("Error Code: " + errorCode);
		setCancelBtn(false);
		mRecoState = 0;
		setStartBtn(true);
	}

	@Override
	public void onGetVoiceRecordState(VoiceRecordState state) {
		if (state == VoiceRecordState.Start) {
			mStartRecordTv.setText("语音已开启，请说话…");
		} else if (state == VoiceRecordState.Complete) {
			mCompleteBtn.setEnabled(false);

			mStartRecordTv.setText("识别中...");
			mRecoState = 2;
			startTask();
		} else if (state == VoiceRecordState.Canceling) {
			mRecoState = 3;
			setStartBtn(false);
			mStartRecordTv.setText("正在取消");
		} else if (state == VoiceRecordState.Canceled) {
			cancelTask();
			mStartRecordTv.setText("点击开始说话");
			mRecoState = 0;
			setCancelBtn(false);
			setStartBtn(true);
		}
		
	}
	
	private void setStartBtn(boolean enabled) {
		if (null != mCompleteBtn) {
			if (true == enabled) {
				mCompleteBtn.setEnabled(enabled);
				mCompleteBtn.setBackgroundResource(R.drawable.recogstart);
			}
			else {
				mCompleteBtn.setEnabled(enabled);
				mCompleteBtn.setBackgroundResource(R.drawable.recoggray);
			}
		}
	}
	private void setCancelBtn(boolean enabled) {
		if (null != mCancelBtn) {
			if (true == enabled) {
				mCancelBtn.setEnabled(enabled);
				mCancelBtn.setBackgroundResource(R.drawable.recogcancel);
			}
			else {
				mCancelBtn.setEnabled(enabled);
				mCancelBtn.setBackgroundResource(R.drawable.recogcancelgray);
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