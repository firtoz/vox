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

public struct OctreeCoordinate
{
    public int x;
    public int y;
    public int z;

    public OctreeCoordinate(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString()
    {
        return string.Format("[{0},{1},{2}]", x, y, z);
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        //       
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237  
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        // TODO: write your implementation of Equals() here

        var otherCoordinates = (OctreeCoordinate) obj;

        return otherCoordinates.x == x && otherCoordinates.y == y && otherCoordinates.z == z;
    }

// override object.GetHashCode
    public override int GetHashCode()
    {
        // TODO: write your implementation of GetHashCode() here
        throw new NotImplementedException();
        return base.GetHashCode();
    }
}

public enum OctreeNodeChildIndex
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

public enum OctreeNodeNeighbourSide
{
    Above,
    Below,
    Left,
    Right,
    Forward,
    Back
}

public class OctreeNode<T>
{

    private readonly OctreeCoordinate[] _coords;
    private readonly int _depth;
    private readonly OctreeNodeChildIndex _indexInParent;
    private readonly OctreeNode<T> _parent;
    private readonly OctreeNode<T> _root;
    private Bounds _bounds;
    private int _childCount;
    private OctreeNode<T>[] _children;
    private T _item;

    public OctreeNode(Bounds bounds) : this(bounds, null, OctreeNodeChildIndex.Invalid, 0)
    {
    }

    public OctreeNode(Bounds bounds, OctreeNode<T> parent, OctreeNodeChildIndex indexInParent, int depth)
    {
        _bounds = bounds;
        _parent = parent;
        if (parent == null)
        {
            _root = this;
            _coords = new OctreeCoordinate[0];
        }
        else
        {
            _root = _parent._root;
            _coords = parent._coords.Concat(new[] {GetCoordinate(indexInParent)}).ToArray();
        }
        _indexInParent = indexInParent;
        _item = default(T);
        _depth = depth;
    }

    private OctreeCoordinate GetCoordinate(OctreeNodeChildIndex index)
    {
        switch (index)
        {
            case OctreeNodeChildIndex.TopFwdLeft:
                return new OctreeCoordinate(0, 1, 1);
            case OctreeNodeChildIndex.TopFwdRight:
                return new OctreeCoordinate(1, 1, 1);
            case OctreeNodeChildIndex.TopBackLeft:
                return new OctreeCoordinate(0, 1, 0);
            case OctreeNodeChildIndex.TopBackRight:
                return new OctreeCoordinate(1, 1, 0);
            case OctreeNodeChildIndex.BotFwdLeft:
                return new OctreeCoordinate(0, 0, 1);
            case OctreeNodeChildIndex.BotFwdRight:
                return new OctreeCoordinate(1, 0, 1);
            case OctreeNodeChildIndex.BotBackLeft:
                return new OctreeCoordinate(0, 0, 0);
            case OctreeNodeChildIndex.BotBackRight:
                return new OctreeCoordinate(1, 0, 0);
            default:
                throw new ArgumentOutOfRangeException("index", index, null);
        }
    }

    public OctreeNode<T> GetChildAtCoords(OctreeNodeChildIndex[] coords)
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
    private static bool IsAbove(OctreeNodeChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt < 4;
    }

    private static OctreeNodeChildIndex AboveIndex(OctreeNodeChildIndex index)
    {
        if (IsAbove(index))
        {
            return OctreeNodeChildIndex.Invalid;
        }

        return (OctreeNodeChildIndex) (((int) index) - 4);
    }

    //below = index >= 4
    private static bool IsBelow(OctreeNodeChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt >= 4;
    }

    private static OctreeNodeChildIndex BelowIndex(OctreeNodeChildIndex index)
    {
        if (IsBelow(index))
        {
            return OctreeNodeChildIndex.Invalid;
        }

        return (OctreeNodeChildIndex) (((int) index) + 4);
    }

    //left = index % 2 == 0
    private static bool IsLeft(OctreeNodeChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%2 == 0;
    }

    private static OctreeNodeChildIndex LeftIndex(OctreeNodeChildIndex index)
    {
        if (IsLeft(index))
        {
            return OctreeNodeChildIndex.Invalid;
        }

        return (OctreeNodeChildIndex) (((int) index) - 1);
    }

    //right = index % 2 == 1
    private static bool IsRight(OctreeNodeChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%2 == 1;
    }

    private static OctreeNodeChildIndex RightIndex(OctreeNodeChildIndex index)
    {
        if (IsRight(index))
        {
            return OctreeNodeChildIndex.Invalid;
        }

        return (OctreeNodeChildIndex) (((int) index) + 1);
    }

    //forward = index % 4 < 2
    private static bool IsForward(OctreeNodeChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%4 < 2;
    }

    private static OctreeNodeChildIndex ForwardIndex(OctreeNodeChildIndex index)
    {
        if (IsForward(index))
        {
            return OctreeNodeChildIndex.Invalid;
        }

        return (OctreeNodeChildIndex) (((int) index) - 2);
    }

    //back = index % 4 >= 2
    private static bool IsBack(OctreeNodeChildIndex index)
    {
        var indexInt = (int) index;

        return indexInt%4 >= 2;
    }

    private static OctreeNodeChildIndex BackIndex(OctreeNodeChildIndex index)
    {
        if (IsBack(index))
        {
            return OctreeNodeChildIndex.Invalid;
        }

        return (OctreeNodeChildIndex) (((int) index) + 2);
    }

    public OctreeCoordinate[] GetNeighbourCoords(OctreeNodeNeighbourSide side)
    {
        return GetNeighbourCoords(_coords, side);
    }

    public static OctreeCoordinate[] GetNeighbourCoords(OctreeCoordinate[] coords, OctreeNodeNeighbourSide side)
    {
        OctreeCoordinate[] newCoords;

        if (coords.Length > 0)
        {
            newCoords = new OctreeCoordinate[coords.Length];

            OctreeCoordinate? lastCoords = null;

            for (var i = coords.Length - 1; i >= 0; --i)
            {
                var currentCoords = coords[i];
                if (lastCoords == null)
                {
                    //final coords!
                    //update coords from the side
                    switch (side)
                    {
                        case OctreeNodeNeighbourSide.Above:
                            currentCoords.y += 1;
                            break;
                        case OctreeNodeNeighbourSide.Below:
                            currentCoords.y -= 1;
                            break;
                        case OctreeNodeNeighbourSide.Left:
                            currentCoords.x -= 1;
                            break;
                        case OctreeNodeNeighbourSide.Right:
                            currentCoords.x += 1;
                            break;
                        case OctreeNodeNeighbourSide.Forward:
                            currentCoords.z += 1;
                            break;
                        case OctreeNodeNeighbourSide.Back:
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

        return newCoords;
    }

    private SideState GetSideState(OctreeNodeNeighbourSide side)
    {
        return SideState.Empty;
//
////        var coords = _coords;
//
//        if (_coords.Length > 0)
//        {
//            var newCoords = new OctreeCoordinate[_coords.Length];
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
////                case OctreeNodeNeighbourSide.Above:
////                    currentCoord.y -= 1;
////                    break;
////                case OctreeNodeNeighbourSide.Below:
////                    currentCoord.y += 1;
////                    break;
////                case OctreeNodeNeighbourSide.Left:
////                    currentCoord.x -= 1;
////                    break;
////                case OctreeNodeNeighbourSide.Right:
////                    currentCoord.x += 1;
////                    break;
////                case OctreeNodeNeighbourSide.Forward:
////                    currentCoord.z += 1;
////                    break;
////                case OctreeNodeNeighbourSide.Back:
////                    currentCoord.z -= 1;
////                    break;
////                default:
////                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
////            }
////
////            var parentOffset = new OctreeCoordinate(0, 0, 0);
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

    //    public OctreeNeighbour<T> GetNeighbour(OctreeNodeNeighbourSide side)
    //    {
    //        OctreeNode<T> neighbour = null;
    //
    //        if (_parent != null)
    //        {
    //            var lookInParent = false;
    //
    //            switch (side)
    //            {
    //                case OctreeNodeNeighbourSide.Above:
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
    //                case OctreeNodeNeighbourSide.Below:
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
    //                case OctreeNodeNeighbourSide.Left:
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
    //                case OctreeNodeNeighbourSide.Right:
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
    //                case OctreeNodeNeighbourSide.Forward:
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
    //                case OctreeNodeNeighbourSide.Back:
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

    public OctreeNode<T> GetChild(OctreeNodeChildIndex index)
    {
        if (index == OctreeNodeChildIndex.Invalid)
        {
            return null;
        }

        return _children[(int) index];
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

    public Bounds GetChildBounds(int i)
    {
        var childIndex = (OctreeNodeChildIndex) i;

        Vector3 childDirection;

        switch (childIndex)
        {
            case OctreeNodeChildIndex.TopFwdLeft:
                childDirection = new Vector3(-1, -1, 1);
                break;
            case OctreeNodeChildIndex.TopFwdRight:
                childDirection = new Vector3(1, -1, 1);
                break;
            case OctreeNodeChildIndex.TopBackLeft:
                childDirection = new Vector3(-1, -1, -1);
                break;
            case OctreeNodeChildIndex.TopBackRight:
                childDirection = new Vector3(1, -1, -1);
                break;
            case OctreeNodeChildIndex.BotFwdLeft:
                childDirection = new Vector3(-1, 1, 1);
                break;
            case OctreeNodeChildIndex.BotFwdRight:
                childDirection = new Vector3(1, 1, 1);
                break;
            case OctreeNodeChildIndex.BotBackLeft:
                childDirection = new Vector3(-1, 1, -1);
                break;
            case OctreeNodeChildIndex.BotBackRight:
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

    public OctreeNode<T> AddChild(int index)
    {
        if (index < 0 || index > 8)
        {
            throw new ArgumentException("The child index should be between 0 and 8!", "index");
        }

        if (_children == null)
        {
            _children = new OctreeNode<T>[8];
        }

        if (_children[index] != null)
        {
            throw new ArgumentException("There is already a child at this index", "index");
        }

        _childCount++;
        _children[index] = new OctreeNode<T>(GetChildBounds(index), this, (OctreeNodeChildIndex) index, _depth + 1);
        return _children[index];
    }

    public void RemoveChild(OctreeNodeChildIndex index)
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
            var childBounds = GetChildBounds(i);

            //child intersects but is not completely contained by it
            if (childBounds.Intersects(bounds) &&
                !(bounds.Contains(childBounds.min) && bounds.Contains(childBounds.max)))
            {
                var child = GetChild(i) ?? AddChild(i);
                child.AddBounds(bounds, item - 1);
            }
        }
    }

    public Bounds GetBounds()
    {
        return _bounds;
    }

    public OctreeCoordinate[] GetCoords()
    {
        return _coords;
    }

    private enum SideState
    {
        Empty,
        Partial,
        Full
    }
}