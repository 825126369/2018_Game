package com.qq.wx.voiceui.demo;

import java.io.FileInputStream;
import java.io.IOException;

import android.media.AudioManager;
import android.media.MediaPlayer;
import android.media.MediaPlayer.OnCompletionListener;
import android.os.Handler;


public class Player implements OnCompletionListener {
	private MediaPlayer mediaPlayer;
	private PlayerHandler mPlayerhandler;
	public Player(PlayerCallback playCallback)
	{
		mPlayerhandler = new PlayerHandler();
		mPlayerhandler.setListener(playCallback);
		try {
			mediaPlayer = new MediaPlayer();
			mediaPlayer.setAudioStreamType(AudioManager.STREAM_MUSIC);
			mediaPlayer.setOnCompletionListener(this);
		} catch (Exception e) {
			e.printStackTrace();
		}
		
	}
	
	public void play()
	{
		mediaPlayer.start();
		mPlayerhandler.onGetPalyerState(PlayerState.PLAYING);
	}

	public void playFile(String videoFile)
	{
		try {
			mediaPlayer.reset();
			FileInputStream fis = new FileInputStream(videoFile); 
			mediaPlayer.setDataSource(fis.getFD());
			fis.close();
			mediaPlayer.prepare();
			mediaPlayer.start();
			mPlayerhandler.onGetPalyerState(PlayerState.PLAYING);
		} catch (IllegalArgumentException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IllegalStateException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	
	public void pause()
	{
		mediaPlayer.pause();
		mPlayerhandler.onGetPalyerState(PlayerState.PAUSE);
	}
	
	public void stop()
	{
		if (mediaPlayer != null) { 
			mediaPlayer.stop();
			mPlayerhandler.onGetPalyerState(PlayerState.STOP);
        } 
	}
	
	public void release() {
		if (mediaPlayer != null) {
			mediaPlayer.release();
			mediaPlayer = null;
		}
	}

	public boolean isPlaying() {
		return mediaPlayer.isPlaying();
	}

	@Override
	public void onCompletion(MediaPlayer arg0) {
		mPlayerhandler.onGetPalyerState(PlayerState.STOP);
	}
	
	class PlayerHandler {
		static final String TAG = "PalyerCallback";
		
		protected static final int VOICE_THREAD_STOP = 0;
		protected static final int VOICE_RECORD_STOP = 1;
		
		private PlayerCallback mCallback;
		
		private Handler mHandler = new Handler() {
			public void handleMessage(android.os.Message msg) {
				switch (msg.what) {
				case PlayerState.PLAYING:
					mCallback.onGetPlayerStatePlaying();
					break;
				case PlayerState.PAUSE:
					mCallback.onGetPlayerStatePause();
					break;
				case PlayerState.STOP:
					mCallback.onGetPlayerStateStop();
					break;
				}
			}
		};
		
		public PlayerHandler() {
			
		}
		
		public void setListener(PlayerCallback listener) {
			mCallback = listener;
		}
		
		public void onGetPalyerState(int state) {
			mHandler.sendMessage(mHandler.obtainMessage(state));
		}
	}
	
	interface PlayerCallback {
		public void onGetPlayerStatePlaying();
		public void onGetPlayerStatePause();
		public void onGetPlayerStateStop();
	}

}

