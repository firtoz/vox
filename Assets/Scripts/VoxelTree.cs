using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class VoxelTree : OctreeBase<int, VoxelNode, VoxelTree> {
        public VoxelTree(Vector3 center, Vector3 size) : base(RootConstructor, new Bounds(center, size)) {
            
        }

        private static VoxelNode RootConstructor(VoxelTree self, Bounds bounds) {
            return new VoxelNode(bounds, self);
        }

        public override VoxelNode ConstructNode(Bounds bounds, VoxelNode parent, OctreeNode.ChildIndex indexInParent, int depth) {
            return new VoxelNode( bounds, parent, indexInParent, depth, this);
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

        protected override VoxelTree CreateNeighbour(NeighbourSide side) {
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
