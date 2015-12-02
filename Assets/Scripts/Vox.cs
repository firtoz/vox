using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Vox : MonoBehaviour {
    public float size = 100.0f;

    public VoxelTree voxelTree = new VoxelTree(Vector3.zero, Vector3.one * 100);

    // Use this for initialization
    private void Start() {}

    public void OnEnable() {
        voxelTree = new VoxelTree(Vector3.zero, Vector3.one * size);

        for (var i = 0; i < indices.Count; i++) {
            var index = indices[i];

            voxelTree.SetMaterial(index, materials[i]);
//            voxelTree.SetMaterial(i, 
        }
        //                vox.octree.AddBounds(new Bounds(new Vector3(0, 0.1f, -0.4f), Vector3.one), 5, 8);
        //                vox.octree.AddBounds(new Bounds(new Vector3(0, -.75f, -0.35f), Vector3.one*0.5f), 6, 8);
        //                vox.octree.AddBounds(new Bounds(new Vector3(0.25f, -.35f, -0.93f), Vector3.one*0.7f), 7, 8);

        //                vox.octree.GetRoot().RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);
        voxelTree.GetRoot().AddChild(OctreeNode.ChildIndex.TopFwdRight).SetItem(4);
        voxelTree.GetRoot().AddChild(OctreeNode.ChildIndex.TopBackLeft).SetItem(5);
//        octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopBackRight).SetItem(4);

//        topFwdLeft.SetItem(4);
//        topFwdLeft.SubDivide();
//
//        topFwdLeft.RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);

        //                topFwdLeft.SubDivide();
        voxelTree.Render(gameObject);

//        octree.ApplyToMesh(GetComponent<MeshFilter>().sharedMesh);
    }

    public bool useDepth = true;
    public int wantedDepth = 5;

    public List<int> indices = new List<int> {4, 5};
    public List<Material> materials = new List<Material>();

    public int materialIndex = 0;

    // Update is called once per frame
    private void Update() {
        if (addBoundsNextFrame) {
            addBoundsNextFrame = false;

            //                vox.octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one*7.5f));

            //                vox.octree.AddBounds(new Bounds(new Vector3(0, 0.1f, -0.4f), Vector3.one*4), 5, 6);
            //                vox.octree.AddBounds(new Bounds(new Vector3(0, -.75f, -0.35f), Vector3.one*0.5f), 6, 8);
            voxelTree.AddBounds(
                new Bounds(
                    new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * 20.0f,
                    new Vector3(Random.Range(0.1f, 10.0f), Random.Range(0.1f, 10.0f), Random.Range(0.1f, 10.0f))), 7, 8);

            //                vox.octree.GetRoot().RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);
            //                var topFwdLeft = vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopFwdLeft);
            //                topFwdLeft.SetItem(4);

            //                topFwdLeft.SubDivide();
            //                vox.octree.ProcessDrawQueue();
//            Undo.RegisterCompleteObjectUndo(vox.gameObject, "Modify");
            voxelTree.Render(gameObject);
//            EditorUtility.SetDirty(vox.gameObject);
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            wantedDepth--;
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            wantedDepth++;
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            materialIndex = (materialIndex + indices.Count + 1) % indices.Count;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            materialIndex = (materialIndex + 1) % indices.Count;
        }
        if (voxelTree == null) {
            return;
        }

        Profiler.BeginSample("Intersect");
        RayIntersectionResult<int> result;

        if (useDepth) {
            voxelTree.Intersect(transform, Camera.main.ScreenPointToRay(Input.mousePosition), out result, wantedDepth);
        } else {
            voxelTree.Intersect(transform, Camera.main.ScreenPointToRay(Input.mousePosition), out result);
        }

        Profiler.EndSample();

        if (result.hit) {
            if (Press(0)) {
                var neighbourCoords = result.coordinates.GetNeighbourCoords(result.neighbourSide);
                if (neighbourCoords != null) {
//                    Debug.Log(neighbourCoords);
                    Profiler.BeginSample("AddRecursive");
                    var final = voxelTree.GetRoot().AddRecursive(neighbourCoords);
                    final.SetItem(indices[materialIndex], true);
                    Profiler.EndSample();

                    Profiler.BeginSample("Render");
                    voxelTree.Render(gameObject);
                    Profiler.EndSample();
                }
            }
            if (Press(1))
            {
                Profiler.BeginSample("RemoveRecursive");
                voxelTree.GetRoot().RemoveRecursive(result.coordinates, true);
                Profiler.EndSample();

                Profiler.BeginSample("Render");
                voxelTree.Render(gameObject);
                Profiler.EndSample();
            }
            if (Press(2)) {
                Debug.Log(result.coordinates + " : " + result.neighbourSide);
            }
        }
    }

    private static bool Press(int button) {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            return Input.GetMouseButton(button);
        }

        return Input.GetMouseButtonDown(button);
    }

    //    public void AddBounds(Bounds bounds)
    //    {
    //        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        cube.transform.position = bounds.center;
    //        cube.transform.localScale = bounds.size;
    //
    //#if UNITY_EDITOR
    //        Undo.RegisterCreatedObjectUndo(cube, "Add Bounds");
    //        Undo.SetTransformParent(cube.transform, transform, "Add Bounds");
    //#else
    //        cube.transform.parent = transform;
    //#endif
    //
    //        octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one * 100));
    //
    //        octree.AddBounds(bounds, 8);
    //    }

    private void DrawLocalLine(Vector3 a, Vector3 b) {
        Gizmos.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b));
    }

    public bool showGizmos = false;
    public bool addBoundsNextFrame = false;

    public void OnDrawGizmosSelected() {
        if (voxelTree != null && showGizmos) {
            foreach (var node in voxelTree.DepthFirst()) {
                Color color;

                if (node.IsLeafNode()) {
                    color = Color.red;
                } else {
//                    continue;
                    color = Color.white;
                }
                var bounds = node.GetBounds();

                var min = bounds.min;
                var max = bounds.max;

                Gizmos.color = color;

                DrawLocalLine(min, new Vector3(min.x, min.y, max.z));
                DrawLocalLine(min, new Vector3(min.x, max.y, min.z));
                DrawLocalLine(min, new Vector3(max.x, min.y, min.z));

                DrawLocalLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));

                DrawLocalLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z));
                DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z));
                DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z));

                DrawLocalLine(max, new Vector3(max.x, max.y, min.z));
                DrawLocalLine(max, new Vector3(max.x, min.y, max.z));
                DrawLocalLine(max, new Vector3(min.x, max.y, max.z));

                DrawLocalLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z));
                DrawLocalLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z));
            }
        }
    }
}