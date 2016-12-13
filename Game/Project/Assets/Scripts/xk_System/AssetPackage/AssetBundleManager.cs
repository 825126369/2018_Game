using UnityEngine;
using System.Collections;
using xk_System.Debug;
using System.Collections.Generic;
using System.Xml;

namespace xk_System.AssetPackage
{
    /// <summary>
    /// 此类的目的就是加载本地的Bundle进行资源读取操作的
    /// </summary>
    public class AssetBundleManager : SingleTonMonoBehaviour<AssetBundleManager>
    {
        private ResourcesABManager mResourcesABManager = new ResourcesABManager();
        private Dictionary<string, AssetBundle> mBundleDic = new Dictionary<string, AssetBundle>();
        private Dictionary<string, Dictionary<string, UnityEngine.Object>> mAssetDic = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();
        private List<string> mBundleLockList = new List<string>();

        /// <summary>
        /// 加载Assetbundle方案1：初始化时，全部加载
        /// </summary>
        /// <returns></returns>
        public IEnumerator InitLoadAllBundleFromLocal()
        {
            yield return mResourcesABManager.InitLoadMainifestFile();
            List<AssetBundleInfo> bundleList = mResourcesABManager.mNeedLoadBundleList;
            List<AssetBundleInfo>.Enumerator mIter = bundleList.GetEnumerator();
            while (mIter.MoveNext())
            {
                yield return AsyncLoadFromLoaclSingleBundle(mIter.Current);
            }
        }
        public IEnumerator InitAssetBundleManager()
        {
            yield return mResourcesABManager.InitLoadMainifestFile();
        }

        private IEnumerator CheckBundleDependentBundle(AssetBundleInfo mBundle)
        {
            if (mBundle != null)
            {
                string[] mdependentBundles = mBundle.mDependentBundleList;
                foreach (string s in mdependentBundles)
                {
                    AssetBundleInfo mBundleInfo = mResourcesABManager.GetBundleInfo(s);
                    if (mBundleInfo != null)
                    {
                        AssetBundle mAB = null;
                        if (!mBundleDic.TryGetValue(mBundleInfo.bundleName, out mAB))
                        {
                            yield return AsyncLoadFromLoaclSingleBundle(mBundleInfo);
                        }
                        else
                        {
                            if (mAB == null)
                            {
                                yield return AsyncLoadFromLoaclSingleBundle(mBundleInfo);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 从本地外部存储位置加载单个Bundle资源,全部加载
        /// </summary>
        /// <param name="BaseBundleInfo"></param>
        /// <returns></returns>
        private IEnumerator AsyncLoadFromLoaclSingleBundle1(AssetBundleInfo BaseBundleInfo)
        {
            if(mBundleLockList.Contains(BaseBundleInfo.bundleName))
            {
                while(mBundleLockList.Contains(BaseBundleInfo.bundleName))
                {
                    yield return null;
                }
                yield break;
            }     
            mBundleLockList.Add(BaseBundleInfo.bundleName);
            yield return CheckBundleDependentBundle(BaseBundleInfo);
            string path = AssetBundlePath.Instance.ExternalStorePathUrl;
            string url = path + "/" + BaseBundleInfo.bundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    mBundleLockList.Remove(BaseBundleInfo.bundleName);
                    yield break;
                }
            }
            AssetBundle asset = www.assetBundle;
            SaveBundleToDic(BaseBundleInfo.bundleName, asset);
            mBundleLockList.Remove(BaseBundleInfo.bundleName);
            www.Dispose();
        }

        /// <summary>
        /// 从本地外部存储位置加载单个Bundle资源,全部加载
        /// </summary>
        /// <param name="BaseBundleInfo"></param>
        /// <returns></returns>
        private IEnumerator AsyncLoadFromLoaclSingleBundle(AssetBundleInfo BaseBundleInfo)
        {
            if (mBundleLockList.Contains(BaseBundleInfo.bundleName))
            {
                while (mBundleLockList.Contains(BaseBundleInfo.bundleName))
                {
                    yield return null;
                }
                yield break;
            }
            mBundleLockList.Add(BaseBundleInfo.bundleName);
            yield return CheckBundleDependentBundle(BaseBundleInfo);
            string path = AssetBundlePath.Instance.ExternalStorePath+"/"+BaseBundleInfo.bundleName;
            AssetBundleCreateRequest www= AssetBundle.LoadFromFileAsync(path);
            www.allowSceneActivation = true;
            yield return www;
            AssetBundle asset = www.assetBundle;
            SaveBundleToDic(BaseBundleInfo.bundleName, asset);
            mBundleLockList.Remove(BaseBundleInfo.bundleName);
        }
        /// <summary>
        /// 异步从本地外部存储加载单个Asset文件，只加载Bundle中的单个资源
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        private IEnumerator AsyncLoadFromLocalSingleAsset(AssetBundleInfo bundle, string assetName)
        {
            if (bundle != null)
            {
                yield return AsyncLoadFromLoaclSingleBundle(bundle);
                UnityEngine.Object Obj = mBundleDic[bundle.bundleName].LoadAsset(assetName);
                if (Obj != null)
                {
                    DebugSystem.Log("Async Load Asset Success:" + Obj.name);
                    SaveAssetToDic(bundle.bundleName, assetName, Obj);
                }
            }
        }
        /// <summary>
        /// 同步从本地外部存储加载单个Bundle文件
        /// </summary>
        /// <param name="BaseBundleInfo"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private void SyncLoadFromLocalSingleBundle(string bundleName)
        {
            if (!JudegeOrExistBundle(bundleName))
            {
                string path = AssetBundlePath.Instance.ExternalStorePath + "/" + bundleName;
                AssetBundle asset = AssetBundle.LoadFromFile(path);
                SaveBundleToDic(bundleName, asset);
            }else
            {
                DebugSystem.LogError("Bundle 已存在："+bundleName);
            }
        }

        /// <summary>
        /// 同步从本地外部存储加载单个资源文件
        /// </summary>
        /// <param name="BaseBundleInfo"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public UnityEngine.Object SyncLoadFromLocalSingleAsset(AssetInfo mAssetInfo)
        {
            if (!JudgeOrExistAsset(mAssetInfo.bundleName, mAssetInfo.assetName))
            {
                string path = AssetBundlePath.Instance.ExternalStorePath+"/"+mAssetInfo.bundleName;
                AssetBundle asset = AssetBundle.LoadFromFile(path);
                SaveBundleToDic(mAssetInfo.bundleName,asset);
            }
            return GetAssetFromDic(mAssetInfo.bundleName,mAssetInfo.assetName);
        }


        private void SaveBundleToDic(string bundleName, AssetBundle bundle)
        {
            if (bundle == null)
            {
                DebugSystem.LogError("未保存的Bundle为空:"+bundleName);
                return;
            }
            if (!mBundleDic.ContainsKey(bundleName))
            {
                mBundleDic[bundleName] = bundle;
            }else
            {
                DebugSystem.LogError("Bundle资源 重复:"+bundleName);
            }
        }

        private void SaveAssetToDic(string bundleName, string assetName, UnityEngine.Object asset)
        {
            if (asset == null)
            {
                DebugSystem.LogError("未保存的资源为空:"+assetName);
                return;
            }
            if(asset is GameObject)
            {
                GameObject obj = asset as GameObject;
                obj.SetActive(false);
            }
            if (!mAssetDic.ContainsKey(bundleName))
            {
                Dictionary<string, UnityEngine.Object> mDic = new Dictionary<string, UnityEngine.Object>();
                mAssetDic.Add(bundleName, mDic);
            }
            mAssetDic[bundleName][assetName] = asset;
        }

        private bool JudgeOrBundelIsLoading(string bundleName)
        {
            if (mBundleLockList.Contains(bundleName))
            {
                return true;
            }else
            {
                return false;
            }
        }

        private bool JudegeOrExistBundle(string bundleName)
        {
            if (mBundleDic.ContainsKey(bundleName) && mBundleDic[bundleName] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool JudgeOrExistAsset(string bundleName, string asstName)
        {
            if (JudegeOrExistBundle(bundleName))
            {
                if (!mAssetDic.ContainsKey(bundleName) || mAssetDic[bundleName] == null || !mAssetDic[bundleName].ContainsKey(asstName) || mAssetDic[bundleName][asstName] == null)
                {
                    UnityEngine.Object mm = mBundleDic[bundleName].LoadAsset(asstName);
                    if (mm != null)
                    {
                        SaveAssetToDic(bundleName, asstName, mm);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private UnityEngine.Object GetAssetFromDic(string bundleName, string asstName)
        {
            if (JudgeOrExistAsset(bundleName, asstName))
            {
                UnityEngine.Object mAsset1 = mAssetDic[bundleName][asstName];
                if (mAsset1 is GameObject)
                {
                    GameObject obj = Instantiate(mAsset1) as GameObject;
                    return obj;
                }
                else
                {
                    return mAsset1;
                }
            }
            else
            {
                DebugSystem.LogError("Asset is NUll:" + asstName);
            }
            return null;
        }
#if UNITY_EDITOR
        private Dictionary<string, UnityEngine.Object> mEditorAssetDic = new Dictionary<string, UnityEngine.Object>();

        private UnityEngine.Object GetAssetFromEditorDic(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                DebugSystem.LogError("Editor AssetPath is Empty");
                return null;
            }
            UnityEngine.Object asset = null;
            if (!mEditorAssetDic.TryGetValue(assetPath, out asset))
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset != null)
                {
                    if (asset is GameObject)
                    {
                        GameObject obj = asset as GameObject;
                        obj.SetActive(false);
                    }
                    mEditorAssetDic.Add(assetPath, asset);
                }
                else
                {
                    DebugSystem.LogError("找不到资源：" + assetPath);
                }
            }
            if (asset is GameObject)
            {
                GameObject obj = Instantiate(asset) as GameObject;
                return obj;
            }
            else
            {
                return asset;
            }
        }
#endif
        /// <summary>
        /// 本地外部存储加载Bundle文件,顶层使用
        /// </summary>
        /// <param name="BaseBundleInfo"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AssetBundle LoadBundle(string bundleName)
        {
#if UNITY_EDITOR
            if (GameConfig.Instance.orUseAssetBundle)
            {
                SyncLoadFromLocalSingleBundle(bundleName);
                return mBundleDic[bundleName];
            }else
            {
                return null;
            }
#else
            SyncLoadFromLocalSingleBundle(bundleName);
            return mBundleDic[bundleName];
#endif
        }
        /// <summary>
        /// 这个东西用来在顶层使用
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(AssetInfo mAssetInfo)
        {
#if UNITY_EDITOR
            if (GameConfig.Instance.orUseAssetBundle)
            {
                return GetAssetFromDic(mAssetInfo.bundleName, mAssetInfo.assetName);
            }
            else
            {
               return GetAssetFromEditorDic(mAssetInfo.assetPath);
            }
#else
            return GetAssetFromDic(mAssetInfo.bundleName, mAssetInfo.assetName);
#endif
        }

        /// <summary>
        /// 这个东西用来在专门的管理器中使用（底层封装一下），禁止在顶层使用
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public IEnumerator AsyncLoadAsset(AssetInfo mAssetInfo)
        {
#if UNITY_EDITOR
            if (GameConfig.Instance.orUseAssetBundle)
            {
                string bundleName = mAssetInfo.bundleName;
                string asstName = mAssetInfo.assetPath;
                if (!JudgeOrExistAsset(bundleName, asstName))
                {
                    yield return AsyncLoadFromLocalSingleAsset(mResourcesABManager.GetBundleInfo(bundleName), asstName);
                }
            }
#else
                string bundleName = mAssetInfo.bundleName;
                string asstName = mAssetInfo.assetName;
                if (!JudgeOrExistAsset(bundleName, asstName))
                {
                    yield return AsyncLoadFromLocalSingleAsset(mResourcesABManager.GetBundleInfo(bundleName), asstName);
                }
#endif
        }
    }

    public class ResourcesABManager
    {
        public int VersionId = -1;
        public List<AssetBundleInfo> mNeedLoadBundleList = new List<AssetBundleInfo>();
        public AssetBundleInfo GetBundleInfo(string bundleName)
        {
            AssetBundleInfo mBundleInfo = mNeedLoadBundleList.Find((x) =>
            {
                return x.bundleName == bundleName;
            });
            return mBundleInfo;
        }

        public IEnumerator InitLoadMainifestFile()
        {
            if (mNeedLoadBundleList.Count == 0)
            {
                string path = AssetBundlePath.Instance.ExternalStorePathUrl;
                string url = path + "/" + AssetBundlePath.AssetDependentFileBundleName;
                WWW www = new WWW(url);
                yield return www;
                if (www.isDone)
                {
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        DebugSystem.LogError("初始化 MainifestFile 失败：" + www.error);
                        www.Dispose();
                        yield break;
                    }
                }
                AssetBundle asset = www.assetBundle;
                www.Dispose();
                if (asset == null)
                {
                    DebugSystem.LogError("MainifestFile Bundle is Null");
                    www.Dispose();
                    yield break;
                }

                AssetBundleManifest mAllBundleMainifest = asset.LoadAsset<AssetBundleManifest>(AssetBundlePath.AssetDependentFileAssetName);
                if (mAllBundleMainifest == null)
                {
                    DebugSystem.LogError("Mainifest is Null");
                    www.Dispose();
                    yield break;
                }
                string[] mAssetNames = mAllBundleMainifest.GetAllAssetBundles();
                if (mAssetNames != null)
                {
                    foreach (var v in mAssetNames)
                    {

                        string bundleName = v;
                        string[] bundleDependentList = mAllBundleMainifest.GetAllDependencies(v);
                        Hash128 mHash = mAllBundleMainifest.GetAssetBundleHash(v);
                        AssetBundleInfo mABInfo = new AssetBundleInfo(bundleName, mHash, bundleDependentList);
                        mNeedLoadBundleList.Add(mABInfo);
                    }
                }
                else
                {
                    DebugSystem.Log("初始化资源依赖文件： Null");
                }
                asset.Unload(false);
                DebugSystem.Log("初始化资源管理器全局Bundle信息成功");
                www.Dispose();

                yield return InitLoadExternalStoreVersionConfig();
            }
        }

        private IEnumerator InitLoadExternalStoreVersionConfig()
        {
            string url = AssetBundlePath.Instance.ExternalStorePathUrl + "/" + AssetBundlePath.versionConfigBundleName;
            WWW www = new WWW(url);
            yield return www;
            if (www.isDone)
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    DebugSystem.LogError("www Load Error:" + www.error);
                    www.Dispose();
                    yield break;
                }
            }
            AssetBundle mConfigBundle = www.assetBundle;
            TextAsset mVersionConfig = mConfigBundle.LoadAsset<TextAsset>(AssetBundlePath.versionConfigAssetName);
            VersionId = GetVersionIdByParseXML(mVersionConfig);
            DebugSystem.Log("当前版本号："+VersionId);
            mConfigBundle.Unload(false);
            www.Dispose();
        }

        private int GetVersionIdByParseXML(TextAsset mTextAsset)
        {
            XmlDocument mdoc = new XmlDocument();
            mdoc.LoadXml(mTextAsset.text);
            foreach (XmlNode v in mdoc.ChildNodes)
            {
                if (v.Name == "root")
                {
                    foreach (XmlNode x in v.ChildNodes)
                    {
                        if (x.Name.Contains("versionId"))
                        {
                            return int.Parse(x.InnerText);
                        }
                    }
                }
            }
            return 0;
        }
    }

    public class AssetBundleInfo
    {
        public string bundleName;
        public Hash128 mHash;
        public string[] mDependentBundleList;

        public AssetBundleInfo(string bundleName, Hash128 mHash128, string[] mDependentBundleList)
        {
            this.bundleName = bundleName;
            this.mHash = mHash128;
            this.mDependentBundleList = mDependentBundleList;
        }

    }

    public class AssetInfo
    {
        public string bundleName;
        public string assetName;
        public string assetPath;

        public AssetInfo(string assetPath,string bundleName, string assetName)
        {
            this.assetPath = assetPath;
            this.bundleName = bundleName;
            this.assetName = assetName;
        }

        public AssetInfo(string bundleName, string assetName)
        {
            this.bundleName = bundleName;
            this.assetName = assetName;
        }
    }

    public class AssetBundlePath : Singleton<AssetBundlePath>
    {
        public const string versionConfigBundleName = "version.xk_unity3d";
        public const string versionConfigAssetName = "version.xml";
        public const string AssetDependentFileBundleName = "StreamingAssets";
        public const string AssetDependentFileAssetName = "AssetBundleManifest";
        public const string ABExtention = ".xk_unity3d";

        public readonly string StreamingAssetPathUrl;
        public readonly string ExternalStorePathUrl;
        public readonly string WebServerPathUrl;

        public readonly string ExternalStorePath;
        public AssetBundlePath()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                WebServerPathUrl = "file:///F:/WebServer";
                StreamingAssetPathUrl = "file:///" + Application.streamingAssetsPath;
                ExternalStorePathUrl = "file:///" + Application.persistentDataPath;
                ExternalStorePath = Application.persistentDataPath;
            } else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                WebServerPathUrl = "file:///F:/WebServer";
                StreamingAssetPathUrl = "file:///" + Application.streamingAssetsPath;
                ExternalStorePathUrl = "file:///" + Application.persistentDataPath;
                ExternalStorePath = Application.persistentDataPath;
            }else if(Application.platform == RuntimePlatform.Android)
            {
                WebServerPathUrl = "file:///F:/WebServer";
                StreamingAssetPathUrl = "jar:file://" + Application.dataPath + "!/assets";
                ExternalStorePathUrl = "file://" + Application.persistentDataPath;
                ExternalStorePath = Application.persistentDataPath;
            }
            DebugSystem.LogError("www server path: " + WebServerPathUrl);
            DebugSystem.LogError("www local Stream Path: " + StreamingAssetPathUrl);
            DebugSystem.LogError("www local external Path: " + ExternalStorePathUrl);
        }
    }

}