using System;
using NUnit.Framework;

namespace OctreeTest {
    [TestFixture]
    [Category("Octree Tests")]
    internal class CoordinateTests {
        [Test]
        public void Equality() {
            Assert.False(new OctreeChildCoordinates(1, 0, 1).Equals(null));

            // hash collisions and general equality

            for (var i = 0; i < 8; i++) {
                var firstIndex = (OctreeNode.ChildIndex) i;
                for (var j = 0; j < 8; j++) {
                    var secondIndex = (OctreeNode.ChildIndex) j;

                    var firstCoordinate = OctreeChildCoordinates.FromIndex(firstIndex);
                    var secondCoordinate = OctreeChildCoordinates.FromIndex(secondIndex);

                    if (i == j) {
                        Assert.AreEqual(firstCoordinate, secondCoordinate);
                        Assert.AreEqual(firstCoordinate.GetHashCode(), secondCoordinate.GetHashCode());
                    } else {
                        Assert.AreNotEqual(firstCoordinate, secondCoordinate);
                        Assert.AreNotEqual(firstCoordinate.GetHashCode(), secondCoordinate.GetHashCode());
                    }
                }
            }

            Assert.AreEqual(new OctreeNodeCoordinates(new OctreeNodeCoordinates()), new OctreeNodeCoordinates());
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates()), new OctreeChildCoordinates());

            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(0,0,1)), new OctreeChildCoordinates(0,0,1));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(0,1,0)), new OctreeChildCoordinates(0,1,0));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(1,0,0)), new OctreeChildCoordinates(1,0,0));

            // same constructor, equal
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }));

            // construct from parent with no coords
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new OctreeNodeCoordinates(),
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)));

            // construct from parent with no coords
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new OctreeNodeCoordinates(),
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)));

            Assert.IsTrue(Equals(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new OctreeNodeCoordinates(),
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0))));

            Assert.IsFalse(Equals(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), null));

            Assert.IsFalse(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }).Equals(null));

            var a = new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            });

            var b = a;

            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(b));

            // construct from parent with one coord
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0)
            }), new OctreeChildCoordinates(0, 1, 0)));

            // construct from parent with two coords
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            })));

            // get parent coordinates
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0),
                new OctreeChildCoordinates(0, 1, 1)
            }).GetParentCoordinates());

            // get parent coordinates
            Assert.AreEqual(new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0),
                new OctreeChildCoordinates(1, 1, 0)
            }).GetParentCoordinates());
        }

        [Test]
        public void NeighboursTest() {
            var grandChildCoords = new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 1, 1),
                new OctreeChildCoordinates(0, 1, 0)
            });

            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords.GetCoord(1));
            var leftOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.AreEqual(2, leftOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), leftOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), leftOfGrandChildCoords.GetCoord(1));

            Assert.NotNull(leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right));

            Assert.AreNotEqual(leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right),
                leftOfGrandChildCoords);

            var furtherLeftCoords = leftOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.AreEqual(2, furtherLeftCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), furtherLeftCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furtherLeftCoords.GetCoord(1));

            furtherLeftCoords = furtherLeftCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left);

            Assert.IsNull(furtherLeftCoords, "furtherLeftCoords");

            var rightOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right);

            Assert.AreEqual(rightOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left), grandChildCoords);


            Assert.AreEqual(2, rightOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), rightOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), rightOfGrandChildCoords.GetCoord(1));

            var aboveGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above);

            Assert.IsNull(aboveGrandChildCoords);

            var belowGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Below);
            Assert.AreEqual(belowGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above), grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), belowGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 0, 0), belowGrandChildCoords.GetCoord(1));

            var furterBelowGrandChildCoords = belowGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Below);
            Assert.AreEqual(furterBelowGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above),
                belowGrandChildCoords);

            Assert.AreEqual(2, furterBelowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 0, 1), furterBelowGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furterBelowGrandChildCoords.GetCoord(1));

            var behindGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Back);
            Assert.AreEqual(behindGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Forward),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), behindGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), behindGrandChildCoords.GetCoord(1));

            var inFrontOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Forward);
            Assert.AreEqual(inFrontOfGrandChildCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Back),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), inFrontOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), inFrontOfGrandChildCoords.GetCoord(1));

            const int incorrectSide = 6;
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((OctreeNode.NeighbourSide) incorrectSide));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((OctreeNode.NeighbourSide) incorrectSide + 1));

            var rootCoords = new OctreeNodeCoordinates();

            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Above));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Below));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Back));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Forward));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Left));
            Assert.IsNull(rootCoords.GetNeighbourCoords(OctreeNode.NeighbourSide.Right));
        }

        [Test]
        public void String() {
            Assert.AreEqual("[0, 1, 0]", new OctreeChildCoordinates(0, 1, 0).ToString());
            Assert.AreEqual("[0, 0, 1]", new OctreeChildCoordinates(0, 0, 1).ToString());
            Assert.AreEqual("[1, 0, 0]", new OctreeChildCoordinates(1, 0, 0).ToString());

            Assert.AreEqual("[ [1, 0, 0], [0, 1, 0] ]", new OctreeNodeCoordinates(new[] {
                new OctreeChildCoordinates(1, 0, 0),
                new OctreeChildCoordinates(0, 1, 0)
            }).ToString());
        }

        [Test]
        public void ToAndFromIndices() {
            var wantedCoords = new OctreeChildCoordinates(0, 1, 1);
            var actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopFwdLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopFwdLeft, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(1, wantedCoords.y, wantedCoords.z);
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopFwdRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopFwdRight, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoordinates(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopBackLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopBackLeft, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(1, wantedCoords.y, wantedCoords.z);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.TopBackRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.TopBackRight, actualCoords.ToIndex());

            //bot
            //left
            //fwd
            wantedCoords = new OctreeChildCoordinates(0, 0, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotFwdLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotFwdLeft, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(1, wantedCoords.y, wantedCoords.z);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotFwdRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotFwdRight, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoordinates(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotBackLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotBackLeft, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(1, wantedCoords.y, wantedCoords.z);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BotBackRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BotBackRight, actualCoords.ToIndex());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => { actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.Invalid); });

            //and some invalid ones!
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(-1, 0, 1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(0, -1, 1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(0, 0, -1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(1, 2, 0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(1, 0, 2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2, 0, 0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2, 0, 2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2, 2, 2).ToIndex());
        }
    }
}