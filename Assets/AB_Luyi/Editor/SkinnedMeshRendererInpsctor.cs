using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SkinnedMeshRenderer), true)]
public class SkinnedMeshRendererInpsctor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Bake"))
        {
            SkinnedMeshRenderer r = 
                Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
            Mesh m = new Mesh();
            r.BakeMesh(m);
            GameObject o = new GameObject();
            MeshFilter mf = o.AddComponent<MeshFilter>();
            mf.mesh = m;
            MeshRenderer mr = o.AddComponent<MeshRenderer>();
            mr.sharedMaterials = r.sharedMaterials;
            o.transform.position = r.transform.position;
            o.transform.rotation = r.transform.rotation;
            o.transform.localScale = Vector3.one;

            Selection.activeGameObject = o;
        }
        GUILayout.EndHorizontal();
    }

}
