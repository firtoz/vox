using System;
using NUnit.Framework;
using UnityEngine;

namespace OctreeTest {
    internal class TestOctreeNode<T> : OctreeNodeBase<T, TestOctree<T>, TestOctreeNode<T>> {
        public TestOctreeNode(Bounds bounds, TestOctree<T> tree) : base(bounds, tree) {}

        public TestOctreeNode(Bounds bounds, TestOctreeNode<T> parent, ChildIndex indexInParent, int depth,
            TestOctree<T> tree) : base(bounds, parent, indexInParent, depth, tree) {}
    }

    internal class TestOctree<T> : OctreeBase<T, TestOctreeNode<T>, TestOctree<T>> {
        internal TestOctree(Bounds bounds) : base(CreateRootNode, bounds) {}

        private static TestOctreeNode<T> CreateRootNode(TestOctree<T> self, Bounds bounds) {
            return new TestOctreeNode<T>(bounds, self);
        }

        public override TestOctreeNode<T> ConstructNode(Bounds bounds, TestOctreeNode<T> parent,
            OctreeNode.ChildIndex indexInParent, int depth) {
            return new TestOctreeNode<T>(bounds, parent, indexInParent, depth, this);
        }
    }

    [TestFixture]
    [Category("Octree Tests")]
    internal class CoordsTests {
        [Test]
        public void Equality() {
            Assert.False(new OctreeChildCoords(1, 0, 1).Equals(null));

            // hash collisions and general equality

            for (var i = 0; i < 8; i++) {
                var firstIndex = (OctreeNode.ChildIndex) i;
                for (var j = 0; j < 8; j++) {
                    var secondIndex = (OctreeNode.ChildIndex) j;

                    var firstCoordinate = OctreeChildCoords.FromIndex(firstIndex);
                    var secondCoordinate = OctreeChildCoords.FromIndex(secondIndex);

                    if (i == j) {
                        Assert.AreEqual(firstCoordinate, secondCoordinate);
                        Assert.AreEqual(firstCoordinate.GetHashCode(), secondCoordinate.GetHashCode());
                    } else {
                        Assert.AreNotEqual(firstCoordinate, secondCoordinate);
                        Assert.AreNotEqual(firstCoordinate.GetHashCode(), secondCoordinate.GetHashCode());
                    }
                }
            }

            Assert.AreEqual(new Coords(new Coords()), new Coords());
            Assert.AreEqual(new OctreeChildCoords(new OctreeChildCoords()), new OctreeChildCoords());

            Assert.AreEqual(new OctreeChildCoords(new OctreeChildCoords(1, 0, 0)),
                new OctreeChildCoords(1, 0, 0));
            Assert.AreEqual(new OctreeChildCoords(new OctreeChildCoords(0, 1, 0)),
                new OctreeChildCoords(0, 1, 0));
            Assert.AreEqual(new OctreeChildCoords(new OctreeChildCoords(0, 0, 1)),
                new OctreeChildCoords(0, 0, 1));

            // same constructor, equal
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }));

            // construct from parent with no _coords
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new Coords(),
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)));

            // construct from parent with no _coords
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new Coords(),
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)));

            Assert.IsTrue(Equals(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new Coords(),
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0))));

            Assert.IsFalse(Equals(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), null));

            Assert.IsFalse(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }).Equals(null));

            var a = new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            });

            var b = a;

            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(b));

            // construct from parent with one coord
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1)
            }), new OctreeChildCoords(0, 1, 0)));

            // construct from parent with two _coords
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            })));

            // get parent Coords
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0),
                new OctreeChildCoords(1, 1, 0)
            }).GetParentCoords());

            // get parent Coords
            Assert.AreEqual(new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }), new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0),
                new OctreeChildCoords(0, 1, 1)
            }).GetParentCoords());
        }

        [Test]
        public void NeighboursTest() {
            var grandChildCoords = new Coords(new[] {
                new OctreeChildCoords(1, 1, 1),
                new OctreeChildCoords(0, 1, 0)
            });

            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoords(1, 1, 1), grandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(0, 1, 0), grandChildCoords.GetCoord(1));
            var rightOfGrandChildCoords = VoxelNode.GetNeighbourCoords(grandChildCoords, NeighbourSide.Right);

            Assert.AreEqual(2, rightOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoords(1, 1, 1), rightOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(1, 1, 0), rightOfGrandChildCoords.GetCoord(1));

            Assert.NotNull(VoxelNode.GetNeighbourCoords(rightOfGrandChildCoords, NeighbourSide.Left));

            Assert.AreNotEqual(VoxelNode.GetNeighbourCoords(rightOfGrandChildCoords, NeighbourSide.Left),
                rightOfGrandChildCoords);

            var furtherRightCoords = VoxelNode.GetNeighbourCoords(rightOfGrandChildCoords, NeighbourSide.Right);

            // uh oh, we just went out of bounds
//          Assert.AreEqual(2, furtherRightCoords.Length);
            Assert.IsNull(furtherRightCoords);

//            Assert.AreEqual(new OctreeChildCoords(0, 1, 1), furtherRightCoords.GetCoord(0));
//            Assert.AreEqual(new OctreeChildCoords(0, 1, 0), furtherRightCoords.GetCoord(1));

//            furtherRightCoords = VoxelNode.GetNeighbourCoords(furtherRightCoords, NeighbourSide.Right);

//            Assert.IsNotNull(furtherRightCoords, "furtherLeftCoords");
                // not null because it will just look at another voxelTree instead

            var leftOfGrandChildCoords = VoxelNode.GetNeighbourCoords(grandChildCoords, NeighbourSide.Left);

            Assert.AreEqual(VoxelNode.GetNeighbourCoords(leftOfGrandChildCoords, NeighbourSide.Right), grandChildCoords);


            Assert.AreEqual(2, leftOfGrandChildCoords.Length);
            // and back again
            Assert.AreEqual(new OctreeChildCoords(0, 1, 1), leftOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(1, 1, 0), leftOfGrandChildCoords.GetCoord(1));

            var aboveGrandChildCoords = VoxelNode.GetNeighbourCoords(grandChildCoords, NeighbourSide.Above);

//            Assert.IsNotNull(aboveGrandChildCoords); // not null, just another voxelTree!
            Assert.IsNull(aboveGrandChildCoords); // null, oob

            var belowGrandChildCoords = VoxelNode.GetNeighbourCoords(grandChildCoords, NeighbourSide.Below);
            Assert.AreEqual(VoxelNode.GetNeighbourCoords(belowGrandChildCoords, NeighbourSide.Above), grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoords(1, 1, 1), belowGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(0, 0, 0), belowGrandChildCoords.GetCoord(1));

            var furterBelowGrandChildCoords = VoxelNode.GetNeighbourCoords(belowGrandChildCoords, NeighbourSide.Below);
            Assert.AreEqual(VoxelNode.GetNeighbourCoords(furterBelowGrandChildCoords, NeighbourSide.Above),
                belowGrandChildCoords);

            Assert.AreEqual(2, furterBelowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoords(1, 0, 1), furterBelowGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(0, 1, 0), furterBelowGrandChildCoords.GetCoord(1));

            // 111, 010, behind, z-1
            // 110, 011
            var behindGrandChildCoords = VoxelNode.GetNeighbourCoords(grandChildCoords, NeighbourSide.Back);
            Assert.AreEqual(VoxelNode.GetNeighbourCoords(behindGrandChildCoords, NeighbourSide.Forward),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoords(1, 1, 0), behindGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(0, 1, 1), behindGrandChildCoords.GetCoord(1));

            var inFrontOfGrandChildCoords = VoxelNode.GetNeighbourCoords(grandChildCoords, NeighbourSide.Forward);
            Assert.AreEqual(VoxelNode.GetNeighbourCoords(inFrontOfGrandChildCoords, NeighbourSide.Back),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoords(1, 1, 1), inFrontOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoords(0, 1, 1), inFrontOfGrandChildCoords.GetCoord(1));

            const int incorrectSide = 6;
            Assert.Throws<ArgumentOutOfRangeException>(
                () => VoxelNode.GetNeighbourCoords(grandChildCoords, (NeighbourSide) incorrectSide));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => VoxelNode.GetNeighbourCoords(grandChildCoords, (NeighbourSide) incorrectSide + 1));

            var rootCoords = new Coords();

            Assert.IsNull(VoxelNode.GetNeighbourCoords(rootCoords, NeighbourSide.Above));
            Assert.IsNull(VoxelNode.GetNeighbourCoords(rootCoords, NeighbourSide.Below));
            Assert.IsNull(VoxelNode.GetNeighbourCoords(rootCoords, NeighbourSide.Forward));
            Assert.IsNull(VoxelNode.GetNeighbourCoords(rootCoords, NeighbourSide.Back));
            Assert.IsNull(VoxelNode.GetNeighbourCoords(rootCoords, NeighbourSide.Right));
            Assert.IsNull(VoxelNode.GetNeighbourCoords(rootCoords, NeighbourSide.Left));
        }

        [Test]
        public void String() {
            Assert.AreEqual("[0, 1, 0]", new OctreeChildCoords(0, 1, 0).ToString());
            Assert.AreEqual("[1, 0, 0]", new OctreeChildCoords(1, 0, 0).ToString());
            Assert.AreEqual("[0, 0, 1]", new OctreeChildCoords(0, 0, 1).ToString());

            Assert.AreEqual("[ [0, 0, 1], [0, 1, 0] ]", new Coords(new[] {
                new OctreeChildCoords(0, 0, 1),
                new OctreeChildCoords(0, 1, 0)
            }).ToString());
        }

        [Test]
        public void ToAndFromIndices() {
            var wantedCoords = new OctreeChildCoords(1, 1, 0);
            var actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightAboveBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightAboveBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoords(wantedCoords.x, wantedCoords.y, 1);
            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightAboveForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightAboveForward, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoords(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftAboveBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftAboveBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoords(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftAboveForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftAboveForward, actualCoords.ToIndex());

            //bot
            //left
            //fwd
            wantedCoords = new OctreeChildCoords(1, 0, 0);

            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightBelowBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightBelowBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoords(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightBelowForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightBelowForward, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoords(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftBelowBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftBelowBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoords(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftBelowForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftBelowForward, actualCoords.ToIndex());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => { actualCoords = OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.Invalid); });

            //and some invalid ones!
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(1, 0, -1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(1, -1, 0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(-1, 0, 0).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(0, 2, 1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(2, 0, 1).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(0, 0, 2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(2, 0, 2).ToIndex());
            Assert.AreEqual(OctreeNode.ChildIndex.Invalid, new OctreeChildCoords(2, 2, 2).ToIndex());
        }
    }
}