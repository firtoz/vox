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
            var neighbourBounds = GetNeighbourBounds(side);

            var neighbour = new VoxelTree(neighbourBounds.center, neighbourBounds.size);

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
