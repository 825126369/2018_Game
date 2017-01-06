using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using xk_System.Debug;
using xk_System.Sdk;
using System.Collections.Generic;
using xk_System.AssetPackage;
using System.IO;

namespace xk_System.View.Modules
{
    public class ShareView : xk_View
    {
        private Button mButton_QQ;
        private Button mButton_Qzone;
        private Button mButton_Sina;
        private Button mButton_WeChat;
        private Button mButton_WeChat_TimeLine;
        private Button mButton_WeChat_Session;
        private Button mCloseBtn;

        private ShareSdk mShareSDK=null;
        private Texture2D o;
        protected override void Awake()
        {
            base.Awake();
            FindObject();
            mButton_WeChat_TimeLine.gameObject.SetActive(false);
            mButton_WeChat_Session.gameObject.SetActive(false);
            Init_ShareSDK();

            mButton_QQ.onClick.AddListener(OnClickToShare);
            mButton_Qzone.onClick.AddListener(OnClickToShare);
            mButton_Sina.onClick.AddListener(OnClickToShare);
            mButton_WeChat.onClick.AddListener(OnClickToShare);
            mButton_WeChat_Session.onClick.AddListener(OnClickToShare);
            mButton_WeChat_TimeLine.onClick.AddListener(OnClickToShare);
            mCloseBtn.onClick.AddListener(OnClickCloseBtn);
        }

        public override IEnumerator PrepareResource()
        {
            DebugSystem.Log("准备加载ShareView资源");
            AssetInfo mAssetInfo = ResourceABsFolder.Instance.atlas.share.mshare_1;
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
            Texture2D o1 = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as Texture2D;
            o = o1;
            if (o == null)
            {
                DebugSystem.LogError("加载分享本地图片失败");
            }
            else
            {
                DebugSystem.LogError("加载分享本地图片成功");
            }
        }
        private void FindObject()
        {
            Transform mT = transform.FindChild("Button_QQ");
            mButton_QQ = mT.GetComponent<Button>();

            mT = transform.FindChild("Button_QZONE");
            mButton_Qzone  = mT.GetComponent<Button>();

            mT = transform.FindChild("Button_Sina");
            mButton_Sina = mT.GetComponent<Button>();

            mT = transform.FindChild("Button_WeChat");
            mButton_WeChat  = mT.GetComponent<Button>();

            mT = transform.FindChild("Button_WeChat_timeline");
            mButton_WeChat_TimeLine  = mT.GetComponent<Button>();

            mT = transform.FindChild("Button_WeChat_session");
            mButton_WeChat_Session = mT.GetComponent<Button>();

            mT = transform.FindChild("Close");
            mCloseBtn = mT.GetComponent<Button>();
        }

        private void OnClickCloseBtn()
        {
            HideView<ShareView>();
        }

        private void OnClickToShare()
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            DebugSystem.Log("分享点击物体：" + obj.name);
            switch (obj.name)
            {
                case "Button_QQ":
                    Share_QQ();
                    break;
                case "Button_QZONE":
                    Share_QZone();
                    break;
                case "Button_Sina":
                    Share_Sina();
                    break;
                case "Button_WeChat":
                    Share_WeChat();
                    break;
                case "Button_WeChat_timeline":
                    Share_WeChat_TimeLine();           
                    break;
                case "Button_WeChat_session":
                    Share_WeChat_Session();
                    break;
            }
        }

        private void Init_ShareSDK()
        {
            mShareSDK = new ShareSdk(this.gameObject,"ShareResultHandler");
        }

        private void Share_QQ()
        {
            QQShareContent mContent = new QQShareContent();
            mContent.SetShareType(QQShareContent.sharetype_DEFAULT);
            mContent.SetTitle("AAA11111454545BBBBBBBB");
            mContent.SetSummary("BBB1234123123132123123132123");
            mContent.SetTargetUrl("http://www.baidu.com");
            //mContent.SetImageUrl("http://img0.bdstatic.com/img/image/shouye/xiaoxiao/%E6%98%9F%E7%A9%BA614.jpg");
            mContent.SetImageUrl(GetLocalImageUrl());
            mShareSDK.ShareQQContent(SharePlatformManager.QQ_Share_Platform,mContent);
        }

        private void Share_QZone()
        {
            QZoneShareContent mContent = new QZoneShareContent();
            mContent.SetShareType(QZoneShareContent.sharetype_IMAGE_TEXT);
            mContent.SetTitle("AAA11111454545BBBBBBBB");
            mContent.SetSummary("BBB1234123123132123123132123");
            mContent.SetTargetUrl("http://www.baidu.com");
            mContent.SetImageUrl(new List<string>() { "http://img3.douban.com/lpic/s3635685.jpg" });
            mShareSDK.ShareQzoneContent(SharePlatformManager.Qzone_Share_Platform, mContent);
        }

        private void Share_WeChat()
        {
            if (!PlatformManager.Instance.isClientValid(PlatformManager.WeChat_Platform))
            {
                MsgBoxView.InitViewInfo mInfo = new MsgBoxView.InitViewInfo();
                mInfo.mType = MsgBoxView.YesNoView;
                mInfo.mContent = "WeChat平台不存在";
                ShowView<MsgBoxView>(mInfo);
                return;
            }
            WeChatShareContent mContent = new WeChatShareContent();
            mContent.SetShareContentType(WeChatShareContent.sharContentType_WebPage);
            mContent.SetShareSceneType(WeChatShareContent.shareSceneType_WXSceneSession);
            mContent.SetTitle("title: AAA111114");
            mContent.SetDescription("des11111");
            //mContent.SetImageUrl("http://img0.bdstatic.com/img/image/shouye/xiaoxiao/%E6%98%9F%E7%A9%BA614.jpg");
            mContent.SetWebPageUrl("http://www.baidu.com");
            mContent.SetImageUrl("file://" + GetLocalImageUrl());
            mShareSDK.ShareWeChatContent(SharePlatformManager.WeChat_Share_Platform, mContent);
        }

        private void Share_WeChat_Session()
        {
            if (!PlatformManager.Instance.isClientValid(PlatformManager.QQ_Platform))
            {
                MsgBoxView.InitViewInfo mInfo = new MsgBoxView.InitViewInfo();
                mInfo.mType = MsgBoxView.YesNoView;
                mInfo.mContent = "QQ平台不存在";
                ShowView<MsgBoxView>(mInfo);
                return;
            }
            WeChatShareContent mContent = new WeChatShareContent();
            mContent.SetShareContentType(WeChatShareContent.sharContentType_WebPage);
            mContent.SetShareSceneType(WeChatShareContent.shareSceneType_WXSceneFavorite);
            mContent.SetTitle("title: AAA111114");
            mContent.SetDescription("des11111");
           // mContent.SetImageUrl("http://img0.bdstatic.com/img/image/shouye/xiaoxiao/%E6%98%9F%E7%A9%BA614.jpg");
            mContent.SetWebPageUrl("http://www.baidu.com");
            mContent.SetImageUrl("file://" + GetLocalImageUrl());
            mShareSDK.ShareWeChatContent(SharePlatformManager.WeChat_Share_Platform, mContent);
        }

        private void Share_WeChat_TimeLine()
        {
            if (!PlatformManager.Instance.isClientValid(PlatformManager.QQ_Platform))
            {
                MsgBoxView.InitViewInfo mInfo = new MsgBoxView.InitViewInfo();
                mInfo.mType = MsgBoxView.YesNoView;
                mInfo.mContent = "QQ平台不存在";
                ShowView<MsgBoxView>(mInfo);
                return;
            }
            WeChatShareContent mContent = new WeChatShareContent();
            mContent.SetShareContentType(WeChatShareContent.sharContentType_WebPage);
            mContent.SetShareSceneType(WeChatShareContent.shareSceneType_WXSceneTimeline);
            mContent.SetTitle("title: AAA111114");
            mContent.SetDescription("des11111");
            mContent.SetText("Text：1452464646464");
           // mContent.SetImageUrl("http://img0.bdstatic.com/img/image/shouye/xiaoxiao/%E6%98%9F%E7%A9%BA614.jpg");
            mContent.SetWebPageUrl("http://www.baidu.com");
            mContent.SetImageUrl("file://"+GetLocalImageUrl());
            mShareSDK.ShareWeChatContent(SharePlatformManager.WeChat_Share_Platform, mContent);
        }

        private void Share_Sina()
        {
            if (!PlatformManager.Instance.isClientValid(PlatformManager.Sina_Platform))
            {
                MsgBoxView.InitViewInfo mInfo = new MsgBoxView.InitViewInfo();
                mInfo.mType = MsgBoxView.YesNoView;
                mInfo.mContent = "Sina平台不存在";
                ShowView<MsgBoxView>(mInfo);
                return;
            }
            SinaShareContent mContent = new SinaShareContent();
            mContent.SetShareContentType(WeChatShareContent.sharContentType_WebPage);
            mContent.SetTitle("title: AAA111114");
            mContent.SetDescription("des:11111");
            mContent.SetText("Text：1452464646464");
            mContent.SetImageUrl("http://img0.bdstatic.com/img/image/shouye/xiaoxiao/%E6%98%9F%E7%A9%BA614.jpg");
            mContent.SetWebPageUrl("http://www.baidu.com");
           // mContent.SetImageUrl(GetLocalImageUrl());
            mShareSDK.ShareSinaContent(SharePlatformManager.Sina_Share_Platform, mContent);
        }

        private void ShareResultHandler(string mJsonStr)
        {
            ShareResult mShareResult = mShareSDK.ParseShareResult(mJsonStr);
            switch(mShareResult.status)
            {
                case ShareResult.Share_result_Success:
                    DebugSystem.Log("Share Success");
                    MsgBoxView.InitViewInfo mInfo = new MsgBoxView.InitViewInfo();
                    mInfo.mType = MsgBoxView.YesNoView;
                    mInfo.mContent = "分享成功: "+mShareResult.resultStr;
                    ShowView<MsgBoxView>(mInfo);
                    break;
                case ShareResult.Share_result_Fail:
                    DebugSystem.Log("Share Fail");
                    mInfo = new MsgBoxView.InitViewInfo();
                    mInfo.mType = MsgBoxView.YesNoView;
                    mInfo.mContent = "分享失败: "+mShareResult.resultStr;
                    ShowView<MsgBoxView>(mInfo);
                    break;
                case ShareResult.Share_result_Cancel:
                    DebugSystem.Log("Share Cancel");
                    mInfo = new MsgBoxView.InitViewInfo();
                    mInfo.mType = MsgBoxView.YesNoView;
                    mInfo.mContent = "分享取消: "+mShareResult.resultStr;
                    ShowView<MsgBoxView>(mInfo);
                    break;
            }
        }

        private string GetLocalImageUrl()
        {
            AssetInfo mAssetInfo = ResourceABsFolder.Instance.atlas.share.mshare_1;       
            string imagePath = Application.persistentDataPath + "/"+mAssetInfo.assetName; 
            if (!File.Exists(imagePath))  
            {
                if (o != null)
                {
                    if (imagePath.EndsWith(".png"))
                    {
                        File.WriteAllBytes(imagePath, o.EncodeToPNG());
                    }else if(imagePath.EndsWith(".jpg"))
                    {
                        File.WriteAllBytes(imagePath, o.EncodeToJPG());
                    }else
                    {
                        DebugSystem.LogError("图片格式不对");
                    }
                }else
                {
                    DebugSystem.LogError("分享本地图片失败1111111111111");
                }
            }
            DebugSystem.Log("本地图片地址： "+imagePath);
            return imagePath;  
        }

    }
}