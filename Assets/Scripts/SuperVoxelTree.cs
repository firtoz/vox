using UnityEngine;

public class SuperVoxelTree : OctreeBase<VoxelTree, SuperVoxelTree.Node, SuperVoxelTree, SuperVoxelTree.Node.Coords> {
    internal SuperVoxelTree(Bounds bounds) : base(CreateRootNode, bounds) {}

    public override Node ConstructNode(Bounds bounds, Node parent, OctreeNode.ChildIndex indexInParent, int depth) {
        return new Node(bounds, parent, indexInParent, depth, this);
    }

    private static Node CreateRootNode(SuperVoxelTree self, Bounds bounds) {
        return new Node(bounds, self);
    }

    public class Node : OctreeNodeBase<VoxelTree, SuperVoxelTree, Node, Node.Coords> {
        public Node(Bounds bounds, SuperVoxelTree tree) : base(bounds, tree) {}

        public Node(Bounds bounds, Node parent, ChildIndex indexInParent, int depth, SuperVoxelTree ocTree)
            : base(bounds, parent, indexInParent, depth, ocTree) {}

        public class Coords : Coordinates {
            public Coords() {}

            public Coords(SuperVoxelTree tree) : base(tree) {}

            public Coords(SuperVoxelTree tree, Coords parentCoordinates, params OctreeChildCoordinates[] furtherChildren)
                : base(tree, parentCoordinates, furtherChildren) {}

            public Coords(SuperVoxelTree tree, OctreeChildCoordinates[] coords) : base(tree, coords) {}

            public override Coords Construct(SuperVoxelTree tree) {
                return new Coords(tree);
            }

            public override Coords Construct(SuperVoxelTree tree, OctreeChildCoordinates[] newCoords) {
                return new Coords(tree, newCoords);
            }

            public override Coords Construct(SuperVoxelTree tree, Coords nodeCoordinates,
                OctreeChildCoordinates octreeChildCoordinates) {
                return new Coords(tree, nodeCoordinates, octreeChildCoordinates);
            }
        }
    }
}