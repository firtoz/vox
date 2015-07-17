using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

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
        Back = 5
    }

    protected static readonly NeighbourSide[] Sides = {
        NeighbourSide.Above,
        NeighbourSide.Below,
        NeighbourSide.Forward,
        NeighbourSide.Back,
        NeighbourSide.Left,
        NeighbourSide.Right
    };

    protected static readonly OctreeChildCoordinates[] AboveCoords = {
        new OctreeChildCoordinates(0, 1, 0),
        new OctreeChildCoordinates(1, 1, 0),
        new OctreeChildCoordinates(0, 1, 1),
        new OctreeChildCoordinates(1, 1, 1)
    };

    protected static readonly OctreeChildCoordinates[] BelowCoords = {
        new OctreeChildCoordinates(0, 0, 0),
        new OctreeChildCoordinates(1, 0, 0),
        new OctreeChildCoordinates(0, 0, 1),
        new OctreeChildCoordinates(1, 0, 1)
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
}

public class OctreeNode<T> : OctreeNode {
//    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _depth;
    private readonly ChildIndex _indexInParent;
    private readonly OctreeNodeCoordinates _nodeCoordinates;
    private readonly OctreeNode<T> _parent;
    private readonly OctreeNode<T> _root;
    private Bounds _bounds;
    private int _childCount;
    private OctreeNode<T>[] _children;
    private bool _deleted;
    private bool _hasItem;
    private T _item;
    public OctreeNode(Bounds bounds) : this(bounds, null, ChildIndex.Invalid, 0) {}

    public OctreeNode(Bounds bounds, OctreeNode<T> parent, ChildIndex indexInParent, int depth) {
        _deleted = false;
        _bounds = bounds;
        _parent = parent;
        if (parent == null) {
            _root = this;
            _nodeCoordinates = new OctreeNodeCoordinates();
//            _coords = new OctreeChildCoordinates[0];
        }
        else {
            _root = _parent._root;
            _nodeCoordinates = new OctreeNodeCoordinates(parent._nodeCoordinates,
                OctreeChildCoordinates.FromIndex(indexInParent));
//            _coords = parent._coords.Concat(new[] {OctreeChildCoordinates.FromIndex(indexInParent)}).ToArray();
        }
        _indexInParent = indexInParent;
        _item = default(T);
        _depth = depth;
    }

    private void AssertNotDeleted() {
        if (_deleted) {
            throw new Exception("Node is deleted!");
        }
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

    private SideState GetSideState(OctreeNodeCoordinates coords, NeighbourSide side) {
        AssertNotDeleted();
        var neighbourCoords = coords.GetNeighbourCoords(side);

        //out of the boundaries
        if (neighbourCoords == null) {
            return SideState.Empty;
        }

        var currentNode = _root;

        foreach (var coord in neighbourCoords) {
            if (currentNode == null) {
                return SideState.Empty;
            }
            if (currentNode.IsLeafNode()) {
                return SideState.Full;
            }

            currentNode = currentNode.GetChild(coord.ToIndex());
        }

        //last currentNode is the actual node at the neighbour coordinates

        if (currentNode == null) {
            return SideState.Empty;
        }
        if (currentNode.IsLeafNode()) {
            return SideState.Full;
        }

        //not null and not leaf, so it must be partial
        return SideState.Partial;
//
//
//        //        while (currentNode != null) {
//
//        //        }
//
//        var neighbour = _root.GetChildAtCoords(neighbourCoords);
//
//        if (neighbour != null) {
//            return neighbour.IsLeafNode() ? SideState.Full : SideState.Partial;
//        }
//
//        var neighbourLength = neighbourCoords.Length;
//
//        while (neighbourLength > 0)
//        {
//            neighbourCoords = new OctreeNodeCoordinates(neighbourCoords.Take(neighbourCoords.Length - 1).ToArray());
//            neighbour = _root.GetChildAtCoords(neighbourCoords);
//            if (neighbour != null && neighbour.IsLeafNode())
//            {
//                return SideState.Full;
//            }
//
//            neighbourLength = neighbourCoords.Length;
//        }
//
//        return SideState.Empty;
    }

    public void GetVertices(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs) {
        foreach (var side in Sides) {
            GetVertices(vertices, normals, uvs, side, _bounds, _nodeCoordinates);
        }
    }

    private void GetVertices(ICollection<Vector3> vertices, ICollection<Vector3> normals, List<Vector2> uvs, NeighbourSide side, Bounds bounds, OctreeNodeCoordinates coords) {
        var sidestate = GetSideState(coords, side);

        switch (sidestate) {
            case SideState.Empty:

                var min = bounds.min;
                var max = bounds.max;

                Vector3 n;

                switch (side) {
                    case NeighbourSide.Above:
                        vertices.Add(new Vector3(min.x, max.y, min.z));
                        vertices.Add(new Vector3(min.x, max.y, max.z));
                        vertices.Add(max);
                        vertices.Add(new Vector3(max.x, max.y, min.z));

                        n = Vector3.up;
                        //
                        //                        uvs.Add(new Vector2(min.x, min.z));
                        //                        uvs.Add(new Vector2(min.x, max.z));
                        //                        uvs.Add(new Vector2(max.x, max.z));
                        //                        uvs.Add(new Vector2(max.x, min.z));
                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.up);
                        uvs.Add(Vector2.one);
                        uvs.Add(Vector2.right);
                        break;
                    case NeighbourSide.Below:
                        vertices.Add(new Vector3(min.x, min.y, max.z));
                        vertices.Add(min);
                        vertices.Add(new Vector3(max.x, min.y, min.z));
                        vertices.Add(new Vector3(max.x, min.y, max.z));

                        n = Vector3.down;
                        //
                        //                        uvs.Add(new Vector2(min.x, min.z));
                        //                        uvs.Add(new Vector2(max.x, min.z));
                        //                        uvs.Add(new Vector2(max.x, max.z));
                        //                        uvs.Add(new Vector2(min.x, max.z));
                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.up);
                        uvs.Add(Vector2.one);
                        uvs.Add(Vector2.right);
                        break;
                    case NeighbourSide.Right:
                        vertices.Add(new Vector3(min.x, min.y, max.z));
                        vertices.Add(new Vector3(min.x, max.y, max.z));
                        vertices.Add(new Vector3(min.x, max.y, min.z));
                        vertices.Add(min);

                        n = Vector3.left;

                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.up);
                        uvs.Add(Vector2.one);
                        uvs.Add(Vector2.right);
                        break;
                    case NeighbourSide.Left:
                        vertices.Add(new Vector3(max.x, min.y, min.z));
                        vertices.Add(new Vector3(max.x, max.y, min.z));
                        vertices.Add(max);
                        vertices.Add(new Vector3(max.x, min.y, max.z));


                        n = Vector3.right;

                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.up);
                        uvs.Add(Vector2.one);
                        uvs.Add(Vector2.right);
                        break;
                    case NeighbourSide.Back:
                        vertices.Add(new Vector3(max.x, min.y, max.z));
                        vertices.Add(max);
                        vertices.Add(new Vector3(min.x, max.y, max.z));
                        vertices.Add(new Vector3(min.x, min.y, max.z));

                        n = Vector3.forward;

                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.up);
                        uvs.Add(Vector2.one);
                        uvs.Add(Vector2.right);
                        break;
                    case NeighbourSide.Forward:
                        vertices.Add(min);
                        vertices.Add(new Vector3(min.x, max.y, min.z));
                        vertices.Add(new Vector3(max.x, max.y, min.z));
                        vertices.Add(new Vector3(max.x, min.y, min.z));

                        n = Vector3.back;

                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.up);
                        uvs.Add(Vector2.one);
                        uvs.Add(Vector2.right);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("side", side, null);
                }


                normals.Add(n);
                normals.Add(n);
                normals.Add(n);
                normals.Add(n);
                break;
            case SideState.Partial:
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

                foreach (var childCoord in childCoords) {
                    GetVertices(vertices, normals, uvs, side,
                        GetChildBounds(bounds, childCoord.ToIndex()),
                        new OctreeNodeCoordinates(coords, childCoord));
                }
                break;
            case SideState.Full:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        if (!Enum.IsDefined(typeof (ChildIndex), index)) {
            throw new ArgumentOutOfRangeException("index");
        }
        if (index == ChildIndex.Invalid || _children == null) {
            return null;
        }

        return _children[(int) index];
    }

    private OctreeNode<T> SetChild(ChildIndex index, OctreeNode<T> child) {
        AssertNotDeleted();
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

//        _bounds.size
        var childSize = _bounds.extents;

        var childBounds = new Bounds(_bounds.center - Vector3.Scale(childSize, childDirection*0.5f), childSize);

        return childBounds;
    }

    public Bounds GetChildBounds(Bounds originalBounds, ChildIndex childIndex) {
        AssertNotDeleted();
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

//        _bounds.size
        var childSize = originalBounds.extents;

        var childBounds = new Bounds(originalBounds.center - Vector3.Scale(childSize, childDirection*0.5f), childSize);

        return childBounds;
    }

    public OctreeNode<T> AddChild(ChildIndex index) {
        AssertNotDeleted();
        if (!Enum.IsDefined(typeof (ChildIndex), index) || index == ChildIndex.Invalid) {
            throw new ArgumentException("The child index should be between 0 and 8!", "index");
        }

        if (_children == null) {
            _children = new OctreeNode<T>[8];
        }

        if (GetChild(index) != null) {
            throw new ArgumentException("There is already a child at this index", "index");
        }

        _childCount++;
        return SetChild(index, new OctreeNode<T>(GetChildBounds(index), this, index, _depth + 1));
    }

    public void RemoveChild(ChildIndex index) {
        AssertNotDeleted();
        if (_children == null) {
            throw new ArgumentException("The child at that index is already removed!", "index");
        }

        var indexInt = (int) index;

        if (_children[indexInt] != null) {
            _childCount--;
        }
        else {
            throw new ArgumentException("The child at that index is already removed!", "index");
        }

        _children[indexInt].SetDeleted();
        _children[indexInt] = null;

        if (_childCount == 0) {
            _children = null;
        }
    }

    private void SetDeleted() {
        //calling toarray here to force enumeration to flag deleted
        foreach (var octreeNode in BreadthFirst().ToArray()) {
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
                    }
                    else {
                        if (HasItem()) {
                            for (var j = 0; j < 8; j++) {
                                AddChild((ChildIndex) j).SetItem(_item);
                            }
                            RemoveItem();

                            child = GetChild(octreeNodeChildIndex);
                        }
                        else {
                            child = AddChild(octreeNodeChildIndex);
                        }
                    }

                    child.SetItem(item);
                }
                else {
                    //child intersects but is not completely contained by it

                    var child = GetChild(octreeNodeChildIndex);

                    if (child == null) {
                        if (HasItem()) {
                            for (var j = 0; j < 8; j++) {
                                AddChild((ChildIndex) j).SetItem(_item);
                            }
                            RemoveItem();

                            child = GetChild(octreeNodeChildIndex);
                        }
                        else {
                            child = AddChild(octreeNodeChildIndex);
                        }
                    }
                    child.AddBounds(bounds, item, remainingDepth - 1);
                }
            }
        }
    }

    public void SetItem(T item) {
        _item = item;
        _hasItem = true;
    }

    public bool HasItem() {
        return _hasItem;
    }

    public void RemoveItem() {
        _item = default(T);
        _hasItem = false;
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

    private enum SideState {
        Empty,
        Partial,
        Full
    }
}