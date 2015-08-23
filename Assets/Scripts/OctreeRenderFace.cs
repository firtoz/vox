using UnityEngine;

public class OctreeRenderFace<T>
{
    public readonly Vector3[] vertices;
    public readonly Vector2[] uvs;
    public Vector3 normal;
    public int faceIndexInTree;
    private OctreeNode<T> owner;
    public int meshIndex;
    public int vertexIndexInMesh;
    public bool isRemoved = false;

    public OctreeRenderFace(OctreeNode<T> owner) {
        this.owner = owner;
        vertices = new Vector3[4];
        uvs = new Vector2[4];
        normal = Vector3.zero;
    }
}