using UnityEngine;
using System.Collections;
using System;
using xk_System.Debug;
using System.Reflection;
using xk_System.View.Modules;
using xk_System.View;
using System.Collections.Generic;

namespace xk_System.Sdk
{
    public class SharePlatformManager
    {
        public  static int QQ_Share_Platform = 101;
        public  static int Qzone_Share_Platform = 102;
        public  static int WeChat_Share_Platform = 103;
        public  static int Sina_Share_Platform = 104;
    }
    public class ShareSdk
    {
        private AndroidJavaObject ssdk;
        private const string android_share_className = "com.xk.sharesdk.UnityShareManager";
        private const string android_share_Fun = "ShareContent";
        public ShareSdk(GameObject obj,string callBackFunName)
        {
            DebugSystem.Log("AndroidImpl  ===>>>  AndroidImpl");
            try
            {
                ssdk = new AndroidJavaObject(android_share_className, obj.name, callBackFunName);
            }
            catch (Exception e)
            {
                DebugSystem.LogError(e.Message + " Exception caught.");
            }
        }

        public void ShareQQContent(int platform, QQShareContent content)
        {
            DebugSystem.Log("AndroidImpl  ===>>>  ShareContent to one platform");
            if (ssdk == null)
            {
                DebugSystem.LogError("ShareSDk is null: " + android_share_className);
                return;
            }
            ssdk.Call(android_share_Fun, platform, content.GetShareParamsStr());
        }

        public void ShareQzoneContent(int platform, QZoneShareContent content)
        {
            DebugSystem.Log("AndroidImpl  ===>>>  ShareContent to one platform");
            if (ssdk == null)
            {
                DebugSystem.LogError("ShareSDk is null: " + android_share_className);
                return;
            }
            ssdk.Call(android_share_Fun, platform, content.GetShareParamsStr());
        }

        public void ShareWeChatContent(int platform, WeChatShareContent content)
        {
            DebugSystem.Log("AndroidImpl  ===>>>  ShareContent to one platform");
            if (ssdk == null)
            {
                DebugSystem.LogError("ShareSDk is null: " + android_share_className);
                return;
            }
            ssdk.Call(android_share_Fun, platform, content.GetShareParamsStr());
        }

        public void ShareSinaContent(int platform, SinaShareContent content)
        {
            DebugSystem.Log("AndroidImpl  ===>>>  ShareContent to one platform");
            if (ssdk == null)
            {
                DebugSystem.LogError("ShareSDk is null: " + android_share_className);
                return;
            }
            ssdk.Call(android_share_Fun, platform, content.GetShareParamsStr());
        }

        public ShareResult ParseShareResult(string data)
        {
            if (data == null)
            {
                DebugSystem.LogError("data is null");
                return null;
            }
            Hashtable res = (Hashtable)MiniJSON.jsonDecode(data);
            if (res == null || res.Count <= 0)
            {
                DebugSystem.LogError("解析Json出错");
                return null;
            }

            ShareResult mShareResult = new ShareResult();
            mShareResult.status =Convert.ToInt32(res[ShareResult.hash_key_share_result_status]);
            mShareResult.platformId= Convert.ToInt32(res[ShareResult.hash_key_share_result_platformId]);
            mShareResult.resultStr = res[ShareResult.hash_key_share_result_resultStr].ToString();
            DebugSystem.Log("分享返回结果： "+mShareResult.status);
            return mShareResult;
        }
    }
    /// <summary>
    /// QQ分享内容
    /// </summary>
    public class QQShareContent
    {
        public static  String hash_key_sharetype="sharetype";
		public static  String hash_key_title="title";
		public static  String hash_key_targetUrl="targeturl";
		public static  String hash_key_summary="summary";

		public static  String hash_key_imageUrl="imageUrl";
		public static  String hash_key_musicUrl="musicUrl";
		public static  String hash_key_AppName="appName";
		public static  String hash_key_OrHideQzoneBtn="orhideQzoneBtn";

        public static  int sharetype_DEFAULT = 1;
        public static  int sharetype_IMAGE = 2;
        public static  int sharetype_AUDIO = 3;
        public static  int sharetype_APP = 4;

        private static  int QzoneBtnState_DEFAULT = 1;
        private static  int QzoneBtnState_Show = 2;
        private static  int QzoneBtnState_Hide = 3;
        Hashtable shareParams = new Hashtable();       
              
        public void SetShareType(int shareType)
        {
            shareParams[hash_key_sharetype] = shareType;
        }

        public void SetTitle(String title)
        {
            shareParams[hash_key_title] = title;
        }

        public void SetSummary(String text)
        {
            shareParams[hash_key_summary] = text;
        }

        public void SetTargetUrl(String url)
        {
            shareParams[hash_key_targetUrl] = url;
        }

        public void SetImageUrl(String imageUrl)
        {
            shareParams[hash_key_imageUrl] = imageUrl;
        }

        public void SetMusicUrl(String musicUrl)
        {
            shareParams[hash_key_musicUrl] = musicUrl;
        }

        public void SetAppName(String appName)
        {
            shareParams[hash_key_AppName] = appName;
        }

        public void SetOrHideQzoneBtn(int QzoneBtnState)
        {
            shareParams[hash_key_OrHideQzoneBtn] = QzoneBtnState;
        }
        public String GetShareParamsStr()
        {
            String jsonStr = MiniJSON.jsonEncode(shareParams);
            return jsonStr;
        }
    }
    /// <summary>
    /// QQ空间分享内容
    /// </summary>
    public class QZoneShareContent
    {
        public static String hash_key_sharetype = "sharetype";
        public static String hash_key_title = "title";
        public static String hash_key_targetUrl = "targeturl";
        public static String hash_key_summary = "summary";

        private static String hash_key_imageUrlList = "imageUrlList";
        public static String hash_key_musicUrl = "musicUrl";
        public static String hash_key_videoUrl= "videoUrl";
        public static String hash_key_AppName = "appName";

        public static  int sharetype_IMAGE_TEXT = 1;
        public static  int sharetype_IMAGE = 2;
        public static  int sharetype_APP = 3;
        public static  int sharetype_NOTYPE = 4;

        public static  int sharetype_VEDIO = 5;
        public static  int sharetype_MOOD = 6;

        Hashtable shareParams = new Hashtable();

        public void SetShareType(int shareType)
        {
            shareParams[hash_key_sharetype] = shareType;
        }

        public void SetTitle(String title)
        {
            shareParams[hash_key_title] = title;
        }

        public void SetSummary(String text)
        {
            shareParams[hash_key_summary] = text;
        }

        public void SetTargetUrl(String url)
        {
            shareParams[hash_key_targetUrl] = url;
        }

        public void SetImageUrl(List<string> imageUrlList)
        {
           string imageStr = "";
           for(int i=0;i<imageUrlList.Count;i++)
           {
                imageStr += imageUrlList[i];
                if(i+1<imageUrlList.Count)
                {
                    imageStr += "|";
                }
            }
            shareParams[hash_key_imageUrlList] = imageStr;
        }

        public void SetMusicUrl(String musicUrl)
        {
            shareParams[hash_key_musicUrl] = musicUrl;
        }

        public void SetAppName(String appName)
        {
            shareParams[hash_key_AppName] = appName;
        }

        public void SetVideoUrl(string videoUrl)
        {
            shareParams[hash_key_videoUrl] = videoUrl;
        }
        public String GetShareParamsStr()
        {
            String jsonStr = MiniJSON.jsonEncode(shareParams);
            return jsonStr;
        }
    }

    /// <summary>
    /// WeChat分享内容
    /// </summary>
    public class WeChatShareContent
    {
        // 发送到朋友圈
        public static int shareSceneType_WXSceneTimeline = 1;
        // 添加到微信收藏
        public static int shareSceneType_WXSceneFavorite = 2;
        // 发送到聊天界面
        public static int shareSceneType_WXSceneSession = 3;

        public static int sharContentType_Text = 1;
        public static int sharContentType_Image = 2;
        public static int sharContentType_Music = 3;
        public static int sharContentType_Video = 4;
        public static int sharContentType_WebPage = 5;


        private static String hash_key_sharescenetype = "sharescenetype";
        private static String hash_key_sharecontenttype = "sharecontenttype";
        private static String hash_key_title = "title";
        private static String hash_key_description = "description";
        private static String hash_key_text = "text";
        private static String hash_key_thumbUrl = "thumbUrl";
        private static String hash_key_imageUrl = "imageUrl";
        private static String hash_key_musicUrl = "musicUrl";
        private static String hash_key_videoUrl = "videoUrl";
        private static String hash_key_WebPageUrl = "webPageUrl";

        Hashtable shareParams = new Hashtable();

        public void SetShareSceneType(int shareType)
        {
            shareParams[hash_key_sharescenetype] = shareType;
        }
        public void SetShareContentType(int shareType)
        {
            shareParams[hash_key_sharecontenttype] = shareType;
        }

        public void SetTitle(String title)
        {
            shareParams[hash_key_title] = title;
        }

        public void SetDescription(String text)
        {
            shareParams[hash_key_description] = text;
        }

        public void SetText(string text)
        {
            shareParams[hash_key_text] = text;
        }

        public void SetImageUrl(string imageUrl)
        {
            shareParams[hash_key_imageUrl] = imageUrl;
        }

        public void SetMusicUrl(String musicUrl)
        {
            shareParams[hash_key_musicUrl] = musicUrl;
        }

        public void SetVideoUrl(string videoUrl)
        {
            shareParams[hash_key_videoUrl] = videoUrl;
        }

        public void SetWebPageUrl(string webPageUrl)
        {
            shareParams[hash_key_WebPageUrl] = webPageUrl;
        }

        public String GetShareParamsStr()
        {
            String jsonStr = MiniJSON.jsonEncode(shareParams);
            return jsonStr;
        }
    }

    public class SinaShareContent
    {
        // 文字图片（默认） //视频、音乐,网页，声音
        public static int sharContentType_Text = 1;
        public static int sharContentType_Image = 2;
        public static int sharContentType_Music = 3;
        public static int sharContentType_Video = 4;
        public static int sharContentType_WebPage = 5;
        public static int sharContentType_Voice = 6;

        private static String hash_key_sharecontenttype = "sharecontenttype";
        private static String hash_key_title = "title";
        private static String hash_key_description = "description";
        private static String hash_key_text = "text";
        private static String hash_key_imageUrl = "imageUrl";
        private static String hash_key_musicUrl = "musicUrl";
        private static String hash_key_videoUrl = "videoUrl";
        private static String hash_key_WebPageUrl = "webPageUrl";
        private static String hash_key_voiceUrl = "voiceUrl";

        Hashtable shareParams = new Hashtable();

        public void SetShareContentType(int shareType)
        {
            shareParams[hash_key_sharecontenttype] = shareType;
        }

        public void SetTitle(String title)
        {
            shareParams[hash_key_title] = title;
        }

        public void SetDescription(String text)
        {
            shareParams[hash_key_description] = text;
        }

        public void SetText(string text)
        {
            shareParams[hash_key_text] = text;
        }

        public void SetImageUrl(string imageUrl)
        {
            shareParams[hash_key_imageUrl] = imageUrl;
        }

        public void SetMusicUrl(String musicUrl)
        {
            shareParams[hash_key_musicUrl] = musicUrl;
        }

        public void SetVideoUrl(string videoUrl)
        {
            shareParams[hash_key_videoUrl] = videoUrl;
        }

        public void SetWebPageUrl(string webPageUrl)
        {
            shareParams[hash_key_WebPageUrl] = webPageUrl;
        }

        public String GetShareParamsStr()
        {
            String jsonStr = MiniJSON.jsonEncode(shareParams);
            return jsonStr;
        }
    }


    public class ShareResult
    {
        public const int Share_result_Success = 1;
        public const int Share_result_Fail = 2;
        public const int Share_result_Cancel = 3;

        public const string hash_key_share_result_status = "status";
        public const string hash_key_share_result_platformId = "platformId";
        public const string hash_key_share_result_resultStr = "resultstr";

        public int status;
        public int platformId;
        public string resultStr;
    }
}