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

        public void SetMaterial(int index, Material material) {
            _materials[index] = material;
        }
    }
}
