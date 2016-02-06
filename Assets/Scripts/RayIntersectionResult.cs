using System;
using UnityEngine;

public struct RayIntersectionResult
{
	public readonly INode node;
	public readonly float entryDistance;
	public readonly Vector3 position;
	public readonly Vector3 normal;
	public readonly NeighbourSide neighbourSide;
	public readonly bool hit;
	private readonly Coords _coords;

	public Coords GetCoords() {
		if (!hit) {
			throw new Exception("There are no Coords for this intersection as the .hit property is false.");
		}
		return _coords;
	}

	public readonly IOctree tree;

	public RayIntersectionResult(bool hit)
	{
		this.hit = hit;
		node = null;
		_coords = new Coords();
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
		_coords = coords;
		this.entryDistance = entryDistance;
		this.position = position;
		this.normal = normal;
		this.neighbourSide = neighbourSide;
	}
}