using UnityEngine;
using System.Collections;

[System.Serializable]
public class LightMapAsset : ScriptableObject
{
    //
    public StringPrefabLightmap[] material;

    //  贴图
    public Texture[] lightmapFar;

    //  贴图
    public Texture[] lightmapNear;
}
