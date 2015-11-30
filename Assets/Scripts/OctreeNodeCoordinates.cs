using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class OctreeNodeCoordinates : IEnumerable<OctreeChildCoordinates> {
    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _length;

    public OctreeNodeCoordinates() {
        _coords = new OctreeChildCoordinates[0];
        _length = _coords.Length;
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

    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoordinates> coords) {
        _coords = coords.ToArray();
        _length = _coords.Length;
    }

    public OctreeNodeCoordinates GetParentCoordinates() {
        Assert.IsTrue(_coords.Length > 0, "Cannot get the parent of empty coords");

        var newCoords = new OctreeChildCoordinates[_coords.Length - 1];
        Array.Copy(_coords, newCoords, _coords.Length - 1);

        return new OctreeNodeCoordinates(newCoords);
    }

    public int Length {
        get { return _length; }
    }

    public OctreeChildCoordinates this[int i] {
        get { return _coords[i]; }
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
        unchecked {
            return _coords.Aggregate(_coords.Length, (current, coord) => current * 31 + coord.GetHashCode());
        }
    }

    public override string ToString() {
        var s = "[ ";
        for (var i = 0; i < _length; i++) {
            var coord = this[i];

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

            OctreeChildCoordinates? lastCoords = null;

            for (var i = _coords.Length - 1; i >= 0; --i) {
                var coord = _coords[i];

                var currentX = coord.x;
                var currentY = coord.y;
                var currentZ = coord.z;

                if (lastCoords != null) {
                    //let's check the lower coords, if it's out of that bounds then we need to modify ourselves!
                    var lastCoordValue = lastCoords.Value;

                    var lastCoordX = lastCoordValue.x;
                    var lastCoordY = lastCoordValue.y;
                    var lastCoordZ = lastCoordValue.z;

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

                newCoords[i] = new OctreeChildCoordinates(currentX, currentY, currentZ);
                lastCoords = newCoords[i];
            }

            if (lastCoords != null) {
                var lastCoordValue = lastCoords.Value;

                if (lastCoordValue.x < 0 || lastCoordValue.x > 1 ||
                    lastCoordValue.y < 0 || lastCoordValue.y > 1 ||
                    lastCoordValue.z < 0 || lastCoordValue.z > 1) {
                    //invalid coords
                    newCoords = null;
                }
            }
        } else {
            newCoords = null;
        }

        if (newCoords == null) {
            return null;
        }

        return new OctreeNodeCoordinates(newCoords);
    }
}