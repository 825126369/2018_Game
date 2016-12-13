using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using xk_System.AssetPackage;
namespace TEST
{
    public class CSharp_Main : MonoBehaviour
    {
        void Start()
        {
            Debug.LogError("热更新代码提示：2222222222222222222222");
            Debug.LogError(GetType().Assembly.FullName+": "+GetType().FullName);
            AssetBundleManager manager = GetComponent<AssetBundleManager>();
            if(manager!=null)
            {
                Debug.LogError(manager.GetType().Assembly.FullName + ": " + manager.GetType().FullName);
                Debug.LogError("第三方程序集找到了: " + manager.GetType());
            }
            else
            {
                Debug.LogError("第三方程序集没找到: ");
            }
        }

        void AAA()
        {

            
        }

        // Update is called once per frame
        void Update()
        {
            //test2.AAA();
        }

        void OnDestroy()
        {

        }
    }
}