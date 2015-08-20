using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OctreeNodeCoordinates : IEnumerable<OctreeChildCoordinates>
{
    protected bool Equals(OctreeNodeCoordinates other)
    {
        return Length == other.Length && _coords.SequenceEqual(other._coords);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((_coords != null ? _coords.GetHashCode() : 0)*397) ^ Length;
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private readonly OctreeChildCoordinates[] _coords;
    private readonly int _length;

    public int Length
    {
        get { return _length; }
    }

    public OctreeNodeCoordinates()
    {
        _coords = new OctreeChildCoordinates[0];
        _length = _coords.Length;
    }

    public OctreeNodeCoordinates(OctreeNodeCoordinates parentCoordinates, params OctreeChildCoordinates[] furtherChildren)
    {
        _coords = parentCoordinates._coords.Concat(furtherChildren).ToArray();
        _length = _coords.Length;
    }

    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoordinates> coords)
    {
        _coords = coords.ToArray();
        _length = _coords.Length;
    }

    public IEnumerator<OctreeChildCoordinates> GetEnumerator()
    {
        return _coords.AsEnumerable().GetEnumerator();
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((OctreeNodeCoordinates) obj);
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
                var currentCoords = new OctreeChildCoordinates(_coords[i]);
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
                        currentCoords.x += 1;
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
                        currentCoords.y += 1;
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
                        currentCoords.z += 1;
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

    public OctreeChildCoordinates this[int i]
    {
        get { return _coords[i]; }
    }
}