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
        private InputField mAccount;
        private InputField mPassword;
        private  InputField mRepeatPassword;
        private  Button mLogin;
        private  Button mRegister;

        private LoginModel mLoginModel;

        protected override void Awake()
        {
            base.Awake();
            mLogin.name = "Login";
            mLogin.GetComponentInChildren<Text>().text = "登陆";
            mRepeatPassword.gameObject.SetActive(false);
            mAccount.gameObject.SetActive(true);
            mPassword.gameObject.SetActive(true);
            mLoginModel = GetModel<LoginModel>();

            mAccount.text= PlayerPrefs.GetString(CacheManager.cache_key_account,"");
            mPassword.text = PlayerPrefs.GetString(CacheManager.cache_key_password,"");
        }

        protected override void FindObject()
        {
            Transform mt = transform.FindChild("account");
            mAccount = mt.GetComponent<InputField>();

            mt = transform.FindChild("password");
            mPassword = mt.GetComponent<InputField>();

            mt = transform.FindChild("repeatpassword");
            mRepeatPassword = mt.GetComponent<InputField>();

            mt = transform.FindChild("zhuce");
            mRegister = mt.GetComponent<Button>();

            mt = transform.FindChild("denglu");
            mLogin = mt.GetComponent<Button>();
        }

        protected override void AddListener()
        {
            base.AddListener();
            mLogin.onClick.AddListener(OnClick_Login);
            mRegister.onClick.AddListener(OnClick_Register);
        }

        protected override void AddNetEvent()
        {
            base.AddNetEvent();
           // mLoginModel.mRegisterResult.addDataBind(JudgeOrRegisterSuccess);
          //  mLoginModel.mLoginResult.addDataBind(JudegeOrLoginSuccess);
        }

        protected override void RemoveNetEvent()
        {
            base.RemoveNetEvent();
          //  mLoginModel.mLoginResult.removeDataBind(JudegeOrLoginSuccess);
          //  mLoginModel.mRegisterResult.removeDataBind(JudgeOrRegisterSuccess);
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
            if (mLogin.name == "Login")
            {
                DebugSystem.Log("点击登陆");
                // mLoginModel.send_LoginGame(mAccount.text, mPassword.text);
                JudegeOrLoginSuccess(true);
            }
            else if (mLogin.name == "finishregister")
            {
                DebugSystem.Log("点击注册");
               // mLoginModel.Send_RegisterAccount(mAccount.text, mPassword.text, mPassword.text);
                mLogin.name = "Login";
                mLogin.GetComponentInChildren<Text>().text = "登陆";
            }

        }

        private void OnClick_Register()
        {
            mAccount.text = "";
            mPassword.text = "";
            mLogin.name = "finishregister";
            mLogin.GetComponentInChildren<Text>().text = "提交注册";
        }

        public void JudgeOrRegisterSuccess(bool result)
        {
            if (result)
            {
                mLogin.name = "Login";
                mLogin.GetComponentInChildren<Text>().text = "登陆";
                DebugSystem.LogError("注册成功");
                mAccount.text = "";
                mPassword.text = "";
            }
            else
            {
                DebugSystem.LogError("注册失败");
            }
        }

        public void JudegeOrLoginSuccess(bool result)
        {
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