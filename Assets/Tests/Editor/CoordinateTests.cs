using System;
using NUnit.Framework;

namespace OctreeTest
{
    [TestFixture]
    [Category("Octree Tests")]
    internal class CoordinateTests
    {

        [Test]
        public void String()
        {

            //tostring
            Assert.AreEqual("[0, 1, 0]", new OctreeChildCoordinates(0, 1, 0).ToString());
            Assert.AreEqual("[0, 0, 1]", new OctreeChildCoordinates(0, 0, 1).ToString());
            Assert.AreEqual("[1, 0, 0]", new OctreeChildCoordinates(1, 0, 0).ToString());

        }

        [Test]
        public void Equality()
        {

            Assert.False(new OctreeChildCoordinates(1, 0, 1).Equals(null));

            //hash collisions and general equality

            for (int i = 0; i < 8; i++)
            {
                var firstIndex = (OctreeNode.ChildIndex)i;
                for (int j = 0; j < 8; j++)
                {
                    var secondIndex = (OctreeNode.ChildIndex)j;

                    var firstCoordinate = OctreeChildCoordinates.FromIndex(firstIndex);
                    var secondCoordinate = OctreeChildCoordinates.FromIndex(secondIndex);

                    if (i == j)
                    {
                        Assert.AreEqual(firstCoordinate, secondCoordinate);
                        Assert.AreEqual(firstCoordinate.GetHashCode(), secondCoordinate.GetHashCode());
                    }
                    else
                    {
                        Assert.AreNotEqual(firstCoordinate, secondCoordinate);
                        Assert.AreNotEqual(firstCoordinate.GetHashCode(), secondCoordinate.GetHashCode());
                    }
                }
            }
        }

        [Test]
        public void ToAndFromIndices()
        {
            var wantedCoords = new OctreeChildCoordinates(0, 1, 1);
            var actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopFwdLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopFwdLeft, actualCoords.ToIndex());

            //right
            wantedCoords.x = 1;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopFwdRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopFwdRight, actualCoords.ToIndex());

            //left
            wantedCoords.x = 0;
            //back
            wantedCoords.z = 0;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopBackLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopBackLeft, actualCoords.ToIndex());

            //right
            wantedCoords.x = 1;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopBackRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopBackRight, actualCoords.ToIndex());

            //bot
            wantedCoords.y = 0;
            //left
            wantedCoords.x = 0;
            //fwd
            wantedCoords.z = 1;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotFwdLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotFwdLeft, actualCoords.ToIndex());

            //right
            wantedCoords.x = 1;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotFwdRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotFwdRight, actualCoords.ToIndex());

            //left
            wantedCoords.x = 0;
            //back
            wantedCoords.z = 0;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotBackLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotBackLeft, actualCoords.ToIndex());

            //right
            wantedCoords.x = 1;
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotBackRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotBackRight, actualCoords.ToIndex());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.Invalid);
            });

            //and some invalid ones!
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(-1,0,1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(0,-1,1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(0,0,-1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(1,2,0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(1,0,2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2,0,0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2,0,2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2,2,2).ToIndex());

        }

        [Test]
        public void NeighboursTest()
        {
            var grandChildCoords = new OctreeNodeCoordinates(new[]
            {
                new OctreeChildCoordinates(1, 1, 1),
                new OctreeChildCoordinates(0, 1, 0)
            });

            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords[1]);
            var leftOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.AreEqual(2, leftOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), leftOfGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), leftOfGrandChildCoords[1]);

            Assert.NotNull(leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right));

            Assert.AreNotEqual(leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right),
                leftOfGrandChildCoords);

            var furtherLeftCoords = leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.AreEqual(2, furtherLeftCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), furtherLeftCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furtherLeftCoords[1]);

            furtherLeftCoords = furtherLeftCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.IsNull(furtherLeftCoords, "furtherLeftCoords");

            var rightOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right);

            Assert.AreEqual(rightOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left), grandChildCoords);


            Assert.AreEqual(2, rightOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), rightOfGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), rightOfGrandChildCoords[1]);

            var aboveGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above);

            Assert.IsNull(aboveGrandChildCoords);

            var belowGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Below);
            Assert.AreEqual(belowGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above), grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), belowGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 0, 0), belowGrandChildCoords[1]);

            var furterBelowGrandChildCoords = belowGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Below);
            Assert.AreEqual(furterBelowGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above), belowGrandChildCoords);

            Assert.AreEqual(2, furterBelowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 0, 1), furterBelowGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furterBelowGrandChildCoords[1]);

            var behindGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Back);
            Assert.AreEqual(behindGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Forward),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), behindGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), behindGrandChildCoords[1]);

            var inFrontOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Forward);
            Assert.AreEqual(inFrontOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Back),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), inFrontOfGrandChildCoords[0]);
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), inFrontOfGrandChildCoords[1]);

            const int incorrectSide = 6;
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((OctreeNode.NeighbourSide)incorrectSide));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((OctreeNode.NeighbourSide)incorrectSide + 1));

            var rootCoords = new OctreeNodeCoordinates();

            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Below));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Back));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Forward));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right));
        }
    }
}