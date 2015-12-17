using System;
using NUnit.Framework;
using UnityEngine;

namespace OctreeTest {
    [TestFixture]
    [Category("Octree Tests")]
    internal class OctreeNodeTests {
        private class TestOctreeNode<T> : OctreeNodeBase<T, TestOctree<T>, TestOctreeNode<T>> {
            public TestOctreeNode(Bounds bounds, TestOctree<T> tree) : base(bounds, tree) {}
            public TestOctreeNode(Bounds bounds, TestOctreeNode<T> parent, ChildIndex indexInParent, int depth, TestOctree<T> tree) : base(bounds, parent, indexInParent, depth, tree) {}
        }

        private class TestOctree<T> : OctreeBase<T, TestOctreeNode<T>, TestOctree<T>> {
            public TestOctree(Bounds bounds) : base(CreateRootNode, bounds) {}

            private static TestOctreeNode<T> CreateRootNode(TestOctree<T> self, Bounds bounds) {
                return new TestOctreeNode<T>(bounds, self);
            }

            public override TestOctreeNode<T> ConstructNode(Bounds bounds, TestOctreeNode<T> parent, OctreeNode.ChildIndex indexInParent, int depth) {
                return new TestOctreeNode<T>(bounds, parent, indexInParent, depth, this);
            }

            protected override int GetItemMeshId(T item) {
                return 0;
            }

            protected override Material GetMeshMaterial(int meshId) {
                return new Material(Shader.Find("Standard"));
            }

            protected override TestOctree<T> CreateNeighbour(NeighbourSide side) {
                var neighbourBounds = GetNeighbourBounds(side);
                return new TestOctree<T>(neighbourBounds);
            }
        }

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

            var aboveBackLeft = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.LeftAboveBack);
            var childBounds = root.GetChildBounds(new OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>>(testOctree,
                new[] {aboveBackLeft}));

            var count = 1; // 0.5 by def already

            var up = 0;
            var right = 0;
            var forward = 0;

            count++;

            up = up * 2 + 1;
            right = right * 2 - 1;
            forward = forward * 2 - 1;

            Assert.AreEqual(up, 1);
            Assert.AreEqual(right, -1);
            Assert.AreEqual(forward, -1);

            Assert.AreEqual(childBounds.center,
                (Vector3.up * (rootSize.y * up) +
                 Vector3.right * (rootSize.x * right) +
                 Vector3.forward * (rootSize.z * forward)) / Mathf.Pow(2, count)
                );

            Assert.AreEqual(rootSize * 0.5f, 
                childBounds.size
                );

            childBounds = root.GetChildBounds(new OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>>(testOctree,
                new[] {
                    aboveBackLeft,
                    aboveBackLeft
                }));


            count++;

            up = up * 2 + 1; // above
            right = right * 2 - 1; // left
            forward = forward * 2 - 1; // back

            Assert.AreEqual(3, up);
            Assert.AreEqual(-3, right);
            Assert.AreEqual(-3, forward);

            Assert.AreEqual(childBounds.center,
                (Vector3.up * (rootSize.y * up) +
                 Vector3.right * (rootSize.x * right) +
                 Vector3.forward * (rootSize.z * forward)) / Mathf.Pow(2, count)
                );

            Assert.AreEqual(childBounds.center,
                Vector3.up * (rootSize.y * 0.25f) +
                Vector3.up * (rootSize.y * 0.125f) +
                Vector3.left * (rootSize.x * 0.25f) +
                Vector3.left * (rootSize.x * 0.125f) +
                Vector3.back * (rootSize.z * 0.25f) +
                Vector3.back * (rootSize.z * 0.125f)
                );

            Assert.AreEqual(childBounds.size,
                rootSize * 0.25f
                );

            var aboveBackRight = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.RightAboveBack);

            childBounds = root.GetChildBounds(new OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>>(testOctree,
                new[] {
                    aboveBackLeft,
                    aboveBackLeft,
                    aboveBackRight
                }));

            count++;

            up = up * 2 + 1;
            right = right * 2 + 1;
            forward = forward * 2 - 1;

            Assert.AreEqual(7, up);
            Assert.AreEqual(-5, right);
            Assert.AreEqual(-7, forward);

            Assert.AreEqual(childBounds.center,
                (Vector3.up * (rootSize.y * up) +
                 Vector3.right * (rootSize.x * right) +
                 Vector3.forward * (rootSize.z * forward)) / Mathf.Pow(2, count)
                );

            Assert.AreEqual(childBounds.center,
                Vector3.up * (rootSize.y * 0.25f) +
                Vector3.up * (rootSize.y * 0.125f) +
                Vector3.up * (rootSize.y * 0.0625f) +
                Vector3.left * (rootSize.x * 0.25f) +
                Vector3.left * (rootSize.x * 0.125f) +
                Vector3.left * (-rootSize.x * 0.0625f) +
                Vector3.back * (rootSize.z * 0.25f) +
                Vector3.back * (rootSize.z * 0.125f) +
                Vector3.back * (rootSize.z * 0.0625f)
                );

            Assert.AreEqual(childBounds.size,
                rootSize * 0.125f
                );

            var belowForwardLeft = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.LeftBelowForward);

            childBounds = root.GetChildBounds(new OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>>(testOctree,
                new[] {
                    aboveBackLeft,
                    aboveBackLeft,
                    aboveBackRight,
                    belowForwardLeft
                }));

            count++;

            up = up * 2 - 1;
            right = right * 2 - 1;
            forward = forward * 2 + 1;

            Assert.AreEqual(13, up);
            Assert.AreEqual(-11, right);
            Assert.AreEqual(-13, forward);

            Assert.AreEqual(childBounds.center,
                (Vector3.up * (rootSize.y * up) +
                 Vector3.right * (rootSize.x * right) +
                 Vector3.forward * (rootSize.z * forward)) / Mathf.Pow(2, count)
                );

            Assert.AreEqual(childBounds.center,
                Vector3.up * (rootSize.y * 0.25f) +
                Vector3.up * (rootSize.y * 0.125f) +
                Vector3.up * (rootSize.y * 0.0625f) +
                Vector3.up * (-rootSize.y * 0.03125f) +
                Vector3.left * (rootSize.x * 0.25f) +
                Vector3.left * (rootSize.x * 0.125f) +
                Vector3.left * (-rootSize.x * 0.0625f) +
                Vector3.left * (rootSize.x * 0.03125f) +
                Vector3.back * (rootSize.z * 0.25f) +
                Vector3.back * (rootSize.z * 0.125f) +
                Vector3.back * (rootSize.z * 0.0625f) +
                Vector3.back * (-rootSize.z * 0.03125f)
                );

            Assert.AreEqual(childBounds.size,
                rootSize * 0.0625f
                );
        }

        [Test]
        public void CoordinateTests() {
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
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), firstChildCoords.GetCoord(0));


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
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords.GetCoord(1));

            var rootCoords = new OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>>(null);

            Assert.AreEqual(root, root.GetChildAtCoords(rootCoords));
            Assert.AreEqual(firstChild, root.GetChildAtCoords(firstChild.GetCoords()));
            Assert.AreEqual(grandChild, root.GetChildAtCoords(grandChild.GetCoords()));

            var inexistentNodeCoords =
                new OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>>(null, new[] {
                    new OctreeChildCoordinates(1, 1, 1),
                    new OctreeChildCoordinates(1, 1, 1),
                    new OctreeChildCoordinates(1, 1, 1)
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