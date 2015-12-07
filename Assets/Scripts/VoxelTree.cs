using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class VoxelTree : Octree<int> {
        public VoxelTree(Vector3 center, Vector3 size) : base(new Bounds(center, size)) {
            
        }

        protected override int GetItemMeshId(int item) {
            return item;
        }

        private readonly Dictionary<int, Material> _materials = new Dictionary<int, Material>();
         
        protected override Material GetMeshMaterial(int meshId) {
            if (_materials.ContainsKey(meshId)) {
                return _materials[meshId];
            }

            return new Material(Shader.Find("Standard")) {
                hideFlags = HideFlags.DontSave
            };
        }

        protected override Octree<int> CreateNeighbour(NeighbourSide side) {
            var rootBounds = GetRoot().GetBounds();

            var center = rootBounds.center;
            var size = rootBounds.size;

            switch (side) {
                case NeighbourSide.Above:
                    center += Vector3.up * size.y;
                    break;
                case NeighbourSide.Below:
                    center += Vector3.down * size.y;
                    break;
                case NeighbourSide.Right:
                    center += Vector3.right* size.x;
                    break;
                case NeighbourSide.Left:
                    center += Vector3.left * size.x;
                    break;
                case NeighbourSide.Back:
                    center += Vector3.back * size.z;
                    break;
                case NeighbourSide.Forward:
                    center += Vector3.forward * size.z;
                    break;
                case NeighbourSide.Invalid:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("side", side, null);
            }

            var neighbour = new VoxelTree(center, size);

            foreach (var material in _materials) {
                neighbour.SetMaterial(material.Key, material.Value);
            }

            return neighbour;
        }

        public void SetMaterial(int index, Material material) {
            _materials[index] = material;
        }
    }
}
