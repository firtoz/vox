using NUnit.Framework;
using UnityEngine;

namespace UnityTest
{
    [TestFixture]
    [Category("Octree Tests")]
    internal class OctreeTests
    {
        [Test]
        public void Coordinates()
        {
            var testOctree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one));
            var root = testOctree.GetRoot();

            var firstChild = root.AddChild(1);
            //TopFwdRight , 1, 1, 1

            Assert.AreEqual(0, root.GetCoords().Length);

            var firstChildCoords = firstChild.GetCoords();
            Assert.AreEqual(1, firstChildCoords.Length);
            Assert.AreEqual(new OctreeCoordinate(1, 1, 1), firstChildCoords[0]);

            var grandChild = firstChild.AddChild(2);
            //TopBackLeft = 0, 1, 0

            var grandChildCoords = grandChild.GetCoords();
            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeCoordinate(1, 1, 1), grandChildCoords[0]);
            Assert.AreEqual(new OctreeCoordinate(0, 1, 0), grandChildCoords[1]);

            var leftOfGrandChildCoords = grandChild.GetNeighbourCoords(OctreeNodeNeighbourSide.Left);

            Assert.AreEqual(2, leftOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeCoordinate(0, 1, 1), leftOfGrandChildCoords[0]);
            Assert.AreEqual(new OctreeCoordinate(1, 1, 0), leftOfGrandChildCoords[1]);

            var furtherLeftCoords = OctreeNode<int>.GetNeighbourCoords(leftOfGrandChildCoords,
                OctreeNodeNeighbourSide.Left);

            Assert.AreEqual(2, furtherLeftCoords.Length);

            Assert.AreEqual(new OctreeCoordinate(0, 1, 1), furtherLeftCoords[0]);
            Assert.AreEqual(new OctreeCoordinate(0, 1, 0), furtherLeftCoords[1]);

            furtherLeftCoords = OctreeNode<int>.GetNeighbourCoords(furtherLeftCoords, OctreeNodeNeighbourSide.Left);

            Assert.IsNull(furtherLeftCoords, "furtherLeftCoords");
        }
    }
}