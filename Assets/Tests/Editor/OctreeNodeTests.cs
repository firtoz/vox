using System;
using NUnit.Framework;
using UnityEngine;

namespace OctreeTest {
	[TestFixture]
	[Category("Octree Tests")]
	internal class OctreeNodeTests {
		[Test]
		public void Bounds() {
			var testOctree = new TestOctree<int>(new Bounds(Vector3.zero, Vector3.one * 5));

			var root = testOctree.GetRoot();
			root.SubDivide();

			var rootBounds = root.GetBounds();

			Assert.AreEqual(Vector3.zero, rootBounds.center);
			var rootSize = rootBounds.size;
			Assert.AreEqual(Vector3.one * 5, rootSize);
			Assert.AreNotEqual(Vector3.one * 4, rootSize); // just to confirm vector equality

			var leftAboveBackIndex = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftAboveBack);
			var childBounds = root.GetChildBounds(new Coords(
				new[] {leftAboveBackIndex}));

			Assert.AreEqual(rootBounds.center 
				+ Vector3.Scale(rootBounds.extents, new Vector3(-0.5f, 0.5f, -0.5f)), childBounds.center);

			var childNode = root.GetChild(OctreeNode.ChildIndex.LeftAboveBack);

			var actualChildBounds = childNode.GetBounds();

			Assert.AreEqual(childBounds.center, actualChildBounds.center);
			Assert.AreEqual(childBounds.extents, actualChildBounds.extents);

			Assert.AreEqual(rootSize * 0.5f,
				childBounds.size
				);

			var oldBounds = childBounds;


			childBounds = root.GetChildBounds(new Coords(
				new[] {
					leftAboveBackIndex,
					leftAboveBackIndex
				}));

			Assert.AreEqual(oldBounds.center
				+ Vector3.Scale(oldBounds.extents, new Vector3(-0.5f, 0.5f, -0.5f)), childBounds.center);

			Assert.AreEqual(childBounds.center,
				Vector3.up * (rootSize.y * 0.25f) +
				Vector3.left * (rootSize.x * 0.25f) +
				Vector3.back * (rootSize.z * 0.25f) +

				Vector3.up * (rootSize.y * 0.125f) +
				Vector3.left * (rootSize.x * 0.125f) +
				Vector3.back * (rootSize.z * 0.125f)
				);

			var oldChild = childNode;
			var parentChildBounds = childNode.GetChildBounds(new Coords(new[] { leftAboveBackIndex }));

			Assert.AreEqual(childBounds.center, parentChildBounds.center);
			Assert.AreEqual(childBounds.extents, parentChildBounds.extents);

			Assert.AreEqual(childBounds.size,
				rootSize * 0.25f
				);

			var rightAboveBackIndex = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightAboveBack);

			oldBounds = childBounds;
			childBounds = root.GetChildBounds(new Coords(
				new[] {
					leftAboveBackIndex,
					leftAboveBackIndex,
					rightAboveBackIndex
				}));


			Assert.AreEqual(oldBounds.center
				+ Vector3.Scale(oldBounds.extents, new Vector3(0.5f, 0.5f, -0.5f)), childBounds.center);

			Assert.AreEqual(childBounds.center,
				Vector3.left * (rootSize.x * 0.25f) +
				Vector3.up * (rootSize.y * 0.25f) +
				Vector3.back * (rootSize.z * 0.25f) +
				Vector3.left * (rootSize.x * 0.125f) +
				Vector3.up * (rootSize.y * 0.125f) +
				Vector3.back * (rootSize.z * 0.125f) +
				Vector3.right * (rootSize.x * 0.0625f) +
				Vector3.up * (rootSize.y * 0.0625f) +
				Vector3.back * (rootSize.z * 0.0625f)
				);

			Assert.AreEqual(childBounds.size,
				rootSize * 0.125f
				);

			//var belowForwardLeft = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftBelowForward);

			//childBounds = root.GetChildBounds(new Coords(
			//	new[] {
			//		leftAboveBack,
			//		leftAboveBack,
			//		aboveBackRight,
			//		belowForwardLeft
			//	}));

			//count++;

			//up = up * 2 - 1;
			//right = right * 2 - 1;
			//forward = forward * 2 + 1;

			//Assert.AreEqual(13, up);
			//Assert.AreEqual(-11, right);
			//Assert.AreEqual(-13, forward);

			//Assert.AreEqual(childBounds.center,
			//	(Vector3.up * (rootSize.y * up) +
			//	 Vector3.right * (rootSize.x * right) +
			//	 Vector3.forward * (rootSize.z * forward)) / Mathf.Pow(2, count)
			//	);

			//Assert.AreEqual(childBounds.center,
			//	Vector3.up * (rootSize.y * 0.25f) +
			//	Vector3.up * (rootSize.y * 0.125f) +
			//	Vector3.up * (rootSize.y * 0.0625f) +
			//	Vector3.up * (-rootSize.y * 0.03125f) +
			//	Vector3.left * (rootSize.x * 0.25f) +
			//	Vector3.left * (rootSize.x * 0.125f) +
			//	Vector3.left * (-rootSize.x * 0.0625f) +
			//	Vector3.left * (rootSize.x * 0.03125f) +
			//	Vector3.back * (rootSize.z * 0.25f) +
			//	Vector3.back * (rootSize.z * 0.125f) +
			//	Vector3.back * (rootSize.z * 0.0625f) +
			//	Vector3.back * (-rootSize.z * 0.03125f)
			//	);

			//Assert.AreEqual(childBounds.size,
			//	rootSize * 0.0625f
			//	);
		}

		[Test]
		public void NodeCoordsTests() {
			var testOctree = new TestOctree<int>(new Bounds(Vector3.zero, Vector3.one));
			var root = testOctree.GetRoot();

			Assert.IsTrue(root.IsLeafNode());
			Assert.AreEqual(0, root.GetChildCount());

			Assert.AreEqual(0, root.GetCoords().Length);

			Assert.Throws<ArgumentOutOfRangeException>(() => { root.GetChild(OctreeNode.ChildIndex.Invalid); });

			for (var i = 0; i < 8; i++) {
				Assert.IsNull(root.GetChild((OctreeNode.ChildIndex) i));
			}

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				const int invalidIndex = -2;
				root.AddChild((OctreeNode.ChildIndex) invalidIndex);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				const int invalidIndex = 9;
				root.AddChild((OctreeNode.ChildIndex) invalidIndex);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() => { root.AddChild(OctreeNode.ChildIndex.Invalid); });

			Assert.IsTrue(root.IsLeafNode());

			var firstChild = root.AddChild(OctreeNode.ChildIndex.LeftAboveBack); // 0, 1, 0
			Assert.AreEqual(firstChild, root.GetChild(OctreeNode.ChildIndex.LeftAboveBack));

			Assert.IsFalse(root.IsLeafNode());
			Assert.IsTrue(firstChild.IsLeafNode());
			Assert.AreEqual(0, firstChild.GetChildCount());
			Assert.AreEqual(1, root.GetChildCount());
			//LeftAboveBack , 1, 1, 1

			Assert.Throws<ArgumentException>(() => { root.AddChild(OctreeNode.ChildIndex.LeftAboveBack); });

			for (var i = 0; i < 8; i++) {
				if (i == 2) {
					Assert.AreEqual(firstChild, root.GetChild((OctreeNode.ChildIndex) i));
				} else {
					Assert.IsNull(root.GetChild((OctreeNode.ChildIndex) i));
				}
			}

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				const int invalidIndex = -2;
				root.GetChild((OctreeNode.ChildIndex) invalidIndex);
			});

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				const int invalidIndex = 9;
				root.GetChild((OctreeNode.ChildIndex) invalidIndex);
			});

			var firstChildCoords = firstChild.GetCoords();
			Assert.AreEqual(1, firstChildCoords.Length);
			Assert.AreEqual(new OctreeChildCoords(0, 1, 0), firstChildCoords.GetCoord(0));


			var grandChild = firstChild.AddChild(OctreeNode.ChildIndex.RightAboveForward);

			Assert.AreEqual(1, root.GetChildCount());
			Assert.AreEqual(1, firstChild.GetChildCount());
			Assert.AreEqual(0, grandChild.GetChildCount());
			Assert.IsFalse(root.IsLeafNode());
			Assert.IsFalse(firstChild.IsLeafNode());
			Assert.IsTrue(grandChild.IsLeafNode());
			//RightAboveForward = 0, 1, 0

			var grandChildCoords = grandChild.GetCoords();
			Assert.AreEqual(2, grandChildCoords.Length);
			Assert.AreEqual(new OctreeChildCoords(0, 1, 0), grandChildCoords.GetCoord(0));
			Assert.AreEqual(new OctreeChildCoords(1, 1, 1), grandChildCoords.GetCoord(1));

			var rootCoords = new Coords();

			Assert.AreEqual(root, root.GetChildAtCoords(rootCoords));
			Assert.AreEqual(firstChild, root.GetChildAtCoords(firstChild.GetCoords()));
			Assert.AreEqual(grandChild, root.GetChildAtCoords(grandChild.GetCoords()));

			var inexistentNodeCoords =
				new Coords(new[] {
					new OctreeChildCoords(1, 1, 1),
					new OctreeChildCoords(1, 1, 1),
					new OctreeChildCoords(1, 1, 1)
				});

			Assert.IsNull(root.GetChildAtCoords(inexistentNodeCoords));

			Assert.IsFalse(firstChild.IsDeleted());
			Assert.IsFalse(grandChild.IsDeleted());

			firstChild.RemoveChild(grandChild.GetIndexInParent());

			Assert.AreEqual(0, firstChild.GetChildCount());
			Assert.IsTrue(firstChild.IsLeafNode());

			Assert.IsTrue(grandChild.IsDeleted());

			grandChild = firstChild.AddChild(OctreeNode.ChildIndex.RightAboveForward);

			Assert.AreEqual(1, firstChild.GetChildCount());
			Assert.IsFalse(firstChild.IsLeafNode());

			Assert.IsFalse(grandChild.IsDeleted());

			root.RemoveChild(firstChild.GetIndexInParent());

			Assert.IsTrue(firstChild.IsDeleted());
			Assert.IsTrue(grandChild.IsDeleted());

			Assert.AreEqual(0, root.GetChildCount());
			Assert.IsTrue(root.IsLeafNode());
		}
	}
}