using UnityEngine;

public struct RayIntersectionResult<T>
{
    public readonly OctreeNode<T> node;
    public readonly float entryDistance;
    public readonly Vector3 position;
    public readonly Vector3 normal;
    public readonly NeighbourSide neighbourSide;
    public readonly bool hit;
    public readonly OctreeNodeCoordinates<T> coordinates;

    public RayIntersectionResult(bool hit) {
        this.hit = hit;
        node = null;
        coordinates = null;
        entryDistance = 0;
        position = new Vector3();
        normal = new Vector3();
        neighbourSide = NeighbourSide.Invalid;
    }

    public RayIntersectionResult(OctreeNode<T> node, 
        OctreeNodeCoordinates<T> coordinates, 
        float entryDistance, 
        Vector3 position, 
        Vector3 normal, NeighbourSide neighbourSide) {
        hit = true;
        this.node = node;
        this.coordinates = coordinates;
        this.entryDistance = entryDistance;
        this.position = position;
        this.normal = normal;
        this.neighbourSide = neighbourSide;
    }
}