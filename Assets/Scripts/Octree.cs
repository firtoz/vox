using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class MeshInfo<T> {
    public readonly List<Vector3> normals = new List<Vector3>();
    public readonly List<Vector2> uvs = new List<Vector2>();
    public readonly List<Vector3> vertices = new List<Vector3>();
    public readonly List<int> indices = new List<int>();

    public readonly SortedDictionary<int, OctreeRenderFace<T>> removalQueue = new SortedDictionary<int, OctreeRenderFace<T>>();

    public readonly List<OctreeRenderFace<T>> allFaces = new List<OctreeRenderFace<T>>();

    public readonly HashSet<OctreeNode<T>> drawQueue = new HashSet<OctreeNode<T>>();

    public readonly Material material;

    public MeshInfo(Material material) {
        this.material = material;
    }


    /// <summary>
    /// Removes a face from the _allFaces list.
    /// </summary>
    /// <param name="index">The face index</param>
    /// <param name="count">Number of faces to remove</param>
    /// <param name="vertexIndexInMesh"></param>
    /// 
    public void PopFaces(int index, int count, int vertexIndexInMesh) {
        allFaces.RemoveRange(index, count);

        vertices.RemoveRange(vertexIndexInMesh, 4 * count);
        uvs.RemoveRange(vertexIndexInMesh, 4 * count);
        normals.RemoveRange(vertexIndexInMesh, 4 * count);

        indices.RemoveRange(index * 6, 6 * count);
    }
}

public abstract class Octree<T> {
    private readonly Dictionary<OctreeNode<T>, List<OctreeRenderFace<T>>> _nodeFaces =
        new Dictionary<OctreeNode<T>, List<OctreeRenderFace<T>>>();

    private readonly OctreeNode<T> _root;

    private readonly Dictionary<int, MeshInfo<T>> _meshInfos = new Dictionary<int, MeshInfo<T>>();

    protected Octree(Bounds bounds) {
        _root = new OctreeNode<T>(bounds, this);
    }

    public OctreeNode<T> GetRoot() {
        return _root;
    }

    // https://en.wikipedia.org/wiki/Breadth-first_search#Pseudocode

    public IEnumerable<OctreeNode<T>> BreadthFirst() {
        return _root.BreadthFirst();
    }

    // https://en.wikipedia.org/wiki/Depth-first_search#Pseudocode
    public IEnumerable<OctreeNode<T>> DepthFirst() {
        return _root.DepthFirst();
    }

    public void AddBounds(Bounds bounds, T item, int i) {
        _root.AddBounds(bounds, item, i);
    }

    private MeshInfo<T> GetMeshInfo(T item) {
        var meshId = GetItemMeshId(item);

        if (!_meshInfos.ContainsKey(meshId)) {
            _meshInfos.Add(meshId, new MeshInfo<T>(GetMeshMaterial(meshId)));
        }

        return _meshInfos[meshId];
    }


    public void NodeAdded(OctreeNode<T> octreeNode) {
        var meshInfo = GetMeshInfo(octreeNode.GetItem());

        var drawQueue = meshInfo.drawQueue;

        if (!drawQueue.Contains(octreeNode)) {
            drawQueue.Add(octreeNode);
        }

        foreach (var side in OctreeNode.AllSides)
        {
            var neighbours = octreeNode.GetAllSolidNeighbours(side);

            foreach (var neighbour in neighbours)
            {
                if (neighbour == null || neighbour.IsDeleted() || !neighbour.HasItem())
                {
                    continue;
                }

                var neighbourDrawQueue = GetMeshInfo(neighbour.GetItem()).drawQueue;
                if (!neighbourDrawQueue.Contains(neighbour))
                {
                    neighbourDrawQueue.Add(neighbour);
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

    private void ProcessDrawQueue(MeshInfo<T> meshInfo) {
        var drawQueue = meshInfo.drawQueue;

        Profiler.BeginSample("Draw Queue Length : " + drawQueue.Count);
        foreach (var octreeNode in drawQueue)
        {
            //redraw all nodes in the 'redraw queue'
            if (_nodeFaces.ContainsKey(octreeNode))
            {
                RemoveNodeInternal(octreeNode);
            }
            AddNodeInternal(octreeNode);
        }
        Profiler.EndSample();

        Profiler.BeginSample("Clear draw queue");
        drawQueue.Clear();
        Profiler.EndSample();
    }


    private static void ProcessRemovalQueue(MeshInfo<T> meshInfo) {
        var allFacesOfMesh = meshInfo.allFaces;

        var removalQueue = meshInfo.removalQueue;
        if (!removalQueue.Any()) {
            return;
        }

        var removedFaces = removalQueue.ToArray();

        var indexOfFirstFaceToReplace = 0;

        var firstFaceToReplace = removedFaces[indexOfFirstFaceToReplace].Value;
        var faceIndexOfFirstFace = firstFaceToReplace.faceIndexInTree;

        var i = allFacesOfMesh.Count - 1;

        //iterate backwards to fill up any blanks
        for (; i >= 0; --i) {
            //iterate only until the first face index
            if (i < faceIndexOfFirstFace) {
                break;
            }

            var currentFace = allFacesOfMesh[i];

            meshInfo.PopFaces(i, 1, currentFace.vertexIndexInMesh);

            //this face is already removed
            if (currentFace.isRemoved) {
                continue;
            }

            //replace the current face with the last non-null face

            allFacesOfMesh[faceIndexOfFirstFace] = currentFace;

            var vertexIndex = firstFaceToReplace.vertexIndexInMesh;

            var vertices = meshInfo.vertices;
            var uvs = meshInfo.uvs;
            var normals = meshInfo.normals;

            for (var j = 0; j < 4; j++) {
                vertices[vertexIndex + j] = currentFace.vertices[j];
                uvs[vertexIndex + j] = currentFace.uvs[j];
                normals[vertexIndex + j] = currentFace.normal;
            }

            //indices don't change, right?

            currentFace.faceIndexInTree = faceIndexOfFirstFace;
            currentFace.vertexIndexInMesh = vertexIndex;

            //this face is replaced, try to replace the next one

            indexOfFirstFaceToReplace++;

            if (indexOfFirstFaceToReplace == removedFaces.Length) {
                break;
            }

            firstFaceToReplace = removedFaces[indexOfFirstFaceToReplace].Value;
            faceIndexOfFirstFace = firstFaceToReplace.faceIndexInTree;
        }

        removalQueue.Clear();
    }

    private void AddNodeInternal(OctreeNode<T> octreeNode)
    {
        Profiler.BeginSample("AddNodeInternal");
        var meshId = GetItemMeshId(octreeNode.GetItem());

        Profiler.BeginSample("Create Faces for octreeNode");
        var newFaces = octreeNode.CreateFaces(meshId);
        Profiler.EndSample();

        var meshInfo = GetMeshInfo(octreeNode.GetItem());

        var vertices = meshInfo.vertices;
        var uvs = meshInfo.uvs;
        var normals = meshInfo.normals;
        var indices = meshInfo.indices;

        var removalQueue = meshInfo.removalQueue;
        var allFaces = meshInfo.allFaces;

        Profiler.BeginSample("Process new faces: " + newFaces.Count);
        foreach (var face in newFaces) {
            //if the removal queue isn't empty, replace the last one from there!
            if (removalQueue.Any()) {
                var lastKey = removalQueue.Keys.Last();

                var faceFromRemovalQueue = removalQueue[lastKey];

                var faceIndex = faceFromRemovalQueue.faceIndexInTree;
                var vertexIndex = faceFromRemovalQueue.vertexIndexInMesh;

                face.faceIndexInTree = faceIndex;
                face.vertexIndexInMesh = vertexIndex;

                allFaces[faceIndex] = face;

                var vertexOffset = vertexIndex;

                //indices don't change!

                for (var i = 0; i < 4; i++) {
                    var currentVertexIndex = vertexOffset + i;
                    vertices[currentVertexIndex] = face.vertices[i];
                    uvs[currentVertexIndex] = face.uvs[i];
                    normals[currentVertexIndex] = face.normal;
                }

                removalQueue.Remove(lastKey);
            } else {
                var vertexIndex = meshInfo.vertices.Count;

                face.faceIndexInTree = allFaces.Count;
                face.vertexIndexInMesh = vertexIndex;

                allFaces.Add(face);

                vertices.AddRange(face.vertices);
                uvs.AddRange(face.uvs);

                normals.Add(face.normal);
                normals.Add(face.normal);
                normals.Add(face.normal);
                normals.Add(face.normal);

                indices.Add(vertexIndex);
                indices.Add(vertexIndex + 1);
                indices.Add(vertexIndex + 2);

                indices.Add(vertexIndex);
                indices.Add(vertexIndex + 2);
                indices.Add(vertexIndex + 3);
            }
        }
        Profiler.EndSample();

        _nodeFaces[octreeNode] = newFaces;

        Profiler.EndSample();
    }


    /*
var arr = [2,3,4,5,6,7,8];
var rems = [0,1];

if(rems.length>0) {
    var remI = 0;
    var firstRem = rems[remI];

    for(var i=0;i<rems.length;i++){
        arr[rems[i]] = null;
    }

    console.log("s",arr);

    for(var i=arr.length-1;i>=0;i--){
        if(i < firstRem){
            break;
        }
        var current = arr[i];
        arr.pop();

        if(current = null) {
            continue;
        }

        //replace with remI
        arr[firstRem] = current;
        remI++;

        if(remI == rems.length) {
            console.log("b",arr, i);
            break;
        }

        firstRem = rems[remI];
        console.log("c",arr,i);
    }

    console.log("f",arr);
}
    */


    public void NodeRemoved(OctreeNode<T> octreeNode) {
        Profiler.BeginSample("Node Removed");
        var drawQueue = GetMeshInfo(octreeNode.GetItem()).drawQueue;

        if (_nodeFaces.ContainsKey(octreeNode)) {
            RemoveNodeInternal(octreeNode);
        }

        if (drawQueue.Contains(octreeNode)) {
            //if it's about to be drawn, it shouldn't.
            drawQueue.Remove(octreeNode);
        }

        foreach (var neighbourSide in OctreeNode.AllSides) {
            var neighbours = octreeNode.GetAllSolidNeighbours(neighbourSide);
            foreach (var neighbour in neighbours) {
                if (neighbour == null || neighbour.IsDeleted() || !neighbour.HasItem()) {
                    continue;
                }

                var neighbourMeshInfo = GetMeshInfo(neighbour.GetItem());
                var neighbourDrawQueue = neighbourMeshInfo.drawQueue;
                if (!neighbourDrawQueue.Contains(neighbour)) {
                    neighbourDrawQueue.Add(neighbour);
                }
            }
        }
        Profiler.EndSample();
    }

    // TODO optimize further!
    // can do the blank filling during the draw queue
    private void RemoveNodeInternal(OctreeNode<T> octreeNode) {
        Profiler.BeginSample("RemoveNodeInternal");
        var facesToRemove = _nodeFaces[octreeNode];
        if (facesToRemove.Count > 0) {
            foreach (var face in facesToRemove) {
                if (!_meshInfos.ContainsKey(face.meshIndex)) {
                    Debug.Log("What?! " + face.meshIndex);
                }
                var meshInfo = _meshInfos[face.meshIndex];

                meshInfo.allFaces[face.faceIndexInTree].isRemoved = true;

                meshInfo.removalQueue.Add(face.faceIndexInTree, face);
            }
        }

        _nodeFaces.Remove(octreeNode);
        Profiler.EndSample();
    }


    private const int MAX_VERTICES_FOR_MESH = 65000 - 4 * 100;
    private const int MAX_FACES_FOR_MESH = MAX_VERTICES_FOR_MESH / 4;
    private const int MAX_INDICES_FOR_MESH = MAX_FACES_FOR_MESH * 6;

    public bool Intersect(Transform transform, Ray ray, int? wantedDepth = null) {
        return new RayIntersection<T>(transform, this, ray, false, wantedDepth).results.Count > 0;
    }

    public bool Intersect(Transform transform, Ray ray, out RayIntersectionResult<T> result, int? wantedDepth = null) {
        if (wantedDepth != null && wantedDepth < 0) {
            throw new ArgumentOutOfRangeException("wantedDepth", "Wanted depth should not be less than zero.");
        }
        // ReSharper disable once ObjectCreationAsStatement
        var results = new RayIntersection<T>(transform, this, ray, false, wantedDepth).results;

        if (results.Count > 0) {
            result = results[0];
            return true;
        }

        result = new RayIntersectionResult<T>(false);
        return false;
    }


    private GameObject _renderObject;
    private readonly List<Mesh> _meshes = new List<Mesh>();
    private readonly List<GameObject> _meshObjects = new List<GameObject>();

    public void Render(GameObject gameObject)
    {
        Profiler.BeginSample("Process draw queue");
        ProcessDrawQueue();
        Profiler.EndSample();

        if (true || _renderObject != gameObject) {
//            for (var i = 0; i < _meshes.Count; i++) {
//                var mesh = _meshes[i];
//                var meshObject = _meshObjects[i];
//                if (Application.isPlaying) {
//                    Object.Destroy(mesh);
//                    Object.Destroy(meshObject);
//                } else {
//                    Object.DestroyImmediate(mesh);
//                    Object.DestroyImmediate(meshObject);
//                }
//            }

            var numChildren = gameObject.transform.childCount;

            Profiler.BeginSample("Destroy children");
            for (var i = numChildren - 1; i >= 0; i--)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Object.Destroy(child);
                } else
                {
                    Object.DestroyImmediate(child);
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Destroy meshes");
            foreach (var mesh in _meshes)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(mesh);
                }
                else
                {
                    Object.DestroyImmediate(mesh);
                }
            }
            Profiler.EndSample();

            _meshes.Clear();
            _meshObjects.Clear();

            //recreate meshes
            _renderObject = gameObject;

            Profiler.BeginSample("Recreate meshes");
            foreach (var meshPair in _meshInfos) {
                var meshInfo = meshPair.Value;
                var meshId = meshPair.Key;

                var vertices = meshInfo.vertices;
                var normals = meshInfo.normals;
                var uvs = meshInfo.uvs;
                var indices = meshInfo.indices;

                var verticesCount = vertices.Count;
                var numMesheObjects = (verticesCount / MAX_VERTICES_FOR_MESH) + 1;

                for (var i = 0; i < numMesheObjects; ++i)
                {
                    Profiler.BeginSample("Create mesh " + i);

                    Profiler.BeginSample("new mesh");
                    var newMesh = new Mesh();
                    Profiler.EndSample();

                    var vertexStart = i * MAX_VERTICES_FOR_MESH;
                    var vertexCount = Mathf.Min(vertexStart + MAX_VERTICES_FOR_MESH, verticesCount) - vertexStart;

                    Profiler.BeginSample("Get vertices range");
                    var vertexArray = vertices.GetRange(vertexStart, vertexCount).ToArray();
                    Profiler.EndSample();

                    Profiler.BeginSample("Set mesh properties");

                    Profiler.BeginSample("Set mesh vertices");
                    newMesh.vertices = vertexArray;
                    Profiler.EndSample();

                    Profiler.BeginSample("Set mesh normals");
                    newMesh.normals = normals.GetRange(vertexStart, vertexCount).ToArray();
                    Profiler.EndSample();

                    Profiler.BeginSample("Set mesh uvs");
                    newMesh.uv = uvs.GetRange(vertexStart, vertexCount).ToArray();
                    Profiler.EndSample();

                    Profiler.EndSample(); // mesh properties

                    var indexStart = i * MAX_INDICES_FOR_MESH;
                    var indexCount = vertexCount * 3 / 2;

                    Profiler.BeginSample("Set mesh triangles");
                    newMesh.triangles =
                        indices.GetRange(indexStart, indexCount).Select(index => index - vertexStart).ToArray();
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


                    _meshes.Add(newMesh);
                    _meshObjects.Add(meshObject);

                    Profiler.BeginSample("Set transform parent");
                    meshObject.transform.SetParent(gameObject.transform, false);
                    Profiler.EndSample();

                    Profiler.EndSample();
                }
            }
            Profiler.EndSample();
        } else {}
    }

    protected abstract int GetItemMeshId(T item);
    protected abstract Material GetMeshMaterial(int meshId);

    public bool ItemsBelongInSameMesh(T a, T b) {
        return GetItemMeshId(a) == GetItemMeshId(b);
    }
}