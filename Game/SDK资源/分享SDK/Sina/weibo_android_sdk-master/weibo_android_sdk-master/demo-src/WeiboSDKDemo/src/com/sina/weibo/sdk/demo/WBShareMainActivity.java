/*
 * Copyright (C) 2010-2013 The SINA WEIBO Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.sina.weibo.sdk.demo;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.text.method.LinkMovementMethod;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;
import com.sina.weibo.sdk.api.share.IWeiboShareAPI;
import com.sina.weibo.sdk.api.share.WeiboShareSDK;

/**
 * 璇ョ被鏄垎浜姛鑳界殑鍏ュ彛銆�
 * 
 * @author SINA
 * @since 2013-09-29
 */
public class WBShareMainActivity extends Activity {

    /** 寰崥鍒嗕韩鐨勬帴鍙ｅ疄渚� */
    private IWeiboShareAPI mWeiboShareAPI;
    
    /** 寰崥鍒嗕韩鎸夐挳 */
    private Button mShareButton;
    
    /** 寰崥 ALL IN ONE 鍒嗕韩鎸夐挳 */
    private Button mShareAllInOneButton;

    /**
     * @see {@link Activity#onCreate}
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_share_main);
        initialize();
        
        Log.d("ShareMain" , "onCreate");
    }

    /**
     * 鍒濆鍖� UI 鍜屽井鍗氭帴鍙ｅ疄渚� 銆�
     */
    private void initialize() {
        
        // 鍒涘缓寰崥 SDK 鎺ュ彛瀹炰緥
        mWeiboShareAPI = WeiboShareSDK.createWeiboAPI(this, Constants.APP_KEY);
        
        // 鑾峰彇寰崥瀹㈡埛绔浉鍏充俊鎭紝濡傛槸鍚﹀畨瑁呫�佹敮鎸� SDK 鐨勭増鏈�
        boolean isInstalledWeibo = mWeiboShareAPI.isWeiboAppInstalled();
        int supportApiLevel = mWeiboShareAPI.getWeiboAppSupportAPI(); 
        
        /**
         * 鍒濆鍖� UI
         */
        // 璁剧疆鎻愮ず鏂囨湰
        ((TextView)findViewById(R.id.register_app_to_weibo_hint)).setMovementMethod(LinkMovementMethod.getInstance());
        ((TextView)findViewById(R.id.weibosdk_demo_support_api_level_hint)).setMovementMethod(LinkMovementMethod.getInstance());
        
        // 璁剧疆寰崥瀹㈡埛绔浉鍏充俊鎭�
        String installInfo = getString(isInstalledWeibo ? R.string.weibosdk_demo_has_installed_weibo : R.string.weibosdk_demo_has_installed_weibo);
        ((TextView)findViewById(R.id.weibosdk_demo_is_installed_weibo)).setText(installInfo);
        ((TextView)findViewById(R.id.weibosdk_demo_support_api_level)).setText("\t" + supportApiLevel);
        
        // 璁剧疆娉ㄥ唽鎸夐挳瀵瑰簲鍥炶皟
        ((Button) findViewById(R.id.register_app_to_weibo)).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                // 娉ㄥ唽鍒版柊娴井鍗�
                mWeiboShareAPI.registerApp();
                Toast.makeText(WBShareMainActivity.this, 
                        R.string.weibosdk_demo_toast_register_app_to_weibo, Toast.LENGTH_LONG).show();
                
                mShareButton.setEnabled(true);
                mShareAllInOneButton.setEnabled(true);
            }
        });
        
        // 璁剧疆鍒嗕韩鎸夐挳瀵瑰簲鍥炶皟
        mShareButton = (Button) findViewById(R.id.share_to_weibo);
        mShareButton.setEnabled(false);
        mShareButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent i = new Intent(WBShareMainActivity.this, WBShareActivity.class);
                i.putExtra(WBShareActivity.KEY_SHARE_TYPE, WBShareActivity.SHARE_CLIENT);
                startActivity(i);
            }
        });
        
        // 璁剧疆ALL IN ONE鍒嗕韩鎸夐挳瀵瑰簲鍥炶皟
        mShareAllInOneButton = (Button) findViewById(R.id.share_to_weibo_all_in_one);
        mShareAllInOneButton.setEnabled(false);
        mShareAllInOneButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent i = new Intent(WBShareMainActivity.this, WBShareActivity.class);
                i.putExtra(WBShareActivity.KEY_SHARE_TYPE, WBShareActivity.SHARE_ALL_IN_ONE);
                startActivity(i);
            }
        });
    }
}
