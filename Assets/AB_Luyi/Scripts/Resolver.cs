using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

public class Resolver {
    public static string AssetBundlePath;
    protected static string ROOT_PATH;
    protected static string ResourceRootPath;

    public string SHARD_AB;
    public string AB;
    public string RELATED_ROOT_PATH;
    public string prefix;

    protected Dictionary<string, string> sharedFiles = new Dictionary<string, string>();

    protected Dictionary<string, SortedList<string, string>> sharedDeps = 
        new Dictionary<string, SortedList<string, string>>();

    protected Dictionary<string, List<string>> abPack = new Dictionary<string, List<string>>();
    protected Dictionary<string, List<string>> objectAB = new Dictionary<string, List<string>>();

    protected Dictionary<string, List<string>> objectBundle = new Dictionary<string, List<string>>();

    public Resolver()
    {
        ROOT_PATH = Application.dataPath + "/";
        RELATED_ROOT_PATH = "Assets/Workshop/Scene/";
        AssetBundlePath = ROOT_PATH + "../AssetBundle";
        ResourceRootPath = ROOT_PATH + "Workshop/";
        
	}
	
    public Dictionary<string, string> LoadShared(string path)
    {
        sharedFiles.Clear();
        LoadFile(path, ref sharedFiles, true);

        return sharedFiles;
    }

    public void RemoveStandaloneDeps()
    {
        foreach (var item in new List<string>(sharedDeps.Keys))
        {
            if(sharedDeps[item].Count == 1)
            {
                sharedDeps.Remove(item);
            }
        }
    }

    public void CalculateDepsSet()
    {
        abPack.Clear();
        objectAB.Clear();
        foreach (var item in sharedDeps)
        {
            string s = "";
            foreach (var scene in item.Value.Keys)
            {
                s += scene;
            }
            s = CalculateMD5Hash(s);
            if(!abPack.ContainsKey(s))
            {
                abPack[s] = new List<string>();
                foreach (var scene in item.Value)
                {
                    if(!objectAB.ContainsKey(scene.Key))
                    {
                        objectAB[scene.Key] = new List<string>();
                    }
                    objectAB[scene.Key].Add(s);
                }
            }
            abPack[s].Add(item.Key);
        }
        int i = 1;
    }

    public void CheckTexture(ref GameObject o, ref Dictionary<string, string> shareds, ref HashSet<string> deps)
    {
        //MeshRenderer[] rs = o.GetComponentsInChildren<MeshRenderer>(true);
        //Object[] objs =  EditorUtility.CollectDependencies((UnityEngine.Object[])rs);
        Object[] objs = EditorUtility.CollectDependencies(new UnityEngine.Object[] {o});
        //for (int i = 0; i < rs.Length; i++)
        {
            foreach (Object obj in objs)
            {
                if (obj is Texture)
                {
                    Texture t = obj as Texture;
                    string name = t.name.ToLower() + ".png";
                    if (shareds.ContainsKey(name))
                    {
                        deps.Add(name);
                    }
                    else
                    {
                        name = t.name.ToLower() + ".tga";

                        if (shareds.ContainsKey(name))
                        {
                            deps.Add(name);
                        }
                        else
                        {
                            name = t.name.ToLower() + ".psd";

                            if (shareds.ContainsKey(name))
                            {
                                deps.Add(name);
                            }
                            else
                            {
                                name = t.name.ToLower() + ".cubemap";

                                if (shareds.ContainsKey(name))
                                {
                                    deps.Add(name);
                                }
                            }

                        }
                    }
                }
                else if (obj is Cubemap)
                {
                    Cubemap t = obj as Cubemap;
                    string name = t.name.ToLower() + ".cubemap";
                    if (shareds.ContainsKey(name))
                    {
                        deps.Add(name);
                    }
                }
            }
        }

        
    }
    public void CheckMesh(ref GameObject o, ref Dictionary<string, string> shareds, ref HashSet<string> deps)
    {
        MeshFilter[] rs = o.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < rs.Length; i++)
        {
            Mesh mesh = rs[i].sharedMesh;
            if(mesh != null)
            {
                string name = mesh.name.ToLower() + ".fbx";
                if (shareds.ContainsKey(name))
                {
                    deps.Add(name);
                }
            }
        }

        SkinnedMeshRenderer[] smrs = o.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < smrs.Length; i++)
        {
            Mesh mesh = smrs[i].sharedMesh;
            if (mesh != null)
            {
                string name = mesh.name.ToLower() + ".fbx";
                if (shareds.ContainsKey(name))
                {
                    deps.Add(name);
                }
            }
        }

        MeshCollider[] mcs = o.GetComponentsInChildren<MeshCollider>(true);
        for (int i = 0; i < mcs.Length; i++)
        {
            Mesh mesh = mcs[i].sharedMesh;
            if (mesh != null)
            {
                string name = mesh.name.ToLower() + ".fbx";
                if (shareds.ContainsKey(name))
                {
                    deps.Add(name);
                }
            }
        }
    }

    public void CheckAnimation(ref GameObject o, ref Dictionary<string, string> shareds, ref HashSet<string> deps)
    {
        Animation[] rs = o.GetComponentsInChildren<Animation>(true);
        for (int i = 0; i < rs.Length; i++)
        {
            foreach (AnimationState item in rs[i])
            {
                string name = AssetDatabase.GetAssetPath(item.clip);
                if (shareds.ContainsKey(name))
                {
                    deps.Add(name);
                }
            }
        }
    }

    public void CheckAudioClip(ref GameObject o, ref Dictionary<string, string> shareds, ref HashSet<string> deps)
    {
        AudioSource[] rs = o.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < rs.Length; i++)
        {
            AudioClip ac = rs[i].clip;
            if(ac != null)
            {
                string name = ac.name.ToLower() + ".mp3";
                if (shareds.ContainsKey(name))
                {
                    deps.Add(name);
                }
                else
                {
                    name = ac.name.ToLower() + ".wav";

                    if (shareds.ContainsKey(name))
                    {
                        deps.Add(name);
                    }
                    else
                    {
                        name = ac.name.ToLower() + ".ogg";

                        if (shareds.ContainsKey(name))
                        {
                            deps.Add(name);
                        }
                    }
                }
            }
           
        }
    }

    public void LoadFile(string path, ref Dictionary<string, string> dict, bool contain_ext = false)
    {
        string[] files = System.IO.Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            string filename = files[i].ToLower();
            if (!filename.EndsWith(".meta"))
            {
                string name;
                if (filename.EndsWith(".anim"))
                {
                    name = files[i].Replace("\\", "/");
                    string sub = "Assets/Workshop/";
                    int index = name.IndexOf(sub);
                    name = name.Substring(index);
                }
                else
                {
                    if (contain_ext)
                    {
                        name = System.IO.Path.GetFileName(files[i]).ToLower();
                    }
                    else
                    {
                        name = System.IO.Path.GetFileNameWithoutExtension(files[i]).ToLower();
                    }
                }
                dict[name] = files[i];
            }
        }
        string[] dirs = System.IO.Directory.GetDirectories(path);
        for (int i = 0; i < dirs.Length; i++)
        {
            LoadFile(dirs[i], ref dict, contain_ext);
        }
    }

    public class DataItem
    {
        public string Key;
        public string NavName;
        public string LMName;
        public bool New;

        public List<string> Value;

        public DataItem(string key, string nav, string lm, List<string> value)
        {
            Key = key;
            NavName = nav;
            LMName = lm;
            Value = value;
        }

        public DataItem()
        {
        }
    }

    public class AssetItem
    {
        public string Name;

        public AssetItem(string n)
        {
            Name = n;
        }

        public AssetItem()
        {
        }
    }

    public class SharedItem
    {
        public string Key;

        public List<AssetItem> Value;

        public SharedItem(string key, List<AssetItem> value)
        {
            Key = key;
            Value = value;
        }

        public SharedItem()
        {
        }
    }

    public string SerializeABPackData()
    {
        Dictionary<string, List<Resolver.AssetItem>> ssab;
        if(System.IO.File.Exists(GetShardABFileName()))
        {
            string[] lines = System.IO.File.ReadAllLines(GetShardABFileName());
            string s = "";
            foreach (var item in lines)
            {
                s += item;
            }
            ssab = Resolver.DeserializSharedItem(s);
        }
        else
        {
            ssab = new Dictionary<string, List<Resolver.AssetItem>>();
        }

        List<SharedItem> tempdataitems = new List<SharedItem>(abPack.Count);

        foreach (string key in abPack.Keys)
        {
            List<AssetItem> vals = new List<AssetItem>();
            foreach (var item in abPack[key])
            {
                if(item != "")//没有依赖的场景
                {

                    string v = sharedFiles[item];
                    int index = v.IndexOf(RELATED_ROOT_PATH);
                    string name = v.Substring(index);

                    bool b = true;
                    if (ssab.ContainsKey(key))
                    {
                        for (int i = 0; i < ssab[key].Count; i++)
                        {
                            if (ssab[key][i].Name == name)
                            {
                                b = false;
                                break;
                            }
                        }
                    }
                    vals.Add(new AssetItem(name));
                }
            }
            tempdataitems.Add(new SharedItem(key, vals));
        }

        XmlSerializer serializer = new XmlSerializer(typeof(List<SharedItem>));
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        serializer.Serialize(sw, tempdataitems, ns);

        return sw.ToString();
    }

    public string SerializeABData()
    {
        Dictionary<string, Resolver.DataItem> ssab;
        if (System.IO.File.Exists(GetABDataFileName()))
        {
            string[] lines = System.IO.File.ReadAllLines(GetABDataFileName());
            string s = "";
            foreach (var item in lines)
            {
                s += item;
            }
            ssab = Resolver.DeserializeDataItem(s);
        }
        else
        {
            ssab = new Dictionary<string, Resolver.DataItem>();
        }

        List<DataItem> tempdataitems = new List<DataItem>(objectAB.Count);

        foreach (string key in objectAB.Keys)
        {
            bool n = false;
            if (!ssab.ContainsKey(key)) n = true;
            if(objectBundle.ContainsKey(key) && objectBundle[key].Count >= 2)
            {
                tempdataitems.Add(new DataItem(key, objectBundle[key][0], objectBundle[key][1], objectAB[key]));
            }
            else
            {
                tempdataitems.Add(new DataItem(key, null, null, objectAB[key]));
            }
        }

        XmlSerializer serializer = new XmlSerializer(typeof(List<DataItem>));
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        serializer.Serialize(sw, tempdataitems, ns);

        return sw.ToString();
    }

    public static Dictionary<string, DataItem> DeserializeDataItem(string RawData)
    {
        Dictionary<string, DataItem> dict = new Dictionary<string, DataItem>();

        XmlSerializer xs = new XmlSerializer(typeof(List<DataItem>));
        StringReader sr = new StringReader(RawData);

        List<DataItem> templist = (List<DataItem>)xs.Deserialize(sr);

        foreach (DataItem di in templist)
        {
            dict.Add(di.Key, di);
        }
        return dict;
    }

    public static Dictionary<string, List<AssetItem>> DeserializSharedItem(string RawData)
    {
        Dictionary<string, List<AssetItem>> dict = new Dictionary<string, List<AssetItem>>();

        XmlSerializer xs = new XmlSerializer(typeof(List<SharedItem>));
        StringReader sr = new StringReader(RawData);

        List<SharedItem> templist = (List<SharedItem>)xs.Deserialize(sr);

        foreach (SharedItem di in templist)
        {
            dict.Add(di.Key, di.Value);
        }
        return dict;
    }

    public string CalculateMD5Hash(string input)
    {
        // step 1, calculate MD5 hash from input
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        // step 2, convert byte array to hex string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public void Serialize()
    {
        string s = SerializeABPackData();
        string s2 = SerializeABData();

        using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(ResourceRootPath + SHARD_AB))
        {
            file.Write(s);
        }
        using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(ResourceRootPath + AB))
        {
            file.Write(s2);
        }
    }

    public string GetShardABFileName()
    {
        ROOT_PATH = Application.dataPath + "/";
        AssetBundlePath = ROOT_PATH + "../AssetBundle";
        ResourceRootPath = ROOT_PATH + "Workshop/";

        return ResourceRootPath + SHARD_AB;
    }

    public string GetABDataFileName()
    {
        ROOT_PATH = Application.dataPath + "/";
        AssetBundlePath = ROOT_PATH + "../AssetBundle";
        ResourceRootPath = ROOT_PATH + "Workshop/";

        return ResourceRootPath + AB;
    }

    public void BuildDepsList()
    {
        AssetImporter.GetAtPath("Assets/Workshop/" + SHARD_AB).assetBundleName = prefix + "DepsList.assetBundles";
        AssetImporter.GetAtPath("Assets/Workshop/" + AB).assetBundleName = prefix + "DepsList.assetBundles";
    }

    public void BuildDeps()
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

    public void BuildTarget()
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

            string v = name;
            v = v.Replace("\\", "/");
            string sub = RELATED_ROOT_PATH;
            int index = v.IndexOf(sub);
            v = v.Substring(index + sub.Length);
            v = v.Replace(".prefab", "");

            AssetImporter.GetAtPath(name).assetBundleName = prefix + v + ".assetBundles";

        }
        int i = 1;
    }
}
