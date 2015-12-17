using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

//public abstract class Octree<T> : Octree<T, OctreeNode<T>, Octree<T>>{
//   
//}
//
//public abstract class Octree<T, TNode, TSelf>
//    where TNode : OctreeNode<T>
//    where TSelf : Octree<T, TNode, TSelf> {
//    private readonly TNode _root;
//
//    protected Octree(Bounds bounds, Func<Octree<T, TNode, TSelf>, Bounds, TNode> nodeConstructor) {
//        _root = nodeConstructor(this, bounds);
//    }
//}