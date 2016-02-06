using UnityEngine.Assertions;

public struct NeighbourCoordsResult
{
	public readonly Coords coordsResult;
	public readonly bool sameTree;
	public readonly IOctree tree;

	public NeighbourCoordsResult(bool sameTree, Coords coordsResult, IOctree tree)
	{
		Assert.IsNotNull(tree, "Cannot have a null tree for a neighbour Coords result, return null instead");
		this.sameTree = sameTree;
		this.coordsResult = coordsResult;
		this.tree = tree;
	}
}