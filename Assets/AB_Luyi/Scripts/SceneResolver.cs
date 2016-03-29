using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

public class SceneResolver : Resolver
{
    public SceneResolver() : base()
    {
        SHARD_AB = @"Scene/Deps/SceneShardAB.txt";
        AB = @"Scene/Deps/SceneAB.txt";
        prefix = "scene/";
    }

    public HashSet<string> CheckScene(string path)
    {
        //输入值为：Grammil_01
        //"Assets/Workshop/Scene/Grammil_01/Grammil_01.prefab"
        string scene_file = RELATED_ROOT_PATH + path + "/" + path + ".prefab";
        GameObject scene = LoadScene(path);//(GameObject)GameObject.Instantiate(LoadScene(path));
        HashSet<string> deps = new HashSet<string>();
        CheckTexture(ref scene, ref sharedFiles, ref deps);
        CheckMesh(ref scene, ref sharedFiles, ref deps);
        CheckAudioClip(ref scene, ref sharedFiles, ref deps);
        deps.Add("");

        Debug.Log("-------------------------");
        string log_string = "";
        foreach (var item in deps)
        {
            log_string += item + "\n";
            if (!sharedDeps.ContainsKey(item))
            {
                sharedDeps[item] = new SortedList<string, string>();
            }
            if (!sharedDeps[item].ContainsKey(scene_file))
            {
                sharedDeps[item].Add(scene_file, scene_file);
            }
        }
        Debug.Log(log_string);
        Debug.Log("-------------------------");
        return deps;
    }

    public GameObject LoadScene(string path)
    {
        string scene_root = RELATED_ROOT_PATH;
        string scene_file = scene_root + path + "/" + path + ".prefab";
        string scene_nav_file = scene_root + path + "/" + path + "_nav.prefab";
        string lightmap_file = scene_root + path + "/Lightmap-0_comp_light.exr";

        GameObject scene = AssetDatabase.LoadMainAssetAtPath(scene_file) as GameObject;
        GameObject scene_nav = AssetDatabase.LoadMainAssetAtPath(scene_nav_file) as GameObject;
        GameObject lightmap = AssetDatabase.LoadMainAssetAtPath(lightmap_file) as GameObject;

        if (!objectBundle.ContainsKey(scene_file))
            objectBundle[scene_file] = new List<string>();
        objectBundle[scene_file].Add(scene_nav_file);
        objectBundle[scene_file].Add(lightmap_file);

        if (scene == null || scene_nav == null || lightmap_file == null)
            throw new System.Exception("[Scene]" + path + " :Scene, Navmesh or Lightmap not exist!");

        return scene;
    }

    public void BuildSceneDeps()
    {
        string[] lines = System.IO.File.ReadAllLines(GetShardABFileName());
        string s = "";
        foreach (var item in lines)
        {
            s += item;
        }
        Dictionary<string, List<Resolver.AssetItem>> ssab = DeserializSharedItem(s);
        foreach (var item in ssab)
        {
            string name = item.Key;

            foreach (var it in item.Value)
            {
                AssetImporter.GetAtPath(it.Name).assetBundleName = prefix + name + ".assetBundles";
            }

        }
    }

    public void BuildScene()
    {
        string[] lines = System.IO.File.ReadAllLines(GetABDataFileName());
        string s = "";
        foreach (var item in lines)
        {
            s += item;
        }
        Dictionary<string, Resolver.DataItem> ssab = Resolver.DeserializeDataItem(s);
        foreach (var it in ssab)
        {
            Resolver.DataItem item = it.Value;
            string name = item.Key;


            Object obj = AssetDatabase.LoadMainAssetAtPath(name);
            string objname = obj.name;


            AssetImporter.GetAtPath(name).assetBundleName = prefix + objname + ".assetBundles";
            AssetImporter.GetAtPath(item.NavName).assetBundleName = prefix + objname + ".assetBundles";
            //多张lightmap只能恶心的硬编码了
            int index = item.LMName.IndexOf("Lightmap-0_comp_light.exr");
            string begin = item.LMName.Substring(0, index);
            AssetImporter o;
            int count = 0;
            do
            {
                string lm = begin + "Lightmap-" + count + "_comp_light.exr";
                o = AssetImporter.GetAtPath(lm);
                if(o != null)
                {
                    o.assetBundleName = prefix + objname + ".assetBundles";
                }
                ++count;
            } while (o != null);

        }
        int i = 1;
    }
}
