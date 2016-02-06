using UnityEngine;

internal class MeshSegment {
	public readonly int[] indices = new int[VoxelTree.MaxIndicesForMesh];
	public readonly Vector3[] vertices = new Vector3[VoxelTree.MaxVerticesForMesh];
	public readonly Vector2[] uvs = new Vector2[VoxelTree.MaxVerticesForMesh];
	public readonly Vector3[] normals = new Vector3[VoxelTree.MaxVerticesForMesh];
}