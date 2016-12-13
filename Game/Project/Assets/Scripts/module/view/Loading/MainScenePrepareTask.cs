using UnityEngine;
using System.Collections;
using xk_System.AssetPackage;

public class MainScenePrepareTask:Singleton<MainScenePrepareTask>
{
    public LoadProgressInfo mTask = new LoadProgressInfo();
    public IEnumerator Prepare()
    {
        AssetInfo mAssetInfo = ResourceABsFolder.Instance.manager.mScene_Camera;
        yield return AssetBundleManager.Instance.AsyncLoadAsset(mAssetInfo);
        GameObject obj = AssetBundleManager.Instance.LoadAsset(mAssetInfo) as GameObject;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.SetActive(true);
        mTask.progress = 100;
    }
}
