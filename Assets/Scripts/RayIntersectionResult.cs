using UnityEngine;

public struct RayIntersectionResult
{
	public readonly INode node;
	public readonly float entryDistance;
	public readonly Vector3 position;
	public readonly Vector3 normal;
	public readonly NeighbourSide neighbourSide;
	public readonly bool hit;
	public readonly Coords coords;
	public readonly IOctree tree;

	public RayIntersectionResult(bool hit)
	{
		this.hit = hit;
		node = null;
		coords = null;
		entryDistance = 0;
		position = new Vector3();
		normal = new Vector3();
		neighbourSide = NeighbourSide.Invalid;
		tree = null;

	}

	public RayIntersectionResult(IOctree tree, 
		INode node,
		Coords coords,
		float entryDistance,
		Vector3 position,
		Vector3 normal, NeighbourSide neighbourSide)
	{
		hit = true;
		this.tree = tree;
		this.node = node;
		this.coords = coords;
		this.entryDistance = entryDistance;
		this.position = position;
		this.normal = normal;
		this.neighbourSide = neighbourSide;
	}
}