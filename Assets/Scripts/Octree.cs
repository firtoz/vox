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

    class RayIntersection {
        private byte _a;
        private Ray _ray;

        public RayIntersection(Octree<T> octree, Ray r) {
            _ray = r;
            _a = 0;

            var rd = r.direction;
            var ro = r.origin;

            var rootBounds = octree.GetRoot().GetBounds();
            var rootBoundsSize = rootBounds.size;

            if (rd.x < 0.0f) {
                ro.x = rootBoundsSize.x - ro.x;
                rd.x = -rd.x;
                _a |= 4;
            }

            if (rd.y < 0.0f) {
                ro.y = rootBoundsSize.y - ro.y;
                rd.y = -rd.y;
                _a |= 2;
            }

            if (rd.z < 0.0f) {
                ro.z = rootBoundsSize.z - ro.z;
                rd.z = -rd.z;
                _a |= 1;
            }

            var ocMin = rootBounds.min;
            var ocMax = rootBounds.max;

            float tx0, tx1, ty0, ty1, tz0, tz1;

            if (!Mathf.Approximately(rd.x, 0.0f)) {
                tx0 = (ocMin.x - ro.x)/rd.x;
                tx1 = (ocMax.x - ro.x)/rd.x;
            } else {
                tx0 = 99999.9f;
                tx1 = 99999.9f;
            }

            if (!Mathf.Approximately(rd.y, 0.0f)) {
                ty0 = (ocMin.y - ro.y)/rd.y;
                ty1 = (ocMin.y - ro.y)/rd.y;
            }
            else
            {
                ty0 = 99999.9f;
                ty1 = 99999.9f;
            }

            if (!Mathf.Approximately(rd.z, 0.0f)) {
                tz0 = (ocMin.z - ro.z)/rd.z;
                tz1 = (ocMin.z - ro.z)/rd.z;
            }
            else
            {
                tz0 = 99999.9f;
                tz1 = 99999.9f;
            }

            if (Mathf.Max(tx0, ty0, tz0) < Mathf.Min(tx1, ty1, tz1))
            {
                proc_subtree(tx0, ty0, tz0, tx1, ty1, tz1, octree.GetRoot());
            }
        }
        enum EntryPlane
        {
            XY,
            XZ,
            YZ
        };

        private EntryPlane GetEntryPlane(float tx0, float ty0, float tz0)
        {

            if (tx0 > ty0)
            {
                if (tx0 > tz0)
                {
                    //x greatest
                    return EntryPlane.YZ;
                }
            }
            else if (ty0 > tz0)
            {
                //y greatest
                return EntryPlane.XZ;
            }

            //z greatest

            return EntryPlane.XY;
        }



        int first_node(float tx0, float ty0, float tz0, float txm, float tym, float tzm)
        {
            EntryPlane entryPlane = GetEntryPlane(tx0, ty0, tz0);

            int firstNode = 0;

            switch (entryPlane)
            {
                case EntryPlane.XY:
                    if (tzm < tz0)
                    {
                        firstNode |= 1;
                    }
                    if (tym < tz0)
                    {
                        firstNode |= 2;
                    }
                    break;
                case EntryPlane.XZ:
                    if (txm < tz0)
                    {
                        firstNode |= 1;
                    }
                    if (tzm < ty0)
                    {
                        firstNode |= 4;
                    }
                    break;
                case EntryPlane.YZ:
                    if (tym < tx0)
                    {
                        firstNode |= 2;
                    }
                    if (tzm < tx0)
                    {
                        firstNode |= 4;
                    }
                    break;
            }

            return firstNode;
        }

        int new_node(double x, int xi, double y, int yi, double z, int zi)
        {
            if (x < y)
            {
                if (x < z)
                {
                    return xi;
                }
            }
            else if (y < z)
            {
                return yi;
            }

            return zi;
        }

        private void proc_subtree(float tx0, float ty0, float tz0, float tx1, float ty1, float tz1, OctreeNode<T> node) {
            float txm, tym, tzm;

            int currNode;

            if (tx1 < 0.0 || ty1 < 0.0 || tz1 < 0.0) {
                return;
            }

            if (node.IsLeafNode() && node.HasItem()) {
                ProcessTerminal(node, tx0, ty0, tz0);
                return;
            }

            txm = 0.5f*(tx0 + tx1);
            tym = 0.5f*(ty0 + ty1);
            tzm = 0.5f*(tz0 + tz1);

            
        }

        private void ProcessTerminal(OctreeNode<T> node, float tx0, float ty0, float tz0) {
            float entryDistance = Mathf.Max(tx0, ty0, tz0);

            Debug.DrawLine(_ray.origin, _ray.GetPoint(entryDistance));
        }
    }


    /*
    unsigned char a;

    void ray_parameter(octree *oct, ray r) {
        a = 0;

        if(r.dx < 0.0) {
            r.ox = oct->sizeX - r.ox;
            r.dx = -r.dx;
            a |= 4;
        }

        if(r.dy < 0.0) {
            r.oy = oc->sizeY - r.oy;
            r.dy = -r.dy;
            a |= 2;
        }

        if(r.dz < 0.0) {
            r.oz = oct->sizeZ - r.oz;
            r.dz = -r.dz;
            a |= 1;
        }

        tx0 = (oct->xmin - r.ox)/r.dx;
        tx1 = (oct->xmax - r.ox)/r.dx;
        
        ty0 = (oct->ymin - r.oy)/r.dy;
        ty1 = (oct->ymax - r.oy)/r.dy;
        
        tz0 = (oct->zmin - r.oz)/r.dz;
        tz1 = (oct->zmax - r.oz)/r.dz;

        if(Max(tz0, ty0, tz0) < Min(tx1, ty1, tz1)) {
            proc_subtree(tz0, ty0, tz0, tx1, ty1, z1, oct->root);
        }
    }
    
    float Max(a, b, c) {
        if(a > b) {
            if(a > c) {
                //a > b and c 
                return a;
            }
            //a > b but not > c
            //so c is greater
        } else if(b > c) {
            // b > a and c
            return b;
        }

        return c;
    }

    float Min(a, b, c) {
        if(a < b) {
            if(a < c) {
                //a < b and c 
                return a;
            }
            //a < b but not < c
            //so c is greater
        } else if(b < c) {
            // b < a and c
            return b;
        }

        return c;
    }
    */

    /*
    void proc_subtree ( real tx0, real ty0, real tz0,
                        real tx1, real ty1, real tz1,
                        node *n ) {
        real txm, tym, tzm;

        int currNode;

        if(tx1 < 0.0 || ty1 < 0.0 || tz1 < 0.0) {
            return;
        }

        if(n->type == TERMINAL) {
            proc_terminal(n);
            return;
        }

        txm = 0.5 * (tx0 + tx1);
        tym = 0.5 * (ty0 + ty1);
        tzm = 0.5 * (tz0 + tz1);

        currNode = first_node(tx0, ty0, tz0, txm, tym, tzm);

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
}