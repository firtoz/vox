using System;
using System.Collections.Generic;

public struct OctreeChildCoords {
    private static readonly Dictionary<int, OctreeNode.ChildIndex> CoordsToChildIndices =
        new Dictionary<int, OctreeNode.ChildIndex>();

    static OctreeChildCoords() {
        for (var i = 0; i < 8; i++) {
            var index = (OctreeNode.ChildIndex) i;
            CoordsToChildIndices[FromIndex(index).GetHashCode()] = index;
        }
    }

    public bool Equals(OctreeChildCoords other) {
        return z == other.z && y == other.y && x == other.x;
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = z;
            hashCode = (hashCode * 397) ^ y;
            hashCode = (hashCode * 397) ^ x;
            return hashCode;
        }
    }

    public readonly int z;
    public readonly int y;
    public readonly int x;

    public OctreeChildCoords(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public OctreeChildCoords(OctreeChildCoords other) {
        x = other.x;
        y = other.y;
        z = other.z;
    }

    public override string ToString() {
        return string.Format("[{0}, {1}, {2}]", x, y, z);
    }

    // override object.Equals
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }
        return obj is OctreeChildCoords && Equals((OctreeChildCoords) obj);
    }

    public OctreeNode.ChildIndex ToIndex() {
        OctreeNode.ChildIndex index;
        if (!CoordsToChildIndices.TryGetValue(GetHashCode(), out index)) {
            index = OctreeNode.ChildIndex.Invalid;
        }
        return index;
    }

    public static OctreeChildCoords FromIndex(OctreeNode.ChildIndex index) {
        switch (index)
        {
            case OctreeNode.ChildIndex.LeftBelowBack:
                return new OctreeChildCoords(0, 0, 0);
            case OctreeNode.ChildIndex.LeftBelowForward:
                return new OctreeChildCoords(0, 0, 1);
            case OctreeNode.ChildIndex.LeftAboveBack:
                return new OctreeChildCoords(0, 1, 0);
            case OctreeNode.ChildIndex.LeftAboveForward:
                return new OctreeChildCoords(0, 1, 1);
            case OctreeNode.ChildIndex.RightBelowBack:
                return new OctreeChildCoords(1, 0, 0);
            case OctreeNode.ChildIndex.RightBelowForward:
                return new OctreeChildCoords(1, 0, 1);
            case OctreeNode.ChildIndex.RightAboveBack:
                return new OctreeChildCoords(1, 1, 0);
            case OctreeNode.ChildIndex.RightAboveForward:
                return new OctreeChildCoords(1, 1, 1);
            default:
                throw new ArgumentOutOfRangeException("index", index, null);
        }
    }
}