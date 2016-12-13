using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using xk_System.Db;
using xk_System.Debug;
using UnityEngine.EventSystems;
using xk_System.View;

namespace xk_System.View.Modules
{
    public class SelectServerView : xk_View
    {
        public SelectServerScrollView mScrollView;
        public Text mServerText;
        public Button mSureGame;

        protected override void Awake()
        {
            base.Awake();
            mServerText.text = string.Empty;
        }

        public void RefreshText(ServerListDB mdata)
        {
            mServerText.text = mdata.serverName+mdata.id;
        }
        protected override void AddListener()
        {
            base.AddListener();
            mSureGame.onClick.AddListener(OnClick_SureGame);
        }

        private void OnClick_SureGame()
        {
            HideView<SelectServerView>(true);
            SceneSystem.Instance.GoToScene(SceneInfo.Scene_2);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshView();
        }

        private void RefreshView()
        {
            List<ServerListDB> mLsit= DbManager.Instance.GetDb<ServerListDB>();
            mScrollView.InitView(mLsit);
        }
    }
}