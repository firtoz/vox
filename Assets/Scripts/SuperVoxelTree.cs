using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SuperVoxelItem {
    public SuperVoxelTree childTree;
    public VoxelTree voxelTree;
}

public class SuperVoxelTree : OctreeBase<SuperVoxelItem, SuperVoxelTree.Node, SuperVoxelTree> {
    private OctreeNode.ChildIndex _indexInParent;
    private SuperVoxelTree _parent;


    public SuperVoxelTree(Bounds bounds, VoxelTree voxelTree) : base(CreateRootNode, bounds) {
        if (voxelTree == null) {
            throw new ArgumentException("The voxelTree argument cannot be null.", "voxelTree");
        }

        GetRoot().SetItem(new SuperVoxelItem {
            childTree = this,
            voxelTree = voxelTree
        });

        _parent = null;
    }

    private SuperVoxelTree(Bounds bounds) : base(CreateRootNode, bounds) {
        GetRoot().SetItem(new SuperVoxelItem {
            childTree = this,
            voxelTree = null
        });

        _parent = null;
    }

    public override Node ConstructNode(Bounds bounds, Node parent, OctreeNode.ChildIndex indexInParent, int depth) {
        return new Node(bounds, parent, indexInParent, depth, this);
    }

    private static Node CreateRootNode(SuperVoxelTree self, Bounds bounds) {
        return new Node(bounds, self);
    }

    public SuperVoxelTree GetOrCreateNeighbour(NeighbourSide side, bool readOnly) {
        if (_parent == null) {
            if (readOnly) {
                return null;
            }
            AssignIndexInParent(side);

            _parent = new SuperVoxelTree(CreateParentBounds());
        }

//        var neighbourCoords =
//            VoxelTree.GetNeighbourCoords(new Coords(new[] {OctreeChildCoords.FromIndex(_indexInParent)}), side);

//        if (neighbourCoords == null) {
        // uh oh, gotta go further

        // cases:
        // length 0: not gonna happen.
        // not at edge, if parent was null, we get neighbour coords result.
        // at edge: ??

        var neighbourCoordsResult = GetNeighbourCoordsInfinite(_parent,
            new Coords(new[] {OctreeChildCoords.FromIndex(_indexInParent)}), side, GetOrCreateParentNeighbour);

        var neighbourParent = neighbourCoordsResult.tree;
        var neighbourCoords = neighbourCoordsResult.coordsResult;

        var parentRoot = neighbourParent.GetRoot();

        var neighbour = parentRoot.GetChildAtCoords(neighbourCoords);

        if (neighbour == null) {
            if (readOnly) {
                return null;
            }

            neighbour = parentRoot.AddRecursive(neighbourCoords);
            CreateNewNeighbourSuperVoxelTree(neighbour);
        }

        return neighbour.GetItem().childTree;

        //        }

        //        var neighbour = _parent.GetRoot().GetChildAtCoords(neighbourCoords) ??
        //                        _parent.GetRoot().AddRecursive(neighbourCoords);

        //        return neighbour.GetItem().childTree;
    }

    private SuperVoxelTree GetOrCreateParentNeighbour(NeighbourSide side, bool readOnly) {
        Assert.IsNotNull(_parent, "Trying to get a neighbour for a parent that does not exist");

        return _parent.GetOrCreateNeighbour(side, readOnly);
    }

    private void CreateNewNeighbourSuperVoxelTree(Node ownerNode) {
        var myRoot = GetRoot();

        var myRootBounds = myRoot.GetBounds();

        var neighbourBounds = ownerNode.GetBounds();

        Assert.IsTrue(neighbourBounds.extents == myRootBounds.extents);

        var myItem = myRoot.GetItem();

        SuperVoxelItem neighbourSuperVoxelItem;

        var myVoxelTree = myItem.voxelTree;
        if (myVoxelTree != null) {
            var newNeighbourSuperVoxelTree = new SuperVoxelTree(neighbourBounds,
                new VoxelTree(neighbourBounds.center, neighbourBounds.size, false)) {
                    _indexInParent = ownerNode.GetIndexInParent(),
                    _parent = ownerNode.GetTree()
                };

            neighbourSuperVoxelItem = newNeighbourSuperVoxelTree.GetRoot().GetItem();

            var newGameObject = new GameObject();

            var originalGameObject = myVoxelTree.GetGameObject();

            newGameObject.transform.position = originalGameObject.transform.position;
            newGameObject.transform.rotation = originalGameObject.transform.rotation;
            newGameObject.transform.localScale = originalGameObject.transform.lossyScale;

            var neighbourVoxelTree = neighbourSuperVoxelItem.voxelTree;
            neighbourVoxelTree.SetOwnerNode(ownerNode);

            neighbourVoxelTree.SetGameObject(newGameObject);

            neighbourVoxelTree.CopyMaterialsFrom(myVoxelTree);

        } else {
            var newNeighbourSuperVoxelTree = new SuperVoxelTree(neighbourBounds) {
                _indexInParent = ownerNode.GetIndexInParent(),
                _parent = ownerNode.GetTree()
            };

            neighbourSuperVoxelItem = newNeighbourSuperVoxelTree.GetRoot().GetItem();
        }

        ownerNode.SetItem(neighbourSuperVoxelItem);
    }

    private Bounds CreateParentBounds() {
        var myBounds = GetRoot().GetBounds();

        var parentBoundsCenter = myBounds.center -
                                 Vector3.Scale(myBounds.extents, OctreeNode.GetChildDirection(_indexInParent));

        return new Bounds(parentBoundsCenter, Vector3.zero) {extents = myBounds.extents * 2};
    }

    private void AssignIndexInParent(NeighbourSide sideToGrowTowards) {
        switch (sideToGrowTowards) {
            case NeighbourSide.Above:
            case NeighbourSide.Right:
            case NeighbourSide.Forward:
                _indexInParent = OctreeNode.ChildIndex.LeftBelowBack;
                break;
            case NeighbourSide.Left:
                _indexInParent = OctreeNode.ChildIndex.RightBelowBack;
                break;
            case NeighbourSide.Back:
                _indexInParent = OctreeNode.ChildIndex.LeftBelowForward;
                break;
            case NeighbourSide.Below:
                _indexInParent = OctreeNode.ChildIndex.LeftAboveBack;
                break;
            case NeighbourSide.Invalid:
                throw new ArgumentOutOfRangeException("sideToGrowTowards", sideToGrowTowards, null);
            default:
                throw new ArgumentOutOfRangeException("sideToGrowTowards", sideToGrowTowards, null);
        }
    }


    public VoxelTree GetVoxelTree() {
        return GetRoot().GetItem().voxelTree;
    }

    public class Node : OctreeNodeBase<SuperVoxelItem, SuperVoxelTree, Node> {
        public Node(Bounds bounds, SuperVoxelTree tree) : base(bounds, tree) {}

        public Node(Bounds bounds, Node parent, ChildIndex indexInParent, int depth, SuperVoxelTree ocTree)
            : base(bounds, parent, indexInParent, depth, ocTree) {}
    }
}