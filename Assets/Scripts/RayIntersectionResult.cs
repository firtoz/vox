using UnityEngine;

public struct RayIntersectionResult<T>
{
    public readonly OctreeNode<T> node;
    public readonly float entryDistance;
    public readonly Vector3 position;
    public readonly Vector3 normal;
    public readonly OctreeNode.NeighbourSide neighbourSide;
    public readonly bool hit;
    public readonly OctreeNodeCoordinates coordinates;

    public RayIntersectionResult(bool hit) {
        this.hit = hit;
        node = null;
        coordinates = null;
        entryDistance = 0;
        position = new Vector3();
        normal = new Vector3();
        neighbourSide = OctreeNode.NeighbourSide.Invalid;
    }

    public RayIntersectionResult(OctreeNode<T> node, OctreeNodeCoordinates coordinates, float entryDistance, Vector3 position, Vector3 normal, OctreeNode.NeighbourSide neighbourSide) {
        hit = true;
        this.node = node;
        this.coordinates = coordinates;
        this.entryDistance = entryDistance;
        this.position = position;
        this.normal = normal;
        this.neighbourSide = neighbourSide;
    }
}