using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using xk_System.Model;
using xk_System.Model.Modules;
using xk_System.Debug;

namespace xk_System.View.Modules
{
    public class LoginView : xk_View
    {
        public GameObject mLoginView;
        public GameObject mRegisterView;
        public InputField mAccount;
        public InputField mPassword;

        public InputField mRegisterAccount;
        public InputField mRegisterPassword;
        public InputField mRepeatPassword;

        public Button mLoginBtn;
        public Button mShowRegtisterViewBtn;
        public Button mReturnLoginBtn;
        public Button mRegisterBtn;

        private LoginMessage mLoginModel=null;

        protected override void Awake()
        {
            base.Awake();
            mLoginModel = GetModel<LoginMessage>();

            mLoginBtn.onClick.AddListener(OnClick_Login);
            mShowRegtisterViewBtn.onClick.AddListener(OnClick_ShowRegisterView);
            mRegisterBtn.onClick.AddListener(OnClick_Register);
            mReturnLoginBtn.onClick.AddListener(OnClick_ReturnLogin);

            mAccount.text = PlayerPrefs.GetString(CacheManager.cache_key_account, "");
            mPassword.text = PlayerPrefs.GetString(CacheManager.cache_key_password, "");
            mLoginView.SetActive(true);
            mRegisterView.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mLoginModel.mRegisterResult.addDataBind(JudgeOrRegisterSuccess);
            mLoginModel.mLoginResult.addDataBind(JudegeOrLoginSuccess);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mLoginModel.mLoginResult.removeDataBind(JudegeOrLoginSuccess);
            mLoginModel.mRegisterResult.removeDataBind(JudgeOrRegisterSuccess);
        }


        private void OnClick_Login()
        {
            if (string.IsNullOrEmpty(mAccount.text))
            {
                DebugSystem.LogError("账号不能为空");
                return;
            }
            if (string.IsNullOrEmpty(mPassword.text))
            {
                DebugSystem.LogError("密码不能为空");
                return;
            }
            DebugSystem.Log("点击登陆");
            mLoginModel.send_LoginGame(mAccount.text.Trim(), mPassword.text.Trim());     
        }

        private void OnClick_ShowRegisterView()
        {
            mLoginView.SetActive(false);
            mRegisterView.SetActive(true);
        }


        private void OnClick_Register()
        {
            if (string.IsNullOrEmpty(mRegisterAccount.text.Trim()))
            {
                DebugSystem.LogError("注冊账号不能为空");
                return;
            }
            if (string.IsNullOrEmpty(mRegisterPassword.text.Trim()))
            {
                DebugSystem.LogError("注冊密码不能为空");
                return;
            }
            if (mRepeatPassword.text.Trim() !=mRegisterPassword.text.Trim())
            {
                DebugSystem.LogError("Register Password no Equal");
                return;
            }
            DebugSystem.Log("Click RegisterBtn");
            mLoginModel.Send_RegisterAccount(mAccount.text, mPassword.text, mPassword.text);
        }

        private void OnClick_ReturnLogin()
        {
            mLoginView.SetActive(true);
            mRegisterView.SetActive(false);
        }

        public void JudgeOrRegisterSuccess(bool data)
        {
            bool result = data;
            if (result)
            {
                DebugSystem.LogError("注册成功");
                mLoginView.SetActive(true);
                mRegisterView.SetActive(false);
            }
            else
            {
                DebugSystem.LogError("注册失败");
            }
        }

        public void JudegeOrLoginSuccess(bool data)
        {
            bool result = data;
            if (result)
            {
                DebugSystem.Log("登陆成功");
                ShowView<SelectServerView>();
                HideView<LoginView>();

                PlayerPrefs.SetString(CacheManager.cache_key_account,mAccount.text);
                PlayerPrefs.SetString(CacheManager.cache_key_password,mPassword.text);
            }
            else
            {
                DebugSystem.Log("登陆失败");
            }
        }
    }
}