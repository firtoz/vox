using System;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class OctreeBase<TItem, TNode, TSelf>
    where TSelf : OctreeBase<TItem, TNode, TSelf>
    where TNode : OctreeNodeBase<TItem, TSelf, TNode> {
    private readonly TNode _root;

    protected OctreeBase(Func<TSelf, Bounds, TNode> nodeConstructor, Bounds bounds) {
        _root = nodeConstructor((TSelf) this, bounds);
    }


    public TNode GetRoot() {
        return _root;
    }

    public abstract TNode ConstructNode(Bounds bounds, TNode parent, OctreeNode.ChildIndex indexInParent, int depth);


    private readonly Dictionary<NeighbourSide, TSelf> _neighbourTrees = new Dictionary<NeighbourSide, TSelf>();


    protected bool _isCreatedByAnotherTree;


    //    protected OctreeBase(Bounds bounds) : base(bounds, (self, bounds) => new TNode(bounds, self))
    //    {
    //                _root = new OctreeNode<T>(bounds, this);
    //    }


    // https://en.wikipedia.org/wiki/Breadth-first_search#Pseudocode

    public IEnumerable<TNode> BreadthFirst()
    {
        return _root.BreadthFirst();
    }

    // https://en.wikipedia.org/wiki/Depth-first_search#Pseudocode
    public IEnumerable<TNode> DepthFirst()
    {
        return _root.DepthFirst();
    }

    public void AddBounds(Bounds bounds, TItem item, int i)
    {
        _root.AddBounds(bounds, item, i);
    }


    public virtual void NodeAdded(TNode octreeNode, bool updateNeighbours) {
        
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


    public virtual void NodeRemoved(TNode octreeNode, bool updateNeighbours)
    {
    }

    public bool Intersect(Transform transform, Ray ray, int? wantedDepth = null)
    {
        return new RayIntersection(transform, (TSelf)this, ray, false, wantedDepth).results.Count > 0;
    }

    private bool _intersecting = false;
    public bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result, int? wantedDepth = null)
    {
        _intersecting = true;
        if (wantedDepth != null && wantedDepth < 0)
        {
            throw new ArgumentOutOfRangeException("wantedDepth", "Wanted depth should not be less than zero.");
        }

        var results = new RayIntersection(transform, (TSelf)this, ray, false, wantedDepth).results;

        if (results.Count > 0)
        {
            result = results[0];
            _intersecting = false;
            return true;
        }

        foreach (var neighbourTree in _neighbourTrees.Values)
        {
            if (neighbourTree._intersecting || !neighbourTree.Intersect(transform, ray, out result, wantedDepth))
            {
                continue;
            }

            _intersecting = false;
            return true;
        }

        result = new RayIntersectionResult(false);
        _intersecting = false;
        return false;
    }


    

    public TSelf GetOrCreateNeighbour(NeighbourSide side)
    {
        TSelf neighbour;
        if (_neighbourTrees.TryGetValue(side, out neighbour))
        {
            return neighbour;
        }

        neighbour = CreateNeighbour(side);
        neighbour._root.RemoveItem();

        neighbour._isCreatedByAnotherTree = true;

        _neighbourTrees.Add(side, neighbour);

        // TODO relink other neighbours.

        neighbour._neighbourTrees.Add(OctreeNode.GetOpposite(side), (TSelf) this);

        return neighbour;
    }

    protected abstract TSelf CreateNeighbour(NeighbourSide side);

    protected Bounds GetNeighbourBounds(NeighbourSide side)
    {
        var rootBounds = GetRoot().GetBounds();

        var center = rootBounds.center;
        var size = rootBounds.size;

        switch (side)
        {
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

    public virtual void UpdateNeighbours(VoxelNode octreeNode) {
        
    }
}
