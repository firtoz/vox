using System;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class OctreeBase<TItem, TNode, TTree>
    where TTree : OctreeBase<TItem, TNode, TTree>
    where TNode : OctreeNodeBase<TItem, TTree, TNode> {
    private readonly TNode _root;

    protected OctreeBase(Func<TTree, Bounds, TNode> nodeConstructor, Bounds bounds) {
        _root = nodeConstructor((TTree) this, bounds);
    }


    public TNode GetRoot() {
        return _root;
    }

    public abstract TNode ConstructNode(Bounds bounds, TNode parent, OctreeNode.ChildIndex indexInParent, int depth);


    //    protected OctreeBase(Bounds bounds) : base(bounds, (self, bounds) => new TNode(bounds, self))
    //    {
    //                _root = new OctreeNode<T>(bounds, this);
    //    }


    // https://en.wikipedia.org/wiki/Breadth-first_search#Pseudocode

    public IEnumerable<TNode> BreadthFirst() {
        return _root.BreadthFirst();
    }

    // https://en.wikipedia.org/wiki/Depth-first_search#Pseudocode
    public IEnumerable<TNode> DepthFirst() {
        return _root.DepthFirst();
    }

    public void AddBounds(Bounds bounds, TItem item, int i) {
        _root.AddBounds(bounds, item, i);
    }


    public virtual void NodeAdded(TNode octreeNode, bool updateNeighbours) {}


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


    public virtual void NodeRemoved(TNode octreeNode, bool updateNeighbours) {}

    public bool Intersect(Transform transform, Ray ray, int? wantedDepth = null) {
        return new RayIntersection(transform, (TTree) this, ray, false, wantedDepth).results.Count > 0;
    }
    
    public virtual bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result, int? wantedDepth = null) {
        if (wantedDepth != null && wantedDepth < 0) {
            throw new ArgumentOutOfRangeException("wantedDepth", "Wanted depth should not be less than zero.");
        }

        var results = new RayIntersection(transform, (TTree) this, ray, false, wantedDepth).results;

        if (results.Count > 0) {
            result = results[0];
            return true;
        }

        result = new RayIntersectionResult(false);
        return false;
    }

    protected Bounds GetNeighbourBounds(NeighbourSide side) {
        var rootBounds = GetRoot().GetBounds();

        var center = rootBounds.center;
        var size = rootBounds.size;

        switch (side) {
            case NeighbourSide.Above:
                center += Vector3.up * size.y;
                break;
            case NeighbourSide.Below:
                center += Vector3.down * size.y;
                break;
            case NeighbourSide.Right:
                center += Vector3.right * size.x;
                break;
            case NeighbourSide.Left:
                center += Vector3.left * size.x;
                break;
            case NeighbourSide.Back:
                center += Vector3.back * size.z;
                break;
            case NeighbourSide.Forward:
                center += Vector3.forward * size.z;
                break;
            case NeighbourSide.Invalid:
                break;
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }

        var neighbourBounds = new Bounds(center, size);
        return neighbourBounds;
    }

    public virtual void UpdateNeighbours(VoxelNode octreeNode) {}
}