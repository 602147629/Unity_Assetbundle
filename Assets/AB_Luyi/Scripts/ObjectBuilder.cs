using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class ObjectBuilder {
    public static string AssetBundlePath;
    private static string ROOT_PATH;
    private static string ResourceRootPath;

    public static string SCENE_SHARD_AB = @"Scene/Deps/SceneShardAB.txt";
    public static string SCENE_AB = @"Scene/Deps/SceneAB.txt";

    public string prefix;
    public string dict;

    public static BuildAssetBundleOptions options =
            //BuildAssetBundleOptions.UncompressedAssetBundle |
            BuildAssetBundleOptions.DeterministicAssetBundle;

    public static BuildAssetBundleOptions options_single =
            BuildAssetBundleOptions.CompleteAssets |
        BuildAssetBundleOptions.CollectDependencies |
            BuildAssetBundleOptions.DeterministicAssetBundle;

    public ObjectBuilder(string _prefix, string _dict)
    {
        ROOT_PATH = Application.dataPath + "/";
        AssetBundlePath = ROOT_PATH + "../AssetBundle";
        ResourceRootPath = ROOT_PATH + "Workshop/";
        dict = _dict;
        prefix = _prefix;
    }

    public void LoadObjects(string path)
    {
        string[] files = System.IO.Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            string filename = files[i].ToLower();
            if (filename.EndsWith(".prefab"))
            {
                string v = files[i];
                int index = v.IndexOf("Assets/Workshop/"+ prefix);
                AssetImporter o = AssetImporter.GetAtPath(files[i]);
                o.assetBundleName = prefix + v.Substring(index) + ".assetBundles";
            }
        }
        string[] dirs = System.IO.Directory.GetDirectories(path);
        for (int i = 0; i < dirs.Length; i++)
        {
            LoadObjects(dirs[i]);
        }
    }

    public void Build(BuildTarget bt)
    {
        LoadObjects("Assets/Workshop/" + dict);
        BuildPipeline.BuildAssetBundles("AssetBundle/" + bt.ToString(), options, bt);
        EditorUtility.DisplayDialog("AssetBunlde", "Finish", "Close");
    }

    public static void BuildSingle(Object obj, BuildTarget bt)
    {
        string path = @"Assets/Workshop/Scene/";
        BuildPipeline.PushAssetDependencies();
        BuildPipeline.BuildAssetBundle(null, new Object[] { obj }, "AssetBundle/" + bt.ToString() + "/" + obj.name.ToLower() + ".assetBundles".ToLower(), options_single, bt);
        BuildPipeline.PopAssetDependencies();
    }

    public static void BuildSingle(Object[] objs, BuildTarget bt)
    {
        string path = @"Assets/Workshop/Scene/";

        //var obj = objs[0];
        foreach (var obj in objs)
        {
            BuildPipeline.PushAssetDependencies();
            BuildPipeline.BuildAssetBundle(null, new Object[] { obj }, "AssetBundle/" + bt.ToString() + "/" + obj.name.ToLower() + ".assetBundles".ToLower(), options_single, bt);
            BuildPipeline.PopAssetDependencies();
        }

    }
}
