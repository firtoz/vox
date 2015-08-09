using System;
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

    /*
1     procedure BFS(G,v) is
2      let Q be a queue
3      Q.enqueue(v)
4      label v as discovered
5      while Q is not empty
6         v ← Q.dequeue()
7         process(v)
8         for all edges from v to w in G.adjacentEdges(v) do
9             if w is not labeled as discovered
10                 Q.enqueue(w)
11                label w as discovered
    */

    public IEnumerable<OctreeNode<T>> BreadthFirst() {
        return _root.BreadthFirst();
    }

    // https://en.wikipedia.org/wiki/Depth-first_search#Pseudocode
    /*
    1  procedure DFS-iterative(G,v):
    2      let S be a stack
    3      S.push(v)
    4      while S is not empty
    5            v = S.pop() 
    6            if v is not labeled as discovered:
    7                label v as discovered
    8                for all edges from v to w in G.adjacentEdges(v) do
    9                    S.push(w)
    */

    public IEnumerable<OctreeNode<T>> DepthFirst() {
        return _root.DepthFirst();
    }

    public void AddBounds(Bounds bounds, T item, int i) {
        _root.AddBounds(bounds, item, i);
    }

    private HashSet<OctreeNode<T>> drawQueue = new HashSet<OctreeNode<T>>();

    public void NodeAdded(OctreeNode<T> octreeNode) {
        if (!drawQueue.Contains(octreeNode)) {
            drawQueue.Add(octreeNode);
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
        foreach (var octreeNode in drawQueue) {
            if (_nodeFaces.ContainsKey(octreeNode)) {
                RemoveNodeInternal(octreeNode);
            }
            AddNodeInternal(octreeNode);
        }

        drawQueue.Clear();
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

        if (drawQueue.Contains(octreeNode)) {
            drawQueue.Remove(octreeNode);
        }

        foreach (var neighbourSide in OctreeNode.AllSides) {
            var neighbour = octreeNode.GetDeepestSolidNeighbour(neighbourSide);
            if (neighbour == null || !neighbour.HasItem()) {
                continue;
            }

            if (!drawQueue.Contains(neighbour)) {
                drawQueue.Add(neighbour);
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

    private void RemoveNodeInternal(OctreeNode<T> octreeNode) {
        var rems = _nodeFaces[octreeNode];
        if (rems.Count > 0) {
            var remI = 0;
            var firstRem = rems[remI];
            var firstRemIndex = firstRem.faceIndexInTree;

            foreach (var face in rems) {
                _allFaces[face.faceIndexInTree] = null;
            }

            for (var i = _allFaces.Count - 1; i >= 0; --i) {
                if (i < firstRemIndex) {
                    break;
                }

                var currentFace = _allFaces[i];
                PopFace(i);

                if (currentFace == null) {
                    continue;
                }

                _allFaces[firstRemIndex] = currentFace;

                var vertexIndex = firstRemIndex * 4;

                for (var j = 0; j < 4; j++) {
                    _vertices[vertexIndex + j] = currentFace.vertices[j];
                    _uvs[vertexIndex + j] = currentFace.uvs[j];
                    _normals[vertexIndex + j] = currentFace.normal;
                }

                //indices don't change, right?

                currentFace.faceIndexInTree = firstRemIndex;

                remI++;

                if (remI == rems.Count) {
                    break;
                }

                firstRem = rems[remI];
                firstRemIndex = firstRem.faceIndexInTree;
            }
        }

        _nodeFaces.Remove(octreeNode);
    }

    private void PopFace(int index) {
        _allFaces.RemoveAt(index);

        var vertexIndex = index * 4;

        _vertices.RemoveRange(vertexIndex, 4);
        _uvs.RemoveRange(vertexIndex, 4);
        _normals.RemoveRange(vertexIndex, 4);

        _indices.RemoveRange(index * 6, 6);
    }

    private const int MAX_VERTICES_FOR_MESH = 65000-4*100;
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

    public bool Intersect(Transform transform, Ray ray) {
        return new RayIntersection(transform, this, ray, false).results.Count > 0;
    }

    public bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result) {
        // ReSharper disable once ObjectCreationAsStatement
        var results = new RayIntersection(transform, this, ray, false).results;
        if (results.Count > 0) {
            result = results[0];
            return true;
        } else {
            result = new RayIntersectionResult();
            return false;
        }
    }

    public struct RayIntersectionResult {
        public readonly OctreeNode<T> node;
        public readonly float entryDistance;
        public readonly Vector3 position;
        public readonly Vector3 normal;

        public RayIntersectionResult(OctreeNode<T> node, float entryDistance, Vector3 position, Vector3 normal) {
            this.node = node;
            this.entryDistance = entryDistance;
            this.position = position;
            this.normal = normal;
        }
    }

    private class RayIntersection {
        private readonly byte _a;
        private Ray _ray;
        private readonly bool _intersectMultiple;
        private readonly Transform _transform;

        public readonly List<RayIntersectionResult> results = new List<RayIntersectionResult>();

        public RayIntersection(Transform transform, Octree<T> octree, Ray r, bool intersectMultiple) {
            _transform = transform;
            _ray = r;
            _intersectMultiple = intersectMultiple;
            _a = 0;

            var ro = transform.InverseTransformPoint(r.origin);
            var rd = transform.InverseTransformDirection(r.direction);

            var rootBounds = octree.GetRoot().GetBounds();

            var rox = ro.x;
            var roy = ro.y;
            var roz = ro.z;

            var rdx = rd.x;
            var rdy = rd.y;
            var rdz = rd.z;

            var ocMin = rootBounds.min;
            var ocMax = rootBounds.max;

            var rootCenter = rootBounds.center;
            if (rdx < 0.0f) {
                rox = rootCenter.x - rox;
                rdx = -rdx;
                _a |= 1;
            }

            if (rdy < 0.0f) {
                roy = rootCenter.y - roy;
                rdy = -rdy;
                _a |= 2;
            }

            if (rdz < 0.0f) {
                roz = rootCenter.z - roz;
                rdz = -rdz;
                _a |= 4;
            }

            float tx0, tx1, ty0, ty1, tz0, tz1;

            if (!Mathf.Approximately(rdx, 0.0f)) {
                tx0 = (ocMin.x - rox) / rdx;
                tx1 = (ocMax.x - rox) / rdx;
            } else {
                tx0 = 99999.9f;
                tx1 = 99999.9f;
            }

            if (!Mathf.Approximately(rdy, 0.0f)) {
                ty0 = (ocMin.y - roy) / rdy;
                ty1 = (ocMax.y - roy) / rdy;
            } else {
                ty0 = 99999.9f;
                ty1 = 99999.9f;
            }

            if (!Mathf.Approximately(rdz, 0.0f)) {
                tz0 = (ocMin.z - roz) / rdz;
                tz1 = (ocMax.z - roz) / rdz;
            } else {
                tz0 = 99999.9f;
                tz1 = 99999.9f;
            }

            if (Mathf.Max(tx0, ty0, tz0) < Mathf.Min(tx1, ty1, tz1)) {
                ProcSubtree(tx0, ty0, tz0, tx1, ty1, tz1, octree.GetRoot());
            }
        }

        private enum EntryPlane {
            XY,
            XZ,
            YZ
        };

        private EntryPlane GetEntryPlane(float tx0, float ty0, float tz0) {
            if (tx0 > ty0) {
                if (tx0 > tz0) {
                    //x greatest
                    return EntryPlane.YZ;
                }
            } else if (ty0 > tz0) {
                //y greatest
                return EntryPlane.XZ;
            }

            //z greatest

            return EntryPlane.XY;
        }


        private int FirstNode(float tx0, float ty0, float tz0, float txm, float tym, float tzm) {
            var entryPlane = GetEntryPlane(tx0, ty0, tz0);

            var firstNode = 0;

            switch (entryPlane) {
                case EntryPlane.XY:
                    if (txm < tz0) {
                        firstNode |= 1;
                    }
                    if (tym < tz0) {
                        firstNode |= 2;
                    }
                    break;
                case EntryPlane.XZ:
                    if (txm < ty0) {
                        firstNode |= 1;
                    }
                    if (tzm < ty0) {
                        firstNode |= 4;
                    }
                    break;
                case EntryPlane.YZ:
                    if (tym < tx0) {
                        firstNode |= 2;
                    }
                    if (tzm < tx0) {
                        firstNode |= 4;
                    }
                    break;
            }

            return firstNode;
        }

        private static int NewNode(double x, int xi, double y, int yi, double z, int zi) {
            if (x < y) {
                if (x < z) {
                    return xi;
                }
            } else if (y < z) {
                return yi;
            }

            return zi;
        }

        private void DrawLocalLine(Vector3 a, Vector3 b, Color color) {
            Debug.DrawLine(_transform.TransformPoint(a), _transform.TransformPoint(b), color, 0, false);
        }

        private void ProcSubtree(float tx0, float ty0, float tz0, float tx1, float ty1, float tz1, OctreeNode<T> node) {
            if (!_intersectMultiple && results.Count > 0) {
                return;
            }

            if (node == null) {
                return;
            }

            var bounds = node.GetBounds();
            DrawBounds(bounds, Color.white);

            if (node.IsLeafNode() && node.HasItem()) {
                ProcessTerminal(node, tx0, ty0, tz0);
                return;
            }

            if (tx1 < 0.0 || ty1 < 0.0 || tz1 < 0.0) {
                return;
            }

            var txm = 0.5f * (tx0 + tx1);
            var tym = 0.5f * (ty0 + ty1);
            var tzm = 0.5f * (tz0 + tz1);

            var currNode = FirstNode(tx0, ty0, tz0, txm, tym, tzm);

            while (currNode < 8) {
                var childIndex = (OctreeNode.ChildIndex) (currNode ^ _a);
                if (!_intersectMultiple && results.Count > 0) {
                    return;
                }

                switch (currNode) {
                    //0= none
                    //1 = only z
                    //2 = only y
                    //3 = 2 + 1 = y and z
                    //4 = only x
                    //5 = 4 + 1 = x and z
                    //6 = 4 + 2 = x and y
                    //7 = 4 + 2 + 1 = x and y and z
                    //x sets 4, y set 2, z sets 1
                    //except if the bit is already set, then it can't set it again so 8
                    case 0:
                        //0= none
                        ProcSubtree(tx0, ty0, tz0, txm, tym, tzm, node.GetChild(childIndex));
                        currNode = NewNode(txm, 1, tym, 2, tzm, 4);
                        break;
                    case 1:
                        //1 = only x
                        ProcSubtree(txm, ty0, tz0, tx1, tym, tzm, node.GetChild(childIndex));
                        currNode = NewNode(tx1, 8, tym, 3, tzm, 5);
                        break;
                    case 2:
                        //2 = only y
                        ProcSubtree(tx0, tym, tz0, txm, ty1, tzm, node.GetChild(childIndex));
                        currNode = NewNode(txm, 3, ty1, 8, tzm, 6);
                        break;
                    case 3:
                        //3 = 2 + 1 = y and z
                        ProcSubtree(txm, tym, tz0, tx1, ty1, tzm, node.GetChild(childIndex));
                        currNode = NewNode(tx1, 8, ty1, 8, tzm, 7);
                        break;
                    case 4:
                        //4 = only x
                        ProcSubtree(tx0, ty0, tzm, txm, tym, tz1, node.GetChild(childIndex));
                        currNode = NewNode(txm, 5, tym, 6, tz1, 8);
                        break;
                    case 5:
                        //5 = 4 + 1 = x and z
                        ProcSubtree(txm, ty0, tzm, tx1, tym, tz1, node.GetChild(childIndex));
                        currNode = NewNode(tx1, 8, tym, 7, tz1, 8);
                        break;
                    case 6:
                        //6 = 4 + 2 = x and y
                        ProcSubtree(tx0, tym, tzm, txm, ty1, tz1, node.GetChild(childIndex));
                        currNode = NewNode(txm, 7, ty1, 8, tz1, 8);
                        break;
                    case 7:
                        //7 = 4 + 2 + 1 = x and y and z
                        ProcSubtree(txm, tym, tzm, tx1, ty1, tz1, node.GetChild(childIndex));
                        currNode = 8;
                        break;
                }
            }
        }

        private void DrawBounds(Bounds bounds) {
            DrawBounds(bounds, Color.white);
        }

        private void DrawBounds(Bounds bounds, Color color) {
            var min = bounds.min;
            var max = bounds.max;

            DrawLocalLine(min, new Vector3(min.x, min.y, max.z), color);
            DrawLocalLine(min, new Vector3(min.x, max.y, min.z), color);
            DrawLocalLine(min, new Vector3(max.x, min.y, min.z), color);

            DrawLocalLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color);

            DrawLocalLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), color);
            DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z), color);
            DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z), color);

            DrawLocalLine(max, new Vector3(max.x, max.y, min.z), color);
            DrawLocalLine(max, new Vector3(max.x, min.y, max.z), color);
            DrawLocalLine(max, new Vector3(min.x, max.y, max.z), color);

            DrawLocalLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color);
            DrawLocalLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), color);
        }

        private Vector3 GetNormal(EntryPlane entryPlane) {
            Vector3 normal;
            switch (entryPlane) {
                case EntryPlane.XY:
                    if ((_a & 4) == 0) {
                        normal = Vector3.back;
                    } else {
                        normal = Vector3.forward;
                    }

                    break;
                case EntryPlane.XZ:
                    if ((_a & 2) == 0) {
                        normal = Vector3.down;
                    } else {
                        normal = Vector3.up;
                    }
                    break;
                case EntryPlane.YZ:
                    if ((_a & 1) == 0) {
                        normal = Vector3.left;
                    } else {
                        normal = Vector3.right;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("entryPlane", entryPlane, null);
            }

            return normal;
        }

        private void ProcessTerminal(OctreeNode<T> node, float tx0, float ty0, float tz0) {
            var entryDistance = Mathf.Max(tx0, ty0, tz0);

            var entryPlane = GetEntryPlane(tx0, ty0, tz0);

            var normal = GetNormal(entryPlane);

            var size = 1f;
            Debug.DrawLine(_ray.origin, _ray.GetPoint(entryDistance), Color.white, 0, false);

            Debug.DrawLine(_ray.GetPoint(entryDistance),
                _ray.GetPoint(entryDistance) + _transform.TransformDirection(normal) * size, Color.green, 0, false);

            var bounds = node.GetBounds();
            DrawBounds(bounds, Color.red);

            results.Add(new RayIntersectionResult(node, entryDistance, _ray.GetPoint(entryDistance), normal));
        }
    }


    /*
    void proc_subtree ( real tx0, real ty0, real tz0,
                        real tx1, real ty1, real tz1,
                        node *n ) {

        while( currNode < 8 ) {
            switch(currNode) {
                //0= none
                //1 = only z
                //2 = only y
                //3 = 2 + 1 = y and z
                //4 = only x
                //5 = 4 + 1 = x and z
                //6 = 4 + 2 = x and y
                //7 = 4 + 2 + 1 = x and y and z

                //x sets 4, y set 2, z sets 1
                //except if the bit is already set, then it can't set it again so 8
                case 0: 
                    //0= none
                    proc_subtree(tx0, ty0, tz0, txm, tym, tzm, n->son[a]);
                    currNode = new_node(txm, 4, tym, 2, tzm, 1);
                    break;
                case 1:
                    //1 = only z
                    proc_subtree(tx0, ty0, tzm, txm, tym, tz1, n->son[1^a]);
                    currNode = new_node(txm, 5, tym, 3, tz1, 8);
                    break;
                case 2:
                    //2 = only y
                    proc_subtree(tx0, tym, tz0, txm, ty1, tzm, n->son[2^a]);
                    currNode = new_node(txm, 6, ty1, 8, tzm, 3);
                    break;
                case 3:
                    //3 = 2 + 1 = y and z
                    proc_subtree(tx0, tym, tzm, txm, ty1, tz1, n->son[3^a]);
                    currNode = new_node(txm, 7, ty1, 8, tz1, 8);
                    break;
                case 4:
                    //4 = only x
                    proc_subtree(txm, ty0, tz0, tx1, tym, tzm, n->son[4^a]);
                    currNode = new_node(tx1, 8, tym, 6, tzm, 5);
                    break;
                case 5:
                    //5 = 4 + 1 = x and z
                    proc_subtree(txm, ty0, tzm, tx1, tym, tz1, n->son[5^a]);
                    currNode = new_node(tx1, 8, tym, 7, tz1, 8);
                    break;
                case 6:
                    //6 = 4 + 2 = x and y
                    proc_subtree(txm, tym, tz0, tx1, ty1, tzM, n->son[6^a]);
                    currNode = new_node(tx1, 8, ty1, 8, tzm, 7);
                    break;
                case 7:
                    //7 = 4 + 2 + 1 = x and y and z
                    proc_subtree(txm, tym, tzm, tx1, ty1, tz1, n->son[7^a]);
                    currNode = 8;
                    break;
            }
        } ;
    }

    */

    private GameObject _renderObject;
    private readonly List<Mesh> _meshes = new List<Mesh>();
    private readonly List<GameObject> _meshObjects = new List<GameObject>();

    public void Render(GameObject gameObject) {
        ProcessDrawQueue();

        if (true || _renderObject != gameObject) {
            for (var i = 0; i < _meshes.Count; i++) {
                var mesh = _meshes[i];
                var meshObject = _meshObjects[i];
                if (Application.isPlaying) {
                    Object.Destroy(mesh);
                    Object.Destroy(meshObject);
                } else {
                    Object.DestroyImmediate(mesh);
                    Object.DestroyImmediate(meshObject);
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

                newMesh.triangles = _indices.GetRange(indexStart, indexCount).Select(index => index - vertexStart).ToArray();

                var meshObject = new GameObject("mesh " + i, typeof (MeshFilter), typeof(MeshRenderer));
                meshObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
                meshObject.GetComponent<MeshRenderer>().sharedMaterial =
                    gameObject.GetComponent<MeshRenderer>().sharedMaterial;

                _meshes.Add(newMesh);
                _meshObjects.Add(meshObject);

                meshObject.transform.parent = gameObject.transform;
            }
        } else {}
    }
}