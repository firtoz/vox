using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public class VoxelTree : OctreeBase<int, VoxelNode, VoxelTree> {
	private const int MAX_VERTICES_FOR_MESH = 1024 - 4 * 100;
	private const int MAX_FACES_FOR_MESH = MAX_VERTICES_FOR_MESH / 4;
	private const int MAX_INDICES_FOR_MESH = MAX_FACES_FOR_MESH * 6;


	private static readonly Vector3[] VerticesArrayForMesh = new Vector3[MAX_VERTICES_FOR_MESH];
	private static readonly Vector3[] NormalsArrayForMesh = new Vector3[MAX_VERTICES_FOR_MESH];
	private static readonly Vector2[] UvsArrayForMesh = new Vector2[MAX_VERTICES_FOR_MESH];

	//    private readonly List<GameObject> _meshObjects = new List<GameObject>();
	private readonly Dictionary<int, List<GameObject>> _gameObjectForMeshInfo = new Dictionary<int, List<GameObject>>();


	[SerializeField]
	[FormerlySerializedAs("_materials")]
	public IntMaterial materials = new IntMaterial();

	private readonly Dictionary<int, HashSet<OctreeRenderFace>> _nodeFaces =
		new Dictionary<int, HashSet<OctreeRenderFace>>();

	private GameObject _gameObject;

	//    private readonly List<Mesh> _meshes = new List<Mesh>();

	private readonly Dictionary<int, MeshInfo<VoxelNode>> _meshInfos = new Dictionary<int, MeshInfo<VoxelNode>>();

	private SuperVoxelTree.Node _ownerNode;

	private GameObject _renderObject;
	private readonly HashSet<VoxelTree> _dirtyTrees = new HashSet<VoxelTree>();

	public VoxelTree(Vector3 center, Vector3 size, bool setOwnerTree = true) : base(RootConstructor, new Bounds(center, size)) {
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

//    public VoxelTree CreateNeighbour(NeighbourSide side) {
//        var neighbourBounds = GetNeighbourBounds(side);
//
//        var neighbour = new VoxelTree(neighbourBounds.center, neighbourBounds.size);
//
//        foreach (var material in materials) {
//            neighbour.SetMaterial(material.Key, material.Value);
//        }
//
//        neighbour._meshInfos = _meshInfos;
//
//        return neighbour;
//    }

//    private readonly Dictionary<NeighbourSide, TTree> _neighbourTrees = new Dictionary<NeighbourSide, TTree>();

	private bool IntersectInternal(Transform transform, Ray ray, out RayIntersectionResult result, int? wantedDepth, bool debugRaycasts) {
		return base.Intersect(transform, ray, out result, wantedDepth, debugRaycasts);
	}

	public override bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result, int? wantedDepth = null, bool debugRaycasts = false) {
		var subIntersectionResult = new RayIntersectionResult(false);

		var intersection = new RayIntersection(transform, GetOwnerNode().GetTree(), ray, false, null, intersectionResult => {
			var intersectionSuperNode = intersectionResult.node as SuperVoxelTree.Node;
			if (intersectionSuperNode == null)
			{
				return false;
			}

			var intersectedVoxel = intersectionSuperNode.GetItem();

			if (intersectedVoxel == null)
			{
				return false;
			}

			if (intersectedVoxel.IntersectInternal(transform, ray, out subIntersectionResult, wantedDepth, debugRaycasts))
			{
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
//        return null;
//        VoxelTree neighbour;
//        if (_neighbourTrees.TryGetValue(side, out neighbour))
//        {
//            return neighbour;
//        }
//
//        neighbour = CreateNeighbour(side);
//        neighbour._root.RemoveItem();
//
//        neighbour._isCreatedByAnotherTree = true;
//
//        _neighbourTrees.Add(side, neighbour);
//
//        // TODO relink other neighbours.
//
//        neighbour._neighbourTrees.Add(OctreeNode.GetOpposite(side), (TTree)this);
//
//        return neighbour;
	}

	public void SetMaterial(int index, Material material) {
		materials.value[index] = material;
	}

	private static void UpdateMeshes(GameObject container, int meshId, MeshInfo<VoxelNode> meshInfo,
		List<GameObject> objectsForMesh) {
		if (!meshInfo.isDirty) {
			return;
		}

		meshInfo.isDirty = false;

		var verticesArray = meshInfo.vertices.ToArray();
		var normalsArray = meshInfo.normals.ToArray();
		var uvsArray = meshInfo.uvs.ToArray();
		var indicesArray = meshInfo.indices.ToArray();

		var verticesCount = verticesArray.Length;
		var numMeshObjects = verticesCount == 0 ? 0 : verticesCount / MAX_VERTICES_FOR_MESH + 1;

		var numExistingMeshObjects = objectsForMesh.Count;

		if (numExistingMeshObjects > numMeshObjects) {
			// destroy additional mesh objects

			for (var i = numMeshObjects; i < numExistingMeshObjects; ++i) {
				if (meshInfo.dirtyMeshes.Contains(i)) {
					meshInfo.dirtyMeshes.Remove(i);
				}
				var meshObject = objectsForMesh[i];

				if (Application.isPlaying) {
					Object.Destroy(meshObject);
				} else {
					Object.DestroyImmediate(meshObject);
				}
			}

			objectsForMesh.RemoveRange(numMeshObjects, numExistingMeshObjects - numMeshObjects);
		}

		if (numExistingMeshObjects < numMeshObjects) {
			// create missing mesh objects?
			for (var i = numExistingMeshObjects; i < numMeshObjects; ++i) {
				Profiler.BeginSample("Create new gameobject for mesh");
				var meshObject = new GameObject(string.Empty, typeof (MeshFilter),
					typeof (MeshRenderer), typeof(MeshCollider));
				Profiler.EndSample();

				objectsForMesh.Add(meshObject);

				Profiler.BeginSample("Set transform parent");
				meshObject.transform.SetParent(container.transform, false);
				Profiler.EndSample();

				var meshFilter = meshObject.GetComponent<MeshFilter>();

				meshFilter.sharedMesh = new Mesh();
				meshObject.GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
			}
		}

		for (var i = 0; i < numMeshObjects; ++i) {
			var meshObject = objectsForMesh[i];

			meshObject.name = "mesh " + i + " for " + meshId;

			Profiler.BeginSample("Set mesh material for new game object");
			meshObject.GetComponent<MeshRenderer>().sharedMaterial =
				meshInfo.material;
			Profiler.EndSample();

			// update mesh of object
		}

		if (numMeshObjects == 1) // no need for loop or array copying
		{
			Profiler.BeginSample("Update mesh " + 0);


			var meshCollider = objectsForMesh[0].GetComponent<MeshCollider>();
			if (meshCollider)
			{
				meshCollider.enabled = false;
			}

			var newMesh = objectsForMesh[0].GetComponent<MeshFilter>().sharedMesh;

			newMesh.Clear();

			Profiler.BeginSample("Get vertices range");
			var vertexArray = verticesArray;
			Profiler.EndSample();

			{
				Profiler.BeginSample("Set mesh properties");
				{
					Profiler.BeginSample("Set mesh vertices");
					newMesh.vertices = vertexArray;
					Profiler.EndSample();

					Profiler.BeginSample("Set mesh normals");
					newMesh.normals = normalsArray;
					Profiler.EndSample();

					Profiler.BeginSample("Set mesh uvs");
					newMesh.uv = uvsArray;
					Profiler.EndSample();
				}
				Profiler.EndSample(); // set mesh properties
			}

			Profiler.BeginSample("Set mesh triangles");
			newMesh.triangles = indicesArray;
			Profiler.EndSample();

			Profiler.EndSample(); // create mesh

			if (meshCollider) {
				meshCollider.enabled = true;
			}
		} else {

			var dirtyMeshes = meshInfo.dirtyMeshes;

			foreach (var i in dirtyMeshes) {
				if (objectsForMesh.Count <= i) {
					Debug.Log("??? " + i + " > " + objectsForMesh.Count);
				}
//			for (var i = 0; i < numMeshObjects; ++i) {
				Profiler.BeginSample("Create mesh " + i);
				{
					Profiler.BeginSample("get mesh");
					var newMesh = objectsForMesh[i].GetComponent<MeshFilter>().sharedMesh;
					newMesh.Clear();
					Profiler.EndSample();

					var vertexStart = i * MAX_VERTICES_FOR_MESH;
					var vertexCount = Mathf.Min(vertexStart + MAX_VERTICES_FOR_MESH, verticesCount) - vertexStart;

					Profiler.BeginSample("Get vertices range");
					Array.Copy(verticesArray, vertexStart, VerticesArrayForMesh, 0, vertexCount);
					Profiler.EndSample();

					{
						Profiler.BeginSample("Set mesh properties");

						{
							Profiler.BeginSample("Set mesh vertices");
							newMesh.vertices = VerticesArrayForMesh;
							Profiler.EndSample();

							Profiler.BeginSample("Set mesh normals");
							Array.Copy(normalsArray, vertexStart, NormalsArrayForMesh, 0, vertexCount);
							newMesh.normals = NormalsArrayForMesh;
							Profiler.EndSample();

							Profiler.BeginSample("Set mesh uvs");
							Array.Copy(uvsArray, vertexStart, UvsArrayForMesh, 0, vertexCount);
							newMesh.uv = UvsArrayForMesh;
							Profiler.EndSample();
						}

						Profiler.EndSample(); // set mesh properties
					}

					var indexStart = i * MAX_INDICES_FOR_MESH;
					var indexCount = vertexCount * 3 / 2;

					Profiler.BeginSample("Set mesh triangles");
					{
						var trianglesArrayForMesh = new int[indexCount];

						// manual copy and alter
						for (var j = 0; j < indexCount; ++j) {
							trianglesArrayForMesh[j] = indicesArray[indexStart + j] - vertexStart;
						}

						newMesh.triangles = trianglesArrayForMesh;
					}
					Profiler.EndSample(); // set mesh triangles
				}

				Profiler.EndSample();
			}

			dirtyMeshes.Clear();
		}
	}

	private static void RenderNewMeshes(GameObject container, int meshId, MeshInfo<VoxelNode> meshInfo,
		List<GameObject> objectsForMesh) {
		var verticesArray = meshInfo.vertices.ToArray();
		var normalsArray = meshInfo.normals.ToArray();
		var uvsArray = meshInfo.uvs.ToArray();
		var indicesArray = meshInfo.indices.ToArray();

		var verticesCount = verticesArray.Length;
		var numMeshObjects = verticesCount / MAX_VERTICES_FOR_MESH + 1;

		if (numMeshObjects == 1) // no need for loop or array copying
		{
			Profiler.BeginSample("Create mesh " + 0);

			Profiler.BeginSample("new mesh");
			var newMesh = new Mesh();
			Profiler.EndSample();

			Profiler.BeginSample("Get vertices range");
			var vertexArray = verticesArray;
			Profiler.EndSample();

			Profiler.BeginSample("Set mesh properties");

			Profiler.BeginSample("Set mesh vertices");
			newMesh.vertices = vertexArray;
			Profiler.EndSample();

			Profiler.BeginSample("Set mesh normals");
			newMesh.normals = normalsArray;
			Profiler.EndSample();

			Profiler.BeginSample("Set mesh uvs");
			newMesh.uv = uvsArray;
			Profiler.EndSample();

			Profiler.EndSample(); // mesh properties

			Profiler.BeginSample("Set mesh triangles");
			newMesh.triangles = indicesArray;
			Profiler.EndSample();

			Profiler.BeginSample("Create new gameobject for mesh");
			var meshObject = new GameObject("mesh " + 0 + " for " + meshId, typeof (MeshFilter),
				typeof (MeshRenderer), typeof(MeshCollider));
			Profiler.EndSample();
			Profiler.BeginSample("Set mesh filter for new game object");
			meshObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
			meshObject.GetComponent<MeshCollider>().sharedMesh = newMesh;
			Profiler.EndSample();

			Profiler.BeginSample("Set mesh material for new game object");
			meshObject.GetComponent<MeshRenderer>().sharedMaterial =
				meshInfo.material;
			Profiler.EndSample();

			//                    _meshes.Add(newMesh);
			objectsForMesh.Add(meshObject);

			Profiler.BeginSample("Set transform parent");
			meshObject.transform.SetParent(container.transform, false);
			Profiler.EndSample();

			Profiler.EndSample();
		} else {
			for (var i = 0; i < numMeshObjects; ++i) {
				Profiler.BeginSample("Create mesh " + i);

				Profiler.BeginSample("new mesh");
				var newMesh = new Mesh();
				Profiler.EndSample();

				var vertexStart = i * MAX_VERTICES_FOR_MESH;
				var vertexCount = Mathf.Min(vertexStart + MAX_VERTICES_FOR_MESH, verticesCount) - vertexStart;

				Profiler.BeginSample("Get vertices range");
				Array.Copy(verticesArray, vertexStart, VerticesArrayForMesh, 0, vertexCount);
				Profiler.EndSample();

				Profiler.BeginSample("Set mesh properties");

				Profiler.BeginSample("Set mesh vertices");
				newMesh.vertices = VerticesArrayForMesh;
				Profiler.EndSample();

				Profiler.BeginSample("Set mesh normals");
				Array.Copy(normalsArray, vertexStart, NormalsArrayForMesh, 0, vertexCount);
				newMesh.normals = NormalsArrayForMesh;
				Profiler.EndSample();

				Profiler.BeginSample("Set mesh uvs");
				Array.Copy(uvsArray, vertexStart, UvsArrayForMesh, 0, vertexCount);
				newMesh.uv = UvsArrayForMesh;
				Profiler.EndSample();

				Profiler.EndSample(); // mesh properties

				var indexStart = i * MAX_INDICES_FOR_MESH;
				var indexCount = vertexCount * 3 / 2;

				Profiler.BeginSample("Set mesh triangles");
				var trianglesArrayForMesh = new int[indexCount];
				// manual copy and alter
				for (var j = 0; j < indexCount; ++j) {
					trianglesArrayForMesh[j] = indicesArray[indexStart + j] - vertexStart;
				}

				newMesh.triangles = trianglesArrayForMesh;

				Profiler.EndSample();

				Profiler.BeginSample("Create new gameobject for mesh");
				var meshObject = new GameObject("mesh " + i + " for " + meshId, typeof (MeshFilter),
					typeof (MeshRenderer), typeof(MeshCollider));
				Profiler.EndSample();
				Profiler.BeginSample("Set mesh filter for new game object");
				meshObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
				meshObject.GetComponent<MeshCollider>().sharedMesh = newMesh;
				Profiler.EndSample();

				Profiler.BeginSample("Set mesh material for new game object");
				meshObject.GetComponent<MeshRenderer>().sharedMaterial =
					meshInfo.material;
				Profiler.EndSample();

				//                        _meshes.Add(newMesh);
				objectsForMesh.Add(meshObject);

				Profiler.BeginSample("Set transform parent");
				meshObject.transform.SetParent(container.transform, false);
				Profiler.EndSample();

				Profiler.EndSample();
			}
		}
	}


	public bool ItemsBelongInSameMesh(int a, int b) {
		return GetItemMeshId(a) == GetItemMeshId(b);
	}

	public void Render() {
		foreach (var dirtyTree in _dirtyTrees) {
			dirtyTree.Render();
		}

		_dirtyTrees.Clear();

		Profiler.BeginSample("Process draw queue");
		ProcessDrawQueue();
		Profiler.EndSample();

		var meshInfos = _meshInfos;
		if (_renderObject != _gameObject) {
			var numChildren = _gameObject.transform.childCount;

			Profiler.BeginSample("Destroy children");
			for (var i = numChildren - 1; i >= 0; i--) {
				var child = _gameObject.transform.GetChild(i).gameObject;
				if (Application.isPlaying) {
					Object.Destroy(child);
				} else {
					Object.DestroyImmediate(child);
				}
			}
			Profiler.EndSample();

			//recreate meshes
			_renderObject = _gameObject;

			Profiler.BeginSample("Recreate meshes");
			foreach (var meshPair in meshInfos) {
				var meshInfo = meshPair.Value;
				var meshId = meshPair.Key;

				Debug.Log("mesh id for new game object" + meshId);

				var objectsForMesh = new List<GameObject>();
				_gameObjectForMeshInfo[meshId] = objectsForMesh;
				RenderNewMeshes(_renderObject, meshId, meshInfo, objectsForMesh);
			}
			Profiler.EndSample();
		} else {
			foreach (var meshPair in meshInfos) {
				var meshInfo = meshPair.Value;
				var meshId = meshPair.Key;

//				Debug.Log("mesh id for new game object" + meshId);

				if (!_gameObjectForMeshInfo.ContainsKey(meshId)) {
					var objectsForMesh = new List<GameObject>();
					_gameObjectForMeshInfo[meshId] = objectsForMesh;
				}

				UpdateMeshes(_renderObject, meshId, meshInfo, _gameObjectForMeshInfo[meshId]);
			}
		}
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

		//        Profiler.BeginSample("Draw Queue Length : " + drawQueue.Count);
		foreach (var octreeNode in drawQueue) {
			var nodeHashCode = octreeNode.GetHashCode();
			//redraw all nodes in the 'redraw queue'
			if (_nodeFaces.ContainsKey(nodeHashCode)) {
				RemoveNodeInternal(nodeHashCode);
			}
			AddNodeInternal(octreeNode);
		}
		//        Profiler.EndSample();

		//        Profiler.BeginSample("Clear draw queue");
		drawQueue.Clear();
		//        Profiler.EndSample();
	}


	private void AddNodeInternal(VoxelNode octreeNode) {
		var meshId = GetItemMeshId(octreeNode.GetItem());

		var newFaces = octreeNode.CreateFaces(meshId);

		var meshInfo = GetMeshInfo(octreeNode.GetItem());

		meshInfo.isDirty = true;
		var vertices = meshInfo.vertices;
		var uvs = meshInfo.uvs;
		var normals = meshInfo.normals;
		var indices = meshInfo.indices;

		var allFaces = meshInfo.allFaces;

		var removalQueue = meshInfo.removalQueue;

		var newFacesEnumerator = newFaces.GetEnumerator();
		newFacesEnumerator.MoveNext();

		int numFacesToReplace;
		if (removalQueue.Any()) {
			numFacesToReplace = Mathf.Min(newFaces.Count, removalQueue.Count);

			for (var i = 0; i < numFacesToReplace; ++i) {
				var face = newFacesEnumerator.Current;
				newFacesEnumerator.MoveNext();

				var removalFaceIndex = removalQueue[i];

				allFaces[removalFaceIndex] = face;

				var removalVertexIndex = removalFaceIndex * 4;

				for (var j = 0; j < 4; ++j) {
					vertices[removalVertexIndex + j] = face.vertices[j];
					uvs[removalVertexIndex + j] = face.uvs[j];
					normals[removalVertexIndex + j] = face.normal;
				}

				face.faceIndexInTree = removalFaceIndex;
				face.vertexIndexInMesh = removalVertexIndex;
			}

			removalQueue.RemoveRange(0, numFacesToReplace);
		} else {
			numFacesToReplace = 0;
		}

		var numFacesToAdd = newFaces.Count - numFacesToReplace;

		if (numFacesToAdd > 0) {
			allFaces.Capacity = allFaces.Count + numFacesToAdd;

			meshInfo.dirtyMeshes.Add(allFaces.Count / MAX_FACES_FOR_MESH);

			normals.Capacity = normals.Count + 4 * numFacesToAdd;
			vertices.Capacity = vertices.Count + 4 * numFacesToAdd;
			uvs.Capacity = uvs.Count + 4 * numFacesToAdd;
			indices.Capacity = indices.Count + 6 * numFacesToAdd;

			for (var j = numFacesToReplace; j < newFaces.Count; ++j)
			{
				var face = newFacesEnumerator.Current;
				newFacesEnumerator.MoveNext();

				var vertexIndex = meshInfo.vertices.Count;

				face.faceIndexInTree = allFaces.Count;
				face.vertexIndexInMesh = vertexIndex;

				allFaces.Add(face);

				for (var i = 0; i < 4; ++i)
				{
					vertices.Add(face.vertices[i]);
					uvs.Add(face.uvs[i]);
					normals.Add(face.normal);
				}

				indices.Add(vertexIndex);
				indices.Add(vertexIndex + 1);
				indices.Add(vertexIndex + 2);

				indices.Add(vertexIndex);
				indices.Add(vertexIndex + 2);
				indices.Add(vertexIndex + 3);
			}
		}

		_nodeFaces.Add(octreeNode.GetHashCode(), newFaces);
	}


	// TODO optimize further!
	// can do the blank filling during the draw queue
	private void RemoveNodeInternal(int nodeHashCode) {
		var facesToRemove = _nodeFaces[nodeHashCode];

		if (facesToRemove.Count > 0) {
			foreach (var face in facesToRemove) {
				var meshInfo = _meshInfos[face.meshIndex];

				meshInfo.allFaces[face.faceIndexInTree].isRemoved = true;

				meshInfo.isDirty = true;

				meshInfo.removalQueue.Add(face.faceIndexInTree);

				meshInfo.dirtyMeshes.Add(face.faceIndexInTree / MAX_FACES_FOR_MESH);
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

		var firstRemovalQueueElement = removalQueueArray[removalQueueIndex];
		var faceIndexOfFirstFaceToRemove = firstRemovalQueueElement;
		// [y, y, y, n, y, y, y]
		// [y, y, y, n, y, y] ^ take this and move it left
		// [y, y, y, Y, y, y]

		var numFacesToPop = 0;

		//iterate backwards to fill up any blanks
		for (var i = allFacesOfMesh.Count - 1; i >= 0; --i) {
			//iterate only until the first face index
			if (i < faceIndexOfFirstFaceToRemove) {
				break;
			}

			var currentFace = allFacesOfMesh[i];

			meshInfo.dirtyMeshes.Add(currentFace.faceIndexInTree / MAX_FACES_FOR_MESH);

			numFacesToPop++;

			//this face is already removed
			if (currentFace.isRemoved) {
				continue;
			}

			//replace the current face with the last non-null face

			allFacesOfMesh[faceIndexOfFirstFaceToRemove] = currentFace;

			var vertexIndex = firstRemovalQueueElement * 4;

			var vertices = meshInfo.vertices;
			var uvs = meshInfo.uvs;
			var normals = meshInfo.normals;

			for (var j = 0; j < 4; j++) {
				vertices[vertexIndex + j] = currentFace.vertices[j];
				uvs[vertexIndex + j] = currentFace.uvs[j];
				normals[vertexIndex + j] = currentFace.normal;
			}

			//indices don't change, right?

			currentFace.faceIndexInTree = faceIndexOfFirstFaceToRemove;
			currentFace.vertexIndexInMesh = vertexIndex;

			//this face is replaced, try to replace the next one

			removalQueueIndex++;

			if (removalQueueIndex == removalQueueArray.Length) {
				break;
			}

			firstRemovalQueueElement = removalQueueArray[removalQueueIndex];
			faceIndexOfFirstFaceToRemove = firstRemovalQueueElement;
		}

		if (numFacesToPop > 0) {
			var index = allFacesOfMesh.Count - numFacesToPop;
			meshInfo.PopFaces(index, numFacesToPop, index * 4);
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
				//if it's about to be drawn, it shouldn't.
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

					if (neighbourTree != this)
					{
						_dirtyTrees.Add(neighbourTree);
					}
				}
			}
		}
	}

	public NeighbourCoordsResult GetNeighbourCoordsInfinite(Coords coords, NeighbourSide side, bool readOnly = false) {
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

	public void CopyMaterialsFrom(VoxelTree myVoxelTree) {
		foreach (var material in myVoxelTree.materials.value)
		{
			SetMaterial(material.Key, material.Value);
		}

//        _meshInfos = myVoxelTree._meshInfos;
	}

	//public void OnBeforeSerialize() {
	//	if (materials.value == null || materials.value.Count == 0)
	//	{
	//		materials = null;

	//		return;
	//	}

	//	if (materials == null) {
	//		materials = new IntMaterial();
	//	}

	//	materials.value.Clear();

	//	foreach (var kvp in materials) {
	//		materials.value.Add(kvp.Key, kvp.Value);
	//	}
	//}

	//public void OnAfterDeserialize() {
	//	materials.Clear();

	//	if (materials == null) {
	//		return;
	//	}

	//	foreach (var kvp in materials.value) {
	//		materials.Add(kvp.Key, kvp.Value);
	//	}
	//}

	public int GetNumMaterials() {
		return materials.value.Count;
	}

	public int GetMaterialIndex(int index) {
		return materials.value.Keys.ElementAt(index);
	}
}