using UnityEngine;

public abstract partial class OctreeBase<TItem, TNode, TSelf>
    where TSelf : OctreeBase<TItem, TNode, TSelf>
    where TNode : OctreeNodeBase<TItem, TSelf, TNode> {
    public struct RayIntersectionResult {
        public readonly TNode node;
        public readonly float entryDistance;
        public readonly Vector3 position;
        public readonly Vector3 normal;
        public readonly NeighbourSide neighbourSide;
        public readonly bool hit;
        public readonly OctreeNodeCoordinates<TItem, TNode, TSelf> coordinates;

        public RayIntersectionResult(bool hit) {
            this.hit = hit;
            node = null;
            coordinates = null;
            entryDistance = 0;
            position = new Vector3();
            normal = new Vector3();
            neighbourSide = NeighbourSide.Invalid;
        }

        public RayIntersectionResult(TNode node,
            OctreeNodeCoordinates<TItem, TNode, TSelf> coordinates,
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
}