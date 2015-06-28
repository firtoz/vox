using UnityEditor;
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

        Undo.RegisterCreatedObjectUndo(cube, "Add Bounds");
        Undo.SetTransformParent(cube.transform, transform, "Add Bounds");

        octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one * 100));

        octree.AddBounds(bounds, 8);
    }
}
