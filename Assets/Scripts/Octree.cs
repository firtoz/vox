using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Octree<T> {
    private readonly List<OctreeRenderFace<T>> _allFaces = new List<OctreeRenderFace<T>>();
    private readonly List<int> _indices = new List<int>();

    private readonly Dictionary<OctreeNode<T>, List<OctreeRenderFace<T>>> _nodeFaces =
        new Dictionary<OctreeNode<T>, List<OctreeRenderFace<T>>>();

    private readonly List<Vector3> _normals = new List<Vector3>();
    private readonly OctreeNode<T> _root;
    private readonly List<Vector2> _uvs = new List<Vector2>();
    private readonly List<Vector3> _vertices = new List<Vector3>();

    public Octree(Bounds bounds) {
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

    private readonly HashSet<OctreeNode<T>> _drawQueue = new HashSet<OctreeNode<T>>();

    public void NodeAdded(OctreeNode<T> octreeNode) {
        if (!_drawQueue.Contains(octreeNode)) {
            _drawQueue.Add(octreeNode);
        }

        foreach (var neighbourSide in OctreeNode.AllSides) {
            var neighbour = octreeNode.GetDeepestSolidNeighbour(neighbourSide);
            if (neighbour == null || !neighbour.HasItem()) {
                continue;
            }

            if (!_drawQueue.Contains(neighbour)) {
                _drawQueue.Add(neighbour);
            }
        }

        //        if (_nodeFaces.ContainsKey(octreeNode)) {
        //            throw new ArgumentException("The node is already rendered!", "octreeNode");
        //        }
        //
        //        AddNodeInternal(octreeNode);
        //
        //        return;
        //
        //        foreach (var neighbourSide in OctreeNode.AllSides)
        //        {
        //            var neighbour = octreeNode.GetDeepestSolidNeighbour(neighbourSide);
        //            if (neighbour == null || !neighbour.HasItem())
        //            {
        //                continue;
        //            }
        //
        //            RemoveNodeInternal(neighbour);
        //            AddNodeInternal(neighbour);
        //        }
    }

    private void ProcessDrawQueue() {
        foreach (var octreeNode in _drawQueue) {
            //redraw all nodes in the 'redraw queue'
            if (_nodeFaces.ContainsKey(octreeNode)) {
                RemoveNodeInternal(octreeNode);
            }
            AddNodeInternal(octreeNode);
        }

        _drawQueue.Clear();
    }

    private void AddNodeInternal(OctreeNode<T> octreeNode) {
        var faces = octreeNode.CreateFaces();

        var vertexIndex = _vertices.Count;

        foreach (var face in faces) {
            face.faceIndexInTree = _allFaces.Count;

            _allFaces.Add(face);

            _vertices.AddRange(face.vertices);
            _uvs.AddRange(face.uvs);

            _normals.Add(face.normal);
            _normals.Add(face.normal);
            _normals.Add(face.normal);
            _normals.Add(face.normal);

            _indices.Add(vertexIndex);
            _indices.Add(vertexIndex + 1);
            _indices.Add(vertexIndex + 2);

            _indices.Add(vertexIndex);
            _indices.Add(vertexIndex + 2);
            _indices.Add(vertexIndex + 3);

            vertexIndex += 4;
        }

        _nodeFaces[octreeNode] = faces;
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
        if (_nodeFaces.ContainsKey(octreeNode)) {
            RemoveNodeInternal(octreeNode);
        }

        if (_drawQueue.Contains(octreeNode)) {
            //if it's about to be drawn, it shouldn't.
            _drawQueue.Remove(octreeNode);
        }

        foreach (var neighbourSide in OctreeNode.AllSides) {
            var neighbour = octreeNode.GetDeepestSolidNeighbour(neighbourSide);

            Debug.Log("neighbour for side " + neighbourSide + ":"+neighbour);
            if (neighbour == null || !neighbour.HasItem()) {
                continue;
            }

            if (!_drawQueue.Contains(neighbour)) {
                _drawQueue.Add(neighbour);
            }
        }
//            AddNodeInternal(neighbour);
//
////            var newFaces = neighbour.CreateFaces(OctreeNode.GetOpposite(neighbourSide));
////
////            var startIndex = _vertices.Count;
////
////            foreach (var face in newFaces)
////            {
////                face.faceIndexInTree = _allFaces.Count;
////
////                _allFaces.Add(face);
////
////                _vertices.AddRange(face.vertices);
////                _uvs.AddRange(face.uvs);
////
////                _indices.Add(startIndex);
////                _indices.Add(startIndex + 1);
////                _indices.Add(startIndex + 2);
////
////                _indices.Add(startIndex);
////                _indices.Add(startIndex + 2);
////                _indices.Add(startIndex + 3);
////
////                _normals.Add(face.normal);
////                _normals.Add(face.normal);
////                _normals.Add(face.normal);
////                _normals.Add(face.normal);
////
////                startIndex += 4;
////            }
////
//////            if (_nodeFaces.ContainsKey(neighbour)) {
////            _nodeFaces[neighbour].AddRange(newFaces);
////            }
////            else {
////                _nodeFaces[neighbour] = newFaces;
////            }
//        }
    }

    // TODO optimize further!
    // can do the blank filling during the draw queue
    private void RemoveNodeInternal(OctreeNode<T> octreeNode) {
        var facesToRemove = _nodeFaces[octreeNode];
        if (facesToRemove.Count > 0) {
            var removalIndex = 0;
            var currentFaceToReplace = facesToRemove[removalIndex];
            var currentFaceIndex = currentFaceToReplace.faceIndexInTree;

            foreach (var face in facesToRemove) {
                _allFaces[face.faceIndexInTree] = null;
            }

            //iterate backwards to fill up any blanks
            for (var i = _allFaces.Count - 1; i >= 0; --i) {
                //iterate only until the first face index
                if (i < currentFaceIndex) {
                    break;
                }

                var currentFace = _allFaces[i];
                PopFace(i);

                //this face is already removed
                if (currentFace == null) {
                    continue;
                }

                //replace the current face with the last non-null face

                _allFaces[currentFaceIndex] = currentFace;

                var vertexIndex = currentFaceIndex * 4;

                for (var j = 0; j < 4; j++) {
                    _vertices[vertexIndex + j] = currentFace.vertices[j];
                    _uvs[vertexIndex + j] = currentFace.uvs[j];
                    _normals[vertexIndex + j] = currentFace.normal;
                }

                //indices don't change, right?

                currentFace.faceIndexInTree = currentFaceIndex;

                //this face is replaced, try to replace the next one

                removalIndex++;

                if (removalIndex == facesToRemove.Count) {
                    break;
                }

                currentFaceToReplace = facesToRemove[removalIndex];
                currentFaceIndex = currentFaceToReplace.faceIndexInTree;
            }
        }

        _nodeFaces.Remove(octreeNode);
    }

    /// <summary>
    /// Removes a face from the _allFaces list.
    /// </summary>
    /// <param name="index">The face index</param>
    private void PopFace(int index) {
        _allFaces.RemoveAt(index);

        var vertexIndex = index * 4;

        _vertices.RemoveRange(vertexIndex, 4);
        _uvs.RemoveRange(vertexIndex, 4);
        _normals.RemoveRange(vertexIndex, 4);

        _indices.RemoveRange(index * 6, 6);
    }

    private const int MAX_VERTICES_FOR_MESH = 65000 - 4 * 100;
    private const int MAX_FACES_FOR_MESH = MAX_VERTICES_FOR_MESH / 4;
    private const int MAX_INDICES_FOR_MESH = MAX_FACES_FOR_MESH * 6;


//    private void ApplyToMesh(Mesh sharedMesh, int startIndex, int endIndex) {
//        sharedMesh.Clear();
//
//        sharedMesh.MarkDynamic();
//
//        var verticesSlice = _vertices.GetRange(startIndex, endIndex);
//        var indicesSlice = _indices.GetRange(startIndex/4)
//
//        /*
//            _indices.Add(vertexIndex);
//            _indices.Add(vertexIndex + 1);
//            _indices.Add(vertexIndex + 2);
//
//            _indices.Add(vertexIndex);
//            _indices.Add(vertexIndex + 2);
//            _indices.Add(vertexIndex + 3);    
//        */
//            //        var numVertices = _vertices.Count;
//
//        //        Debug.Log(numVertices);
//
//        if (numVertices < 65535) {
//            sharedMesh.vertices = _vertices.ToArray();
//            sharedMesh.normals = _normals.ToArray();
//            sharedMesh.triangles = _indices.ToArray();
//            sharedMesh.uv = _uvs.ToArray();
//        } else {
//            var numMeshes = numVertices / 65535;
//            sharedMesh.subMeshCount = numMeshes;
//
//            for (var i = 0; i < numMeshes; ++i) {
//                var start = i * 65535;
//                var end = Mathf.Min(start + 65535, numVertices) - start;
//                sharedMesh.SetIndices(_indices.GetRange(start, end).ToArray(), MeshTopology.Triangles, i);
//            }
//        }
//    }

    public bool Intersect(Transform transform, Ray ray, int? wantedDepth = null) {
        return new RayIntersection<T>(transform, this, ray, false, wantedDepth).results.Count > 0;
    }

    public bool Intersect(Transform transform, Ray ray, out RayIntersectionResult<T> result, int? wantedDepth = null) {
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

    public void Render(GameObject gameObject) {
        ProcessDrawQueue();

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

            for (var i = numChildren - 1; i >= 0; i--) {
                if (Application.isPlaying) {
                    Object.Destroy(gameObject.transform.GetChild(i).gameObject);
                } else {
                    Object.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
                }
            }

            _meshes.Clear();
            _meshObjects.Clear();

            //recreate meshes
            _renderObject = gameObject;

            var verticesCount = _vertices.Count;
            var numMeshes = (verticesCount / MAX_VERTICES_FOR_MESH) + 1;

            for (var i = 0; i < numMeshes; ++i) {
                var newMesh = new Mesh();

                var vertexStart = i * MAX_VERTICES_FOR_MESH;
                var vertexCount = Mathf.Min(vertexStart + MAX_VERTICES_FOR_MESH, verticesCount) - vertexStart;

                var vertexArray = _vertices.GetRange(vertexStart, vertexCount).ToArray();

                newMesh.vertices = vertexArray;
                newMesh.normals = _normals.GetRange(vertexStart, vertexCount).ToArray();
                newMesh.uv = _uvs.GetRange(vertexStart, vertexCount).ToArray();


                var indexStart = i * MAX_INDICES_FOR_MESH;
                var indexCount = vertexCount * 3 / 2;

                newMesh.triangles =
                    _indices.GetRange(indexStart, indexCount).Select(index => index - vertexStart).ToArray();

                var meshObject = new GameObject("mesh " + i, typeof (MeshFilter), typeof (MeshRenderer));
                meshObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
                meshObject.GetComponent<MeshRenderer>().sharedMaterial =
                    gameObject.GetComponent<MeshRenderer>().sharedMaterial;

                _meshes.Add(newMesh);
                _meshObjects.Add(meshObject);

                meshObject.transform.SetParent(gameObject.transform, false);
            }
        } else {}
    }
}