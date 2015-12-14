//#define USE_ALL_NODES

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

public enum NeighbourSide {
    Above = 0,
    Below = 1,
    Right = 2,
    Left = 3,
    Back = 4,
    Forward = 5,
    Invalid = -1
}

public abstract class OctreeNode {
    public enum ChildIndex {
        Invalid = -1,

        LeftBelowBack = 0,
        RightBelowBack = 1,

        LeftAboveBack = 2,
        RightAboveBack = 3,

        LeftBelowForward = 4,
        RightBelowForward = 5,

        LeftAboveForward = 6,
        RightAboveForward = 7
    }


    private static readonly Vector3 RightAboveBack = new Vector3(1, 1, -1);
    private static readonly Vector3 LeftAboveBack = new Vector3(-1, 1, -1);

    private static readonly Vector3 RightAboveForward = new Vector3(1, 1, 1);
    private static readonly Vector3 LeftAboveForward = new Vector3(-1, 1, 1);

    private static readonly Vector3 RightBelowBack = new Vector3(1, -1, -1);
    private static readonly Vector3 LeftBelowBack = new Vector3(-1, -1, -1);

    private static readonly Vector3 RightBelowForward = new Vector3(1, -1, 1);
    private static readonly Vector3 LeftBelowForward = new Vector3(-1, -1, 1);

    public static readonly NeighbourSide[] AllSides = {
        NeighbourSide.Left,
        NeighbourSide.Right,
        NeighbourSide.Below,
        NeighbourSide.Above,
        NeighbourSide.Back,
        NeighbourSide.Forward
    };

    protected static readonly OctreeChildCoordinates[] LeftCoords = {
        new OctreeChildCoordinates(0, 0, 0),
        new OctreeChildCoordinates(0, 1, 0),
        new OctreeChildCoordinates(0, 0, 1),
        new OctreeChildCoordinates(0, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] RightCoords = {
        new OctreeChildCoordinates(1, 0, 0),
        new OctreeChildCoordinates(1, 1, 0),
        new OctreeChildCoordinates(1, 0, 1),
        new OctreeChildCoordinates(1, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] AboveCoords = {
        new OctreeChildCoordinates(0, 1, 0),
        new OctreeChildCoordinates(0, 1, 1),
        new OctreeChildCoordinates(1, 1, 0),
        new OctreeChildCoordinates(1, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] BelowCoords = {
        new OctreeChildCoordinates(0, 0, 0),
        new OctreeChildCoordinates(0, 0, 1),
        new OctreeChildCoordinates(1, 0, 0),
        new OctreeChildCoordinates(1, 0, 1)
    };

    protected static readonly OctreeChildCoordinates[] BackCoords = {
        new OctreeChildCoordinates(0, 0, 0),
        new OctreeChildCoordinates(0, 1, 0),
        new OctreeChildCoordinates(1, 0, 0),
        new OctreeChildCoordinates(1, 1, 0)
    };

    protected static readonly OctreeChildCoordinates[] ForwardCoords = {
        new OctreeChildCoordinates(0, 0, 1),
        new OctreeChildCoordinates(0, 1, 1),
        new OctreeChildCoordinates(1, 0, 1),
        new OctreeChildCoordinates(1, 1, 1)
    };

    protected static Vector3 GetChildDirection(ChildIndex childIndex) {
        Vector3 childDirection;

        switch (childIndex) {
            case ChildIndex.RightAboveBack:
                childDirection = RightAboveBack;
                break;
            case ChildIndex.LeftAboveBack:
                childDirection = LeftAboveBack;
                break;
            case ChildIndex.RightAboveForward:
                childDirection = RightAboveForward;
                break;
            case ChildIndex.LeftAboveForward:
                childDirection = LeftAboveForward;
                break;
            case ChildIndex.RightBelowBack:
                childDirection = RightBelowBack;
                break;
            case ChildIndex.LeftBelowBack:
                childDirection = LeftBelowBack;
                break;
            case ChildIndex.RightBelowForward:
                childDirection = RightBelowForward;
                break;
            case ChildIndex.LeftBelowForward:
                childDirection = LeftBelowForward;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return childDirection;
    }

    public static NeighbourSide GetOpposite(NeighbourSide side) {
        switch (side) {
            case NeighbourSide.Above:
                return NeighbourSide.Below;
            case NeighbourSide.Below:
                return NeighbourSide.Above;
            case NeighbourSide.Right:
                return NeighbourSide.Left;
            case NeighbourSide.Left:
                return NeighbourSide.Right;
            case NeighbourSide.Back:
                return NeighbourSide.Forward;
            case NeighbourSide.Forward:
                return NeighbourSide.Back;
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
            case ChildIndex.LeftBelowBack:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.RightBelowBack:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.LeftAboveBack:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.RightAboveBack:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Back;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.LeftBelowForward:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.RightBelowForward:
                verticalSide = NeighbourSide.Below;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Right;
                break;
            case ChildIndex.LeftAboveForward:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Left;
                break;
            case ChildIndex.RightAboveForward:
                verticalSide = NeighbourSide.Above;
                depthSide = NeighbourSide.Forward;
                horizontalSide = NeighbourSide.Right;
                break;
            default:
                throw new ArgumentOutOfRangeException("childIndex", childIndex, null);
        }
    }
}

public class OctreeNode<T> : OctreeNode {
#if USE_ALL_NODES
    private readonly Dictionary<int, OctreeNode<T>> _allNodes;
#endif
    private readonly Bounds _bounds;
//    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _depth;
    private readonly ChildIndex _indexInParent;
    private readonly OctreeNodeCoordinates<T> _nodeCoordinates;
    private readonly OctreeNode<T> _parent;

    private OctreeNode<T> GetRoot() {
        return _tree.GetRoot();
    }

    private readonly Dictionary<NeighbourSide, HashSet<OctreeNode<T>>> _sideSolidChildren =
        new Dictionary<NeighbourSide, HashSet<OctreeNode<T>>>();

    private readonly Dictionary<NeighbourSide, int> _sideSolidCount = new Dictionary<NeighbourSide, int>();
    private readonly Octree<T> _tree;
    private int _childCount;
    private OctreeNode<T>[] _children;
    private bool _deleted;
    private bool _hasItem;

    private T _item;
    private int _solidNodeCount;
    public OctreeNode(Bounds bounds, Octree<T> tree) : this(bounds, null, ChildIndex.Invalid, 0, tree) {}

    public OctreeNode(Bounds bounds, OctreeNode<T> parent, ChildIndex indexInParent, int depth, Octree<T> tree) {
        _deleted = false;
        _bounds = bounds;
        _parent = parent;
        if (parent == null) {
            _nodeCoordinates = new OctreeNodeCoordinates<T>(tree);
#if USE_ALL_NODES
    // ReSharper disable once UseObjectOrCollectionInitializer
            _allNodes = new Dictionary<int, OctreeNode<T>>();
            _allNodes[_nodeCoordinates.GetHashCode()] = this;
#endif
        } else {
#if USE_ALL_NODES
            _allNodes = _root._allNodes;
#endif

            _nodeCoordinates = new OctreeNodeCoordinates<T>(tree, parent._nodeCoordinates,
                OctreeChildCoordinates.FromIndex(indexInParent));
        }

        _indexInParent = indexInParent;
        _item = default(T);
        _depth = depth;
        _tree = tree;

        _sideSolidCount[NeighbourSide.Above] = 0;
        _sideSolidCount[NeighbourSide.Below] = 0;
        _sideSolidCount[NeighbourSide.Right] = 0;
        _sideSolidCount[NeighbourSide.Left] = 0;
        _sideSolidCount[NeighbourSide.Forward] = 0;
        _sideSolidCount[NeighbourSide.Back] = 0;

        _sideSolidChildren[NeighbourSide.Above] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Below] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Right] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Left] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Forward] = new HashSet<OctreeNode<T>>();
        _sideSolidChildren[NeighbourSide.Back] = new HashSet<OctreeNode<T>>();
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
        var neighbourCoords = _nodeCoordinates.GetNeighbourCoords(side);

        //out of the map!
        if (neighbourCoords == null) {
            return null;
        }

#if USE_ALL_NODES
        OctreeNode<T> neighbourNode;

        if (_allNodes.TryGetValue(neighbourCoords.GetHashCode(), out neighbourNode)) {
            if (neighbourNode.IsSolid()) {
                return new HashSet<OctreeNode<T>> {neighbourNode};
            }

            return neighbourNode._sideSolidChildren[GetOpposite(side)];
        }

        // that child doesn't exist

        //let's check the parents
        while (neighbourCoords.Length > 0) {
            // get the next parent
            neighbourCoords = neighbourCoords.GetParentCoordinates();

            //does the next parent exist?
            if (!_allNodes.TryGetValue(neighbourCoords.GetHashCode(), out neighbourNode)) {
                continue;
            }

            // is the parent a leaf?
            if (neighbourNode.IsSolid()) {
                return new HashSet<OctreeNode<T>> {neighbourNode};
            }

            // is not a leaf so cannot have an item

            break;
        }

        return null;
#else

        var currentNeighbourNode = neighbourCoords.GetTree().GetRoot();

        foreach (var coord in neighbourCoords) {
            if (currentNeighbourNode == null || currentNeighbourNode.IsDeleted()) {
                return null;
            }

            if (currentNeighbourNode.IsSolid()) {
                return new HashSet<OctreeNode<T>> {currentNeighbourNode};
            }

            currentNeighbourNode = currentNeighbourNode.GetChild(coord.ToIndex());
        }

//        last currentNode is the actual node at the neighbour coordinates
        if (currentNeighbourNode == null || currentNeighbourNode.IsDeleted()) {
            return null;
        }

        if (currentNeighbourNode.IsSolid()) {
            return new HashSet<OctreeNode<T>> {currentNeighbourNode};
        }

        return currentNeighbourNode._sideSolidChildren[GetOpposite(side)];
#endif
    }

    private static OctreeChildCoordinates[] GetChildCoordsOfSide(NeighbourSide side) {
        OctreeChildCoordinates[] childCoords;

        switch (side) {
            case NeighbourSide.Above:
                childCoords = AboveCoords;
                break;
            case NeighbourSide.Below:
                childCoords = BelowCoords;
                break;
            case NeighbourSide.Right:
                childCoords = RightCoords;
                break;
            case NeighbourSide.Left:
                childCoords = LeftCoords;
                break;
            case NeighbourSide.Back:
                childCoords = BackCoords;
                break;
            case NeighbourSide.Forward:
                childCoords = ForwardCoords;
                break;
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }
        return childCoords;
    }

    private SideState GetSideState(OctreeNodeCoordinates<T> coords, NeighbourSide side) {
        AssertNotDeleted();
        var neighbourCoords = coords.GetNeighbourCoords(side);

        //out of the boundaries
        if (neighbourCoords == null) {
            return SideState.Empty;
        }

#if USE_ALL_NODES
        OctreeNode<T> neighbourNode;

        if (_allNodes.TryGetValue(neighbourCoords.GetHashCode(), out neighbourNode)) {
            if (neighbourNode.IsLeafNode()) {
                return neighbourNode.HasItem() ? SideState.Full : SideState.Empty;
            }

            // not null and not leaf, so the neighbour node must be partial

            SideState sideState;
            if (neighbourNode.SideSolid(GetOpposite(side))) {
                // if the opposite side of current node is solid, then this is a partial node.
                sideState = SideState.Partial;
            } else {
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
            if (!_allNodes.TryGetValue(neighbourCoords.GetHashCode(), out neighbourNode)) {
                continue;
            }

            if (neighbourNode.IsDeleted()) {
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

        var currentNode = neighbourCoords.GetTree().GetRoot();

        // follow the children until you get to the node
        foreach (var coord in neighbourCoords) {
            if (currentNode == null) {
                return SideState.Empty;
            }

            if (currentNode.IsLeafNode()) {
                return currentNode.HasItem() ? SideState.Full : SideState.Empty;
            }

            currentNode = currentNode.GetChild(coord.ToIndex());
        }

        //last currentNode is the actual node at the neighbour coordinates

        if (currentNode == null) {
            return SideState.Empty;
        }

        if (currentNode.IsLeafNode()) {
            return currentNode.HasItem() ? SideState.Full : SideState.Empty;
        }

        // not null and not leaf, so it must be partial
        // try to recursively get all nodes on this side
        SideState sideState;
        if (currentNode.SideSolid(GetOpposite(side))) {
            // if the opposite side of current node is solid, then this is a partial node.
            sideState = SideState.Partial;
        } else {
            sideState = SideState.Empty;
        }
        return sideState;
#endif
    }

    private bool SideSolid(NeighbourSide side) {
        return _sideSolidCount[side] > 0;
    }

    public HashSet<OctreeRenderFace> CreateFaces(int meshIndex) {
        AssertNotDeleted();

        var faces = new HashSet<OctreeRenderFace>();

        foreach (var side in AllSides) {
            CreateFacesForSideInternal(side, meshIndex, faces);
        }

        return faces;
    }

    private void CreateFacesForSideInternal(NeighbourSide side, int meshIndex, ICollection<OctreeRenderFace> faces) {
        CreateFacesForSideInternal(faces, side, _bounds, _nodeCoordinates, meshIndex);
    }

    private void CreateFacesForSideInternal(ICollection<OctreeRenderFace> faces, NeighbourSide side, Bounds bounds,
        OctreeNodeCoordinates<T> coords, int meshIndex, bool parentPartial = false) {
        AssertNotDeleted();
        var sidestate = GetSideState(coords, side);

        switch (sidestate) {
            case SideState.Empty:
//            case SideState.Partial:

                AddFaceToList(faces, side, bounds, meshIndex);
                break;
            case SideState.Partial:
                if (!parentPartial) {
                    var childCoords = GetChildCoordsOfSide(side);

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < childCoords.Length; i++) {
                        var childCoord = childCoords[i];
                        var childBounds = GetChildBoundsInternal(bounds, childCoord.ToIndex());
                        var childAbsCoords = new OctreeNodeCoordinates<T>(_tree, coords, childCoord);

                        CreateFacesForSideInternal(faces, side, childBounds, childAbsCoords, meshIndex, false);
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
            case NeighbourSide.Left:
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
            case NeighbourSide.Right:
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
            case NeighbourSide.Forward:
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
            case NeighbourSide.Back:
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

        faces.Add(face);
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
        child.AssertNotDeleted();
        _allNodes.Add(child._nodeCoordinates.GetHashCode(), child);
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

    private Bounds GetChildBounds(ChildIndex childIndex) {
        AssertNotDeleted();

        return GetChildBoundsInternal(_bounds, childIndex);
    }

    //recursive, can be phantom bounds!
    [Pure]
    public Bounds GetChildBounds(OctreeNodeCoordinates<T> coordinates) {
        AssertNotDeleted();

        var result = GetBounds();
        var rootSize = result.size;

        var count = 1;

        var up = 0;
        var right = 0;
        var forward = 0;

        foreach (var coordinate in coordinates) {
            count++;

            up = up * 2 + coordinate.y * 2 - 1;
            right = right * 2 + coordinate.x * 2 - 1;
            forward = forward * 2 + coordinate.z * 2 - 1;
            //            if (child != null) {
            //                // current child (or self) isn't null, try to get the real child
            //                child = child.GetChild(coordinate.ToIndex());
            //
            //                if (child != null) {
            //                    // the child is there, no need to calculate!
            //                    result = child.GetBounds();
            //                    continue;
            //                }
            //            }
            //            // no child there... get phantom bounds
            //            result = GetChildBoundsInternal(result, coordinate.ToIndex());
        }

        var upVector = Vector3.up;
        var rightVector = Vector3.right;
        var forwardVector = Vector3.forward;

        var center = (upVector * (rootSize.y * up) +
                      rightVector * (rootSize.x * right) +
                      forwardVector * (rootSize.z * forward)) / Mathf.Pow(2, count);

        var size = rootSize / Mathf.Pow(2, count - 1);

        return new Bounds(center, size);
    }


    private static Bounds GetChildBoundsInternal(Bounds originalBounds, ChildIndex childIndex) {
        var childDirection = GetChildDirection(childIndex);

        var childSize = originalBounds.extents;

        var childBounds = new Bounds(originalBounds.center + Vector3.Scale(childSize, childDirection * 0.5f), childSize);

        return childBounds;
    }

    public OctreeNode<T> AddChild(ChildIndex index) {
        if (index == ChildIndex.Invalid) {
            throw new ArgumentOutOfRangeException("index", "Cannot create a child at an invalid index.");
        }

        AssertNotDeleted();

        if (_children == null) {
            _children = new OctreeNode<T>[8];
        }

        if (GetChild(index) != null) {
            throw new ArgumentException("There is already a child at this index", "index");
        }

        _childCount++;
        return SetChild(index, new OctreeNode<T>(GetChildBounds(index), this, index, _depth + 1, _tree));
    }

    public void RemoveChild(ChildIndex index, bool cleanup = false) {
        RemoveChildInternal(index, cleanup, true);
    }

    private void RemoveChildInternal(ChildIndex index, bool cleanup, bool updateNeighbours) {
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

        _children[indexInt].SetDeleted(updateNeighbours);
        _children[indexInt] = null;

        if (_childCount != 0) {
            return;
        }

        _children = null;

        if (cleanup && _parent != null) {
            // no need to update parent's neighbours!
            _parent.RemoveChildInternal(_indexInParent, true, false);
        }
    }

    private void SetDeleted(bool updateNeighbours) {
        //calling toarray here to force enumeration to flag deleted
        foreach (var octreeNode in BreadthFirst().ToArray()) {
#if USE_ALL_NODES
            _allNodes.Remove(octreeNode._nodeCoordinates.GetHashCode());
#endif
            _tree.NodeRemoved(octreeNode, updateNeighbours);

            if (octreeNode._hasItem) {
                octreeNode.RemoveItem(updateNeighbours);
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
                var child = GetChild(octreeNodeChildIndex);
                if (child == null) {
                    if (HasItem()) {
                        SubDivide();
                        child = GetChild(octreeNodeChildIndex);
                    } else {
                        child = AddChild(octreeNodeChildIndex);
                    }
                }

                if (bounds.Contains(childBounds.min) && bounds.Contains(childBounds.max)) {
                    //child intersects and is completely contained by it

                    child.SetItem(item);
                } else {
                    //child intersects but is not completely contained by it

                    child.AddBounds(bounds, item, remainingDepth - 1);
                }
            }
        }
    }

    public void SetItem(T item, bool cleanup = false) {
        SetItemInternal(item, cleanup, true);
    }

    private void SetItemInternal(T item, bool cleanup, bool updateNeighbours) {
        AssertNotDeleted();

        if (!IsLeafNode()) {
            //if it's not a leaf node, we need to remove all children
            // no need to update neighbours

            RemoveAllChildren();
        }

        if (!_hasItem) {
            // still let the neighbours know if necessary
//            _tree.NodeRemoved(this, false);

            _hasItem = true;

            AddSolidNode(ChildIndex.Invalid, true);

            _item = item;
            _tree.NodeAdded(this, false);
        } else if (_tree.ItemsBelongInSameMesh(_item, item)) {
            // has item
            // item not changed or belongs in same mesh as the other one
            _item = item;
        } else {
            // remove from the previous item's mesh
            // no need to update neighbours now, will be done below
            _tree.NodeRemoved(this, false);
            _item = item;
            //add to the next item's mesh!
            _tree.NodeAdded(this, false);
        }

        if (cleanup && _parent != null && _parent._childCount == 8) {
            // check if all other siblings have the same item.
            // if they do, then we can just set the parent's item instead
            for (var i = 0; i < 8; i++) {
                if (i == (int) _indexInParent) {
                    continue;
                }
                var sibling = _parent.GetChild((ChildIndex) i);

                if (!Equals(sibling.GetItem(), item)) {
                    // not all siblings have the same item :(
                    if (updateNeighbours) {
                        Octree<T>.UpdateNeighbours(this);
                    }

                    return;
                }
            }

            // no need to update parent's neighbours since they will be facing full sides anyway
            _parent.SetItemInternal(item, true, false);
        } else {
            // end of the line, can update neighbours if necessary
            // it's either not cleanup,
            // or the parent doesn't exist (reached top),
            // or the parent doesn't have all eight children
            if (updateNeighbours) {
                Octree<T>.UpdateNeighbours(this);
            }
        }
    }

    private void RemoveAllChildren() {
        for (var i = 0; i < 8; i++) {
            if (GetChild((ChildIndex) i) != null) {
                // no need to update neighbours
                RemoveChildInternal((ChildIndex) i, false, false);
            }
        }
    }

    public bool HasItem() {
        return _hasItem;
    }

    public void RemoveItem(bool updateNeighbours = true) {
        if (_hasItem) {
            _tree.NodeRemoved(this, updateNeighbours);

            RemoveSolidNode(ChildIndex.Invalid, true);

            _hasItem = false;
        }

        _item = default(T);
    }

    public Bounds GetBounds() {
        AssertNotDeleted();
        return _bounds;
    }

    public OctreeNodeCoordinates<T> GetCoords() {
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

        var discovered = new HashSet<OctreeNode<T>> {GetRoot()};

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

        var item = _item;

        RemoveItem(false);

        for (var i = 0; i < 8; i++) {
            var newChild = AddChild((ChildIndex) i);

            if (fillChildren) {
                newChild.SetItemInternal(item, false, false);
            }
        }
    }

    public bool IsSolid() {
        return IsLeafNode() && HasItem();
    }

    public OctreeNode<T> AddRecursive(OctreeNodeCoordinates<T> coordinates) {
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

    public void RemoveRecursive(OctreeNodeCoordinates<T> coordinates, bool cleanup = false) {
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

                // set the items for all other children manually
                for (var i = 0; i < 8; ++i) {
                    var childIndex = (ChildIndex) i;

                    if (childIndex != index) {
                        // do not update neighbours as they will mostly be full
                        node.GetChild(childIndex).SetItemInternal(nodeItem, false, false);
                    }
                }

                node = node.GetChild(index);

                subdivided = true;
            } else {
                // subdivision of parent happened, from now on we need to add children ourselves
                // because the current node will not have any children or item defined

                // create all children and set the items for all but the next child manually
                for (var i = 0; i < 8; ++i) {
                    var childIndex = (ChildIndex) i;
                    var newChild = node.AddChild(childIndex);

                    if (childIndex != index) {
                        newChild.SetItemInternal(nodeItem, false, false);
                    }
                }

                node = node.GetChild(index);
            }
        }

        // remove final child now
        // this will result in an update for the neighbours
        node.GetParent().RemoveChildInternal(node.GetIndexInParent(), cleanup, true);
    }

    private enum SideState {
        Empty,
        Partial,
        Full
    }

    public Octree<T> GetTree() {
        return _tree;
    }
}