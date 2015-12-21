using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public abstract partial class OctreeNodeBase<TItem, TTree, TNode, TCoords>
    where TTree : OctreeBase<TItem, TNode, TTree, TCoords>
    where TNode : OctreeNodeBase<TItem, TTree, TNode, TCoords>
    where TCoords : OctreeNodeBase<TItem, TTree, TNode, TCoords>.Coordinates, new() {
    private static readonly TCoords StaticCoordsInstance = new TCoords();

    public abstract class Coordinates : IEnumerable<OctreeChildCoordinates> {
        private readonly int _length;
        protected readonly OctreeChildCoordinates[] coords;
        protected readonly TTree tree;

        private bool _hasHashCode;
        private int _hashCode;

        protected Coordinates() {}

        protected Coordinates(TTree tree) {
            coords = new OctreeChildCoordinates[0];
            _hashCode = 0;
            _length = 0;

            _hasHashCode = true;
            this.tree = tree;
        }

        protected Coordinates(TTree tree, TCoords parentCoordinates,
            params OctreeChildCoordinates[] furtherChildren) {
            var parentCoords = parentCoordinates.coords;

            coords = new OctreeChildCoordinates[parentCoords.Length + furtherChildren.Length];

            for (var i = 0; i < parentCoords.Length; ++i) {
                coords[i] = parentCoords[i];
            }

            for (var i = 0; i < furtherChildren.Length; ++i) {
                coords[parentCoords.Length + i] = furtherChildren[i];
            }

            _length = coords.Length;
            this.tree = tree;
        }

        protected Coordinates(TTree tree, OctreeChildCoordinates[] coords) {
            this.coords = coords;
            this.tree = tree;
            _length = this.coords.Length;
        }

        public int Length {
            get { return _length; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<OctreeChildCoordinates> GetEnumerator() {
            return coords.AsEnumerable().GetEnumerator();
        }

        //    public OctreeNodeCoordinates(IEnumerable<OctreeChildCoordinates> coords) {
        //        coords = coords.ToArray();
        //        _length = coords.Length;
        //    }

        public TCoords GetParentCoordinates() {
            Assert.IsTrue(coords.Length > 0, "Cannot get the parent of empty coords");

            var newCoords = new OctreeChildCoordinates[coords.Length - 1];
            Array.Copy(coords, newCoords, coords.Length - 1);

            return StaticCoordsInstance.Construct(tree, newCoords);
        }

        public abstract TCoords Construct(TTree tree);
        public abstract TCoords Construct(TTree tree, OctreeChildCoordinates[] newCoords);

        public abstract TCoords Construct(TTree tree, TCoords nodeCoordinates,
            OctreeChildCoordinates octreeChildCoordinates);

        protected bool Equals(TCoords other) {
            return other.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode() {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            if (_hasHashCode) {
                return _hashCode;
            }

            unchecked {
                _hashCode = coords.Length;

                if (tree != null) {
                    _hashCode = _hashCode * 17 + tree.GetHashCode();
                }

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var coord in coords) {
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
                var coord = coords[i];

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
            return obj.GetType() == GetType() && Equals((TCoords) obj);
        }

        public OctreeChildCoordinates GetCoord(int i) {
            return coords[i];
        }

        public TTree GetTree() {
            return tree;
        }
    }
}