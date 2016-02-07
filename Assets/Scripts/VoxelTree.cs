using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public class VoxelTree : OctreeBase<int, VoxelNode, VoxelTree> {
	private const int NUM_VERTICES_FOR_FACE = 4;
	private const int NUM_INDICES_FOR_FACE = 6;

	public const int MaxVerticesForMesh = 65000 - NUM_VERTICES_FOR_FACE * 100; // -100 faces for good measure 
	public const int MaxFacesForMesh = MaxVerticesForMesh / NUM_VERTICES_FOR_FACE;
	public const int MaxIndicesForMesh = MaxFacesForMesh * NUM_INDICES_FOR_FACE;
	private readonly HashSet<VoxelTree> _dirtyTrees = new HashSet<VoxelTree>();

	public bool withCollision = true;
	public bool convexCollision = false;

	private readonly Dictionary<int, List<GameObject>> _gameObjectForMeshInfo = new Dictionary<int, List<GameObject>>();

	private readonly Dictionary<int, MeshInfo<VoxelNode>> _meshInfos = new Dictionary<int, MeshInfo<VoxelNode>>();

	private readonly Dictionary<int, HashSet<OctreeRenderFace>> _nodeFaces =
		new Dictionary<int, HashSet<OctreeRenderFace>>();

	private GameObject _gameObject;

	private SuperVoxelTree.Node _ownerNode;


	[SerializeField] [FormerlySerializedAs("_materials")] public IntMaterial materials = new IntMaterial();



	static VoxelTree() {
		Assert.IsTrue(MaxVerticesForMesh % NUM_VERTICES_FOR_FACE == 0);
		Assert.IsTrue(MaxVerticesForMesh <= 65000);

		Assert.IsTrue(MaxFacesForMesh > 0);
	}

	public VoxelTree(Vector3 center, Vector3 size, bool setOwnerTree = true)
		: base(RootConstructor, new Bounds(center, size)) {
		if (setOwnerTree) {
			var superTree = new SuperVoxelTree(new Bounds(center, size), this);

			_ownerNode = superTree.GetRoot();
		}
	}

	private static VoxelNode RootConstructor(VoxelTree self, Bounds bounds) {
		return new VoxelNode(bounds, self);
	}

	public override VoxelNode ConstructNode(Bounds bounds, VoxelNode parent, OctreeNode.ChildIndex indexInParent) {
		return new VoxelNode(bounds, parent, indexInParent, this);
	}

	protected int GetItemMeshId(int item) {
		return item;
	}

	protected Material GetMeshMaterial(int meshId) {
		if (materials.value.ContainsKey(meshId)) {
			return materials.value[meshId];
		}

		return new Material(Shader.Find("Standard")) {
			hideFlags = HideFlags.DontSave
		};
	}

	private bool IntersectInternal(Transform transform, Ray ray, out RayIntersectionResult result, int? wantedDepth,
		bool debugRaycasts) {
		return base.Intersect(transform, ray, out result, wantedDepth, debugRaycasts);
	}

	public override bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result, int? wantedDepth = null,
		bool debugRaycasts = false) {
		var subIntersectionResult = new RayIntersectionResult(false);

		var intersection = new RayIntersection(transform, GetOwnerNode().GetTree(), ray, false, null, intersectionResult => {
			var intersectionSuperNode = intersectionResult.node as SuperVoxelTree.Node;
			if (intersectionSuperNode == null) {
				return false;
			}

			var intersectedVoxel = intersectionSuperNode.GetItem();

			if (intersectedVoxel == null) {
				return false;
			}

			if (intersectedVoxel.IntersectInternal(transform, ray, out subIntersectionResult, wantedDepth, debugRaycasts)) {
				return true;
			}

			return false;
		}, debugRaycasts);

		result = subIntersectionResult;

		if (intersection.results.Count > 0) {
			return true;
		}

		return false;
	}


	private VoxelTree GetOrCreateNeighbour(NeighbourSide side, bool readOnly) {
		var ownerNeighbour = _ownerNode.GetOrCreateNeighbour(side, readOnly);

		if (ownerNeighbour == null) {
			return null;
		}

		return ownerNeighbour.GetItem();
	}

	public void SetMaterial(int index, Material material) {
		materials.value[index] = material;
	}

	private void UpdateMeshes(GameObject container, int meshId, MeshInfo<VoxelNode> meshInfo,
		List<GameObject> objectsForMesh) {
		if (!meshInfo.isDirty) {
			return;
		}

		meshInfo.isDirty = false;

		var numMeshSegments = meshInfo.meshSegments.Count;

		var numExistingMeshObjects = objectsForMesh.Count;

		if (numExistingMeshObjects > numMeshSegments) {
			// destroy additional mesh objects

			for (var i = numMeshSegments; i < numExistingMeshObjects; ++i) {
				if (meshInfo.dirtyMeshes.Contains(i)) {
					meshInfo.dirtyMeshes.Remove(i);
				}
				var meshObject = objectsForMesh[i];

				if (Application.isPlaying) {
					Object.Destroy(meshObject);
				} else {
					Object.DestroyImmediate(meshObject);
				}

				objectsForMesh[i] = null;
			}

			objectsForMesh.RemoveRange(numMeshSegments, numExistingMeshObjects - numMeshSegments);

			numExistingMeshObjects = objectsForMesh.Count;
		}

		if (numExistingMeshObjects < numMeshSegments) {
			// create missing mesh objects
			for (var i = numExistingMeshObjects; i < numMeshSegments; ++i) {
				var meshObject = CreateNewMesh();

				objectsForMesh.Add(meshObject);

				Profiler.BeginSample("Set transform parent");
				meshObject.transform.SetParent(container.transform, false);
				Profiler.EndSample();

				var meshFilter = meshObject.GetComponent<MeshFilter>();

				meshFilter.sharedMesh = new Mesh();
				if (withCollision) {
					meshObject.GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
				}
			}
		}

		Assert.AreEqual(objectsForMesh.Count, meshInfo.meshSegments.Count);

		for (var i = 0; i < numMeshSegments; ++i) {
			var meshObject = objectsForMesh[i];

			Assert.IsNotNull(meshObject, "Mesh should not be null!");
			Assert.IsTrue(meshObject.activeInHierarchy, "Mesh should not be disabled!");
			meshObject.name = "mesh " + i + " for " + meshId;

			Profiler.BeginSample("Set mesh material for new game object");
			meshObject.GetComponent<MeshRenderer>().sharedMaterial =
				meshInfo.material;
			Profiler.EndSample();

			// update mesh of object
		}

		var numFaces = meshInfo.allFaces.Count;
		var numIndices = numFaces * NUM_INDICES_FOR_FACE;

		if (numMeshSegments == 1) // no need for loop or array copying
		{
			Profiler.BeginSample("Update mesh " + 0);


			if (withCollision) {
				var meshCollider = objectsForMesh[0].GetComponent<MeshCollider>();
				if (meshCollider)
				{
					meshCollider.enabled = false;
				}
			}

			var newMesh = objectsForMesh[0].GetComponent<MeshFilter>().sharedMesh;

			newMesh.Clear();

			var firstSegment = meshInfo.meshSegments[0];
			if (withCollision) {
				// colliders don't like unreferences vertices it seems

				var numVertices = numFaces * NUM_VERTICES_FOR_FACE;

				Profiler.BeginSample("Set mesh properties");

				{
					var vector3Array = new Vector3[numVertices];

					Array.Copy(firstSegment.vertices, vector3Array, numVertices);

					Profiler.BeginSample("Set mesh vertices");
					newMesh.vertices = vector3Array;
					Profiler.EndSample();

					Array.Copy(firstSegment.normals, vector3Array, numVertices);

					Profiler.BeginSample("Set mesh normals");
					newMesh.normals = vector3Array;
					Profiler.EndSample();

					var uvArray = new Vector2[numVertices];

					Array.Copy(firstSegment.uvs, uvArray, numVertices);

					Profiler.BeginSample("Set mesh uvs");
					newMesh.uv = uvArray;
					Profiler.EndSample();
				}

				Profiler.EndSample(); // set mesh pr
			}
			else {
				Profiler.BeginSample("Set mesh properties");
				{
					Profiler.BeginSample("Set mesh vertices");
					newMesh.vertices = firstSegment.vertices;
					Profiler.EndSample();

					Profiler.BeginSample("Set mesh normals");
					newMesh.normals = firstSegment.normals;
					Profiler.EndSample();

					Profiler.BeginSample("Set mesh uvs");
					newMesh.uv = firstSegment.uvs;
					Profiler.EndSample();
				}
				Profiler.EndSample(); // set mesh properties
			}

			var indicesArray = new int[numIndices];

			Array.Copy(firstSegment.indices, indicesArray, numIndices);

			Profiler.BeginSample("Set mesh triangles");
			newMesh.triangles = indicesArray;
			Profiler.EndSample();

			Profiler.EndSample(); // create mesh

			if (withCollision) {
				var meshCollider = objectsForMesh[0].GetComponent<MeshCollider>();
				if (meshCollider)
				{
					meshCollider.enabled = true;
				}
			}
		} else {
			var dirtyMeshes = meshInfo.dirtyMeshes;

			foreach (var segmentIndex in dirtyMeshes) {
				if (objectsForMesh.Count <= segmentIndex) {
					Debug.Log("??? " + segmentIndex + " > " + objectsForMesh.Count);
				}

				var segment = meshInfo.meshSegments[segmentIndex];

				Profiler.BeginSample("Create mesh " + segmentIndex);
				{
					Profiler.BeginSample("get mesh");
					var newMesh = objectsForMesh[segmentIndex].GetComponent<MeshFilter>().sharedMesh;
					newMesh.Clear();
					Profiler.EndSample();

					Profiler.BeginSample("Set mesh properties");

					var numFacesInSegment = numFaces - segmentIndex * MaxFacesForMesh;
					var numVerticesInSegment = numFacesInSegment * NUM_VERTICES_FOR_FACE;
					var numIndicesInSegment = numFacesInSegment * NUM_INDICES_FOR_FACE;

					if (withCollision && numVerticesInSegment < MaxVerticesForMesh) {
						var vector3Array = new Vector3[numVerticesInSegment];

						Array.Copy(segment.vertices, vector3Array, numVerticesInSegment);

						Profiler.BeginSample("Set mesh vertices");
						newMesh.vertices = vector3Array;
						Profiler.EndSample();

						Array.Copy(segment.normals, vector3Array, numVerticesInSegment);

						Profiler.BeginSample("Set mesh normals");
						newMesh.normals = vector3Array;
						Profiler.EndSample();

						var uvArray = new Vector2[numVerticesInSegment];

						Array.Copy(segment.uvs, uvArray, numVerticesInSegment);

						Profiler.BeginSample("Set mesh uvs");
						newMesh.uv = uvArray;
						Profiler.EndSample();
					} else {
						Profiler.BeginSample("Set mesh vertices");
						newMesh.vertices = segment.vertices;
						Profiler.EndSample();

						Profiler.BeginSample("Set mesh normals");
						newMesh.normals = segment.normals;
						Profiler.EndSample();

						Profiler.BeginSample("Set mesh uvs");
						newMesh.uv = segment.uvs;
						Profiler.EndSample();
					}
					Profiler.EndSample(); // set mesh properties

					int[] indicesForMesh;

					if (numIndicesInSegment < MaxIndicesForMesh) {
						Profiler.BeginSample("Slice indices");

						indicesForMesh = new int[numIndicesInSegment];
						Array.Copy(segment.indices, indicesForMesh, numIndicesInSegment);

						Profiler.EndSample(); // slice indices
					} else {
						// all good!

						indicesForMesh = segment.indices;
					}

					Profiler.BeginSample("Set mesh triangles");

					newMesh.triangles = indicesForMesh;

					Profiler.EndSample(); // set mesh triangles
				}

				Profiler.EndSample();
			}

			dirtyMeshes.Clear();
		}
	}

	private GameObject CreateNewMesh() {
		Profiler.BeginSample("Create new gameobject for mesh");


		Type[] types;
		if (withCollision) {
			types = new[] {
				typeof (MeshFilter),
				typeof (MeshRenderer),
				typeof (MeshCollider)
			};
		} else {
			types = new[] {
				typeof (MeshFilter),
				typeof (MeshRenderer)
			};
		}

		var meshObject = new GameObject(string.Empty, types);

		if (withCollision && convexCollision) {
			meshObject.GetComponent<MeshCollider>().convex = true;
		}

		Profiler.EndSample();
		return meshObject;
	}

	public bool ItemsBelongInSameMesh(int a, int b) {
		return GetItemMeshId(a) == GetItemMeshId(b);
	}

	private bool _lockDirty;
	private bool _isRendering;

	public void Render() {
		if (_isRendering) {
			return;
		}

		_isRendering = true;
		_lockDirty = true;
		foreach (var dirtyTree in _dirtyTrees.ToArray()) {
			if (dirtyTree == this) {
				Debug.Log("How dare you");
				continue;
			}
			dirtyTree.Render();
		}

		_lockDirty = false;
		_dirtyTrees.Clear();

		Profiler.BeginSample("Process draw queue");
		ProcessDrawQueue();
		Profiler.EndSample();

		var meshInfos = _meshInfos;

		foreach (var meshPair in meshInfos) {
			var meshInfo = meshPair.Value;
			var meshId = meshPair.Key;

			if (!_gameObjectForMeshInfo.ContainsKey(meshId)) {
				_gameObjectForMeshInfo[meshId] = new List<GameObject>();
			}

			UpdateMeshes(_gameObject, meshId, meshInfo, _gameObjectForMeshInfo[meshId]);
		}

		_isRendering = false;
	}


	private void ProcessDrawQueue() {
		var meshIndex = 0;
		foreach (var meshInfo in _meshInfos.Values) {
			Profiler.BeginSample("Process draw queue for " + meshIndex);
			ProcessDrawQueue(meshInfo);
			Profiler.EndSample();

			Profiler.BeginSample("Process removal queue for " + meshIndex);
			ProcessRemovalQueue(meshInfo);
			Profiler.EndSample();

			meshIndex++;
		}
	}

	private void ProcessDrawQueue(MeshInfo<VoxelNode> meshInfo) {
		var drawQueue = meshInfo.drawQueue;

		foreach (var octreeNode in drawQueue) {
			var nodeHashCode = octreeNode.GetHashCode();
			//redraw all nodes in the 'redraw queue'
			if (_nodeFaces.ContainsKey(nodeHashCode)) {
				RemoveNodeInternal(nodeHashCode);
			}
			AddNodeInternal(octreeNode);
		}

		drawQueue.Clear();
	}


	private void AddNodeInternal(VoxelNode octreeNode) {
		var meshId = GetItemMeshId(octreeNode.GetItem());

		var newFaces = octreeNode.CreateFaces(meshId);

		var meshInfo = GetMeshInfo(octreeNode.GetItem());

		meshInfo.isDirty = true;

		var allFaces = meshInfo.allFaces;

		var removalQueue = meshInfo.removalQueue;

		var newFacesEnumerator = newFaces.GetEnumerator();
		newFacesEnumerator.MoveNext();

		int numFacesToReplace;
		if (removalQueue.Any()) {
			numFacesToReplace = Mathf.Min(newFaces.Count, removalQueue.Count);

			for (var i = 0; i < numFacesToReplace; ++i) {
				var face = newFacesEnumerator.Current;
				if (face == null) {
					throw new Exception("Face cannot be null.");
				}

				newFacesEnumerator.MoveNext();

				var removalFaceIndex = removalQueue[i];

				allFaces[removalFaceIndex] = face;

				var removalVertexIndex = removalFaceIndex * NUM_VERTICES_FOR_FACE;

				var segmentIndex = removalFaceIndex / MaxFacesForMesh;
				var segment = meshInfo.meshSegments[segmentIndex];

				var segmentVertexIndex = removalVertexIndex - segmentIndex * MaxVerticesForMesh;

				for (var j = 0; j < NUM_VERTICES_FOR_FACE; ++j) {
					segment.vertices[segmentVertexIndex + j] = face.vertices[j];
					segment.uvs[segmentVertexIndex + j] = face.uvs[j];
					segment.normals[segmentVertexIndex + j] = face.normal;
				}

				face.faceIndexInTree = removalFaceIndex;
			}

			removalQueue.RemoveRange(0, numFacesToReplace);
		} else {
			numFacesToReplace = 0;
		}

		var numFacesToAdd = newFaces.Count - numFacesToReplace;

		if (numFacesToAdd > 0) {
			var wantedNumFaces = allFaces.Count + numFacesToAdd;
			allFaces.Capacity = wantedNumFaces;

			var lastSegmentIndex = wantedNumFaces / MaxFacesForMesh;

			meshInfo.dirtyMeshes.Add(allFaces.Count / MaxFacesForMesh);

			while (meshInfo.meshSegments.Count <= lastSegmentIndex) {
				meshInfo.dirtyMeshes.Add(meshInfo.meshSegments.Count);

				meshInfo.meshSegments.Add(new MeshSegment());
			}

			for (var j = numFacesToReplace; j < newFaces.Count; ++j) {
				var face = newFacesEnumerator.Current;
				if (face == null) {
					throw new Exception("Face cannot be null.");
				}

				newFacesEnumerator.MoveNext();

				var indexInAllFaces = allFaces.Count;

				var segmentIndex = indexInAllFaces / MaxFacesForMesh;
				var segment = meshInfo.meshSegments[segmentIndex];

				var firstTriangleIndexInSegment = segmentIndex * MaxFacesForMesh;

				var faceIndexInSegment = indexInAllFaces - firstTriangleIndexInSegment;

				var vertexIndexForSegment = faceIndexInSegment * NUM_VERTICES_FOR_FACE;

				face.faceIndexInTree = indexInAllFaces;

				var vertices = segment.vertices;
				var uvs = segment.uvs;
				var normals = segment.normals;

				for (var i = 0; i < NUM_VERTICES_FOR_FACE; ++i) {
					vertices[vertexIndexForSegment + i] = face.vertices[i];
					uvs[vertexIndexForSegment + i] = face.uvs[i];
					normals[vertexIndexForSegment + i] = face.normal;
				}

				var indices = segment.indices;

				var triangleIndexInSegment = faceIndexInSegment * NUM_INDICES_FOR_FACE;

				indices[triangleIndexInSegment] = vertexIndexForSegment;
				indices[triangleIndexInSegment + 1] = vertexIndexForSegment + 1;
				indices[triangleIndexInSegment + 2] = vertexIndexForSegment + 2;

				indices[triangleIndexInSegment + 3] = vertexIndexForSegment;
				indices[triangleIndexInSegment + 4] = vertexIndexForSegment + 2;
				indices[triangleIndexInSegment + 5] = vertexIndexForSegment + 3;

				allFaces.Add(face);
			}
		}

		_nodeFaces.Add(octreeNode.GetHashCode(), newFaces);
	}


	private void RemoveNodeInternal(int nodeHashCode) {
		var facesToRemove = _nodeFaces[nodeHashCode];

		if (facesToRemove.Count > 0) {
			foreach (var face in facesToRemove) {
				var meshInfo = _meshInfos[face.meshIndex];

				meshInfo.allFaces[face.faceIndexInTree].isRemoved = true;

				meshInfo.isDirty = true;

				meshInfo.removalQueue.Add(face.faceIndexInTree);

				meshInfo.dirtyMeshes.Add(face.faceIndexInTree / MaxFacesForMesh);
			}
		}

		_nodeFaces.Remove(nodeHashCode);
	}

	private MeshInfo<VoxelNode> GetMeshInfo(int item) {
		var meshId = GetItemMeshId(item);

		MeshInfo<VoxelNode> meshInfo;

		if (_meshInfos.TryGetValue(meshId, out meshInfo)) {
			return meshInfo;
		}

		meshInfo = new MeshInfo<VoxelNode>(GetMeshMaterial(meshId));
		_meshInfos.Add(meshId, meshInfo);

		return meshInfo;
	}

	private static void ProcessRemovalQueue(MeshInfo<VoxelNode> meshInfo) {
		var allFacesOfMesh = meshInfo.allFaces;

		var removalQueue = meshInfo.removalQueue;
		if (!removalQueue.Any()) {
			return;
		}

		meshInfo.isDirty = true;

		removalQueue.Sort();

		var removalQueueArray = removalQueue.ToArray();

		var removalQueueIndex = 0;

		var faceIndexToRemove = removalQueueArray[removalQueueIndex];
		// [y, y, y, n, y, y, y]
		// [y, y, y, n, y, y] ^ take this and move it left
		// [y, y, y, Y, y, y] boom

		var numFacesToPop = 0;

		//iterate backwards to fill up any blanks
		for (var i = allFacesOfMesh.Count - 1; i >= 0; --i) {
			//iterate only until the first face index
			if (i < faceIndexToRemove) {
				break;
			}

			var currentFace = allFacesOfMesh[i];

			meshInfo.dirtyMeshes.Add(currentFace.faceIndexInTree / MaxFacesForMesh);

			numFacesToPop++;

			//this face is already removed
			if (currentFace.isRemoved) {
				continue;
			}

			//replace the current face with the last non-null face

			allFacesOfMesh[faceIndexToRemove] = currentFace;

//			var vertexIndex = firstRemovalQueueElement * 4;

			var segmentIndex = faceIndexToRemove / MaxFacesForMesh;

			var firstFaceIndexInSegment = segmentIndex * MaxFacesForMesh;
			var faceIndexInSegment = faceIndexToRemove - firstFaceIndexInSegment;
			var vertexIndexForSegment = faceIndexInSegment * NUM_VERTICES_FOR_FACE;

			var segment = meshInfo.meshSegments[segmentIndex];

			var vertices = segment.vertices;
			var uvs = segment.uvs;
			var normals = segment.normals;

			for (var j = 0; j < 4; j++) {
				var vertexIndex = vertexIndexForSegment + j;

				vertices[vertexIndex] = currentFace.vertices[j];
				uvs[vertexIndex] = currentFace.uvs[j];
				normals[vertexIndex] = currentFace.normal;
			}

			//indices don't change, right?

			currentFace.faceIndexInTree = faceIndexToRemove;

			//this face is replaced, try to replace the next one

			removalQueueIndex++;

			if (removalQueueIndex == removalQueueArray.Length) {
				break;
			}

			faceIndexToRemove = removalQueueArray[removalQueueIndex];
		}

		if (numFacesToPop > 0) {
			var index = allFacesOfMesh.Count - numFacesToPop;
			meshInfo.PopFacesFromEnd(index, numFacesToPop, index * 4);
		}

		removalQueue.Clear();
	}


	public override void NodeAdded(VoxelNode octreeNode, bool updateNeighbours) {
		var meshInfo = GetMeshInfo(octreeNode.GetItem());

		var drawQueue = meshInfo.drawQueue;

		if (!drawQueue.Contains(octreeNode)) {
			drawQueue.Add(octreeNode);
		}

		if (updateNeighbours) {
			foreach (var side in OctreeNode.AllSides) {
				var neighbours = octreeNode.GetAllSolidNeighbours(side);

				if (neighbours != null) {
					foreach (var neighbour in neighbours) {
						if (neighbour == null || neighbour.IsDeleted() || !neighbour.HasItem()) {
							continue;
						}

						var neighbourTree = neighbour.GetTree();

						var neighbourDrawQueue = neighbourTree.GetMeshInfo(neighbour.GetItem()).drawQueue;
						if (!neighbourDrawQueue.Contains(neighbour)) {
							neighbourDrawQueue.Add(neighbour);

							if (neighbourTree != this) {
								if (_lockDirty) {
									Debug.Log("?!?!");
								}
								_dirtyTrees.Add(neighbourTree);
							}
						}
					}
				}
			}
		}
	}


	public override void NodeRemoved(VoxelNode octreeNode, bool updateNeighbours) {
		Profiler.BeginSample("Node Removed");
		if (octreeNode.HasItem()) {
			var drawQueue = GetMeshInfo(octreeNode.GetItem()).drawQueue;

			var nodeHashCode = octreeNode.GetHashCode();

			if (_nodeFaces.ContainsKey(nodeHashCode)) {
				RemoveNodeInternal(nodeHashCode);
			}

			if (drawQueue.Contains(octreeNode)) {
				// if it's about to be drawn, it shouldn't.
				drawQueue.Remove(octreeNode);
			}
		}

		if (updateNeighbours) {
			UpdateNeighbours(octreeNode);
		}
		Profiler.EndSample();
	}

	public override void UpdateNeighbours(VoxelNode octreeNode) {
		foreach (var neighbourSide in OctreeNode.AllSides) {
			var neighbours = octreeNode.GetAllSolidNeighbours(neighbourSide);
			if (neighbours == null) {
				continue;
			}

			foreach (var neighbour in neighbours) {
				if (neighbour == null || neighbour.IsDeleted() || !neighbour.HasItem()) {
					continue;
				}

				var neighbourTree = neighbour.GetTree();
				var neighbourMeshInfo = neighbourTree.GetMeshInfo(neighbour.GetItem());
				var neighbourDrawQueue = neighbourMeshInfo.drawQueue;
				if (!neighbourDrawQueue.Contains(neighbour)) {
					neighbourDrawQueue.Add(neighbour);

					if (neighbourTree != this) {
						if (_lockDirty) {
							Debug.Log("!?!?!");
						}
						_dirtyTrees.Add(neighbourTree);
					}
				}
			}
		}
	}

	public NeighbourCoordsResult? GetNeighbourCoordsInfinite(Coords coords, NeighbourSide side, bool readOnly = false) {
		return GetNeighbourCoordsInfinite(this, coords, side, GetOrCreateNeighbour, readOnly);
	}

	public Bounds GetNeighbourBoundsForChild(Coords coords, NeighbourSide neighbourSide) {
		var childBounds = GetRoot().GetChildBounds(coords);

		Vector3 sideDirection;

		switch (neighbourSide) {
			case NeighbourSide.Above:
				sideDirection = Vector3.up;
				break;
			case NeighbourSide.Below:
				sideDirection = Vector3.down;
				break;
			case NeighbourSide.Right:
				sideDirection = Vector3.right;
				break;
			case NeighbourSide.Left:
				sideDirection = Vector3.left;
				break;
			case NeighbourSide.Back:
				sideDirection = Vector3.back;
				break;
			case NeighbourSide.Forward:
				sideDirection = Vector3.forward;
				break;
			case NeighbourSide.Invalid:
				throw new ArgumentOutOfRangeException("neighbourSide", neighbourSide, null);
			default:
				throw new ArgumentOutOfRangeException("neighbourSide", neighbourSide, null);
		}

		return new Bounds(childBounds.center + Vector3.Scale(sideDirection, childBounds.size), childBounds.size);
	}

	public void SetGameObject(GameObject gameObject) {
		_gameObject = gameObject;
	}

	public void SetOwnerNode(SuperVoxelTree.Node ownerNode) {
		_ownerNode = ownerNode;
	}

	public GameObject GetGameObject() {
		return _gameObject;
	}

	public SuperVoxelTree.Node GetOwnerNode() {
		return _ownerNode;
	}

	public void CopyPropertiesFrom(VoxelTree other) {
		foreach (var material in other.materials.value) {
			SetMaterial(material.Key, material.Value);
		}

		withCollision = other.withCollision;
		convexCollision = other.convexCollision;
	}


	public int GetNumMaterials() {
		return materials.value.Count;
	}

	public int GetMaterialIndex(int index) {
		return materials.value.Keys.ElementAt(index);
	}

	public void Clear() {
		foreach (var gameObject in _gameObjectForMeshInfo
			.SelectMany(meshInfoObjects => meshInfoObjects.Value)) {
			Object.Destroy(gameObject);
		}

		_gameObjectForMeshInfo.Clear();
	}
}