using System;
using UnityEngine;

[Serializable]
public class MeshCache : ScriptableObject, ISerializationCallbackReceiver {
    [SerializeField]
    private int[] _triangles;

    [SerializeField]
    private Vector3[] _vertices;

    [SerializeField]
    private Vector2[] _uv;

    [SerializeField]
    private Vector2[] _uv2;

    [SerializeField]
    private Vector2[] _uv3;

    [SerializeField]
    private Vector2[] _uv4;

    [SerializeField]
    private Vector3[] _normals;

    [SerializeField]
    private Vector4[] _tangents;

    [SerializeField]
    private Bounds _bounds;

    [SerializeField]
    private Matrix4x4[] _bindposes;

    [SerializeField]
    private BoneWeight[] _boneweights;

    [SerializeField]
    private Color32[] _colors32;

    public Mesh mesh;

    [SerializeField]
    private SubMeshCache[] _subMeshes;

    public void OnBeforeSerialize() {
        if (!mesh) {
            return;
        }

        _uv = mesh.uv;
        _uv2 = mesh.uv2;
        _uv3 = mesh.uv3;
        _uv4 = mesh.uv4;

        _vertices = mesh.vertices;
        _triangles = mesh.triangles;
        _normals = mesh.normals;
        _tangents = mesh.tangents;
        _bounds = mesh.bounds;
        _bindposes = mesh.bindposes;
        _boneweights = mesh.boneWeights;
        _colors32 = mesh.colors32;

        _subMeshes = new SubMeshCache[mesh.subMeshCount];
        for (var i = 0; i < mesh.subMeshCount; i++) {
            _subMeshes[i] = new SubMeshCache {indices = mesh.GetIndices(i), topology = mesh.GetTopology(i)};
        }
    }

    public void OnAfterDeserialize() {
        if (!mesh)
        {
            return;
        }

        mesh.Clear();

        mesh.vertices = _vertices;
        mesh.uv = _uv;
        mesh.uv2 = _uv2;
        mesh.uv3 = _uv3;
        mesh.uv4 = _uv4;
        mesh.normals = _normals;
        mesh.tangents = _tangents;
        mesh.bounds = _bounds;
        mesh.bindposes = _bindposes;
        mesh.boneWeights = _boneweights;
        mesh.colors32 = _colors32;
        mesh.triangles = _triangles;

        mesh.subMeshCount = _subMeshes.Length;

        for (var i = 0; i < _subMeshes.Length; i++) {
            mesh.SetIndices(_subMeshes[i].indices, _subMeshes[i].topology, i);
        }
    }
}