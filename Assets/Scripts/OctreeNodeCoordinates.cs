using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class OctreeNodeCoordinates<TItem, TNode, TTree> : IEnumerable<OctreeChildCoordinates>
    where TTree : OctreeBase<TItem, TNode, TTree> 
    where TNode : OctreeNodeBase<TItem, TTree, TNode> {
    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _length;
    private bool _hasHashCode;
    private int _hashCode;
    private readonly TTree _tree;

    public OctreeNodeCoordinates(TTree tree) {
        _coords = new OctreeChildCoordinates[0];
        _hashCode = 0;
        _length = 0;

        _hasHashCode = true;
        _tree = tree;
    }

    public OctreeNodeCoordinates(TTree tree, OctreeNodeCoordinates<TItem, TNode, TTree> parentCoordinates,
        params OctreeChildCoordinates[] furtherChildren) {
        var parentCoords = parentCoordinates._coords;

        _coords = new OctreeChildCoordinates[parentCoords.Length + furtherChildren.Length];

        for (var i = 0; i < parentCoords.Length; ++i) {
            _coords[i] = parentCoords[i];
        }

        for (var i = 0; i < furtherChildren.Length; ++i) {
            _coords[parentCoords.Length + i] = furtherChildren[i];
        }

        _length = _coords.Length;
        _tree = tree;
    }

    public OctreeNodeCoordinates(TTree tree, OctreeChildCoordinates[] coords) {
        _coords = coords;
        _tree = tree;
        _length = _coords.Length;
    }

    public int Length {
        get { return _length; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerator<OctreeChildCoordinates> GetEnumerator() {
        return _coords.AsEnumerable().GetEnumerator();
    }

//    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoordinates> coords) {
//        _coords = coords.ToArray();
//        _length = _coords.Length;
//    }

    public OctreeNodeCoordinates<TItem, TNode, TTree> GetParentCoordinates() {
        Assert.IsTrue(_coords.Length > 0, "Cannot get the parent of empty coords");

        var newCoords = new OctreeChildCoordinates[_coords.Length - 1];
        Array.Copy(_coords, newCoords, _coords.Length - 1);

        return new OctreeNodeCoordinates<TItem, TNode, TTree>(_tree, newCoords);
    }

    protected bool Equals(OctreeNodeCoordinates<TItem, TNode, TTree> other) {
        return other.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode() {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (_hasHashCode) {
            return _hashCode;
        }

        unchecked {
            _hashCode = _coords.Length;

            if (_tree != null) {
                _hashCode = _hashCode * 17 + _tree.GetHashCode();
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var coord in _coords) {
                _hashCode = _hashCode * 31 + coord.GetHashCode();
            }
        }

        _hasHashCode = true;

        return _hashCode;
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    public override string ToString() {
        var s = "[ ";
        for (var i = 0; i < _length; i++) {
            var coord = _coords[i];

            if (i > 0) {
                s += ", ";
            }

            s += coord;
        }

        return s + " ]";
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }
        if (ReferenceEquals(this, obj)) {
            return true;
        }
        return obj.GetType() == GetType() && Equals((OctreeNodeCoordinates< TItem, TNode, TTree>) obj);
    }

    public OctreeNodeCoordinates<TItem, TNode, TTree> GetNeighbourCoords(NeighbourSide side) {
        OctreeChildCoordinates[] newCoords;

        var tree = _tree;

        if (_coords.Length > 0) {
            newCoords = new OctreeChildCoordinates[_coords.Length];

            var hasLastCoords = false;
            var lastCoordX = 0;
            var lastCoordY = 0;
            var lastCoordZ = 0;

            for (var i = _coords.Length - 1; i >= 0; --i) {
                var coord = _coords[i];

                var currentX = coord.x;
                var currentY = coord.y;
                var currentZ = coord.z;

                if (hasLastCoords) {
                    //let's check the lower coords, if it's out of that bounds then we need to modify ourselves!
                    var lastCoordUpdated = UpdateLastCoord(
                        ref lastCoordX, ref currentX,
                        ref lastCoordY, ref currentY,
                        ref lastCoordZ, ref currentZ);

                    if (lastCoordUpdated) {
                        newCoords[i + 1] = new OctreeChildCoordinates(lastCoordX, lastCoordY, lastCoordZ);
                    }
                } else {
                    //final coords!
                    //update coords from the side
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

                var newCoord = new OctreeChildCoordinates(currentX, currentY, currentZ);
                newCoords[i] = newCoord;

                lastCoordX = currentX;
                lastCoordY = currentY;
                lastCoordZ = currentZ;
                hasLastCoords = true;
            }

            // we're at the end now

            if (hasLastCoords) {
                if (lastCoordX < 0 || lastCoordX > 1 ||
                    lastCoordY < 0 || lastCoordY > 1 ||
                    lastCoordZ < 0 || lastCoordZ > 1) {
                    //invalid coords, out of bounds, pick neighbour tree

                    var currentX = lastCoordX;
                    var currentY = lastCoordY;
                    var currentZ = lastCoordZ;

                    UpdateLastCoord(ref lastCoordX, ref currentX,
                        ref lastCoordY, ref currentY,
                        ref lastCoordZ, ref currentZ);

                    newCoords[0] = new OctreeChildCoordinates(lastCoordX, lastCoordY, lastCoordZ);
                    if (_tree == null) {
                        tree = null;
                    } else {
                        tree = _tree.GetOrCreateNeighbour(side);
                    }
                }
            }
        } else {
            newCoords = null;
        }

        return newCoords == null ? null : new OctreeNodeCoordinates<TItem, TNode, TTree>(tree, newCoords);
    }

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

    public OctreeChildCoordinates GetCoord(int i) {
        return _coords[i];
    }

    public TTree GetTree() {
        return _tree;
    }
}