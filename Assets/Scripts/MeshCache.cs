using System;
using UnityEngine;

[Serializable]
public class MeshCache : ScriptableObject, ISerializationCallbackReceiver {
    [SerializeField]
    private int[] triangles;

    [SerializeField]
    private Vector3[] vertices;

    [SerializeField]
    private Vector2[] uv;

    [SerializeField]
    private Vector2[] uv2;

    [SerializeField]
    private Vector2[] uv3;

    [SerializeField]
    private Vector2[] uv4;

    [SerializeField]
    private Vector3[] normals;

    [SerializeField]
    private Vector4[] tangents;

    [SerializeField]
    private Bounds bounds;

    [SerializeField]
    private Matrix4x4[] bindposes;

    [SerializeField]
    private BoneWeight[] boneweights;

    [SerializeField]
    private Color32[] colors32;

    public Mesh mesh;

    [SerializeField]
    private SubMeshCache[] subMeshes;

    public void OnBeforeSerialize() {
        if (!mesh) {
            return;
        }

        uv = mesh.uv;
        uv2 = mesh.uv2;
        uv3 = mesh.uv3;
        uv4 = mesh.uv4;

        vertices = mesh.vertices;
        triangles = mesh.triangles;
        normals = mesh.normals;
        tangents = mesh.tangents;
        bounds = mesh.bounds;
        bindposes = mesh.bindposes;
        boneweights = mesh.boneWeights;
        colors32 = mesh.colors32;

        subMeshes = new SubMeshCache[mesh.subMeshCount];
        for (var i = 0; i < mesh.subMeshCount; i++) {
            subMeshes[i] = new SubMeshCache {indices = mesh.GetIndices(i), topology = mesh.GetTopology(i)};
        }
    }

    public void OnAfterDeserialize() {
        if (!mesh)
        {
            return;
        }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.uv3 = uv3;
        mesh.uv4 = uv4;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.bounds = bounds;
        mesh.bindposes = bindposes;
        mesh.boneWeights = boneweights;
        mesh.colors32 = colors32;
        mesh.triangles = triangles;

        mesh.subMeshCount = subMeshes.Length;

        for (var i = 0; i < subMeshes.Length; i++) {
            mesh.SetIndices(subMeshes[i].indices, subMeshes[i].topology, i);
        }
    }
}