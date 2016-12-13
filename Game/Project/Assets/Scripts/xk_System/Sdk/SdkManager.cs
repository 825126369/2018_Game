using UnityEngine;
using System.Collections;
using xk_System.Debug;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace xk_System.Sdk
{
    public class SdkManager : MonoBehaviour
    {
        private void Awake()
        {
            PlatformManager.Instance.SetPlatformConfig();
        }
    }

    public class PlatformManager : Singleton<PlatformManager>
    {
        public static int QQ_Platform = 101;
        public static int WeChat_Platform = 102;
        public static int Sina_Platform = 103;

        public Dictionary<int, Platform> mPlatformDic = new Dictionary<int, Platform>();

        private const string android_platform_className = "com.xk.platform.PlatformManager";
        private const string android_platform_fun_InitAllPlatform = "InitAllPlatform";
        private const string android_platform_fun_isClientValid = "isClientValid";
        private AndroidJavaObject ssdk;
        public PlatformManager()
        {
            InitAllPlatform();
        }
        private void InitAllPlatform()
        {
            QQ mQQ_Platform = new QQ();
            mPlatformDic.Add(QQ_Platform,mQQ_Platform);

            WeChat mWeChat_Platform = new WeChat();
            mPlatformDic.Add(WeChat_Platform,mWeChat_Platform);

            SinaWeibo mSina = new SinaWeibo();
            mPlatformDic.Add(Sina_Platform,mSina);
        }

        public bool isClientValid(int Platform)
        {
            if (ssdk != null)
            {
               return ssdk.Call<bool>(android_platform_fun_isClientValid, Platform);
            }else
            {
                return false;
            }
        }



        public void SetPlatformConfig()
        {
            try
            {
                ssdk = new AndroidJavaObject(android_platform_className);
            }
            catch (Exception e)
            {
                DebugSystem.LogError("PlatformManager: " + e.Message);
            }
            Hashtable platformConfigs = new Hashtable();
            foreach (KeyValuePair<int, Platform> k in mPlatformDic)
            {
                platformConfigs.Add(k.Key,k.Value.getHashTable());
            }
            String json = MiniJSON.jsonEncode(platformConfigs);
            if(json!=null)
            {
                DebugSystem.Log(json);
            }
            if (ssdk != null)
            {
                ssdk.Call(android_platform_fun_InitAllPlatform, json);
            }
        }
    }

    public class Platform
    {
        public Hashtable getHashTable()
        {
            Hashtable platformConfigs = new Hashtable();
            Type mType = this.GetType();
            FieldInfo[] mFieldInfos = mType.GetFields();
            foreach (var v in mFieldInfos)
            {
                platformConfigs.Add(v.Name, v.GetValue(this));
            }
            return platformConfigs;
        }
    }

    public class SinaWeibo:Platform
    {
        public string AppId = "1991236894";
        public string AppKey = "291ab2fc9e300c06de9022db238c1a44";
    }

    public class QQ:Platform
    {
        public string AppId = "1105553076";
        public string AppKey = "KaAJ1aKcWwjH5LAa";
    }

    public class WeChat:Platform
    {
        public string AppId = "wx6f8a3595202784e2";
        public string AppKey = "32a624b229b74f807672910f8b240076";
    }

    
}