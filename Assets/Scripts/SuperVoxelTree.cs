using UnityEngine;

public class SuperVoxelTree : OctreeBase<VoxelTree, SuperVoxelTree.Node, SuperVoxelTree> {
    internal SuperVoxelTree(Bounds bounds) : base(CreateRootNode, bounds) {}

    public override Node ConstructNode(Bounds bounds, Node parent, OctreeNode.ChildIndex indexInParent, int depth) {
        return new Node(bounds, parent, indexInParent, depth, this);
    }

    private static Node CreateRootNode(SuperVoxelTree self, Bounds bounds) {
        return new Node(bounds, self);
    }

    public class Node : OctreeNodeBase<VoxelTree, SuperVoxelTree, Node> {
        public Node(Bounds bounds, SuperVoxelTree tree) : base(bounds, tree) {}

        public Node(Bounds bounds, Node parent, ChildIndex indexInParent, int depth, SuperVoxelTree ocTree)
            : base(bounds, parent, indexInParent, depth, ocTree) {}
        
    }
}