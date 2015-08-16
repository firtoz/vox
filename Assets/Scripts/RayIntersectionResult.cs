using UnityEngine;

public struct RayIntersectionResult<T>
{
    public readonly OctreeNode<T> node;
    public readonly float entryDistance;
    public readonly Vector3 position;
    public readonly Vector3 normal;

    public RayIntersectionResult(OctreeNode<T> node, float entryDistance, Vector3 position, Vector3 normal)
    {
        this.node = node;
        this.entryDistance = entryDistance;
        this.position = position;
        this.normal = normal;
    }
}