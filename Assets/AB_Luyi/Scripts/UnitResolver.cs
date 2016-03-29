using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

public class UnitResolver : Resolver
{

    public UnitResolver() : base()
    {
        SHARD_AB = @"Unit/Deps/ShardAB.txt";
        AB = @"Unit/Deps/AB.txt";
        RELATED_ROOT_PATH = "Assets/Workshop/Unit/";
        prefix = "unit/";
    }

    public HashSet<string> CheckUnit(string path)
    {
        //输入值为：Grammil_01
        //"Assets/Workshop/Scene/Grammil_01/Grammil_01.prefab"
        string scene_file = "Assets/Workshop/Unit/" + path;
        GameObject scene = LoadUnit(scene_file);//(GameObject)GameObject.Instantiate(LoadScene(path));
        HashSet<string> deps = new HashSet<string>();
        CheckTexture(ref scene, ref sharedFiles, ref deps);
        CheckMesh(ref scene, ref sharedFiles, ref deps);
        CheckAudioClip(ref scene, ref sharedFiles, ref deps);
        CheckAnimation(ref scene, ref sharedFiles, ref deps);
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

    public GameObject LoadUnit(string path)
    {
        string scene_file = path;

        GameObject scene = AssetDatabase.LoadMainAssetAtPath(scene_file) as GameObject;

        if (!objectBundle.ContainsKey(scene_file))
            objectBundle[scene_file] = new List<string>();

        return scene;
    }
}
