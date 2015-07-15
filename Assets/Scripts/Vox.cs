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
	
	// Update is called once per frame
	void Update () {
	
	}

    public void AddBounds(Bounds bounds)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = bounds.center;
        cube.transform.localScale = bounds.size;

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(cube, "Add Bounds");
        Undo.SetTransformParent(cube.transform, transform, "Add Bounds");
#else
        cube.transform.parent = transform;
#endif

        octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one * 100));

        octree.AddBounds(bounds, 8);
    }

    public void OnDrawGizmosSelected() {
        if (octree != null) {
            foreach (var node in octree.DepthFirst()) {
                Color color;

                if (node.IsLeafNode()) {

                    color = Color.red;
                }
                else {
                    continue;
                    color = Color.white;
                }
                var bounds = node.GetBounds();

                var min = bounds.min;
                var max = bounds.max;

                Gizmos.color = color;

                Gizmos.DrawLine(min, new Vector3(min.x, min.y, max.z));
                Gizmos.DrawLine(min, new Vector3(min.x, max.y, min.z));
                Gizmos.DrawLine(min, new Vector3(max.x, min.y, min.z));

                Gizmos.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));

                Gizmos.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z));
                Gizmos.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z));
                Gizmos.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z));

                Gizmos.DrawLine(max, new Vector3(max.x, max.y, min.z));
                Gizmos.DrawLine(max, new Vector3(max.x, min.y, max.z));
                Gizmos.DrawLine(max, new Vector3(min.x, max.y, max.z));

                Gizmos.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z));
                Gizmos.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z));
            }
        }
    }
}
