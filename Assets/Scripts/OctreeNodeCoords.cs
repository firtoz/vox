using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public sealed class Coords : IEnumerable<OctreeChildCoords> {
    private readonly OctreeChildCoords[] _coords;
    private readonly int _length;

    private bool _hasHashCode;
    private int _hashCode;

    public Coords() {
        _coords = new OctreeChildCoords[0];
        _hashCode = 0;
        _length = 0;

        _hasHashCode = true;
    }

    public Coords(Coords parentCoords,
        params OctreeChildCoords[] furtherChildren) {
        var parentLength = parentCoords.Length;

        _coords = new OctreeChildCoords[parentLength + furtherChildren.Length];

        for (var i = 0; i < parentLength; ++i) {
            _coords[i] = parentCoords[i];
        }

        for (var i = 0; i < furtherChildren.Length; ++i) {
            _coords[parentCoords.Length + i] = furtherChildren[i];
        }

        _length = _coords.Length;
    }

    public Coords(OctreeChildCoords[] coords) {
        _coords = coords;
        _length = _coords.Length;
    }

    private OctreeChildCoords this[int i] {
        get { return _coords[i]; }
    }

    public int Length {
        get { return _length; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerator<OctreeChildCoords> GetEnumerator() {
        return _coords.AsEnumerable().GetEnumerator();
    }

    //    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoords> _coords) {
    //        _coords = _coords.ToArray();
    //        _length = _coords.Length;
    //    }

    public Coords GetParentCoords() {
        Assert.IsTrue(_coords.Length > 0, "Cannot get the parent of empty _coords");

        var newCoords = new OctreeChildCoords[_coords.Length - 1];
        Array.Copy(_coords, newCoords, _coords.Length - 1);

        return new Coords(newCoords);
    }

    public bool Equals(Coords other) {
        return other != null && other.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode() {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (_hasHashCode) {
            return _hashCode;
        }

        unchecked {
            _hashCode = _coords.Length;

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
        return obj.GetType() == GetType() && Equals((Coords) obj);
    }

    public OctreeChildCoords GetCoord(int i) {
        return _coords[i];
    }
}