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
}
