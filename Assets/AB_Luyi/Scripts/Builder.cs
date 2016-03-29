using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

public class Builder {
    public static BuildAssetBundleOptions options =
            //BuildAssetBundleOptions.UncompressedAssetBundle |
            BuildAssetBundleOptions.DeterministicAssetBundle;
    public static BuildAssetBundleOptions options2 =
            //BuildAssetBundleOptions.UncompressedAssetBundle |
            BuildAssetBundleOptions.CollectDependencies |
            BuildAssetBundleOptions.CompleteAssets |
            BuildAssetBundleOptions.DeterministicAssetBundle;

    public Builder()
    {
    }

    public void BuildShader()
    {
        List<Object> objs = new List<Object>();
        LoadObjects("Assets/Workshop/Shader/", ref objs);
        foreach (var item in objs)
        {
            AssetImporter ai = item as AssetImporter;
            ai.assetBundleName = "ShadersList.assetBundles";
        }
    }



    /// <summary>
    /// 注意这种方式不会计算依赖
    /// </summary>
    /// <param name="bt"></param>
    public static void BuildSingleScene(string root, BuildTarget bt)
    {
        string path = @"Assets/Workshop/Scene/";
        BuildPipeline.PushAssetDependencies();
        List<Object> objs = new List<Object>();
        objs.Add(AssetDatabase.LoadMainAssetAtPath(path+root+"/"+root+".prefab"));
		objs.Add(AssetDatabase.LoadMainAssetAtPath(path + root + "/" + root + "_nav.prefab"));
		string begin = path + root + "/Lightmap-";
		Object o;
		int count = 0;
		do
		{
			string lm = begin + count + "_comp_light.exr";
			o = AssetDatabase.LoadMainAssetAtPath(lm);
			if(o != null)
			{
				objs.Add(o);
			}
			++count;
		} while (o != null);
		BuildPipeline.BuildAssetBundle(objs[0], objs.ToArray(), "AssetBundle/" + bt.ToString() + "/" + objs[0].name.ToLower() + ".assetBundles".ToLower(), options2, bt);
        BuildPipeline.PopAssetDependencies();
    }

    public void LoadObjects(string path, ref List<Object> objs)
    {
        string[] files = System.IO.Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            string filename = files[i].ToLower();
			if (filename.EndsWith(".shader") || filename.EndsWith(".cginc"))
            {
                objs.Add(AssetImporter.GetAtPath(files[i]));
            }
        }
        string[] dirs = System.IO.Directory.GetDirectories(path);
        for (int i = 0; i < dirs.Length; i++)
        {
            LoadObjects(dirs[i], ref objs);
        }
    }

    public static void ResolveScene()
    {
        Builder b = new Builder();
        SceneResolver sr = new SceneResolver();
        sr.BuildDepsList();
        b.BuildShader();
        sr.BuildSceneDeps();
        sr.BuildScene();
    }

    public static void ResolveUnit()
    {
        Builder b = new Builder();
        UnitResolver sr = new UnitResolver();
        sr.BuildDepsList();
        b.BuildShader();
        sr.BuildDeps();
        sr.BuildTarget();
    }

    public static void Build(BuildTarget bt)
    {
        Builder b = new Builder();
        b.BuildShader();
        BuildPipeline.BuildAssetBundles("AssetBundle/" + bt.ToString(), options, bt);
    }

    public static void GenerateLightmapParam(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
		{
			LightmapParam lmp = r.gameObject.GetComponent<LightmapParam>();
            if(r.lightmapIndex >= 0)
            {
                if (lmp == null)
                    lmp = r.gameObject.AddComponent<LightmapParam>();
                lmp.lightmapIndex = r.lightmapIndex;
                lmp.lightmapScaleOffset = r.lightmapScaleOffset;
            }
			else
			{
				if (lmp != null)
					GameObject.DestroyImmediate(lmp);
			}
        }
        if(PrefabUtility.GetPrefabParent(go) != null)
        {
            PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go), ReplacePrefabOptions.ConnectToPrefab);
        }
    }
}
