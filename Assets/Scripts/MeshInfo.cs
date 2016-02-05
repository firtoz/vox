using System.Collections.Generic;
using UnityEngine;

internal class MeshSegment {
	public readonly int[] indices = new int[VoxelTree.MaxIndicesForMesh];
	public readonly Vector3[] vertices = new Vector3[VoxelTree.MaxVerticesForMesh];
	public readonly Vector2[] uvs = new Vector2[VoxelTree.MaxVerticesForMesh];
	public readonly Vector3[] normals = new Vector3[VoxelTree.MaxVerticesForMesh];
}

internal class MeshInfo<TNode> {
	public readonly List<OctreeRenderFace> allFaces = new List<OctreeRenderFace>();

	public readonly HashSet<TNode> drawQueue = new HashSet<TNode>();
	public readonly HashSet<int> dirtyMeshes = new HashSet<int>(); 

	public readonly List<MeshSegment> meshSegments = new List<MeshSegment>(); 

	public readonly Material material;

	public readonly List<int> removalQueue =
		new List<int>();

	public bool isDirty;

	public MeshInfo(Material material) {
		this.material = material;
	}


	/// <summary>
	///     Removes a face from the _allFaces list.
	/// </summary>
	/// <param name="faceIndex">The face index</param>
	/// <param name="count">Number of faces to remove</param>
	/// <param name="vertexIndexInMesh"></param>
	public void PopFacesFromEnd(int faceIndex, int count, int vertexIndexInMesh) {
		allFaces.RemoveRange(faceIndex, count);

		// remove all segments after this one
		var firstSegmentToRemove = (faceIndex / VoxelTree.MaxFacesForMesh) + 1;

		if (firstSegmentToRemove < meshSegments.Count) {
			// we can delete some segments!
			meshSegments.RemoveRange(firstSegmentToRemove, meshSegments.Count - firstSegmentToRemove);
		}
	}
}