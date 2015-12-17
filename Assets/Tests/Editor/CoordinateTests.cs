using System;
using Assets.Scripts;
using NUnit.Framework;
using UnityEngine;

namespace OctreeTest {

    [TestFixture]
    [Category("Octree Tests")]
    internal class CoordinateTests {

        private class TestOctreeNode<T> : OctreeNodeBase<T, TestOctree<T>, TestOctreeNode<T>>
        {
            public TestOctreeNode(Bounds bounds, TestOctree<T> tree) : base(bounds, tree) { }
            public TestOctreeNode(Bounds bounds, TestOctreeNode<T> parent, ChildIndex indexInParent, int depth, TestOctree<T> tree) : base(bounds, parent, indexInParent, depth, tree) { }
        }

        private class TestOctree<T> : OctreeBase<T, TestOctreeNode<T>, TestOctree<T>>
        {
            public TestOctree(Bounds bounds) : base(CreateRootNode, bounds) { }

//            protected override 

            private static TestOctreeNode<T> CreateRootNode(TestOctree<T> self, Bounds bounds)
            {
                return new TestOctreeNode<T>(bounds, self);
            }

            public override TestOctreeNode<T> ConstructNode(Bounds bounds, TestOctreeNode<T> parent, 
                OctreeNode.ChildIndex indexInParent, int depth) {
                return new TestOctreeNode<T>(bounds, parent, indexInParent, depth, this);
            }

            protected override int GetItemMeshId(T item)
            {
                return 0;
            }

            protected override Material GetMeshMaterial(int meshId)
            {
                return new Material(Shader.Find("Standard"));
            }

            protected override TestOctree<T> CreateNeighbour(NeighbourSide side)
            {
                var neighbourBounds = GetNeighbourBounds(side);
                return new TestOctree<T>(neighbourBounds);
            }
        }

        private class TestOctreeCoords : OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>> {
            public TestOctreeCoords(TestOctree<int> tree) : base(tree) {}
            public TestOctreeCoords(TestOctree<int> tree, OctreeNodeCoordinates<int, TestOctreeNode<int>, TestOctree<int>> parentCoordinates, params OctreeChildCoordinates[] furtherChildren) : base(tree, parentCoordinates, furtherChildren) {}
            public TestOctreeCoords(TestOctree<int> tree, OctreeChildCoordinates[] coords) : base(tree, coords) {}
        }

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

            Assert.AreEqual(new TestOctreeCoords(null, new TestOctreeCoords(null)), new TestOctreeCoords(null));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates()), new OctreeChildCoordinates());

            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(1, 0, 0)), new OctreeChildCoordinates(1, 0, 0));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(0, 1, 0)), new OctreeChildCoordinates(0, 1, 0));
            Assert.AreEqual(new OctreeChildCoordinates(new OctreeChildCoordinates(0, 0, 1)), new OctreeChildCoordinates(0, 0, 1));

            // same constructor, equal
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }));

            // construct from parent with no coords
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new TestOctreeCoords(null),
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)));

            // construct from parent with no coords
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new TestOctreeCoords(null),
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)));

            Assert.IsTrue(Equals(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new TestOctreeCoords(null),
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0))));

            Assert.IsFalse(Equals(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), null));

            Assert.IsFalse(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }).Equals(null));

            var a = new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            });

            var b = a;

            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(b));

            // construct from parent with one coord
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1)
            }), new OctreeChildCoordinates(0, 1, 0)));

            // construct from parent with two coords
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            })));

            // get parent coordinates
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0),
                new OctreeChildCoordinates(1, 1, 0)
            }).GetParentCoordinates());

            // get parent coordinates
            Assert.AreEqual(new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }), new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0),
                new OctreeChildCoordinates(0, 1, 1)
            }).GetParentCoordinates());
        }

        [Test]
        public void NeighboursTest() {
            var grandChildCoords = new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(1, 1, 1),
                new OctreeChildCoordinates(0, 1, 0)
            });

            Assert.AreEqual(2, grandChildCoords.Length);
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), grandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), grandChildCoords.GetCoord(1));
            var rightOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Right);

            Assert.AreEqual(2, rightOfGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), rightOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), rightOfGrandChildCoords.GetCoord(1));

            Assert.NotNull(rightOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Left));

            Assert.AreNotEqual(rightOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Left),
                rightOfGrandChildCoords);

            var furtherRightCoords = rightOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Right);

            // uh oh, we just went out of bounds
            Assert.AreEqual(2, furtherRightCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), furtherRightCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 0), furtherRightCoords.GetCoord(1));

            furtherRightCoords = furtherRightCoords.GetNeighbourCoords(NeighbourSide.Right);

            Assert.IsNotNull(furtherRightCoords, "furtherLeftCoords"); // not null because it will just look at another tree instead

            var leftOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Left);

            Assert.AreEqual(leftOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Right), grandChildCoords);


            Assert.AreEqual(2, leftOfGrandChildCoords.Length);
            // and back again
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), leftOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), leftOfGrandChildCoords.GetCoord(1));

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
            
            // 111, 010, behind, z-1
            // 110, 011
            var behindGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Back);
            Assert.AreEqual(behindGrandChildCoords.GetNeighbourCoords(NeighbourSide.Forward),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 0), behindGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), behindGrandChildCoords.GetCoord(1));

            var inFrontOfGrandChildCoords = grandChildCoords.GetNeighbourCoords(NeighbourSide.Forward);
            Assert.AreEqual(inFrontOfGrandChildCoords.GetNeighbourCoords(NeighbourSide.Back),
                grandChildCoords);

            Assert.AreEqual(2, belowGrandChildCoords.Length);

            Assert.AreEqual(new OctreeChildCoordinates(1, 1, 1), inFrontOfGrandChildCoords.GetCoord(0));
            Assert.AreEqual(new OctreeChildCoordinates(0, 1, 1), inFrontOfGrandChildCoords.GetCoord(1));

            const int incorrectSide = 6;
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((NeighbourSide) incorrectSide));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grandChildCoords.GetNeighbourCoords((NeighbourSide) incorrectSide + 1));

            var rootCoords = new TestOctreeCoords(null);

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

            Assert.AreEqual("[ [0, 0, 1], [0, 1, 0] ]", new TestOctreeCoords(null, new[] {
                new OctreeChildCoordinates(0, 0, 1),
                new OctreeChildCoordinates(0, 1, 0)
            }).ToString());
        }

        [Test]
        public void ToAndFromIndices() {
            var wantedCoords = new OctreeChildCoordinates(1, 1, 0);
            var actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.RightAboveBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightAboveBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);
            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.RightAboveForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightAboveForward, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoordinates(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.LeftAboveBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftAboveBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.LeftAboveForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftAboveForward, actualCoords.ToIndex());

            //bot
            //left
            //fwd
            wantedCoords = new OctreeChildCoordinates(1, 0, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.RightBelowBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightBelowBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.RightBelowForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.RightBelowForward, actualCoords.ToIndex());

            //left
            //back
            wantedCoords = new OctreeChildCoordinates(0, wantedCoords.y, 0);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.LeftBelowBack);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftBelowBack, actualCoords.ToIndex());

            //right
            wantedCoords = new OctreeChildCoordinates(wantedCoords.x, wantedCoords.y, 1);

            actualCoords = OctreeChildCoordinates.FromIndex(OctreeNode.ChildIndex.LeftBelowForward);
            Assert.AreEqual(wantedCoords, actualCoords);
            Assert.AreEqual(OctreeNode.ChildIndex.LeftBelowForward, actualCoords.ToIndex());

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