using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class VoxelTree : OctreeBase<int, VoxelNode, VoxelTree> {
    private const int MAX_VERTICES_FOR_MESH = 65000 - 4 * 100;
    private const int MAX_FACES_FOR_MESH = MAX_VERTICES_FOR_MESH / 4;
    private const int MAX_INDICES_FOR_MESH = MAX_FACES_FOR_MESH * 6;

    //    private readonly List<GameObject> _meshObjects = new List<GameObject>();
    private readonly Dictionary<int, List<GameObject>> _gameObjectForMeshInfo = new Dictionary<int, List<GameObject>>();

    private readonly Dictionary<int, Material> _materials = new Dictionary<int, Material>();

    private readonly Dictionary<int, HashSet<OctreeRenderFace>> _nodeFaces =
        new Dictionary<int, HashSet<OctreeRenderFace>>();

    private GameObject _gameObject;

    //    private readonly List<Mesh> _meshes = new List<Mesh>();

    private Dictionary<int, MeshInfo<VoxelNode>> _meshInfos = new Dictionary<int, MeshInfo<VoxelNode>>();

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

    public override VoxelNode ConstructNode(Bounds bounds, VoxelNode parent, OctreeNode.ChildIndex indexInParent,
        int depth) {
        return new VoxelNode(bounds, parent, indexInParent, depth, this);
    }

    protected int GetItemMeshId(int item) {
        return item;
    }

    protected Material GetMeshMaterial(int meshId) {
        if (_materials.ContainsKey(meshId)) {
            return _materials[meshId];
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
//        foreach (var material in _materials) {
//            neighbour.SetMaterial(material.Key, material.Value);
//        }
//
//        neighbour._meshInfos = _meshInfos;
//
//        return neighbour;
//    }

//    private readonly Dictionary<NeighbourSide, TTree> _neighbourTrees = new Dictionary<NeighbourSide, TTree>();

    public override bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result,
        int? wantedDepth = null) {
        return base.Intersect(transform, ray, out result, wantedDepth);
//        result = new RayIntersectionResult(false);
//        return false;
//        _intersecting = true;
//        if (wantedDepth != null && wantedDepth < 0)
//        {
//            throw new ArgumentOutOfRangeException("wantedDepth", "Wanted depth should not be less than zero.");
//        }
//
//        var results = new RayIntersection(transform, (TTree)this, ray, false, wantedDepth).results;
//
//        if (results.Count > 0)
//        {
//            result = results[0];
//            _intersecting = false;
//            return true;
//        }
//
//        foreach (var neighbourTree in _neighbourTrees.Values)
//        {
//            if (neighbourTree._intersecting || !neighbourTree.Intersect(transform, ray, out result, wantedDepth))
//            {
//                continue;
//            }
//
//            _intersecting = false;
//            return true;
//        }
//
//        result = new RayIntersectionResult(false);
//        _intersecting = false;
//        return false;
    }

    private VoxelTree GetOrCreateNeighbour(NeighbourSide side, bool readOnly) {
        var ownerNeighbour = _ownerNode.GetTree().GetOrCreateNeighbour(side, readOnly);

        if (ownerNeighbour == null) {
            return null;
        }

        return ownerNeighbour.GetVoxelTree();
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
        _materials[index] = material;
    }

    private static void UpdateMeshes(GameObject container, int meshId, MeshInfo<VoxelNode> meshInfo,
        List<GameObject> objectsForMesh) {
        var verticesArray = meshInfo.vertices.ToArray();
        var normalsArray = meshInfo.normals.ToArray();
        var uvsArray = meshInfo.uvs.ToArray();
        var indicesArray = meshInfo.indices.ToArray();

        var verticesCount = verticesArray.Length;
        var numMeshObjects = verticesCount / MAX_VERTICES_FOR_MESH + 1;

        var numExistingMeshObjects = objectsForMesh.Count;

        if (numExistingMeshObjects > numMeshObjects) {
            // destroy additional mesh objects

            for (var i = numMeshObjects; i < numExistingMeshObjects; ++i) {
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
                    typeof (MeshRenderer));
                Profiler.EndSample();

                objectsForMesh.Add(meshObject);

                Profiler.BeginSample("Set transform parent");
                meshObject.transform.SetParent(container.transform, false);
                Profiler.EndSample();

                meshObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
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

            var meshCollider = objectsForMesh[0].GetComponent<MeshCollider>();
            if (meshCollider) {
                meshCollider.enabled = false;
                meshCollider.enabled = true;
            }
        } else {
            for (var i = 0; i < numMeshObjects; ++i) {
                Profiler.BeginSample("Create mesh " + i);
                {
                    Profiler.BeginSample("get mesh");
                    var newMesh = objectsForMesh[i].GetComponent<MeshFilter>().sharedMesh;
                    newMesh.Clear();
                    Profiler.EndSample();

                    var vertexStart = i * MAX_VERTICES_FOR_MESH;
                    var vertexCount = Mathf.Min(vertexStart + MAX_VERTICES_FOR_MESH, verticesCount) - vertexStart;

                    Profiler.BeginSample("Get vertices range");
                    var verticesArrayForMesh = new Vector3[vertexCount];
                    Array.Copy(verticesArray, vertexStart, verticesArrayForMesh, 0, vertexCount);
                    Profiler.EndSample();

                    {
                        Profiler.BeginSample("Set mesh properties");

                        {
                            Profiler.BeginSample("Set mesh vertices");
                            newMesh.vertices = verticesArrayForMesh;
                            Profiler.EndSample();

                            Profiler.BeginSample("Set mesh normals");
                            var normalsArrayForMesh = new Vector3[vertexCount];
                            Array.Copy(normalsArray, vertexStart, normalsArrayForMesh, 0, vertexCount);
                            newMesh.normals = normalsArrayForMesh;
                            Profiler.EndSample();

                            Profiler.BeginSample("Set mesh uvs");
                            var uvsArrayForMesh = new Vector2[vertexCount];
                            Array.Copy(uvsArray, vertexStart, uvsArrayForMesh, 0, vertexCount);
                            newMesh.uv = uvsArrayForMesh;
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
                typeof (MeshRenderer));
            Profiler.EndSample();
            Profiler.BeginSample("Set mesh filter for new game object");
            meshObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
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
                var verticesArrayForMesh = new Vector3[vertexCount];
                Array.Copy(verticesArray, vertexStart, verticesArrayForMesh, 0, vertexCount);
                Profiler.EndSample();

                Profiler.BeginSample("Set mesh properties");

                Profiler.BeginSample("Set mesh vertices");
                newMesh.vertices = verticesArrayForMesh;
                Profiler.EndSample();

                Profiler.BeginSample("Set mesh normals");
                var normalsArrayForMesh = new Vector3[vertexCount];
                Array.Copy(normalsArray, vertexStart, normalsArrayForMesh, 0, vertexCount);
                newMesh.normals = normalsArrayForMesh;
                Profiler.EndSample();

                Profiler.BeginSample("Set mesh uvs");
                var uvsArrayForMesh = new Vector2[vertexCount];
                Array.Copy(uvsArray, vertexStart, uvsArrayForMesh, 0, vertexCount);
                newMesh.uv = uvsArrayForMesh;
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
                    typeof (MeshRenderer));
                Profiler.EndSample();
                Profiler.BeginSample("Set mesh filter for new game object");
                meshObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
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

                var objectsForMesh = new List<GameObject>();
                _gameObjectForMeshInfo[meshId] = objectsForMesh;
                RenderNewMeshes(_renderObject, meshId, meshInfo, objectsForMesh);
            }
            Profiler.EndSample();
        } else {
            foreach (var meshPair in meshInfos) {
                var meshInfo = meshPair.Value;
                var meshId = meshPair.Key;

                if (_gameObjectForMeshInfo.ContainsKey(meshId)) {
                    var gameObjectForMeshInfo = _gameObjectForMeshInfo[meshId];

                    UpdateMeshes(_renderObject, meshId, meshInfo, gameObjectForMeshInfo);
                } else {
                    var objectsForMesh = new List<GameObject>();
                    _gameObjectForMeshInfo[meshId] = objectsForMesh;
                }
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
        //        Profiler.BeginSample("AddNodeInternal");
        var meshId = GetItemMeshId(octreeNode.GetItem());

        //        Profiler.BeginSample("Create Faces for octreeNode");
        var newFaces = octreeNode.CreateFaces(meshId);
        //        Profiler.EndSample();

        var meshInfo = GetMeshInfo(octreeNode.GetItem());

        var vertices = meshInfo.vertices;
        var uvs = meshInfo.uvs;
        var normals = meshInfo.normals;
        var indices = meshInfo.indices;

        //        var removalQueue = meshInfo.removalQueue;
        var allFaces = meshInfo.allFaces;


        //        Profiler.BeginSample("Process new faces: " + newFaces.Count);

        //        var numFacesToRemove = removalQueue.Count;
        var numFacesToAdd = newFaces.Count;

        //        var numFacesToReplace = Mathf.Min(numFacesToAdd, numFacesToRemove);

        //        for (var i = 0; i < numFacesToReplace; ++i)
        //        {

        //        }
        allFaces.Capacity = allFaces.Count + numFacesToAdd;

        normals.Capacity = normals.Count + 4 * numFacesToAdd;
        vertices.Capacity = vertices.Count + 4 * numFacesToAdd;
        uvs.Capacity = uvs.Count + 4 * numFacesToAdd;
        indices.Capacity = indices.Count + 6 * numFacesToAdd;

        foreach (var face in newFaces) {
            //if the removal queue isn't empty, replace the last one from there!

            var vertexIndex = meshInfo.vertices.Count;

            face.faceIndexInTree = allFaces.Count;
            face.vertexIndexInMesh = vertexIndex;

            allFaces.Add(face);

            for (var i = 0; i < 4; ++i) {
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

                meshInfo.removalQueue.Add(face.faceIndexInTree);
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

        removalQueue.Sort();

        var removedFaces = removalQueue.ToArray();

        var indexOfFirstFaceToReplace = 0;

        var firstFaceToRemove = removedFaces[indexOfFirstFaceToReplace];
        var faceIndexOfFirstFaceToRemove = firstFaceToRemove;
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

            numFacesToPop++;

            //this face is already removed
            if (currentFace.isRemoved) {
                continue;
            }

            //replace the current face with the last non-null face

            allFacesOfMesh[faceIndexOfFirstFaceToRemove] = currentFace;

            var vertexIndex = firstFaceToRemove * 4;

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

            indexOfFirstFaceToReplace++;

            if (indexOfFirstFaceToReplace == removedFaces.Length) {
                break;
            }

            firstFaceToRemove = removedFaces[indexOfFirstFaceToReplace];
            faceIndexOfFirstFaceToRemove = firstFaceToRemove;
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
        foreach (var material in myVoxelTree._materials)
        {
            SetMaterial(material.Key, material.Value);
        }

//        _meshInfos = myVoxelTree._meshInfos;
    }
}