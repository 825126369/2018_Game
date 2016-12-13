using UnityEngine;

public class GameConfig : SingleTonMonoBehaviour<GameConfig>
{
    public bool orUseAssetBundle = true;
    public bool orUseLog = false;
    public UnityConfig mUnityPlatformConfig;
}

public class UnityConfig
{
    public LayerManager mLayerManager = new LayerManager();
}

public class LayerManager
{
    public string layer_rubbish = "rubbish";
}
