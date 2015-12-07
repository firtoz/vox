using System;
using System.Collections.Generic;

public struct OctreeChildCoordinates {
    private static readonly Dictionary<int, OctreeNode.ChildIndex> CoordinateToChildIndices =
        new Dictionary<int, OctreeNode.ChildIndex>();

    static OctreeChildCoordinates() {
        for (var i = 0; i < 8; i++) {
            var index = (OctreeNode.ChildIndex) i;
            CoordinateToChildIndices[FromIndex(index).GetHashCode()] = index;
        }
    }

    public bool Equals(OctreeChildCoordinates other) {
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

    public OctreeChildCoordinates(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public OctreeChildCoordinates(OctreeChildCoordinates other) {
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
        return obj is OctreeChildCoordinates && Equals((OctreeChildCoordinates) obj);
    }

    public OctreeNode.ChildIndex ToIndex() {
        OctreeNode.ChildIndex index;
        if (!CoordinateToChildIndices.TryGetValue(GetHashCode(), out index)) {
            index = OctreeNode.ChildIndex.Invalid;
        }
        return index;
    }

    public static OctreeChildCoordinates FromIndex(OctreeNode.ChildIndex index) {
        switch (index) {
            case OctreeNode.ChildIndex.AboveBackRight:
                return new OctreeChildCoordinates(1, 1, 0);
            case OctreeNode.ChildIndex.AboveBackLeft:
                return new OctreeChildCoordinates(1, 1, 1);
            case OctreeNode.ChildIndex.AboveForwardRight:
                return new OctreeChildCoordinates(0, 1, 0);
            case OctreeNode.ChildIndex.AboveForwardLeft:
                return new OctreeChildCoordinates(0, 1, 1);
            case OctreeNode.ChildIndex.BelowBackRight:
                return new OctreeChildCoordinates(1, 0, 0);
            case OctreeNode.ChildIndex.BelowBackLeft:
                return new OctreeChildCoordinates(1, 0, 1);
            case OctreeNode.ChildIndex.BelowForwardRight:
                return new OctreeChildCoordinates(0, 0, 0);
            case OctreeNode.ChildIndex.BelowForwardLeft:
                return new OctreeChildCoordinates(0, 0, 1);
            default:
                throw new ArgumentOutOfRangeException("index", index, null);
        }
    }
}