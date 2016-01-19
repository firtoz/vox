using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public abstract class OctreeNodeBase {
    public sealed class Coordinates : IEnumerable<OctreeChildCoordinates> {
        private readonly OctreeChildCoordinates[] _coords;
        private readonly int _length;

        private bool _hasHashCode;
        private int _hashCode;

        public Coordinates() {
            _coords = new OctreeChildCoordinates[0];
            _hashCode = 0;
            _length = 0;

            _hasHashCode = true;
        }

        public Coordinates(Coordinates parentCoordinates,
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

        public Coordinates(OctreeChildCoordinates[] coords) {
            _coords = coords;
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

        //    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoordinates> _coords) {
        //        _coords = _coords.ToArray();
        //        _length = _coords.Length;
        //    }

        public Coordinates GetParentCoordinates() {
            Assert.IsTrue(_coords.Length > 0, "Cannot get the parent of empty _coords");

            var newCoords = new OctreeChildCoordinates[_coords.Length - 1];
            Array.Copy(_coords, newCoords, _coords.Length - 1);

            return new Coordinates(newCoords);
        }

        public bool Equals(Coordinates other) {
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
            return obj.GetType() == GetType() && Equals((Coordinates) obj);
        }

        public OctreeChildCoordinates GetCoord(int i) {
            return _coords[i];
        }
    }
}