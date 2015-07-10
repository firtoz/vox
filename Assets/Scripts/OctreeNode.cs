using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OctreeNeighbour<T>
{
    private readonly OctreeNode<T> _node;

    public OctreeNeighbour(OctreeNode<T> node)
    {
        _node = node;
    }

    public bool IsEmpty()
    {
        return _node == null;
    }
}


public abstract class OctreeNode
{
    public enum ChildIndex
    {
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

    public enum NeighbourSide
    {
        Above,
        Below,
        Left,
        Right,
        Forward,
        Back
    }

    
}

public class OctreeNodeCoordinates
{
    private readonly OctreeChildCoordinates[] _coords;
    public readonly int Length;

    public OctreeNodeCoordinates()
    {
        _coords = new OctreeChildCoordinates[0];
        Length = _coords.Length;
    }

    public OctreeNodeCoordinates(OctreeNodeCoordinates parentCoordinates, OctreeChildCoordinates octreeChildCoordinates)
    {
        _coords = parentCoordinates._coords.Concat(new[] {octreeChildCoordinates}).ToArray();
        Length = _coords.Length;
    }

    private OctreeNodeCoordinates(OctreeChildCoordinates[] coords)
    {
        _coords = coords;
        Length = _coords.Length;
    }


    public OctreeNodeCoordinates GetNeighbourCoords(OctreeNode.NeighbourSide side)
    {

        OctreeChildCoordinates[] newCoords;

        if (_coords.Length > 0)
        {
            newCoords = new OctreeChildCoordinates[_coords.Length];

            OctreeChildCoordinates? lastCoords = null;

            for (var i = _coords.Length - 1; i >= 0; --i)
            {
                var currentCoords = _coords[i];
                if (lastCoords == null)
                {
                    //final coords!
                    //update coords from the side
                    switch (side)
                    {
                        case OctreeNode.NeighbourSide.Above:
                            currentCoords.y += 1;
                            break;
                        case OctreeNode.NeighbourSide.Below:
                            currentCoords.y -= 1;
                            break;
                        case OctreeNode.NeighbourSide.Left:
                            currentCoords.x -= 1;
                            break;
                        case OctreeNode.NeighbourSide.Right:
                            currentCoords.x += 1;
                            break;
                        case OctreeNode.NeighbourSide.Forward:
                            currentCoords.z += 1;
                            break;
                        case OctreeNode.NeighbourSide.Back:
                            currentCoords.z -= 1;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("side", side, null);
                    }
                }
                else
                {
                    //let's check the lower coords, if it's out of that bounds then we need to modify ourselves!

                    var lastCoordValue = lastCoords.Value;
                    var updateLastCoord = false;

                    if (lastCoordValue.x < 0)
                    {
                        currentCoords.x -= 1;
                        lastCoordValue.x = 1;
                        updateLastCoord = true;
                    }
                    else if (lastCoordValue.x > 1)
                    {
                        currentCoords.x -= 1;
                        lastCoordValue.x = 0;
                        updateLastCoord = true;
                    }

                    if (lastCoordValue.y < 0)
                    {
                        currentCoords.y -= 1;
                        lastCoordValue.y = 1;
                        updateLastCoord = true;
                    }
                    else if (lastCoordValue.y > 1)
                    {
                        currentCoords.y -= 1;
                        lastCoordValue.y = 0;
                        updateLastCoord = true;
                    }

                    if (lastCoordValue.z < 0)
                    {
                        currentCoords.z -= 1;
                        lastCoordValue.z = 1;
                        updateLastCoord = true;
                    }
                    else if (lastCoordValue.z > 1)
                    {
                        currentCoords.z -= 1;
                        lastCoordValue.z = 0;
                        updateLastCoord = true;
                    }

                    if (updateLastCoord)
                    {
                        newCoords[i + 1] = lastCoordValue;
                    }
                }

                newCoords[i] = currentCoords;
                lastCoords = currentCoords;
            }

            if (lastCoords != null)
            {
                var lastCoordValue = lastCoords.Value;

                if (lastCoordValue.x < 0 || lastCoordValue.x > 1 ||
                    lastCoordValue.y < 0 || lastCoordValue.y > 1 ||
                    lastCoordValue.z < 0 || lastCoordValue.z > 1)
                {
                    //invalid coords
                    newCoords = null;
                }
            }
        }
        else
        {
            newCoords = null;
        }

        if (newCoords == null)
        {
            return null;
        }

        return new OctreeNodeCoordinates(newCoords);
    }

    public object this[int i]
    {
        get { return _coords[i]; }
    }
}

public class OctreeNode<T> : OctreeNode
{

    private readonly OctreeNodeCoordinates _nodeCoordinates;
//    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _depth;
    private readonly ChildIndex _indexInParent;
    private readonly OctreeNode<T> _parent;
    private readonly OctreeNode<T> _root;
    private Bounds _bounds;
    private int _childCount;
    private OctreeNode<T>[] _children;
    private T _item;

    public OctreeNode(Bounds bounds) : this(bounds, null, ChildIndex.Invalid, 0)
    {
    }

    public OctreeNode(Bounds bounds, OctreeNode<T> parent, ChildIndex indexInParent, int depth)
    {
        _bounds = bounds;
        _parent = parent;
        if (parent == null)
        {
            _root = this;
            _nodeCoordinates = new OctreeNodeCoordinates();
//            _coords = new OctreeChildCoordinates[0];
        }
        else
        {
            _root = _parent._root;
            _nodeCoordinates = new OctreeNodeCoordinates(parent._nodeCoordinates, OctreeChildCoordinates.FromIndex(indexInParent));
//            _coords = parent._coords.Concat(new[] {OctreeChildCoordinates.FromIndex(indexInParent)}).ToArray();
        }
        _indexInParent = indexInParent;
        _item = default(T);
        _depth = depth;
    }

    

    public OctreeNode<T> GetChildAtCoords(ChildIndex[] coords)
    {
        var current = this;

        foreach (var coord in coords)
        {
            current = current.GetChild(coord);
            if (current == null)
            {
                break;
            }
        }

        return current;
    }

    //above = index < 4
    private static bool IsAbove(ChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt < 4;
    }

    private static ChildIndex AboveIndex(ChildIndex index)
    {
        if (IsAbove(index))
        {
            return ChildIndex.Invalid;
        }

        return (ChildIndex) (((int) index) - 4);
    }

    //below = index >= 4
    private static bool IsBelow(ChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt >= 4;
    }

    private static ChildIndex BelowIndex(ChildIndex index)
    {
        if (IsBelow(index))
        {
            return ChildIndex.Invalid;
        }

        return (ChildIndex) (((int) index) + 4);
    }

    //left = index % 2 == 0
    private static bool IsLeft(ChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%2 == 0;
    }

    private static ChildIndex LeftIndex(ChildIndex index)
    {
        if (IsLeft(index))
        {
            return ChildIndex.Invalid;
        }

        return (ChildIndex) (((int) index) - 1);
    }

    //right = index % 2 == 1
    private static bool IsRight(ChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%2 == 1;
    }

    private static ChildIndex RightIndex(ChildIndex index)
    {
        if (IsRight(index))
        {
            return ChildIndex.Invalid;
        }

        return (ChildIndex) (((int) index) + 1);
    }

    //forward = index % 4 < 2
    private static bool IsForward(ChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%4 < 2;
    }

    private static ChildIndex ForwardIndex(ChildIndex index)
    {
        if (IsForward(index))
        {
            return ChildIndex.Invalid;
        }

        return (ChildIndex) (((int) index) - 2);
    }

    //back = index % 4 >= 2
    private static bool IsBack(ChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%4 >= 2;
    }

    private static ChildIndex BackIndex(ChildIndex index)
    {
        if (IsBack(index))
        {
            return ChildIndex.Invalid;
        }

        return (ChildIndex) (((int) index) + 2);
    }

//    public OctreeChildCoordinates[] GetNeighbourCoords(NeighbourSide side)
//    {
//        return GetNeighbourCoords(_coords, side);
//    }

    

    private SideState GetSideState(NeighbourSide side)
    {
        return SideState.Empty;
//
////        var coords = _coords;
//
//        if (_coords.Length > 0)
//        {
//            var newCoords = new OctreeChildCoordinates[_coords.Length];
//
//            for (var i = _coords.Length - 1; i >= 0; --i)
//            {
//            }
////
////
////            var currentDepth = coords.Length - 1;
////
////            var currentCoord = coords[currentDepth];
////
////            switch (side)
////            {
////                case NeighbourSide.Above:
////                    currentCoord.y -= 1;
////                    break;
////                case NeighbourSide.Below:
////                    currentCoord.y += 1;
////                    break;
////                case NeighbourSide.Left:
////                    currentCoord.x -= 1;
////                    break;
////                case NeighbourSide.Right:
////                    currentCoord.x += 1;
////                    break;
////                case NeighbourSide.Forward:
////                    currentCoord.z += 1;
////                    break;
////                case NeighbourSide.Back:
////                    currentCoord.z -= 1;
////                    break;
////                default:
////                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
////            }
////
////            var parentOffset = new OctreeChildCoordinates(0, 0, 0);
////            if (currentCoord.x > 1)
////            {
////                //move the higher coord's x up
////                parentOffset.x = 1;
////            } else if (currentCoord.x < 0)
////            {
////                //move the higher coord's x down!
////                parentOffset.x = -1;
////            }
////
////            if (currentCoord.y > 1)
////            {
////                //move the higher coord's y up
////                parentOffset.y = 1;
////            } else if (currentCoord.y < 0)
////            {
////                //move the higher coord's y down
////                parentOffset.y = -1;
////            }
////
////            if (currentCoord.z > 1)
////            {
////                parentOffset.z = 1;
////            } else if (currentCoord.z < 0)
////            {
////                parentOffset.z = -1;
////            }
//        }
    }

    //    public OctreeNeighbour<T> GetNeighbour(NeighbourSide side)
    //    {
    //        OctreeNode<T> neighbour = null;
    //
    //        if (_parent != null)
    //        {
    //            var lookInParent = false;
    //
    //            switch (side)
    //            {
    //                case NeighbourSide.Above:
    //                    if (IsAbove(_indexInParent))
    //                    {
    //                        //get from parent's neighbour
    //                        lookInParent = true;
    //                    }
    //                    else
    //                    {
    //                        neighbour = _parent.GetChild(AboveIndex(_indexInParent));
    //                    }
    //                    break;
    //                case NeighbourSide.Below:
    //                    if (IsBelow(_indexInParent))
    //                    {
    //                        //get from parent's neighbour
    //                        lookInParent = true;
    //                    }
    //                    else
    //                    {
    //                        neighbour = _parent.GetChild(BelowIndex(_indexInParent));
    //                    }
    //                    break;
    //                case NeighbourSide.Left:
    //                    if (IsLeft(_indexInParent))
    //                    {
    //                        //get from parent's neighbour
    //                        lookInParent = true;
    //                    }
    //                    else
    //                    {
    //                        neighbour = _parent.GetChild(LeftIndex(_indexInParent));
    //                    }
    //                    break;
    //                case NeighbourSide.Right:
    //                    if (IsRight(_indexInParent))
    //                    {
    //                        //get from parent's neighbour
    //                        lookInParent = true;
    //                    }
    //                    else
    //                    {
    //                        neighbour = _parent.GetChild(RightIndex(_indexInParent));
    //                    }
    //                    break;
    //                case NeighbourSide.Forward:
    //                    if (IsForward(_indexInParent))
    //                    {
    //                        //get from parent's neighbour
    //                        lookInParent = true;
    //                    }
    //                    else
    //                    {
    //                        neighbour = _parent.GetChild(ForwardIndex(_indexInParent));
    //                    }
    //                    break;
    //                case NeighbourSide.Back:
    //                    if (IsBack(_indexInParent))
    //                    {
    //                        //get from parent's neighbour
    //                        lookInParent = true;
    //                    }
    //                    else
    //                    {
    //                        neighbour = _parent.GetChild(BackIndex(_indexInParent));
    //                    }
    //                    break;
    //                default:
    //                    throw new ArgumentOutOfRangeException("side", side, null);
    //            }
    //
    //            if (lookInParent)
    //            {
    //                var parentNeighbour = _parent.GetNeighbour(side);
    //                if (parentNeighbour.IsEmpty())
    //                {
    //                    
    //                }
    //            }
    //        }
    //
    //        return new OctreeNeighbour<T>(neighbour);
    //    }

    public bool IsLeafNode()
    {
        return _childCount == 0;
    }

    public int GetChildCount()
    {
        return _childCount;
    }

    public OctreeNode<T> GetChild(ChildIndex index)
    {
        if (index == ChildIndex.Invalid)
        {
            return null;
        }

        return _children[(int) index];
    }

    private OctreeNode<T> SetChild(ChildIndex index, OctreeNode<T> child)
    {
        _children[(int) index] = child;
        return child;
    }

    public OctreeNode<T> GetChild(int index)
    {
        if (index < 0 || index > 8)
        {
            throw new ArgumentException("The child index should be between 0 and 8!", "index");
        }

        if (_children == null)
        {
            return null;
        }

        return _children[index];
    }

    public IEnumerable<OctreeNode<T>> GetChildren()
    {
        if (_children == null) yield break;

        foreach (var child in _children.Where(child => child != null))
        {
            yield return child;
        }
    }

    public Bounds GetChildBounds(ChildIndex childIndex)
    {
        Vector3 childDirection;

        switch (childIndex)
        {
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

        var myExtents = _bounds.extents;

        var childBounds = new Bounds(_bounds.center - Vector3.Scale(childSize, childDirection*0.5f), childSize);

//        childBounds.center = childDirection;

        return childBounds;
    }

    public OctreeNode<T> AddChild(ChildIndex index)
    {
        if (!Enum.IsDefined(typeof(ChildIndex), index))
        {
            throw new ArgumentException("The child index should be between 0 and 8!", "index");
        }

        if (_children == null)
        {
            _children = new OctreeNode<T>[8];
        }

        if (GetChild(index) != null)
        {
            throw new ArgumentException("There is already a child at this index", "index");
        }

        _childCount++;
        return SetChild(index, new OctreeNode<T>(GetChildBounds(index), this, index, _depth + 1));
    }

    public void RemoveChild(ChildIndex index)
    {
        if (_children == null)
        {
            return;
        }

        var indexInt = (int) index;

        if (_children[indexInt] != null)
        {
            _childCount--;
        }

        if (_childCount == 0)
        {
            _children = null;
        }
        else
        {
            _children[indexInt] = null;
        }
    }

    public void AddBounds(Bounds bounds, int item)
    {
        if (item <= 0)
        {
            return;
        }

        for (var i = 0; i < 8; i++)
        {
            var octreeNodeChildIndex = (ChildIndex) i;
            var childBounds = GetChildBounds(octreeNodeChildIndex);

            //child intersects but is not completely contained by it
            if (childBounds.Intersects(bounds) &&
                !(bounds.Contains(childBounds.min) && bounds.Contains(childBounds.max)))
            {
                var child = GetChild(octreeNodeChildIndex) ?? AddChild(octreeNodeChildIndex);
                child.AddBounds(bounds, item - 1);
            }
        }
    }

    public Bounds GetBounds()
    {
        return _bounds;
    }

    public OctreeNodeCoordinates GetCoords()
    {
        return _nodeCoordinates;
    }

    private enum SideState
    {
        Empty,
        Partial,
        Full
    }
}