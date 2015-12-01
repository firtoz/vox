#define DISABLE_PROFILER

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class OctreeNode {
    public enum ChildIndex {
        Invalid = -1,
        TopFwdLeft = 0,
        TopFwdRight = 1,
        TopBackLeft = 2,
        TopBackRight = 3,
        BotFwdLeft = 4,
        BotFwdRight = 5,
        BotBackLeft = 6,
        BotBackRight = 7
    }

    public enum NeighbourSide {
        Above = 0,
        Below = 1,
        Left = 2,
        Right = 3,
        Forward = 4,
        Back = 5,
        Invalid = -1
    }

    public static readonly NeighbourSide[] AllSides = {
        NeighbourSide.Above, NeighbourSide.Below, NeighbourSide.Forward, NeighbourSide.Back, NeighbourSide.Left,
        NeighbourSide.Right
    };

    protected static readonly OctreeChildCoordinates[] AboveCoords = {
        new OctreeChildCoordinates(0, 1, 0), new OctreeChildCoordinates(1, 1, 0), new OctreeChildCoordinates(0, 1, 1),
        new OctreeChildCoordinates(1, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] BelowCoords = {
        new OctreeChildCoordinates(0, 0, 0), new OctreeChildCoordinates(1, 0, 0), new OctreeChildCoordinates(0, 0, 1),
        new OctreeChildCoordinates(1, 0, 1)
    };

    protected static readonly OctreeChildCoordinates[] LeftCoords = {
        new OctreeChildCoordinates(0, 0, 0), new OctreeChildCoordinates(0, 1, 0), new OctreeChildCoordinates(0, 0, 1),
        new OctreeChildCoordinates(0, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] RightCoords = {
        new OctreeChildCoordinates(1, 0, 0), new OctreeChildCoordinates(1, 1, 0), new OctreeChildCoordinates(1, 0, 1),
        new OctreeChildCoordinates(1, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] BackCoords = {
        new OctreeChildCoordinates(0, 0, 0), new OctreeChildCoordinates(0, 1, 0), new OctreeChildCoordinates(1, 0, 0),
        new OctreeChildCoordinates(1, 1, 0)
    };

    protected static readonly OctreeChildCoordinates[] ForwardCoords = {
        new OctreeChildCoordinates(0, 0, 1), new OctreeChildCoordinates(0, 1, 1), new OctreeChildCoordinates(1, 0, 1),
        new OctreeChildCoordinates(1, 1, 1)
    };

    public static NeighbourSide GetOpposite(NeighbourSide side) {
        switch (side) {
            case NeighbourSide.Above:
                return NeighbourSide.Below;
            case NeighbourSide.Below:
                return NeighbourSide.Above;
            case NeighbourSide.Left:
                return NeighbourSide.Right;
            case NeighbourSide.Right:
                return NeighbourSide.Left;
            case NeighbourSide.Forward:
                return NeighbourSide.Back;
            case NeighbourSide.Back:
                return NeighbourSide.Forward;
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }
    }


    protected static void GetNeighbourSides(ChildIndex childIndex,
        out NeighbourSide verticalSide,
        out NeighbourSide horizontalSide,
        out NeighbourSide depthSide) {
        switch (childIndex) {
            case ChildIndex.Invalid:
                // self
                verticalSide = NeighbourSide.Invalid;
                horizontalSide = NeighbourSide.Invalid;
                depthSide = NeighbourSide.Invalid;
                break;
            case ChildIndex.BotFwdRight:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.BotFwdLeft:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.TopFwdRight:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.TopFwdLeft:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.BotBackRight:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.BotBackLeft:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.TopBackRight:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.TopBackLeft:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Left;
                break;
            default:
                throw new ArgumentOutOfRangeException("childIndex", childIndex, null);
        }
    }
}

public class OctreeNode<T> : OctreeNode {
//    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _depth;
    private readonly ChildIndex _indexInParent;
    private readonly OctreeNodeCoordinates _nodeCoordinates;
    private readonly OctreeNode<T> _parent;
    private readonly OctreeNode<T> _root;
    private readonly Bounds _bounds;
    private int _childCount;
    private OctreeNode<T>[] _children;
    private bool _deleted;
    private bool _hasItem;
    private int _solidNodeCount;
    private readonly Dictionary<NeighbourSide, int> _sideSolidCount = new Dictionary<NeighbourSide, int>();

    private readonly Dictionary<NeighbourSide, HashSet<OctreeNode<T>>> _sideSolidChildren =
        new Dictionary<NeighbourSide, HashSet<OctreeNode<T>>>();

    private T _item;
    private readonly Octree<T> _tree;
#if USE_ALL_NODES
    private readonly Dictionary<OctreeNodeCoordinates, OctreeNode<T>> _allNodes; 
#endif
    public OctreeNode(Bounds bounds, Octree<T> tree) : this(bounds, null, ChildIndex.Invalid, 0, tree) {}

    public OctreeNode(Bounds bounds, OctreeNode<T> parent, ChildIndex indexInParent, int depth, Octree<T> tree) {
        _deleted = false;
        _bounds = bounds;
        _parent = parent;
        if (parent == null) {
            _root = this;
            _nodeCoordinates = new OctreeNodeCoordinates();
#if USE_ALL_NODES
    // ReSharper disable once UseObjectOrCollectionInitializer
            _allNodes = new Dictionary<OctreeNodeCoordinates, OctreeNode<T>>();
            _allNodes[_nodeCoordinates] = this;
#endif
        } else {
            _root = _parent._root;
#if USE_ALL_NODES
            _allNodes = _root._allNodes;
#endif

            _nodeCoordinates = new OctreeNodeCoordinates(parent._nodeCoordinates,
                OctreeChildCoordinates.FromIndex(indexInParent));
        }

        _indexInParent = indexInParent;
        _item = default(T);
        _depth = depth;
        _tree = tree;

        _sideSolidCount[NeighbourSide.Above] = 0;
        _sideSolidCount[NeighbourSide.Below] = 0;
        _sideSolidCount[NeighbourSide.Left] = 0;
        _sideSolidCount[NeighbourSide.Right] = 0;
        _sideSolidCount[NeighbourSide.Back] = 0;
        _sideSolidCount[NeighbourSide.Forward] = 0;

        _sideSolidChildren[NeighbourSide.Above] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Below] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Left] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Right] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Back] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Forward] = new HashSet<OctreeNode<T>>();
    }

//    private List<OctreeNode<T>> _actuallySolidChildren = new List<OctreeNode<T>>(); 

    private void AddSolidNode(ChildIndex childIndex, bool actuallySolid) {
        NeighbourSide verticalSide, depthSide, horizontalSide;

        if (childIndex != ChildIndex.Invalid) {
            GetNeighbourSides(childIndex, out verticalSide, out horizontalSide, out depthSide);

            _sideSolidCount[verticalSide]++;
            _sideSolidCount[depthSide]++;
            _sideSolidCount[horizontalSide]++;
        } else {
            GetNeighbourSides(_indexInParent, out verticalSide, out horizontalSide, out depthSide);
        }

        if (_parent != null && _solidNodeCount == 0) {
            _parent.AddSolidNode(_indexInParent, false);
        }

        _solidNodeCount++;

        if (!actuallySolid) {
            return;
        }

        OctreeNode<T> actualNode;
        if (childIndex != ChildIndex.Invalid) {
            actualNode = GetChild(childIndex);

            _sideSolidChildren[verticalSide].Add(actualNode);
            _sideSolidChildren[depthSide].Add(actualNode);
            _sideSolidChildren[horizontalSide].Add(actualNode);
        } else {
            actualNode = this;
        }

        var currentParent = _parent;
        while (currentParent != null) {
            currentParent._sideSolidChildren[verticalSide].Add(actualNode);
            currentParent._sideSolidChildren[depthSide].Add(actualNode);
            currentParent._sideSolidChildren[horizontalSide].Add(actualNode);

            currentParent = currentParent._parent;
        }
    }

    private void RemoveSolidNode(ChildIndex childIndex, bool wasActuallySolid) {
        NeighbourSide verticalSide, depthSide, horizontalSide;

        if (childIndex != ChildIndex.Invalid) {
            GetNeighbourSides(childIndex, out verticalSide, out horizontalSide, out depthSide);

            _sideSolidCount[verticalSide]--;
            _sideSolidCount[depthSide]--;
            _sideSolidCount[horizontalSide]--;
        } else {
            GetNeighbourSides(_indexInParent, out verticalSide, out horizontalSide, out depthSide);
        }

        _solidNodeCount--;

        if (_parent != null && _solidNodeCount == 0) {
            _parent.RemoveSolidNode(_indexInParent, false);
        }

        if (!wasActuallySolid) {
            return;
        }

        OctreeNode<T> actualNode;
        if (childIndex != ChildIndex.Invalid) {
            actualNode = GetChild(childIndex);

            _sideSolidChildren[verticalSide].Remove(actualNode);
            _sideSolidChildren[depthSide].Remove(actualNode);
            _sideSolidChildren[horizontalSide].Remove(actualNode);
        } else {
            actualNode = this;
        }

        var currentParent = _parent;
        while (currentParent != null) {
            currentParent._sideSolidChildren[verticalSide].Remove(actualNode);
            currentParent._sideSolidChildren[depthSide].Remove(actualNode);
            currentParent._sideSolidChildren[horizontalSide].Remove(actualNode);

            currentParent = currentParent._parent;
        }
    }

    private void AssertNotDeleted() {
        Assert.IsFalse(_deleted, "Node Deleted");
//        Profiler.BeginSample("Assert not deleted");
//        if (_deleted)
//        {
//            throw new Exception("Node is deleted!");
//        }
//        Profiler.EndSample();
    }

    public T GetItem() {
        AssertNotDeleted();
        return _item;
    }

    public OctreeNode<T> GetChildAtCoords(IEnumerable<OctreeChildCoordinates> coords) {
        AssertNotDeleted();
        return GetChildAtCoords(coords.Select(coord => coord.ToIndex()));
    }

    public OctreeNode<T> GetChildAtCoords(IEnumerable<ChildIndex> coords) {
        AssertNotDeleted();
        var current = this;

        foreach (var coord in coords) {
            current = current.GetChild(coord);
            if (current == null) {
                break;
            }
        }

        return current;
    }

    public IEnumerable<OctreeNode<T>> GetAllSolidNeighbours(NeighbourSide side) {
#if !DISABLE_PROFILER
        Profiler.BeginSample("GetAllSolidNeighbours");
#endif
        var result = new HashSet<OctreeNode<T>>();

        var neighbourCoords = _nodeCoordinates.GetNeighbourCoords(side);

        //out of the map!
        if (neighbourCoords == null) {
#if !DISABLE_PROFILER
            Profiler.EndSample();
#endif
            return result;
        }

        var currentNeighbourNode = _root;

        foreach (var coord in neighbourCoords) {
            if (currentNeighbourNode == null || currentNeighbourNode.IsDeleted()) {
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
                return result;
            }

            if (currentNeighbourNode.IsSolid()) {
                result.Add(currentNeighbourNode);
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
                return result;
            }

            currentNeighbourNode = currentNeighbourNode.GetChild(coord.ToIndex());
        }

//        last currentNode is the actual node at the neighbour coordinates
        if (currentNeighbourNode == null || currentNeighbourNode.IsDeleted()) {
#if !DISABLE_PROFILER
            Profiler.EndSample();
#endif
            return result;
        }

        if (currentNeighbourNode.IsSolid()) {
            result.Add(currentNeighbourNode);
#if !DISABLE_PROFILER
            Profiler.EndSample();
#endif
            return result;
        }

        return currentNeighbourNode._sideSolidChildren[GetOpposite(side)];
    }

    private static IEnumerable<OctreeChildCoordinates> GetChildCoordsOfSide(NeighbourSide side) {
        OctreeChildCoordinates[] childCoords;

        switch (side) {
            case NeighbourSide.Above:
                childCoords = AboveCoords;
                break;
            case NeighbourSide.Below:
                childCoords = BelowCoords;
                break;
            case NeighbourSide.Left:
                childCoords = LeftCoords;
                break;
            case NeighbourSide.Right:
                childCoords = RightCoords;
                break;
            case NeighbourSide.Forward:
                childCoords = ForwardCoords;
                break;
            case NeighbourSide.Back:
                childCoords = BackCoords;
                break;
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }
        return childCoords;
    }

    private SideState GetSideState(OctreeNodeCoordinates coords, NeighbourSide side) {
        AssertNotDeleted();
        var neighbourCoords = coords.GetNeighbourCoords(side);

        //out of the boundaries
        if (neighbourCoords == null) {
            return SideState.Empty;
        }

#if USE_ALL_NODES
        OctreeNode<T> neighbourNode;

        if (_allNodes.TryGetValue(neighbourCoords, out neighbourNode)) {
            if (neighbourNode == null) {
                return SideState.Empty;
            }

            if (neighbourNode.IsLeafNode())
            {
                return neighbourNode.HasItem() ? SideState.Full : SideState.Empty;
            }

            // not null and not leaf, so the neighbour node must be partial

            SideState sideState;
            if (neighbourNode.SideSolid(GetOpposite(side)))
            {
                // if the opposite side of current node is solid, then this is a partial node.
                sideState = SideState.Partial;
            }
            else
            {
                sideState = SideState.Empty;
            }

            return sideState;
        }

        // that child doesn't exist

        //let's check the parents
        while (neighbourCoords.Length > 0) {
            // get the next parent
            neighbourCoords = neighbourCoords.GetParentCoordinates();

            //does the next parent exist?
            if (!_allNodes.TryGetValue(neighbourCoords, out neighbourNode)) {
                continue;
            }

            // is the parent a leaf?
            if (neighbourNode.IsLeafNode()) {
                return neighbourNode.HasItem() ? SideState.Full : SideState.Empty;
            }

            // is not a leaf so cannot have an item

            break;
        }

        return SideState.Empty;
#else

        var currentNode = _root;

#if !DISABLE_PROFILER
        Profiler.BeginSample("Follow neighbour coords");
#endif

        // follow the children until you get to the node
        foreach (var coord in neighbourCoords) {
            if (currentNode == null) {
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
                return SideState.Empty;
            }

            if (currentNode.IsLeafNode()) {
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
                return currentNode.HasItem() ? SideState.Full : SideState.Empty;
            }

            currentNode = currentNode.GetChild(coord.ToIndex());
        }
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif

        //last currentNode is the actual node at the neighbour coordinates

        if (currentNode == null) {
            return SideState.Empty;
        }

        if (currentNode.IsLeafNode()) {
            return currentNode.HasItem() ? SideState.Full : SideState.Empty;
        }

        // not null and not leaf, so it must be partial
        // try to recursively get all nodes on this side
#if !DISABLE_PROFILER
        Profiler.BeginSample("Get current node solidity state");
#endif
        SideState sideState;
        if (currentNode.SideSolid(GetOpposite(side))) {
            // if the opposite side of current node is solid, then this is a partial node.
            sideState = SideState.Partial;
        } else {
            sideState = SideState.Empty;
        }
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
        return sideState;
#endif
    }

    private bool SideSolid(NeighbourSide side) {
        return _sideSolidCount[side] > 0;
//
//        //var solidChildrenList = new List<OctreeNode<T>>();
//
//        //GetAllSolidChildrenOnSide(solidChildrenList, side);
//
//        //return solidChildrenList.Count > 0;
//
//        Profiler.BeginSample("SideSolid");
//        var childCoords = GetChildCoordsOfSide(side);
//
//        Profiler.BeginSample("Get valid children");
//        var validChildren = childCoords.Select(childCoord => GetChild(childCoord.ToIndex())).Where(childNode => childNode != null && !childNode.IsDeleted());
//        Profiler.EndSample();
//
//        foreach (var childNode in validChildren)
//        {
//            if (childNode.IsSolid())
//            {
//                Profiler.EndSample();
//                return true;
//            }
//
//            if (childNode.IsLeafNode()) continue;
//            if (!childNode.SideSolid(side)) continue;
//
//            Profiler.EndSample();
//            return true;
//        }
//
//        Profiler.EndSample();
//        return false;
    }


    public HashSet<OctreeRenderFace> CreateFaces(int meshIndex) {
        AssertNotDeleted();

#if !DISABLE_PROFILER
        Profiler.BeginSample("New List");
#endif
        var faces = new HashSet<OctreeRenderFace>();
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif

        foreach (var side in AllSides) {
            CreateFacesForSideInternal(side, meshIndex, faces);
        }

        return faces;
    }

    private void CreateFacesForSideInternal(NeighbourSide side, int meshIndex, ICollection<OctreeRenderFace> faces) {
#if !DISABLE_PROFILER
        Profiler.BeginSample("New List");
#endif
//        var faces = new HashSet<OctreeRenderFace<T>>();
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
        CreateFacesForSideInternal(faces, side, _bounds, _nodeCoordinates, meshIndex);
    }

    private void CreateFacesForSideInternal(ICollection<OctreeRenderFace> faces, NeighbourSide side, Bounds bounds,
        OctreeNodeCoordinates coords, int meshIndex, bool parentPartial = false) {
        AssertNotDeleted();
#if !DISABLE_PROFILER
        Profiler.BeginSample("get side state");
#endif
        var sidestate = GetSideState(coords, side);
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif

#if !DISABLE_PROFILER
        Profiler.BeginSample("Create faces for side: " + side + " state: " + sidestate);
#endif

        switch (sidestate) {
            case SideState.Empty:
//            case SideState.Partial:

                AddFaceToList(faces, side, bounds, meshIndex);
                break;
            case SideState.Partial:
                if (!parentPartial) {
                    var childCoords = GetChildCoordsOfSide(side);
#if !DISABLE_PROFILER
                var childIndex = 0;
#endif

                    foreach (var childCoord in childCoords) {
#if !DISABLE_PROFILER
                    Profiler.BeginSample("Create faces for child " + childIndex);
                    Profiler.BeginSample("Get bounds and coords");
#endif
                        var childBounds = GetChildBounds(bounds, childCoord.ToIndex());
                        var childAbsCoords = new OctreeNodeCoordinates(coords, childCoord);
#if !DISABLE_PROFILER
                    Profiler.EndSample();
#endif

                        CreateFacesForSideInternal(faces, side, childBounds, childAbsCoords, meshIndex);
#if !DISABLE_PROFILER
                    Profiler.EndSample();
                    childIndex++;
#endif
                    }
                } else {
                    AddFaceToList(faces, side, bounds, meshIndex);
                }
                break;
            case SideState.Full:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
    }

    private static void AddFaceToList(ICollection<OctreeRenderFace> faces, NeighbourSide side, Bounds bounds,
        int meshIndex) {
        var face = new OctreeRenderFace(meshIndex);

        var min = bounds.min;
        var max = bounds.max;

        Vector3 n;

        switch (side) {
            case NeighbourSide.Above:
                face.vertices[0] = new Vector3(min.x, max.y, min.z);
                face.vertices[1] = new Vector3(min.x, max.y, max.z);
                face.vertices[2] = max;
                face.vertices[3] = new Vector3(max.x, max.y, min.z);

                n = Vector3.up;

                face.uvs[0] = new Vector2(min.x, min.z);
                face.uvs[1] = new Vector2(min.x, max.z);
                face.uvs[2] = new Vector2(max.x, max.z);
                face.uvs[3] = new Vector2(max.x, min.z);
                break;
            case NeighbourSide.Below:
                face.vertices[0] = new Vector3(min.x, min.y, max.z);
                face.vertices[1] = min;
                face.vertices[2] = new Vector3(max.x, min.y, min.z);
                face.vertices[3] = new Vector3(max.x, min.y, max.z);

                n = Vector3.down;

                face.uvs[0] = new Vector2(min.x, max.z);
                face.uvs[1] = new Vector2(min.x, min.z);
                face.uvs[2] = new Vector2(max.x, min.z);
                face.uvs[3] = new Vector2(max.x, max.z);
                break;
            case NeighbourSide.Right:
                face.vertices[0] = new Vector3(min.x, min.y, max.z);
                face.vertices[1] = new Vector3(min.x, max.y, max.z);
                face.vertices[2] = new Vector3(min.x, max.y, min.z);
                face.vertices[3] = min;

                n = Vector3.left;

                face.uvs[0] = new Vector2(max.z, min.y);
                face.uvs[1] = new Vector2(max.z, max.y);
                face.uvs[2] = new Vector2(min.z, max.y);
                face.uvs[3] = new Vector2(min.z, min.y);
                break;
            case NeighbourSide.Left:
                face.vertices[0] = new Vector3(max.x, min.y, min.z);
                face.vertices[1] = new Vector3(max.x, max.y, min.z);
                face.vertices[2] = max;
                face.vertices[3] = new Vector3(max.x, min.y, max.z);


                n = Vector3.right;

                face.uvs[0] = new Vector2(min.z, min.y);
                face.uvs[1] = new Vector2(min.z, max.y);
                face.uvs[2] = new Vector2(max.z, max.y);
                face.uvs[3] = new Vector2(max.z, min.y);
                break;
            case NeighbourSide.Back:
                face.vertices[0] = new Vector3(max.x, min.y, max.z);
                face.vertices[1] = max;
                face.vertices[2] = new Vector3(min.x, max.y, max.z);
                face.vertices[3] = new Vector3(min.x, min.y, max.z);

                n = Vector3.forward;

                face.uvs[0] = new Vector2(max.x, min.y);
                face.uvs[1] = new Vector2(max.x, max.y);
                face.uvs[2] = new Vector2(min.x, max.y);
                face.uvs[3] = new Vector2(min.x, min.y);
                break;
            case NeighbourSide.Forward:
                face.vertices[0] = min;
                face.vertices[1] = new Vector3(min.x, max.y, min.z);
                face.vertices[2] = new Vector3(max.x, max.y, min.z);
                face.vertices[3] = new Vector3(max.x, min.y, min.z);

                n = Vector3.back;

                face.uvs[0] = new Vector2(min.x, min.y);
                face.uvs[1] = new Vector2(min.x, max.y);
                face.uvs[2] = new Vector2(max.x, max.y);
                face.uvs[3] = new Vector2(max.x, min.y);
                break;
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }

        face.normal = n;

#if !DISABLE_PROFILER
                Profiler.BeginSample("Add face into list");
#endif
        faces.Add(face);
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
    }

    [Pure]
    public bool IsLeafNode() {
        AssertNotDeleted();
        return _childCount == 0;
    }

    [Pure]
    public int GetChildCount() {
        AssertNotDeleted();
        return _childCount;
    }

    public OctreeNode<T> GetChild(ChildIndex index) {
        AssertNotDeleted();

        var intIndex = (int) index;

        if (intIndex < 0 || intIndex > 7) {
            throw new ArgumentOutOfRangeException("index", "Invalid index specified for GetChild.");
        }

        if (_children == null) {
            return null;
        }

        return _children[(int) index];
    }

    private OctreeNode<T> SetChild(ChildIndex index, OctreeNode<T> child) {
        AssertNotDeleted();
#if USE_ALL_NODES
        _allNodes[child._nodeCoordinates] = child;
#endif

        _children[(int) index] = child;
        return child;
    }

    public IEnumerable<OctreeNode<T>> GetChildren() {
        AssertNotDeleted();
        if (_children == null) {
            yield break;
        }

        foreach (var child in _children.Where(child => child != null)) {
            yield return child;
        }
    }

    public Bounds GetChildBounds(ChildIndex childIndex) {
        AssertNotDeleted();

        return GetChildBounds(_bounds, childIndex);
    }

    //recursive, can be phantom bounds!
    public Bounds GetChildBounds(OctreeNodeCoordinates coordinates) {
        AssertNotDeleted();

        var result = GetBounds();

        return coordinates.Aggregate(result, (current, coordinate) => GetChildBounds(current, coordinate.ToIndex()));
    }

    public static Bounds GetChildBounds(Bounds originalBounds, ChildIndex childIndex) {
        Vector3 childDirection;

        switch (childIndex) {
            case ChildIndex.TopFwdLeft:
                childDirection = new Vector3(-1, -1, 1);
                break;
            case ChildIndex.TopFwdRight:
                childDirection = new Vector3(1, -1, 1);
                break;
            case ChildIndex.TopBackLeft:
                childDirection = new Vector3(-1, -1, -1);
                break;
            case ChildIndex.TopBackRight:
                childDirection = new Vector3(1, -1, -1);
                break;
            case ChildIndex.BotFwdLeft:
                childDirection = new Vector3(-1, 1, 1);
                break;
            case ChildIndex.BotFwdRight:
                childDirection = new Vector3(1, 1, 1);
                break;
            case ChildIndex.BotBackLeft:
                childDirection = new Vector3(-1, 1, -1);
                break;
            case ChildIndex.BotBackRight:
                childDirection = new Vector3(1, 1, -1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var childSize = originalBounds.extents;

        var childBounds = new Bounds(originalBounds.center - Vector3.Scale(childSize, childDirection * 0.5f), childSize);

        return childBounds;
    }

    public OctreeNode<T> AddChild(ChildIndex index) {
        if (index == ChildIndex.Invalid) {
            throw new ArgumentOutOfRangeException("index", "Cannot create a child at an invalid index.");
        }

#if !DISABLE_PROFILER
        Profiler.BeginSample("Add child " + index);
#endif
        AssertNotDeleted();

        if (_children == null) {
            _children = new OctreeNode<T>[8];
        }

        if (GetChild(index) != null) {
#if !DISABLE_PROFILER
            Profiler.EndSample();
#endif
            throw new ArgumentException("There is already a child at this index", "index");
        }

        _childCount++;
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
        return SetChild(index, new OctreeNode<T>(GetChildBounds(index), this, index, _depth + 1, _tree));
    }

    public void RemoveChild(ChildIndex index, bool cleanup = false) {
        AssertNotDeleted();
        if (_children == null) {
            throw new ArgumentException("The child at that index is already removed!", "index");
        }

        var indexInt = (int) index;

        if (_children[indexInt] != null) {
            _childCount--;
        } else {
            throw new ArgumentException("The child at that index is already removed!", "index");
        }

        _children[indexInt].SetDeleted();
        _children[indexInt] = null;

        if (_childCount != 0) {
            return;
        }

        _children = null;

        if (cleanup && _parent != null) {
            _parent.RemoveChild(_indexInParent, true);
        }
    }

    private void SetDeleted() {
#if USE_ALL_NODES
        _allNodes.Remove(_nodeCoordinates);
#endif

        //calling toarray here to force enumeration to flag deleted
        foreach (var octreeNode in BreadthFirst().ToArray()) {
            if (octreeNode._hasItem) {
                octreeNode.RemoveItem();
            }
            octreeNode._deleted = true;
        }
    }

    public bool IsDeleted() {
        return _deleted;
    }

    public void AddBounds(Bounds bounds, T item, int remainingDepth) {
        AssertNotDeleted();
        if (remainingDepth <= 0) {
            SetItem(item);
            return;
        }

        for (var i = 0; i < 8; i++) {
            var octreeNodeChildIndex = (ChildIndex) i;
            var childBounds = GetChildBounds(octreeNodeChildIndex);

            if (childBounds.Intersects(bounds)) {
                if (bounds.Contains(childBounds.min) && bounds.Contains(childBounds.max)) {
                    //child intersects and is completely contained by it

                    var child = GetChild(octreeNodeChildIndex);
                    if (child != null) {
                        RemoveChild(octreeNodeChildIndex);
                        child = AddChild(octreeNodeChildIndex);
                    } else {
                        if (HasItem()) {
                            SubDivide();

                            child = GetChild(octreeNodeChildIndex);
                        } else {
                            child = AddChild(octreeNodeChildIndex);
                        }
                    }

                    child.SetItem(item);
                } else {
                    //child intersects but is not completely contained by it

                    var child = GetChild(octreeNodeChildIndex);

                    if (child == null) {
                        if (HasItem()) {
                            SubDivide();

                            child = GetChild(octreeNodeChildIndex);
                        } else {
                            child = AddChild(octreeNodeChildIndex);
                        }
                    }
                    child.AddBounds(bounds, item, remainingDepth - 1);
                }
            }
        }
    }

    public void SetItem(T item, bool cleanup = false) {
        AssertNotDeleted();

        if (!IsLeafNode()) {
            //if it's not a leaf node, we need to remove all children

            RemoveAllChildren();
        }

        if (!_hasItem) {
            _hasItem = true;

            AddSolidNode(ChildIndex.Invalid, true);

            _item = item;
            _tree.NodeAdded(this);
        } else if (_tree.ItemsBelongInSameMesh(_item, item)) {
            //item not changed or belongs in same mesh as the other one
            _item = item;
        } else {
            //remove from the previous item's mesh
            _tree.NodeRemoved(this);
            _item = item;
            //add to the next item's mesh!
            _tree.NodeAdded(this);
        }

        if (cleanup && _parent._childCount == 8) {
            for (var i = 0; i < 8; i++) {
                if (i == (int) _indexInParent) {
                    continue;
                }
                var sibling = _parent.GetChild((ChildIndex) i);
                if (!Equals(sibling.GetItem(), item)) {
                    return;
                }
            }

            _parent.SetItem(item, true);
        }
    }

    private void RemoveAllChildren() {
#if !DISABLE_PROFILER
        Profiler.BeginSample("Remove All Children");
#endif
        for (var i = 0; i < 8; i++) {
            if (GetChild((ChildIndex) i) != null) {
#if !DISABLE_PROFILER
                Profiler.BeginSample("Remove child " + i);
#endif
                RemoveChild((ChildIndex) i);
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
            }
        }
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
    }

    public bool HasItem() {
        return _hasItem;
    }

    public void RemoveItem() {
#if !DISABLE_PROFILER
        Profiler.BeginSample("Remove Item");
#endif
        if (_hasItem) {
            _tree.NodeRemoved(this);

            RemoveSolidNode(ChildIndex.Invalid, true);

            _hasItem = false;
        }

        _item = default(T);
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
    }

    public Bounds GetBounds() {
        AssertNotDeleted();
        return _bounds;
    }

    public OctreeNodeCoordinates GetCoords() {
        AssertNotDeleted();
        return _nodeCoordinates;
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
        AssertNotDeleted();
        var queue = new Queue<OctreeNode<T>>();
        queue.Enqueue(this);

        var discovered = new HashSet<OctreeNode<T>> {_root};

        while (queue.Count > 0) {
            var node = queue.Dequeue();
            yield return node;

            foreach (var child in node.GetChildren().Where(child => !discovered.Contains(child))) {
                queue.Enqueue(child);
                discovered.Add(child);
            }
        }
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
        AssertNotDeleted();
        var stack = new Stack<OctreeNode<T>>();
        stack.Push(this);

        while (stack.Count > 0) {
            var node = stack.Pop();
            yield return node;

            foreach (var child in node.GetChildren()) {
                stack.Push(child);
            }
        }
    }

    public ChildIndex GetIndexInParent() {
        return _indexInParent;
    }

    public void SubDivide(bool fillChildren = true) {
        AssertNotDeleted();

        if (!IsLeafNode()) {
            // if it's not a leaf node then it's already divided
            return;
        }

#if !DISABLE_PROFILER
        Profiler.BeginSample("Subdivide");

        Profiler.BeginSample("Add children");
#endif
        for (var i = 0; i < 8; i++) {
#if !DISABLE_PROFILER
            Profiler.BeginSample("Add child " + i);
#endif
            var newChild = AddChild((ChildIndex) i);
#if !DISABLE_PROFILER
            Profiler.EndSample();
#endif

            if (fillChildren) {
#if !DISABLE_PROFILER
                Profiler.BeginSample("Set child item " + i);
#endif
                newChild.SetItem(_item);
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif
            }
        }
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif

        RemoveItem();
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
    }

    private enum SideState {
        Empty,
        Partial,
        Full
    }

    public bool IsSolid() {
        return IsLeafNode() && HasItem();
    }

    public OctreeNode<T> AddRecursive(OctreeNodeCoordinates coordinates) {
        var node = this;

        foreach (var coordinate in coordinates) {
            var index = coordinate.ToIndex();

            var child = node.GetChild(index);
            if (child != null) {
                node = child;
            } else {
                if (node.HasItem()) {
                    node.SubDivide();
                    node = node.GetChild(index);
                } else {
                    node = node.AddChild(index);
                }
            }
        }

        return node;
    }

    public OctreeNode<T> GetParent() {
        return _parent;
    }

    public void RemoveRecursive(OctreeNodeCoordinates coordinates, bool cleanup = false) {
        if (coordinates.Length == 0) {
            return;
        }

        var node = this;

        var subdivided = false;

        var nodeItem = default(T);

        foreach (var coordinate in coordinates) {
            var index = coordinate.ToIndex();

            if (!subdivided) {
                var child = node.GetChild(index);
                if (child != null) {
                    // has a child at that node, so go deeper
                    node = child;
                    continue;
                }

                // no child at that node
                if (!node.HasItem()) {
                    //it doesn't have an item! but the child is null!? then it has a child somewhere else, so nothing to remove!
                    return;
                }

                nodeItem = node.GetItem();

                node.SubDivide(false);

#if !DISABLE_PROFILER
                Profiler.BeginSample("Set items of other children");
#endif
                // set the items for all other children manually
                for (var i = 0; i < 8; ++i) {
                    var childIndex = (ChildIndex) i;

                    if (childIndex != index) {
                        node.GetChild(childIndex).SetItem(nodeItem);
                    }
                }
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif

                node = node.GetChild(index);

                subdivided = true;
            } else {
#if !DISABLE_PROFILER
                Profiler.BeginSample("Create and set items of other children");
#endif
                // subdivision of parent happened, from now on we need to add children ourselves
                // because the current node will not have any children or item defined

                // create all children and set the items for all but the next child manually
                for (var i = 0; i < 8; ++i) {
                    var childIndex = (ChildIndex) i;
                    var newChild = node.AddChild(childIndex);

                    if (childIndex != index) {
                        newChild.SetItem(nodeItem);
                    }
                }
#if !DISABLE_PROFILER
                Profiler.EndSample();
#endif

                node = node.GetChild(index);
            }
        }

#if !DISABLE_PROFILER
        Profiler.BeginSample("Remove final child");
#endif
        node.GetParent().RemoveChild(node.GetIndexInParent(), cleanup);
#if !DISABLE_PROFILER
        Profiler.EndSample();
#endif
    }
}