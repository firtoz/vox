using UnityEngine;

public class OctreeRenderFace {
    public readonly Vector3[] vertices;
    public readonly Vector2[] uvs;
    public Vector3 normal;
    public int faceIndexInTree;
    public int meshIndex;
    public int vertexIndexInMesh;
    public bool isRemoved = false;

    public OctreeRenderFace(int meshIndex) {
        this.meshIndex = meshIndex;
        vertices = new Vector3[4];
        uvs = new Vector2[4];
        normal = Vector3.zero;
    }
}