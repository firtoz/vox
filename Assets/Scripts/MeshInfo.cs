using System.Collections.Generic;
using UnityEngine;

internal class MeshInfo<TNode> {
	public readonly List<OctreeRenderFace> allFaces = new List<OctreeRenderFace>();

	public readonly HashSet<TNode> drawQueue = new HashSet<TNode>();
	public readonly HashSet<int> dirtyMeshes = new HashSet<int>(); 
	public readonly List<int> indices = new List<int>();

	public readonly Material material;
	public readonly List<Vector3> normals = new List<Vector3>();

	public readonly List<int> removalQueue =
		new List<int>();

	public readonly List<Vector2> uvs = new List<Vector2>();
	public readonly List<Vector3> vertices = new List<Vector3>();
	public bool isDirty;

	public MeshInfo(Material material) {
		this.material = material;
	}


	/// <summary>
	///     Removes a face from the _allFaces list.
	/// </summary>
	/// <param name="index">The face index</param>
	/// <param name="count">Number of faces to remove</param>
	/// <param name="vertexIndexInMesh"></param>
	public void PopFaces(int index, int count, int vertexIndexInMesh) {
		allFaces.RemoveRange(index, count);

		vertices.RemoveRange(vertexIndexInMesh, 4 * count);
		uvs.RemoveRange(vertexIndexInMesh, 4 * count);
		normals.RemoveRange(vertexIndexInMesh, 4 * count);

		indices.RemoveRange(index * 6, 6 * count);
	}
}