#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Vox : MonoBehaviour {
    public float size = 100.0f;

    public Octree<int> octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one * 100));

	// Use this for initialization
	void Start () {
	    
	}

    public void OnEnable() {
        octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one * 7.5f));

        //                vox.octree.AddBounds(new Bounds(new Vector3(0, 0.1f, -0.4f), Vector3.one), 5, 8);
        //                vox.octree.AddBounds(new Bounds(new Vector3(0, -.75f, -0.35f), Vector3.one*0.5f), 6, 8);
        //                vox.octree.AddBounds(new Bounds(new Vector3(0.25f, -.35f, -0.93f), Vector3.one*0.7f), 7, 8);

        //                vox.octree.GetRoot().RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);
        octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopFwdRight).SetItem(4);
        octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopBackLeft).SetItem(4);
//        octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopBackRight).SetItem(4);

//        topFwdLeft.SetItem(4);
//        topFwdLeft.SubDivide();
//
//        topFwdLeft.RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);

        //                topFwdLeft.SubDivide();
        octree.ProcessDrawQueue();

        octree.ApplyToMesh(GetComponent<MeshFilter>().sharedMesh);
    }
	
	// Update is called once per frame
	void Update () {
	    if (octree != null) {
            octree.Intersect(transform, Camera.main.ScreenPointToRay(Input.mousePosition));
        }
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

    public void OnDrawGizmosSelected() {
        if (octree != null && showGizmos) {
            foreach (var node in octree.DepthFirst()) {
                Color color;

                if (node.IsLeafNode()) {

                    color = Color.red;
                }
                else {
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
