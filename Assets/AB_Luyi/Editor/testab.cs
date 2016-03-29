using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class testab : MonoBehaviour {
    //[MenuItem("Assets/Export all asset bundles")]
    //static void Export()
    //{
    //    BuildAssetBundleOptions options =
    //        BuildAssetBundleOptions.CollectDependencies |
    //        BuildAssetBundleOptions.CompleteAssets | 
    //        BuildAssetBundleOptions.UncompressedAssetBundle |
    //        BuildAssetBundleOptions.DeterministicAssetBundle;

    //    GameObject go = GameObject.Find("ShadersList");
    //    ShadersList s = go.GetComponent<ShadersList>();

    //    BuildPipeline.PushAssetDependencies();
    //    BuildPipeline.BuildAssetBundle(null, s.list, "AssetBundle/ShadersList.unity3d", options);

    //    GameObject go2 = GameObject.Find("SharedList");
    //    SharedList s2 = go2.GetComponent<SharedList>();

    //    BuildPipeline.PushAssetDependencies();
    //    BuildPipeline.BuildAssetBundle(null, s2.list, "AssetBundle/Shared.unity3d", options);

    //    BuildPipeline.PushAssetDependencies();
    //    BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/Workshop/Scene1.prefab"), null, "AssetBundle/Scene1.unity3d", options);
    //    BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/Workshop/Scene2.prefab"), null, "AssetBundle/Scene2.unity3d", options);

    //    BuildPipeline.PopAssetDependencies();
    //    BuildPipeline.PopAssetDependencies();
    //    BuildPipeline.PopAssetDependencies();
    //}

    //[MenuItem("Assets/Update shader bundle")]
    //static void ExportShaders()
    //{
    //    BuildAssetBundleOptions options =
    //        BuildAssetBundleOptions.CollectDependencies |
    //        BuildAssetBundleOptions.CompleteAssets |
    //        BuildAssetBundleOptions.DeterministicAssetBundle;

    //    BuildPipeline.PushAssetDependencies();
    //    BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/ShadersList.prefab"), null, "WebPlayer/ShadersList.unity3d", options);

    //    BuildPipeline.PopAssetDependencies();
    //}

	public static void ClearAssetBundleName(string path)
	{
		string[] files = System.IO.Directory.GetFiles(path);
		for (int i = 0; i < files.Length; i++)
		{
			string filename = files[i].ToLower();
			if (!filename.EndsWith(".meta"))
			{
				AssetImporter o = AssetImporter.GetAtPath(files[i]);
				if(o != null)
					o.assetBundleName = "";
			}
		}
		string[] dirs = System.IO.Directory.GetDirectories(path);
		for (int i = 0; i < dirs.Length; i++)
		{
			ClearAssetBundleName(dirs[i]);
		}
	}

    [MenuItem("ABBuilder/Resolve Scene", false, 10)]
    static void Resolve()
    {
		ClearAssetBundleName (Application.dataPath + "/Workshop/Scene/Shared/");
        SceneResolver r;
        r = new SceneResolver();
        r.LoadShared(Application.dataPath + "/Workshop/Scene/Shared/");
        string[] dirs = System.IO.Directory.GetDirectories(Application.dataPath + "/Workshop/Scene/");
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = System.IO.Path.GetFileName(dirs[i]);
            if (name != "Deps" && name != "Shared")
                r.CheckScene(name);
        }
        r.RemoveStandaloneDeps();
        r.CalculateDepsSet();
        r.Serialize();
        Builder.ResolveScene();
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Generate LightmapParam", false, 10)]
    static void GenerateLightmapParam()
    {
        Builder.GenerateLightmapParam(Selection.activeGameObject);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Resolve Objects", false, 10)]
    static void BuildObjects()
    {
        ObjectResolver ob = new ObjectResolver("objs/", "Objects");
        ob.Resolve();
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }


    public static void LoadUnits(string path, ref List<string> objs, string postfix = ".prefab")
    {
        string[] dirs = System.IO.Directory.GetDirectories(path);
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = System.IO.Path.GetFileName(dirs[i]);
            if (name != "Deps" && name != "Shared")
                LoadUnits(dirs[i], ref objs, postfix);
        }
        string[] files = System.IO.Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            string filename = files[i].ToLower();
            if (filename.EndsWith(".prefab"))
            {
                string v = files[i];
                v = v.Replace("\\", "/");
                string sub = "Assets/Workshop/Unit/";
                int index = v.IndexOf(sub);
                v = v.Substring(index + sub.Length);
                objs.Add(v);
            }
        }
    }

    [MenuItem("ABBuilder/Resolve Units", false, 10)]
    static void ResolveUnits()
    {
        UnitResolver r;
        r = new UnitResolver();
        r.LoadShared(Application.dataPath + "/Workshop/Unit/Shared/");
        List<string> objs = new List<string>();
        LoadUnits(Application.dataPath + "/Workshop/Unit/", ref objs);
        for (int i = 0; i < objs.Count; i++)
        {
            r.CheckUnit(objs[i]);
        }
        r.RemoveStandaloneDeps();
        r.CalculateDepsSet();
        r.Serialize();
        Builder.ResolveUnit();
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Resolve Effects", false, 10)]
    static void ResolveEffects()
    {
        ObjectResolver ob = new ObjectResolver("effect/", "Effect");
        ob.Resolve();
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Resolve Sound", false, 10)]
    static void BuildSound()
    {
        ObjectResolver ob = new ObjectResolver("sound/", "Sound");
        ob.Resolve(".mp3");
		ob.Resolve(".wav");
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build All/All", false, 20)]
    static void Build()
    {
        Builder.Build(BuildTarget.Android);
        Builder.Build(BuildTarget.iOS);
        Builder.Build(BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build All/Android", false, 20)]
    static void Build1()
    {
        Builder.Build(BuildTarget.Android);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }


    [MenuItem("ABBuilder/Build All/iOS", false, 20)]
    static void Build2()
    {
        Builder.Build(BuildTarget.iOS);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }


    [MenuItem("ABBuilder/Build All/StandaloneWindows", false, 20)]
    static void Build3()
    {
        Builder.Build(BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build Single Scene/All", false, 20)]
    static void Build4()
    {
        string root = Selection.activeGameObject.name;
        Builder.BuildSingleScene(root, BuildTarget.Android);
        Builder.BuildSingleScene(root, BuildTarget.iOS);
        Builder.BuildSingleScene(root, BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build Single Scene/StandaloneWindows", false, 20)]
    static void Build5()
    {
        string root = Selection.activeGameObject.name;
        Builder.BuildSingleScene(root, BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }


    [MenuItem("ABBuilder/Build Single Scene/Android", false, 20)]
    static void Build31()
    {
        string root = Selection.activeGameObject.name;
        Builder.BuildSingleScene(root, BuildTarget.Android);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build Single Scene/iOS", false, 20)]
    static void Build32()
    {
        string root = Selection.activeGameObject.name;
        Builder.BuildSingleScene(root, BuildTarget.iOS);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build Single Object/All", false, 20)]
    static void Build6()
    {
        Object[] root = Selection.objects;
        ObjectBuilder.BuildSingle(root, BuildTarget.Android);
        ObjectBuilder.BuildSingle(root, BuildTarget.iOS);
        ObjectBuilder.BuildSingle(root, BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build Single Object/Android", false, 20)]
    static void Build7()
    {
        Object[] root = Selection.objects;
        ObjectBuilder.BuildSingle(root, BuildTarget.Android);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }


    [MenuItem("ABBuilder/Build Single Object/iOS", false, 20)]
    static void Build8()
    {
        Object[] root = Selection.objects;
        ObjectBuilder.BuildSingle(root, BuildTarget.iOS);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/Build Single Object/StandaloneWindows", false, 20)]
    static void Build9()
    {
        Object[] root = Selection.objects;
        ObjectBuilder.BuildSingle(root, BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    [MenuItem("ABBuilder/CleanCache", false, 30)]
    static void CleanCache()
    {
        Caching.CleanCache();
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

	[MenuItem("ABBuilder/CleanAssetBundleName", false, 30)]
	static void CleanAssetBundleName()
	{

		string[] names = AssetDatabase.GetAllAssetBundleNames ();
		foreach (var item in names) {
			string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(item);
			foreach (var asset in assets) {			
				AssetImporter.GetAtPath(asset).assetBundleName = null;
			}
		}
		EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
	}
}
