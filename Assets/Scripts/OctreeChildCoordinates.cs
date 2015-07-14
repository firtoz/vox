using System;
using System.Collections.Generic;

public struct OctreeChildCoordinates
{
    private static readonly Dictionary<OctreeChildCoordinates, OctreeNode.ChildIndex> CoordinateToChildIndices = new Dictionary<OctreeChildCoordinates, OctreeNode.ChildIndex>();

    static OctreeChildCoordinates()
    {
        for (var i = 0; i < 8; i++)
        {
            var index = (OctreeNode.ChildIndex) i;
            CoordinateToChildIndices[FromIndex(index)] = index;
        }
    }

    public bool Equals(OctreeChildCoordinates other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = x;
            hashCode = (hashCode*397) ^ y;
            hashCode = (hashCode*397) ^ z;
            return hashCode;
        }
    }

    public int x;
    public int y;
    public int z;

    public OctreeChildCoordinates(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public OctreeChildCoordinates(OctreeChildCoordinates other)
    {
        x = other.x;
        y = other.y;
        z = other.z;
    }

    public override string ToString()
    {
        return string.Format("[{0}, {1}, {2}]", x, y, z);
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is OctreeChildCoordinates && Equals((OctreeChildCoordinates) obj);
    }

    public OctreeNode.ChildIndex ToIndex()
    {
        OctreeNode.ChildIndex index;
        if (!CoordinateToChildIndices.TryGetValue(this, out index))
        {
            index = OctreeNode.ChildIndex.Invalid;
        };
        return index;
    }

    public static OctreeChildCoordinates FromIndex(OctreeNode.ChildIndex index)
    {
        switch (index)
        {
            case OctreeNode.ChildIndex.TopFwdLeft:
                return new OctreeChildCoordinates(0, 1, 1);
            case OctreeNode.ChildIndex.TopFwdRight:
                return new OctreeChildCoordinates(1, 1, 1);
            case OctreeNode.ChildIndex.TopBackLeft:
                return new OctreeChildCoordinates(0, 1, 0);
            case OctreeNode.ChildIndex.TopBackRight:
                return new OctreeChildCoordinates(1, 1, 0);
            case OctreeNode.ChildIndex.BotFwdLeft:
                return new OctreeChildCoordinates(0, 0, 1);
            case OctreeNode.ChildIndex.BotFwdRight:
                return new OctreeChildCoordinates(1, 0, 1);
            case OctreeNode.ChildIndex.BotBackLeft:
                return new OctreeChildCoordinates(0, 0, 0);
            case OctreeNode.ChildIndex.BotBackRight:
                return new OctreeChildCoordinates(1, 0, 0);
            default:
                throw new ArgumentOutOfRangeException("index", index, null);
        }
    }
}