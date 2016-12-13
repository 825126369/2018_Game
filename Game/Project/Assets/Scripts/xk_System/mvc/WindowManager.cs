using UnityEngine;
using System.Collections.Generic;
using System;
using xk_System.Debug;
using System.Collections;
using xk_System.Model;
using xk_System.AssetPackage;
using xk_System.View.Modules;

namespace xk_System.View
{
    /// <summary>
    /// 提供给Unity的View管理器接口
    /// </summary>
    public class WindowManager : SingleTonMonoBehaviour<WindowManager>
    {
        private Dictionary<Type, xk_View> mViewPrefabDic=new Dictionary<Type, xk_View>();
        public UILayout mUILayout=new UILayout();

        public IEnumerator InitWindowManager()
        {
            AssetInfo mAssetInfo = ResourceABsFolder.Instance.manager.mUIRoot;
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
            GameObject obj = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(true);
            DontDestroyOnLoad(obj);
            GameObject mRoot = obj;
            Transform mCanvas = mRoot.transform.FindChild("Canvas/hide");
            mUILayout.hideParent = mCanvas;

            mCanvas = mRoot.transform.FindChild("Canvas/show");
            mUILayout.showParent = mCanvas;

            Camera mCamera = mRoot.transform.FindChild("Camera").GetComponent<Camera>();
            mUILayout.mCamera = mCamera;

            yield return InitAsyncLoadGlobalView<WindowLoadingView>();
        }
        /// <summary>
        /// 唯一对外接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void ShowView<T>(object data = null) where T : xk_View
        {
            Type mType = typeof(T);
            if (!mViewPrefabDic.ContainsKey(mType))
            {
                StartCoroutine(AsyncLoadView<T>(data));
            }
            else
            {
                if (!mViewPrefabDic[mType].gameObject.activeSelf)
                {
                    if (data != null)
                    {
                        mViewPrefabDic[mType].SetInitViewInfo(data);
                    }
                    mViewPrefabDic[mType].gameObject.SetActive(true);
                    DebugSystem.Log("显示界面：" + mType.Name);
                }
            }
        }

        public T GetView<T>() where T : xk_View
        {
            Type mType = typeof(T);
            if (mViewPrefabDic.ContainsKey(mType))
            {
                T view = mViewPrefabDic[mType] as T;
                return view;
            }

            return null;
        }

        public void HideView<T>(bool orDestroy = false) where T : xk_View
        {
            Type mType = typeof(T);
            if (mViewPrefabDic.ContainsKey(mType))
            {
                mViewPrefabDic[mType].gameObject.SetActive(false);
                if (orDestroy)
                {
                    RemoveView<T>();
                }
            }
        }

        private IEnumerator InitAsyncLoadGlobalView<T>(object data = null) where T : xk_View
        {
            AssetInfo mAssetInfo = ViewCollection.Instance.GetViewAssetInfo<T>();
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
            GameObject viewPrefab = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
            if (viewPrefab == null)
            {
                DebugSystem.LogError("没有找到资源:" + mAssetInfo.assetName);
                yield break;
            }
            
            yield return AddView<T>(viewPrefab);
            HideView<T>();
        }

        private IEnumerator AsyncLoadView<T>(object data = null) where T : xk_View
        {
            ShowView<WindowLoadingView>();
            AssetInfo mAssetInfo=ViewCollection.Instance.GetViewAssetInfo<T>();           
            yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
            GameObject viewPrefab = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
            if (viewPrefab == null)
            {
                DebugSystem.LogError("没有找到资源:" + mAssetInfo.assetName);
                yield break;
            }
            yield return AddView<T>(viewPrefab);
            ShowView<T>(data);
            HideView<WindowLoadingView>();            
        }

        private IEnumerator AddView<T>(GameObject obj) where T : xk_View
        {
            Type mType = typeof(T);
            T mView = obj.GetComponent(mType) as T;
            if (mView == null)
            {
                mView = obj.AddComponent<T>();
            }
            yield return mView.WaitInitFinish();
            mView.addLayer();            
            if (!mViewPrefabDic.ContainsKey(mType))
            {
                mViewPrefabDic.Add(mType, mView);
            }
            DebugSystem.Log("Load xk_View Component： " + obj.name);
        }

        private void RemoveView<T>() where T : xk_View
        {
            Type mView = typeof(T);
            if (mViewPrefabDic.ContainsKey(mView))
            {
                Destroy(mViewPrefabDic[mView].gameObject);
                mViewPrefabDic.Remove(mView);
            }
        }

        public void CleanManager()
        {
            List<Type> mRemoveTypeList = new List<Type>();
            foreach(var v in mViewPrefabDic)
            {
                if(!(v.Value is xk_GlobalView))
                {
                    mRemoveTypeList.Add(v.Key);
                    Destroy(v.Value.gameObject);
                }
            }
            foreach(var v in mRemoveTypeList)
            {
                mViewPrefabDic.Remove(v);
            }
        }
    }
    public class UILayout
    {
        public Transform hideParent;
        public Transform showParent;
        public Camera mCamera;
    }

    public class ViewCollection:Singleton<ViewCollection>
    {
        public AssetInfo GetViewAssetInfo<T>() where T : xk_View
        {
            switch (typeof(T).Name)
            {
                case "ChatView":
                    return ResourceABsFolder.Instance.view.mChatView;
                case "LoginView":
                    return ResourceABsFolder.Instance.view.mLoginView;
                case "MainView":
                    return ResourceABsFolder.Instance.view.mMainView;
                case "SelectServerView":
                    return ResourceABsFolder.Instance.view.mSelectServerView;
                case "ShareView":
                    return ResourceABsFolder.Instance.view.mShareView;
                case "HotUpdateView":
                    return ResourceABsFolder.Instance.view.mHotUpdateView;
                case "WindowLoadingView":
                    return ResourceABsFolder.Instance.view.mWindowLoadingView;
                case "MsgBoxView":
                    return ResourceABsFolder.Instance.view.mMsgBoxView;
                case "SceneLoadingView":
                    return ResourceABsFolder.Instance.view.mSceneLoadingView;
                case "StoreView":
                    return ResourceABsFolder.Instance.view.mStoreView;
            }
            DebugSystem.LogError("没有找到资源信息");
            return null;
        }
    }


    public  abstract  class xk_View : MonoBehaviour
    {
        private object data=null;

        protected virtual void FindObject()
        {

        }

        protected virtual void AddListener()
        {
            
        }

        protected virtual void AddNetEvent()
        {

        }

        protected virtual void RemoveNetEvent()
        {

        }

        protected virtual IEnumerator PrepareResource()
        {
            DebugSystem.Log("准备加载xk_View资源");
            yield return 0;
        }

        protected virtual void InitView(object data = null)
        {

        }

        protected virtual void SetViewParent()
        {
            this.transform.SetParent(WindowManager.Instance.mUILayout.showParent);
        }

        protected virtual void SetViewTransform()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            RectTransform mRT = transform.GetComponent<RectTransform>();
            if(mRT==null)
            {
                mRT=gameObject.AddComponent<RectTransform>();
            }
            if (mRT != null)
            {
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
                mRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
                mRT.anchorMin = new Vector2(0, 0);
                mRT.anchorMax = new Vector2(1, 1);
            }
            
        }

        protected virtual void Awake()
        {
            FindObject();
        }

        protected virtual void OnEnable()
        {
            AddNetEvent();
            this.transform.SetAsLastSibling();
        }

        protected virtual void Start()
        {
            AddListener();
        }
        protected virtual void OnDisable()
        {
            this.transform.SetAsFirstSibling();
            RemoveNetEvent();
            StopAllCoroutines();
            data = null;
        }

        protected virtual void OnDestroy()
        {
            
        }

        public IEnumerator WaitInitFinish()
        {
            yield return PrepareResource();
            DebugSystem.Log("初始化" + this.GetType().Name + "完成");
        }

        public void SetInitViewInfo(object data = null)
        {
            this.data = data;
            InitView(data);
        }

        public void addLayer()
        {
            SetViewParent();
            SetViewTransform();
        }


        public T GetModel<T>() where T : xk_Model, new()
        {
            return ModelSystem.Instance.GetModel<T>();
        }

        public T GetView<T>() where T : xk_View
        {
            return WindowManager.Instance.GetView<T>();
        }

        public void ShowView<T>(object data=null) where T:xk_View
        {
            WindowManager.Instance.ShowView<T>(data);
        }

        public void HideView<T>(bool orDestroy=false) where T :xk_View
        {
            WindowManager.Instance.HideView<T>(orDestroy);
        }
    }

    public class xk_GlobalView:xk_View
    {



    }


}