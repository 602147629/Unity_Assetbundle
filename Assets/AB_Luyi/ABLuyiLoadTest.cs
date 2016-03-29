using UnityEngine;
using System.Collections;

public class ABLuyiLoadTest : MonoBehaviour
{
    string url = null;

    IEnumerator Start()
    {
        Shader.WarmupAllShaders();

        //yield return StartCoroutine(ReloadVersionInfo());

        yield return new WaitForEndOfFrame();

        string resname = "Grammil_01";

        string filePath = "/Region/" + resname + ".assetBundles";

        //  加载.
        AssetBundleUti.OnFinishLoading += AssetBundleUtiCallBack;

        //string url = ProjectSystem.StreamingAssets + filePath;
#if UNITY_IOS
		url = ProjectSystem.PrefixPlatform + Application.streamingAssetsPath + "/IOS/scene/map_haiwan_001.assetBundles";
#else
        url = "file:///F:/XSanguoG/WorkSpaceEnv/XSanguoScene/AssetBundle/StandaloneWindows/Grammil_01.assetBundles";
#endif

        Debug.Log(url);

        if (!AssetBundleUti.GetAB(url, resname, typeof(GameObject), AssetBundleEditor.ABDataType.Normal))
        {

        }
    }

    ABData data = null;

    private void AssetBundleUtiCallBack(string url, string resname, System.Type type, bool success, string errorcode = "")
    {
        Debuger.Log("AssetBundleUtiCallBack:" + url + " " + resname + " " + type + " " + success);

        if (success)
        {
            data = AssetBundleUti.GetObject(url, resname, type);

            Shader.WarmupAllShaders();

        }

        LightmapParam[] lmd = data.gameobject.GetComponentsInChildren<LightmapParam>(true);
        Debug.Log("LM: " + lmd.Length.ToString());
        for (int i = 0; i < lmd.Length; i++)
        {
            Renderer r = lmd[i].gameObject.GetComponent<Renderer>();
            r.gameObject.isStatic = true;
            r.lightmapIndex = lmd[i].lightmapIndex;
            r.lightmapScaleOffset = lmd[i].lightmapScaleOffset;
        }

    }
}
