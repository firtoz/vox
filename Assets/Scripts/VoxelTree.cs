using UnityEngine;

namespace Assets.Scripts
{
    public class VoxelTree : Octree<int> {
        public VoxelTree(Vector3 center, Vector3 size) : base(new Bounds(center, size)) {
            
        }

        override protected int GetItemMeshId(int item) {
            return item;
        }

        protected override Material GetMeshMaterial(int meshId) {
            return new Material(Shader.Find("Standard")) {
                hideFlags = HideFlags.DontSave
            };
        }
    }
}
