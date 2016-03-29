using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class ObjectResolver
{
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

    public ObjectResolver(string _prefix, string _dict)
    {
        ROOT_PATH = Application.dataPath + "/";
        AssetBundlePath = ROOT_PATH + "../AssetBundle";
        ResourceRootPath = ROOT_PATH + "Workshop/";
        dict = _dict;
        prefix = _prefix;
    }

    public void LoadObjects(string path, string postfix = ".prefab")
    {
        string[] files = System.IO.Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            string filename = files[i].ToLower();
            if (filename.EndsWith(postfix))
            {
                string v = files[i];
                v = v.Replace("\\", "/");
                string sub = "Assets/Workshop/" + dict + "/";
                int index = v.IndexOf(sub);
                v = v.Substring(index + sub.Length);
                v = v.Replace(postfix, "");
                AssetImporter o = AssetImporter.GetAtPath(files[i]);
                o.assetBundleName = prefix + v + ".assetBundles";
            }
        }
        string[] dirs = System.IO.Directory.GetDirectories(path);
        for (int i = 0; i < dirs.Length; i++)
        {
            LoadObjects(dirs[i], postfix);
        }
    }

    public void Resolve(string postfix = ".prefab")
    {
        LoadObjects("Assets/Workshop/" + dict, postfix);
    }
}
