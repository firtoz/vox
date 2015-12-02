using System;
using NUnit.Framework;
using UnityEngine;

namespace OctreeTest
{
    [TestFixture]
    [Category("Octree Tests")]
    internal class OctreeNodeTests
    {
        private class TestOctree<T> : Octree<T> {
            public TestOctree(Bounds bounds) : base(bounds) {}

            protected override int GetItemMeshId(T item) {
                return 0;
            }

            protected override Material GetMeshMaterial(int meshId) {
                return new Material(Shader.Find("Standard"));
            }
        }

        [Test]
        public void CoordinateTests()
        {
            var testOctree = new TestOctree<int>(new Bounds(Vector3.zero, Vector3.one));
            var root = testOctree.GetRoot();

            Assert.IsTrue(root.IsLeafNode());
            Assert.AreEqual(0, root.GetChildCount());

            Assert.AreEqual(0, root.GetCoords().Length);

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                root.GetChild(OctreeNode.ChildIndex.Invalid);
            });

            for (var i = 0; i < 8; i++)
            {
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

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                root.AddChild(OctreeNode.ChildIndex.Invalid);
            });

            Assert.IsTrue(root.IsLeafNode());

            var firstChild = root.AddChild(OctreeNode.ChildIndex.TopFwdRight);
            Assert.AreEqual(firstChild, root.GetChild(OctreeNode.ChildIndex.TopFwdRight));

            Assert.IsFalse(root.IsLeafNode());
            Assert.IsTrue(firstChild.IsLeafNode());
            Assert.AreEqual(0, firstChild.GetChildCount());
            Assert.AreEqual(1, root.GetChildCount());
            //TopFwdRight , 1, 1, 1

            Assert.Throws<ArgumentException>(() =>
            {
                root.AddChild(OctreeNode.ChildIndex.TopFwdRight);
            });

            for (var i = 0; i < 8; i++)
            {
                if (i == 2)
                {
                    Assert.AreEqual(firstChild, root.GetChild((OctreeNode.ChildIndex) i));
                }
                else
                {
                    Assert.IsNull(root.GetChild((OctreeNode.ChildIndex)i));
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
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), firstChildCoords.GetCoord(0));


            var grandChild = firstChild.AddChild(OctreeNode.ChildIndex.TopBackLeft);

            Assert.AreEqual(1, root.GetChildCount());
            Assert.AreEqual(1, firstChild.GetChildCount());
            Assert.AreEqual(0, grandChild.GetChildCount());
            Assert.IsFalse(root.IsLeafNode());
            Assert.IsFalse(firstChild.IsLeafNode());
            Assert.IsTrue(grandChild.IsLeafNode());
            //TopBackLeft = 0, 1, 0

            var grandChildCoords = grandChild.GetCoords();
            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords.GetCoord(1));

            var rootCoords = new OctreeNodeCoordinates();

            Assert.AreEqual(root, root.GetChildAtCoords(rootCoords));
            Assert.AreEqual(firstChild, root.GetChildAtCoords(firstChild.GetCoords()));
            Assert.AreEqual(grandChild, root.GetChildAtCoords(grandChild.GetCoords()));

            var inexistentNodeCoords =
                new OctreeNodeCoordinates(new[]
                {
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

            grandChild = firstChild.AddChild(OctreeNode.ChildIndex.TopBackLeft);

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