using UnityEngine;

namespace Assets.Scripts
{
    public class VoxelTree : Octree<int> {
        public VoxelTree(Vector3 center, Vector3 size) : base(new Bounds(center, size)) {
            
        }

        override protected bool IsSameMesh(int a, int b) {
            return a == b;
        }
    }
}
