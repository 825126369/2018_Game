using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Debug;

namespace xk_System.AssetPackage
{
    public class AtlasManager : Singleton<AtlasManager>
    {
        public const string Atlas_Type_Item = "item";

        private Dictionary<string, Dictionary<string, Sprite>> mAtlasDic = new Dictionary<string, Dictionary<string, Sprite>>();
        public IEnumerator InitAtals()
        {
            Type mType = ResourceABsFolder.Instance.atlas.GetType();
            foreach (var v in mType.GetFields())
            {
                yield return InitAtlas(v.Name);
                DebugSystem.Log("加载Atlas:" + v.Name);
            }
        }

        public IEnumerator InitAtlas(string atlasName)
        {
            Dictionary<string, Sprite> mDic = null;
            if (!mAtlasDic.TryGetValue(atlasName, out mDic))
            {
                mDic = new Dictionary<string, Sprite>();
                List<AssetInfo> mAllAssetInfo = GetAllAssetInfo(atlasName);
                if (mAllAssetInfo.Count > 0)
                {
                    if (GameConfig.Instance.orUseAssetBundle)
                    {
                        yield return AssetBundleManager.Instance.AsyncLoadBundle(mAllAssetInfo[0].bundleName);
                    }
                    foreach (var v in mAllAssetInfo)
                    {
                        Sprite mSprite = null;
                        if (GameConfig.Instance.orUseAssetBundle)
                        {
                            UnityEngine.Object mObj = AssetBundleManager.Instance.LoadAsset(v);
                            if(mObj!=null)
                            {
                                if(mObj is Texture2D)
                                {
                                    DebugSystem.LogError("这是一张Texture:"+v.assetPath);
                                }else
                                {
                                    mSprite = mObj as Sprite;
                                }
                            }
                        }
                        else
                        {
                            mSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(v.assetPath);
                        }

                        if (mSprite != null)
                        {
                            string spriteName = v.assetName.Substring(0, v.assetName.LastIndexOf("."));
                            mDic.Add(spriteName, mSprite);
                        }
                        else
                        {
                            DebugSystem.LogError("Sprite类型不存在:" + v.assetName);

                        }
                    }
                }
                mAtlasDic.Add(atlasName, mDic);
            }
        }

        public Dictionary<string, Sprite> GetAtlas(string atlasName)
        {
            if (mAtlasDic.ContainsKey(atlasName))
            {
                return mAtlasDic[atlasName];
            }else
            {
                DebugSystem.LogError("此图集不存在：" + atlasName);
                return null;
            }
        }

        public Sprite GetSprite(string atlasName, string spriteName)
        {
            Dictionary<string, Sprite> mDic = GetAtlas(atlasName);
            if (mDic!=null && mDic.ContainsKey(spriteName))
            {
                return mDic[spriteName];
            }
            else
            {
                DebugSystem.LogError("此图片不存在：" + spriteName);
                return null;
            }
        }

        private List<AssetInfo> GetAllAssetInfo(string atlasName)
        {
            List<AssetInfo> mAssetInfoList = new List<AssetInfo>();
            Type mType = ResourceABsFolder.Instance.atlas.GetType();
            foreach (var v in mType.GetFields())
            {
                if (v.Name == atlasName)
                {
                    object atlasObject = v.GetValue(ResourceABsFolder.Instance.atlas);
                    Type atlasType = atlasObject.GetType();
                    foreach (var asset in atlasType.GetFields())
                    {
                        AssetInfo mAssetInfo = asset.GetValue(atlasObject) as AssetInfo;
                        mAssetInfoList.Add(mAssetInfo);
                    }
                    break;
                }
            }
            return mAssetInfoList;
        }
    }
}