using System;
using UnityEngine.Assertions;

public struct OctreeChildCoords {
	public bool Equals(OctreeChildCoords other) {
		return _childIndex == other._childIndex;
	}

	public override int GetHashCode() {
		return (int) _childIndex;
	}

	public readonly int z;
	public readonly int y;
	public readonly int x;

	private readonly OctreeNode.ChildIndex _childIndex;

	public OctreeChildCoords(int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;

		_childIndex = CalculateIndex(x, y, z);
	}

	public OctreeChildCoords(OctreeChildCoords other) {
		x = other.x;
		y = other.y;
		z = other.z;

		_childIndex = CalculateIndex(x, y, z);
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
		return _childIndex;
	}

	private static OctreeNode.ChildIndex CalculateIndex(int x, int y, int z) {
		if ((x == 0 || x == 1) 
			&& (y == 0 || y == 1) 
			&& (z == 0 || z == 1)) {
			return (OctreeNode.ChildIndex)(x + 2 * y + 4 * z);

		}

		return OctreeNode.ChildIndex.Invalid;
	}

	public static OctreeChildCoords leftBelowBack = new OctreeChildCoords(0, 0, 0);
	public static OctreeChildCoords leftBelowForward = new OctreeChildCoords(0, 0, 1);
	public static OctreeChildCoords leftAboveBack = new OctreeChildCoords(0, 1, 0);
	public static OctreeChildCoords leftAboveForward = new OctreeChildCoords(0, 1, 1);
	public static OctreeChildCoords rightBelowBack = new OctreeChildCoords(1, 0, 0);
	public static OctreeChildCoords rightBelowForward = new OctreeChildCoords(1, 0, 1);
	public static OctreeChildCoords rightAboveBack = new OctreeChildCoords(1, 1, 0);
	public static OctreeChildCoords rightAboveForward = new OctreeChildCoords(1, 1, 1);

	public static OctreeChildCoords FromIndex(OctreeNode.ChildIndex index) {
		switch (index)
		{
			case OctreeNode.ChildIndex.LeftBelowBack:
				return leftBelowBack;
			case OctreeNode.ChildIndex.LeftBelowForward:
				return leftBelowForward;
			case OctreeNode.ChildIndex.LeftAboveBack:
				return leftAboveBack;
			case OctreeNode.ChildIndex.LeftAboveForward:
				return leftAboveForward;
			case OctreeNode.ChildIndex.RightBelowBack:
				return rightBelowBack;
			case OctreeNode.ChildIndex.RightBelowForward:
				return rightBelowForward;
			case OctreeNode.ChildIndex.RightAboveBack:
				return rightAboveBack;
			case OctreeNode.ChildIndex.RightAboveForward:
				return rightAboveForward;
			default:
				throw new ArgumentOutOfRangeException("index", index, null);
		}
	}
}