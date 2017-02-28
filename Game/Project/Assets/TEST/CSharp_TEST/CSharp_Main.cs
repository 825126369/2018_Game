using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using xk_System.Crypto;
namespace TEST
{
    public class CSharp_Main : MonoBehaviour
    {
        void Start()
        {
            string data = "我要吃屎了";
            Encryption_RSA m = new Encryption_RSA();
            string publicKey = string.Empty;
            string privateKey = string.Empty;
            m.Initial(ref publicKey,ref privateKey);

            string sign=string.Empty; 
            m.SignatureFormatter(privateKey, data,ref sign);
            bool result= m.SignatureDeformatter(publicKey,data,sign);
            if(result==true)
            {
                Debug.Log("验证签名成功");
            }else
            {
                Debug.Log("验证签名失败");
            }
            
        }

    }
}