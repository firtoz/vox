using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public interface IOctree {
    INode GetRoot();
}

public abstract partial class OctreeBase<TItem, TNode, TTree> : IOctree
    where TTree : OctreeBase<TItem, TNode, TTree> where TNode : OctreeNodeBase<TItem, TTree, TNode> {
    private TNode _root;

    protected OctreeBase(Func<TTree, Bounds, TNode> nodeConstructor, Bounds bounds) {
        _root = nodeConstructor((TTree) this, bounds);
    }


    public TNode GetRoot() {
        return _root;
    }

    protected void SetRoot(TNode newRoot) {
        _root = newRoot;
    }

    INode IOctree.GetRoot() {
        return GetRoot();
    }

    public abstract TNode ConstructNode(Bounds bounds, TNode parent, OctreeNode.ChildIndex indexInParent);


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

    public bool Intersect(Transform transform, Ray ray, int? wantedDepth = null, bool debug = false) {
        return new RayIntersection(transform, this, ray, false, wantedDepth, debug).results.Count > 0;
    }

    public virtual bool Intersect(Transform transform, Ray ray, out RayIntersectionResult result,
        int? wantedDepth = null, bool debug = false) {
        if (wantedDepth != null && wantedDepth < 0) {
            throw new ArgumentOutOfRangeException("wantedDepth", "Wanted depth should not be less than zero.");
        }

        var results = new RayIntersection(transform, this, ray, false, wantedDepth, null, debug).results;

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

    private static bool UpdateLastCoord(ref int lastCoordX, ref int currentX, ref int lastCoordY, ref int currentY,
        ref int lastCoordZ, ref int currentZ) {
        var updateLastCoord = false;

        if (lastCoordX < 0) {
            currentX -= 1;
            lastCoordX = 1;
            updateLastCoord = true;
        } else if (lastCoordX > 1) {
            currentX += 1;
            lastCoordX = 0;
            updateLastCoord = true;
        }

        if (lastCoordY < 0) {
            currentY -= 1;
            lastCoordY = 1;
            updateLastCoord = true;
        } else if (lastCoordY > 1) {
            currentY += 1;
            lastCoordY = 0;
            updateLastCoord = true;
        }

        if (lastCoordZ < 0) {
            currentZ -= 1;
            lastCoordZ = 1;
            updateLastCoord = true;
        } else if (lastCoordZ > 1) {
            currentZ += 1;
            lastCoordZ = 0;
            updateLastCoord = true;
        }
        return updateLastCoord;
    }

    protected static NeighbourCoordsResult GetNeighbourCoordsInfinite(TTree tree, Coords coords,
        NeighbourSide side, Func<NeighbourSide, bool, TTree> getOrCreateNeighbour, bool readOnly = false) {
        var coordsLength = coords.Length;

        if (coordsLength == 0) {
            var neighbourTree = getOrCreateNeighbour(side, readOnly);

            if (neighbourTree == null) {
                return null;
            }
            // get the neighbour tree?
            return new NeighbourCoordsResult(false, coords, neighbourTree);
        }

        var newCoords = new OctreeChildCoords[coordsLength];

        var hasLastCoords = false;
        var lastCoordX = 0;
        var lastCoordY = 0;
        var lastCoordZ = 0;

        for (var i = coordsLength - 1; i >= 0; --i) {
            var coord = coords.GetCoord(i);

            var currentX = coord.x;
            var currentY = coord.y;
            var currentZ = coord.z;

            if (hasLastCoords) {
                //let's check the lower _coords, if it's out of that bounds then we need to modify ourselves!
                var lastCoordUpdated = UpdateLastCoord(
                    ref lastCoordX, ref currentX,
                    ref lastCoordY, ref currentY,
                    ref lastCoordZ, ref currentZ);

                if (lastCoordUpdated) {
                    newCoords[i + 1] = new OctreeChildCoords(lastCoordX, lastCoordY, lastCoordZ);
                }
            } else {
                //final _coords!
                //update _coords from the side
                switch (side) {
                    case NeighbourSide.Above:
                        currentY += 1;
                        break;
                    case NeighbourSide.Below:
                        currentY -= 1;
                        break;
                    case NeighbourSide.Right:
                        currentX += 1;
                        break;
                    case NeighbourSide.Left:
                        currentX -= 1;
                        break;
                    case NeighbourSide.Back:
                        currentZ -= 1;
                        break;
                    case NeighbourSide.Forward:
                        currentZ += 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("side", side, null);
                }
            }

            var newCoord = new OctreeChildCoords(currentX, currentY, currentZ);
            newCoords[i] = newCoord;

            lastCoordX = currentX;
            lastCoordY = currentY;
            lastCoordZ = currentZ;
            hasLastCoords = true;
        }

        // we're at the end now

        if (hasLastCoords && (lastCoordX < 0 || lastCoordX > 1 ||
                              lastCoordY < 0 || lastCoordY > 1 ||
                              lastCoordZ < 0 || lastCoordZ > 1)) {
            //invalid _coords, out of bounds, pick neighbour voxelTree
            var neighbourTree = getOrCreateNeighbour(side, readOnly);
            if (neighbourTree == null)
            {
                return null;
            }

            var currentX = lastCoordX;
            var currentY = lastCoordY;
            var currentZ = lastCoordZ;

            UpdateLastCoord(ref lastCoordX, ref currentX,
                ref lastCoordY, ref currentY,
                ref lastCoordZ, ref currentZ);

            newCoords[0] = new OctreeChildCoords(lastCoordX, lastCoordY, lastCoordZ);

            return new NeighbourCoordsResult(false, new Coords(newCoords), neighbourTree);
        }

        return new NeighbourCoordsResult(true, new Coords(newCoords), tree);
    }

    public static Coords GetNeighbourCoords(Coords coords, NeighbourSide side) {
        //        var voxelTree = GetTree();

        var coordsLength = coords.Length;

        if (coordsLength <= 0) {
            // get the neighbour tree?
            return null;
        }

        var newCoords = new OctreeChildCoords[coordsLength];

        var hasLastCoords = false;
        var lastCoordX = 0;
        var lastCoordY = 0;
        var lastCoordZ = 0;

        for (var i = coordsLength - 1; i >= 0; --i) {
            var coord = coords.GetCoord(i);

            var currentX = coord.x;
            var currentY = coord.y;
            var currentZ = coord.z;

            if (hasLastCoords) {
                //let's check the lower _coords, if it's out of that bounds then we need to modify ourselves!
                var lastCoordUpdated = UpdateLastCoord(
                    ref lastCoordX, ref currentX,
                    ref lastCoordY, ref currentY,
                    ref lastCoordZ, ref currentZ);

                if (lastCoordUpdated) {
                    newCoords[i + 1] = new OctreeChildCoords(lastCoordX, lastCoordY, lastCoordZ);
                }
            } else {
                //final _coords!
                //update _coords from the side
                switch (side) {
                    case NeighbourSide.Above:
                        currentY += 1;
                        break;
                    case NeighbourSide.Below:
                        currentY -= 1;
                        break;
                    case NeighbourSide.Right:
                        currentX += 1;
                        break;
                    case NeighbourSide.Left:
                        currentX -= 1;
                        break;
                    case NeighbourSide.Back:
                        currentZ -= 1;
                        break;
                    case NeighbourSide.Forward:
                        currentZ += 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("side", side, null);
                }
            }

            var newCoord = new OctreeChildCoords(currentX, currentY, currentZ);
            newCoords[i] = newCoord;

            lastCoordX = currentX;
            lastCoordY = currentY;
            lastCoordZ = currentZ;
            hasLastCoords = true;
        }

        // we're at the end now

        if (hasLastCoords && (lastCoordX < 0 || lastCoordX > 1 ||
                              lastCoordY < 0 || lastCoordY > 1 ||
                              lastCoordZ < 0 || lastCoordZ > 1)) {
            return null;
        }

        return new Coords(newCoords);
    }

    public class NeighbourCoordsResult {
        public readonly Coords coordsResult;
        public readonly bool sameTree;
        public readonly TTree tree;

        public NeighbourCoordsResult(bool sameTree, Coords coordsResult, TTree tree) {
            Assert.IsNotNull(tree, "Cannot have a null tree for a neighbour coords result, return null instead");
            this.sameTree = sameTree;
            this.coordsResult = coordsResult;
            this.tree = tree;
        }
    }
}