
using Assets.Scripts;
using NUnit.Framework;
using UnityEngine;

namespace OctreeTest
{
    [TestFixture]
    [Category("Performance")]
    internal class TreeModificationTests
    {
        [TestFixtureSetUp]
        public void Init() {
            
        }

        [SetUp]
        public void SetUp() {
            
        }

        [TearDown]
        public void Teardown() {
            
        }

        private readonly int size = 10000;

        [Test]
        public void Test() {
            var voxelTree = new VoxelTree(Vector3.zero, Vector3.one * size);

            voxelTree.GetRoot().AddChild(OctreeNode.ChildIndex.LeftAboveBack).SetItem(4);
            voxelTree.GetRoot().AddChild(OctreeNode.ChildIndex.RightAboveForward).SetItem(5);


        }
        /*
        [ [1, 1, 0], [0, 0, 1], [0, 0, 1], [0, 0, 1], [0, 0, 1], [0, 0, 1], [0, 0, 1], [0, 0, 1], [0, 1, 1], [0, 0, 0], [0, 1, 0], [0, 1, 1], [0, 1, 1], [0, 0, 0], [0, 0, 0], [0, 1, 1], [0, 1, 0], [0, 0, 0], [0, 1, 1], [0, 0, 1] ]
UnityEngine.Debug:Log(Object)
Vox:Update() (at Assets/Scripts/Vox.cs:106)

*/
    }
}
