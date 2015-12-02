using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class OctreeNodeCoordinates : IEnumerable<OctreeChildCoordinates> {
    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _length;
    private int _hashCode;
    private bool _hasHashCode;

    public OctreeNodeCoordinates() {
        _coords = new OctreeChildCoordinates[0];
        _hashCode = 0;
        _length = 0;

        _hasHashCode = true;
    }

    public OctreeNodeCoordinates(OctreeNodeCoordinates parentCoordinates,
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
    }

    public OctreeNodeCoordinates(OctreeChildCoordinates[] coords) {
        _coords = coords;
        _length = _coords.Length;
    }

//    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoordinates> coords) {
//        _coords = coords.ToArray();
//        _length = _coords.Length;
//    }

    public OctreeNodeCoordinates GetParentCoordinates() {
        Assert.IsTrue(_coords.Length > 0, "Cannot get the parent of empty coords");

        var newCoords = new OctreeChildCoordinates[_coords.Length - 1];
        Array.Copy(_coords, newCoords, _coords.Length - 1);

        return new OctreeNodeCoordinates(newCoords);
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

    protected bool Equals(OctreeNodeCoordinates other) {
        return other.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode() {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (_hasHashCode) {
            return _hashCode;
        }

        unchecked
        {
            _hashCode = _coords.Length;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var coord in _coords)
            {
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
        return obj.GetType() == GetType() && Equals((OctreeNodeCoordinates) obj);
    }

    public OctreeNodeCoordinates GetNeighbourCoords(OctreeNode.NeighbourSide side) {
        OctreeChildCoordinates[] newCoords;

        if (_coords.Length > 0) {
            newCoords = new OctreeChildCoordinates[_coords.Length];

            var hasLastCoords = false;
            var lastCoordX = 0;
            var lastCoordY = 0;
            var lastCoordZ = 0;
//            var lastCoords = new OctreeChildCoordinates();

            for (var i = _coords.Length - 1; i >= 0; --i) {
                var coord = _coords[i];

                var currentX = coord.x;
                var currentY = coord.y;
                var currentZ = coord.z;

                if (hasLastCoords) {
                    //let's check the lower coords, if it's out of that bounds then we need to modify ourselves!
//                    var lastCoordX = lastCoords.x;
//                    var lastCoordY = lastCoords.y;
//                    var lastCoordZ = lastCoords.z;

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

                    if (updateLastCoord) {
                        newCoords[i + 1] = new OctreeChildCoordinates(lastCoordX, lastCoordY, lastCoordZ);
                    }
                } else {
                    //final coords!
                    //update coords from the side
                    switch (side) {
                        case OctreeNode.NeighbourSide.Above:
                            currentY += 1;
                            break;
                        case OctreeNode.NeighbourSide.Below:
                            currentY -= 1;
                            break;
                        case OctreeNode.NeighbourSide.Left:
                            currentX -= 1;
                            break;
                        case OctreeNode.NeighbourSide.Right:
                            currentX += 1;
                            break;
                        case OctreeNode.NeighbourSide.Forward:
                            currentZ += 1;
                            break;
                        case OctreeNode.NeighbourSide.Back:
                            currentZ -= 1;
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

            if (hasLastCoords) {
                if (lastCoordX < 0 || lastCoordX > 1 ||
                    lastCoordY < 0 || lastCoordY > 1 ||
                    lastCoordZ < 0 || lastCoordZ > 1) {
                    //invalid coords
                    newCoords = null;
                }
            }
        } else {
            newCoords = null;
        }

        return newCoords == null ? null : new OctreeNodeCoordinates(newCoords);
    }

    public OctreeChildCoordinates GetCoord(int i) {
        return _coords[i];
    }
}