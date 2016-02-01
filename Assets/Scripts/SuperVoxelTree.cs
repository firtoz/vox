using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SuperVoxelTree : OctreeBase<VoxelTree, SuperVoxelTree.Node, SuperVoxelTree> {
	private OctreeNode.ChildIndex _indexInParent;

	public SuperVoxelTree(Func<SuperVoxelTree, Bounds, Node> nodeConstructor, Bounds bounds)
		: base(nodeConstructor, bounds) {}

	public SuperVoxelTree(Bounds bounds, VoxelTree voxelTree) : base(CreateRootNode, bounds) {
		if (voxelTree == null) {
			throw new ArgumentException("The voxelTree argument cannot be null.", "voxelTree");
		}

		GetRoot().SetItem(voxelTree);
	}

	private SuperVoxelTree(Bounds bounds) : base(CreateRootNode, bounds) {}

	public override Node ConstructNode(Bounds bounds, Node parent, OctreeNode.ChildIndex indexInParent) {
		return new Node(bounds, parent, indexInParent, this);
	}

	private static Node CreateRootNode(SuperVoxelTree self, Bounds bounds) {
		return new Node(bounds, self);
	}

//
//    public SuperVoxelTree GetOrCreateNeighbour(NeighbourSide side, bool readOnly) {
//        if (_parentTree == null) {
//            if (readOnly) {
//                return null;
//            }
//            AssignIndexInParent(side);
//
//            _parentTree = new SuperVoxelTree(CreateParentBounds());
//            _parentTree.GetRoot().AddChild(_indexInParent).SetItem(GetRoot().GetItem());
//        }
//
////        var neighbourCoords =
////            VoxelTree.GetNeighbourCoords(new Coords(new[] {OctreeChildCoords.FromIndex(_indexInParent)}), side);
//
////        if (neighbourCoords == null) {
//        // uh oh, gotta go further
//
//        // cases:
//        // length 0: not gonna happen.
//        // not at edge, if parent was null, we get neighbour coords result.
//        // at edge: ??
//
//        var neighbourCoordsResult = GetNeighbourCoordsInfinite(_parentTree,
//            new Coords(new[] {OctreeChildCoords.FromIndex(_indexInParent)}), side, GetOrCreateParentNeighbour);
//
//        var neighbourParentTree = neighbourCoordsResult.tree;
//        var neighbourCoords = neighbourCoordsResult.coordsResult;
//
//        var parentRoot = neighbourParentTree.GetRoot();
//
//        var neighbour = parentRoot.GetChildAtCoords(neighbourCoords);
//
//        if (neighbour == null) {
//            if (readOnly) {
//                return null;
//            }
//
//            neighbour = parentRoot.AddRecursive(neighbourCoords, false);
//            CreateNewNeighbourSuperVoxelTree(neighbour);
//        }
//
//        return neighbour.GetItem().childTree;
//
//        //        }
//
//        //        var neighbour = _parentTree.GetRoot().GetChildAtCoords(neighbourCoords) ??
//        //                        _parentTree.GetRoot().AddRecursive(neighbourCoords);
//
//        //        return neighbour.GetItem().childTree;
//    }
//
//    private SuperVoxelTree GetOrCreateParentNeighbour(NeighbourSide side, bool readOnly) {
//        Assert.IsNotNull(_parentTree, "Trying to get a neighbour for a parent that does not exist");
//
//        return _parentTree.GetOrCreateNeighbour(side, readOnly);
//    }

//    private void CreateNewNeighbourSuperVoxelTree(Node ownerNode) {
//        var myRoot = GetRoot();
//
//        var myRootBounds = myRoot.GetBounds();
//
//        var neighbourBounds = ownerNode.GetBounds();
//
//        Assert.IsTrue(neighbourBounds.extents == myRootBounds.extents);
//
//        var myItem = myRoot.GetItem();
//
//        SuperVoxelItem neighbourSuperVoxelItem;
//
//        var myVoxelTree = myItem.voxelTree;
//        if (myVoxelTree != null) {
//            var newNeighbourSuperVoxelTree = new SuperVoxelTree(neighbourBounds,
//                new VoxelTree(neighbourBounds.center, neighbourBounds.size, false)) {
//                    _indexInParent = ownerNode.GetIndexInParent(),
//                    _parentTree = ownerNode.GetTree()
//                };
//
//            var neighbourRoot = newNeighbourSuperVoxelTree.GetRoot();
//            neighbourSuperVoxelItem = neighbourRoot.GetItem();
//
//            var newGameObject = new GameObject();
//
//            var originalGameObject = myVoxelTree.GetGameObject();
//
//            newGameObject.transform.position = originalGameObject.transform.position;
//            newGameObject.transform.rotation = originalGameObject.transform.rotation;
//            newGameObject.transform.localScale = originalGameObject.transform.lossyScale;
//
//            var neighbourVoxelTree = neighbourSuperVoxelItem.voxelTree;
//            neighbourVoxelTree.SetOwnerNode(neighbourRoot);
//
//            neighbourVoxelTree.SetGameObject(newGameObject);
//
//            neighbourVoxelTree.CopyMaterialsFrom(myVoxelTree);
//        } else {
//            var newNeighbourSuperVoxelTree = new SuperVoxelTree(neighbourBounds) {
//                _indexInParent = ownerNode.GetIndexInParent(),
//                _parentTree = ownerNode.GetTree()
//            };
//
//            neighbourSuperVoxelItem = newNeighbourSuperVoxelTree.GetRoot().GetItem();
//        }
//
//        ownerNode.SetItem(neighbourSuperVoxelItem);
//    }
//
//    private Bounds CreateParentBounds() {
//        var myBounds = GetRoot().GetBounds();
//
//        var parentBoundsCenter = myBounds.center -
//                                 Vector3.Scale(myBounds.extents, OctreeNode.GetChildDirection(_indexInParent));
//
//        return new Bounds(parentBoundsCenter, Vector3.zero) {extents = myBounds.extents * 2};
//    }
//
//    private void AssignIndexInParent(NeighbourSide sideToGrowTowards) {
//        switch (sideToGrowTowards) {
//            case NeighbourSide.Above:
//            case NeighbourSide.Right:
//            case NeighbourSide.Forward:
//                _indexInParent = OctreeNode.ChildIndex.LeftBelowBack;
//                break;
//            case NeighbourSide.Left:
//                _indexInParent = OctreeNode.ChildIndex.RightBelowBack;
//                break;
//            case NeighbourSide.Back:
//                _indexInParent = OctreeNode.ChildIndex.LeftBelowForward;
//                break;
//            case NeighbourSide.Below:
//                _indexInParent = OctreeNode.ChildIndex.LeftAboveBack;
//                break;
//            case NeighbourSide.Invalid:
//                throw new ArgumentOutOfRangeException("sideToGrowTowards", sideToGrowTowards, null);
//            default:
//                throw new ArgumentOutOfRangeException("sideToGrowTowards", sideToGrowTowards, null);
//        }
//    }


//    public VoxelTree GetVoxelTree() {
//        var root = GetRoot();
//        Assert.IsNotNull(root, "Root should not be null");
//        var rootItem = root.GetItem();
//        Assert.IsNotNull(rootItem, "Root item should not be null");
//        return rootItem.voxelTree;
//    }

	public class Node : OctreeNodeBase<VoxelTree, SuperVoxelTree, Node> {
		public Node(Bounds bounds, SuperVoxelTree tree) : base(bounds, tree) {}

		public Node(Bounds bounds, Node parent, ChildIndex indexInParent, SuperVoxelTree ocTree)
			: base(bounds, parent, indexInParent, ocTree) {}

		public Node GetOrCreateNeighbour(NeighbourSide side, bool readOnly) {
			if (!CreateParentTowardsSide(side, readOnly)) {
				return null;
			}

			//
			////        var neighbourCoords =
			////            VoxelTree.GetNeighbourCoords(new Coords(new[] {OctreeChildCoords.FromIndex(_indexInParent)}), side);
			//
			////        if (neighbourCoords == null) {
			//        // uh oh, gotta go further
			//
			// cases:
			// length 0: not gonna happen.
			// not at edge, if parent was null, we get neighbour coords result.
			// at edge: ??

			var neighbourCoordsResult = GetNeighbourCoordsInfinite(ocTree,
				GetCoords(), side, ExpandRootAndReturnTree);

			var neighbourCoords = neighbourCoordsResult.coordsResult;

			var root = GetRoot();

			var neighbour = root.GetChildAtCoords(neighbourCoords);

			if (neighbour == null) {
				if (readOnly) {
					return null;
				}

				neighbour = root.AddRecursive(neighbourCoords, false);
				CreateNewNeighbourSuperVoxelTree(neighbour);
			}

			return neighbour;
		}


		private void CreateNewNeighbourSuperVoxelTree(Node neighbourNode) {
			var myBounds = GetBounds();

			var neighbourBounds = neighbourNode.GetBounds();

			Assert.AreEqual(neighbourBounds.extents, myBounds.extents);

			var myItem = GetItem();

			var myVoxelTree = myItem;
			if (myVoxelTree == null) {
				return;
			}

			var neighbourVoxelTree = new VoxelTree(neighbourBounds.center, neighbourBounds.size, false);

			var newGameObject = new GameObject();

			var originalGameObject = myVoxelTree.GetGameObject();

			newGameObject.transform.position = originalGameObject.transform.position;
			newGameObject.transform.rotation = originalGameObject.transform.rotation;
			newGameObject.transform.localScale = originalGameObject.transform.lossyScale;

			neighbourVoxelTree.SetOwnerNode(neighbourNode);

			neighbourVoxelTree.SetGameObject(newGameObject);

			neighbourVoxelTree.CopyMaterialsFrom(myVoxelTree);

			neighbourNode.SetItem(neighbourVoxelTree);
		}

		private bool CreateParentTowardsSide(NeighbourSide side, bool readOnly) {
			if (parent == null) {
				// we're the root
				if (readOnly) {
					return false;
				}

				var wantedIndexInParent = GetWantedIndexInParent(side);

				parent = new Node(CreateParentBounds(wantedIndexInParent), ocTree);

				ocTree.SetRoot(parent);

				indexInParent = wantedIndexInParent;

				parent.ReplaceChild(wantedIndexInParent, this);
			}

			return true;
		}

		private SuperVoxelTree ExpandRootAndReturnTree(NeighbourSide side, bool isReadOnly) {
			if (!GetRoot().CreateParentTowardsSide(side, isReadOnly)) {
				return null;
			}

			return ocTree;
		}

		private void ReplaceChild(ChildIndex childIndex, Node newChild) {
			Assert.AreEqual(newChild.bounds.extents * 2, bounds.extents);
			var childCoords = new Coords(new[] {OctreeChildCoords.FromIndex(childIndex)});
			var childBounds = GetChildBounds(childCoords);
			Assert.AreEqual(newChild.bounds.center, childBounds.center);

			if (children == null) {
				children = new Node[8];
			} else if (children[(int) childIndex] != null) {
				RemoveChild(childIndex);
			}

			childCount++;

			SetChild(childIndex, newChild);
		}

		private Bounds CreateParentBounds(ChildIndex wantedIndexInParent) {
			var myBounds = GetBounds();

			var parentBoundsCenter = myBounds.center - Vector3.Scale(myBounds.extents, GetChildDirection(wantedIndexInParent));

			return new Bounds(parentBoundsCenter, Vector3.zero) {extents = myBounds.extents * 2};
		}

		private static ChildIndex GetWantedIndexInParent(NeighbourSide sideToGrowTowards) {
			switch (sideToGrowTowards) {
				case NeighbourSide.Above:
				case NeighbourSide.Right:
				case NeighbourSide.Forward:
					return ChildIndex.LeftBelowBack;
				case NeighbourSide.Left:
					return ChildIndex.RightBelowBack;
				case NeighbourSide.Back:
					return ChildIndex.LeftBelowForward;
				case NeighbourSide.Below:
					return ChildIndex.LeftAboveBack;
				case NeighbourSide.Invalid:
					throw new ArgumentOutOfRangeException("sideToGrowTowards", sideToGrowTowards, null);
				default:
					throw new ArgumentOutOfRangeException("sideToGrowTowards", sideToGrowTowards, null);
			}
		}
	}
}