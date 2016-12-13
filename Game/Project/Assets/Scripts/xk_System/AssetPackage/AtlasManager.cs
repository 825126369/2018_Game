using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Debug;

namespace xk_System.AssetPackage
{
    public class AtlasManager : Singleton<AtlasManager>
    {
        private Dictionary<string, Dictionary<string, Sprite>> mAtlasDic = new Dictionary<string, Dictionary<string, Sprite>>();
        public IEnumerator InitAtals()
        {
            Type mType = ResourceABsFolder.Instance.atlas.GetType();
            foreach (var v in mType.GetFields())
            {
                GetAtlas(v.Name);
                yield return 0;
            }
        }

        public Dictionary<string, Sprite> GetAtlas(string atlasName)
        {
            Dictionary<string, Sprite> mDic = null;
            if (!mAtlasDic.TryGetValue(atlasName,out mDic))
            {
                mDic = new Dictionary<string, Sprite>();
                string bundleName = GetBundleName(atlasName);
                AssetBundle mBundle = AssetBundleManager.Instance.LoadBundle(bundleName);
                foreach (var v in mBundle.GetAllAssetNames())
                {
                    string resName = v.Substring(v.LastIndexOf("/")+1);
                    Sprite mSprite = mBundle.LoadAsset<Sprite>(resName);
                    string spriteName = resName.Substring(0, resName.LastIndexOf("."));
                    mDic.Add(spriteName,mSprite);        
                }
                mAtlasDic.Add(atlasName,mDic);
            }
            return mAtlasDic[atlasName];
        }

        public Sprite GetSprite(string atlasName,string spriteName)
        {
            Dictionary<string, Sprite> mDic = GetAtlas(atlasName);
            if (mDic.ContainsKey(spriteName))
            {
                return mDic[spriteName];
            }else
            {
                DebugSystem.LogError("此图片不存在："+spriteName);
                return null;
            }
        }

        private string GetBundleName(string atlasName)
        {
            string bundleName = "atlas_" + atlasName + AssetBundlePath.ABExtention;
            DebugSystem.Log("加载Atals：" + bundleName);
            return bundleName;
        }

        private AssetInfo GetAssetInfo(string atlasName,string spriteName)
        {
            Type mType = ResourceABsFolder.Instance.atlas.GetType();
            foreach (var v in mType.GetFields())
            {
               if(v.Name==atlasName)
                {
                    object atlasObject = v.GetValue(ResourceABsFolder.Instance.atlas);
                    Type atlasType = atlasObject.GetType();
                    foreach (var asset in atlasType.GetFields())
                    {
                        if (asset.Name.Substring(1)==spriteName)
                        {
                            atlasObject = asset.GetValue(atlasObject);
                            return atlasObject as AssetInfo;
                        }
                    }
                    break;
                }
            }
            return null;
        }
    }
}