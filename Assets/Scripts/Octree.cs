using System;
using System.Collections.Generic;
using UnityEngine;

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

    public void ProcessDrawQueue() {
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
        if (_nodeFaces.ContainsKey(octreeNode))
        {
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

                var vertexIndex = firstRemIndex*4;

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

        var vertexIndex = index*4;

        _vertices.RemoveRange(vertexIndex, 4);
        _uvs.RemoveRange(vertexIndex, 4);
        _normals.RemoveRange(vertexIndex, 4);

        _indices.RemoveRange(index*6, 6);
    }

    public void ApplyToMesh(Mesh sharedMesh) {
        sharedMesh.Clear();

        sharedMesh.vertices = _vertices.ToArray();
        sharedMesh.normals = _normals.ToArray();
        sharedMesh.triangles = _indices.ToArray();
        sharedMesh.uv = _uvs.ToArray();
    }
}