using System;
using Assets.Scripts;
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

            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new OctreeNodeCoordinates<int>(null)), new OctreeNodeCoordinates<int>(null));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates()), new OctreeChildCoordinates());

            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(1, 0, 0)), new OctreeChildCoordinates(1, 0, 0));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(0, 1, 0)), new OctreeChildCoordinates(0, 1, 0));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(0, 0, 1)), new OctreeChildCoordinates(0, 0, 1));

            // same constructor, equal
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }));

            // construct from parent with no coords
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new OctreeNodeCoordinates<int>(null),
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)));

            // construct from parent with no coords
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new OctreeNodeCoordinates<int>(null),
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)));

            Assert.IsTrue(Equals(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new OctreeNodeCoordinates<int>(null),
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0))));

            Assert.IsFalse(Equals(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), null));

            Assert.IsFalse(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }).Equals(null));

            var a = new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            });

            var b = a;

            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(b));

            // construct from parent with one coord
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1)
            }), new OctreeChildCoordinates(0, 1, 0)));

            // construct from parent with two coords
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            })));

            // get parent coordinates
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0),
                new OctreeChildCoordinates(1, 1, 0)
            }).GetParentCoordinates());

            // get parent coordinates
            Assert.AreEqual(new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0),
                new OctreeChildCoordinates(0, 1, 1)
            }).GetParentCoordinates());
        }

        [Test]
        public void NeighboursTest() {
            var grandChildCoords = new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(1, 1, 1),
                new OctreeChildCoordinates(0, 1, 0)
            });

            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords.GetCoord(1));
            var leftOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Right);

            Assert.AreEqual(2, leftOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), leftOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), leftOfGrandChildCoords.GetCoord(1));

            Assert.NotNull(leftOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Left));

            Assert.AreNotEqual(leftOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Left),
                leftOfGrandChildCoords);

            var furtherLeftCoords = leftOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Right);

            Assert.AreEqual(2, furtherLeftCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), furtherLeftCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furtherLeftCoords.GetCoord(1));

            furtherLeftCoords = furtherLeftCoords.GetNeighbourCoords(NeighbourSide.Right);

            Assert.IsNotNull(furtherLeftCoords, "furtherLeftCoords"); // not null because it will just look at another tree instead

            var rightOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Left);

            Assert.AreEqual(rightOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Right), grandChildCoords);


            Assert.AreEqual(2, rightOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), rightOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), rightOfGrandChildCoords.GetCoord(1));

            var aboveGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Above);

            Assert.IsNotNull(aboveGrandChildCoords); // not null, just another tree!

            var belowGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Below);
            Assert.AreEqual(belowGrandChildCoords.GetNeighbourCoords(NeighbourSide.Above), grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), belowGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 0, 0), belowGrandChildCoords.GetCoord(1));

            var furterBelowGrandChildCoords = belowGrandChildCoords.GetNeighbourCoords(NeighbourSide.Below);
            Assert.AreEqual(furterBelowGrandChildCoords.GetNeighbourCoords(NeighbourSide.Above),
                belowGrandChildCoords);

            Assert.AreEqual(2, furterBelowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 0, 1), furterBelowGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furterBelowGrandChildCoords.GetCoord(1));

            var behindGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Forward);
            Assert.AreEqual(behindGrandChildCoords.GetNeighbourCoords(NeighbourSide.Back),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), behindGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), behindGrandChildCoords.GetCoord(1));

            var inFrontOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Back);
            Assert.AreEqual(inFrontOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Forward),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), inFrontOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), inFrontOfGrandChildCoords.GetCoord(1));

            const int incorrectSide = 6;
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((NeighbourSide) incorrectSide));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((NeighbourSide) incorrectSide + 1));

            var rootCoords = new OctreeNodeCoordinates<int>(null);

            Assert.IsNull(rootCoords.GetNeighbourCoords(NeighbourSide.Above));
            Assert.IsNull(rootCoords.GetNeighbourCoords(NeighbourSide.Below));
            Assert.IsNull(rootCoords.GetNeighbourCoords(NeighbourSide.Forward));
            Assert.IsNull(rootCoords.GetNeighbourCoords(NeighbourSide.Back));
            Assert.IsNull(rootCoords.GetNeighbourCoords(NeighbourSide.Right));
            Assert.IsNull(rootCoords.GetNeighbourCoords(NeighbourSide.Left));
        }

        [Test]
        public void String() {
            Assert.AreEqual("[0, 1, 0]", new OctreeChildCoordinates(0, 1, 0).ToString());
            Assert.AreEqual("[1, 0, 0]", new OctreeChildCoordinates(1, 0, 0).ToString());
            Assert.AreEqual("[0, 0, 1]", new OctreeChildCoordinates(0, 0, 1).ToString());

            Assert.AreEqual("[ [0, 0, 1], [0, 1, 0] ]", new OctreeNodeCoordinates<int>(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }).ToString());
        }

        [Test]
        public void ToAndFromIndices() {
            var wantedCoords = new OctreeChildCoordinates(1, 1, 0);
            var actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.AboveBackRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.AboveBackRight, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.AboveBackLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.AboveBackLeft, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoordinates(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.AboveForwardRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.AboveForwardRight, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.AboveForwardLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.AboveForwardLeft, actualCoords.ToIndex());

            //bot
            //left
            //fwd
            wantedCoords = new OctreeChildCoordinates(1, 0, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BelowBackRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BelowBackRight, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BelowBackLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BelowBackLeft, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoordinates(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BelowForwardRight);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BelowForwardRight, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.BelowForwardLeft);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.BelowForwardLeft, actualCoords.ToIndex());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => { actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.Invalid); });

            //and some invalid ones!
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(1, 0, -1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(1, -1, 0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(-1, 0, 0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(0, 2, 1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2, 0, 1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(0, 0, 2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2, 0, 2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoordinates(2, 2, 2).ToIndex());
        }
    }
}