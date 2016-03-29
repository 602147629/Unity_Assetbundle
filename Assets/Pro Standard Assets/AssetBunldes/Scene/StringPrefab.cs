using UnityEngine;
using System.Collections;

[System.Serializable]
public class StringPrefabMaterial
{
    //  资源实际名称.
    public string assetname = string.Empty;

    //  资源对象.
    public string assetbundlePath = string.Empty;

    //  挂载的对象.
    public string meshrendererGameObject = string.Empty;

    //
}

[System.Serializable]
public class StringPrefabLightmap
{
    public int lightmapIndex;

    public Vector4 lightmapScaleOffset;
}

[System.Serializable]
public class StringPrefab : ScriptableObject
{
    //  所有材质的索引
    public StringPrefabMaterial[] material;

    //  所有Lightmap的索引
    public StringPrefabLightmap[] lightmap;
}
