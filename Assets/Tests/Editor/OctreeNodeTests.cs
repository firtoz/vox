using NUnit.Framework;
using UnityEngine;

namespace OctreeTest
{
    [TestFixture]
    [Category("Octree Tests")]
    internal class OctreeNodeTests
    {
        [Test]
        public void CoordinateTests()
        {
            var testOctree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one));
            var root = testOctree.GetRoot();

            Assert.AreEqual(0, root.GetCoords().Length);

            var firstChild = root.AddChild(OctreeNode.ChildIndex.TopFwdRight);
            //TopFwdRight , 1, 1, 1

            var firstChildCoords = firstChild.GetCoords();
            Assert.AreEqual(1, firstChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), firstChildCoords[0]);

            var grandChild = firstChild.AddChild(OctreeNode.ChildIndex.TopBackLeft);
            //TopBackLeft = 0, 1, 0

            var grandChildCoords = grandChild.GetCoords();
            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords[1]);

            var leftOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.AreEqual(2, leftOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), leftOfGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), leftOfGrandChildCoords[1]);

            var furtherLeftCoords = leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.AreEqual(2, furtherLeftCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), furtherLeftCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furtherLeftCoords[1]);

            furtherLeftCoords = furtherLeftCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.IsNull(furtherLeftCoords, "furtherLeftCoords");
        }
    }
}