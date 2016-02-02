using System.Collections.Generic;
using UnityEngine;

public class Vox : MonoBehaviour {
	public bool addBoundsNextFrame;

	public List<int> indices = new List<int> {4, 5};

	public int materialIndex;
	public List<Material> materials = new List<Material>();

	private RayIntersectionResult _result;

	public bool showGizmos = false;
	public float size = 100.0f;

	public bool useDepth = true;

	public VoxelTree voxelTree = new VoxelTree(Vector3.zero, Vector3.one * 100);
	public int wantedDepth = 5;

	public void OnEnable() {
		voxelTree = new VoxelTree(Vector3.zero, Vector3.one * size);

		voxelTree.SetGameObject(gameObject);

		for (var i = 0; i < indices.Count; i++) {
			var index = indices[i];

			voxelTree.SetMaterial(index, materials[i]);
//            voxelTree.SetMaterial(i, 
		}
		//                vox.octree.AddBounds(new Bounds(new Vector3(0, 0.1f, -0.4f), Vector3.one), 5, 8);
		//                vox.octree.AddBounds(new Bounds(new Vector3(0, -.75f, -0.35f), Vector3.one*0.5f), 6, 8);
		//                vox.octree.AddBounds(new Bounds(new Vector3(0.25f, -.35f, -0.93f), Vector3.one*0.7f), 7, 8);

		//                vox.octree.GetRoot().RemoveChild(OctreeNode.ChildIndex.RightAboveBack);
		voxelTree.GetRoot().AddChild(OctreeNode.ChildIndex.LeftAboveBack).SetItem(4);
		voxelTree.GetRoot().AddChild(OctreeNode.ChildIndex.RightAboveForward).SetItem(5);
//        octree.GetRoot().AddChild(OctreeNode.ChildIndex.LeftAboveForward).SetItem(4);

//        topFwdLeft.SetItem(4);
//        topFwdLeft.SubDivide();
//
//        topFwdLeft.RemoveChild(OctreeNode.ChildIndex.RightAboveBack);

		//                topFwdLeft.SubDivide();
		voxelTree.Render();

//        octree.ApplyToMesh(GetComponent<MeshFilter>().sharedMesh);
	}

	public bool debugRaycasts;

	// Update is called once per frame
	public void Update() {
		if (addBoundsNextFrame) {
			addBoundsNextFrame = false;

			//                vox.octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one*7.5f));

			//                vox.octree.AddBounds(new Bounds(new Vector3(0, 0.1f, -0.4f), Vector3.one*4), 5, 6);
			//                vox.octree.AddBounds(new Bounds(new Vector3(0, -.75f, -0.35f), Vector3.one*0.5f), 6, 8);
			voxelTree.AddBounds(
				new Bounds(
					new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * 20.0f,
					new Vector3(Random.Range(0.1f, 10.0f), Random.Range(0.1f, 10.0f), Random.Range(0.1f, 10.0f))), 7, 8);

			//                vox.octree.GetRoot().RemoveChild(OctreeNode.ChildIndex.RightAboveBack);
			//                var topFwdLeft = vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.RightAboveBack);
			//                topFwdLeft.SetItem(4);

			//                topFwdLeft.SubDivide();
			//                vox.octree.ProcessDrawQueue();
//            Undo.RegisterCompleteObjectUndo(vox.gameObject, "Modify");
			voxelTree.Render();
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
		if (Input.GetKeyDown(KeyCode.L)) {
			materialIndex = (materialIndex + 1) % indices.Count;
		}
		if (voxelTree == null) {
			return;
		}

		Profiler.BeginSample("Intersect");

		if (useDepth) {
			voxelTree.Intersect(transform, Camera.main.ScreenPointToRay(Input.mousePosition), out _result, wantedDepth, debugRaycasts);
		} else {
			voxelTree.Intersect(transform, Camera.main.ScreenPointToRay(Input.mousePosition), out _result, null, debugRaycasts);
		}

		Profiler.EndSample();

		if (_result.hit) {
			var resultTree = (VoxelTree) _result.tree;

			DrawBounds(resultTree.GetNeighbourBoundsForChild(_result.coords, _result.neighbourSide), Color.yellow, false);

			if (Press(0)) {
				var neighbourCoordsResult = resultTree.GetNeighbourCoordsInfinite(_result.coords, _result.neighbourSide);

				var neighbourTree = neighbourCoordsResult.tree;
				DrawBounds(neighbourTree.GetRoot().GetChildBounds(neighbourCoordsResult.coordsResult), Color.green, false);

				var neighbourCoords = neighbourCoordsResult.coordsResult;

				if (neighbourCoords != null
//                    && neighbourCoords.GetTree() != null
					) {
//                    Debug.Log(neighbourCoords);
					Profiler.BeginSample("AddRecursive");

					var final = neighbourTree.GetRoot().AddRecursive(neighbourCoords);
					final.SetItem(indices[materialIndex], true);
					Profiler.EndSample();

					Profiler.BeginSample("Render");
					neighbourTree.Render();
					Profiler.EndSample();
				}
			}
			if (Press(1)) {
				Profiler.BeginSample("RemoveRecursive");
				resultTree.GetRoot().RemoveRecursive(_result.coords, true);
				Profiler.EndSample();

				Profiler.BeginSample("Render");
				resultTree.Render();
				Profiler.EndSample();
			}
			if (Press(2)) {
				Debug.Log(_result.coords + " : " + _result.neighbourSide);
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

	private void DrawLocalLine(Vector3 a, Vector3 b, bool gizmos, Color debugColor) {
		if (gizmos) {
			Gizmos.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b));
		} else {
			Debug.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), debugColor);
		}
	}

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

				DrawBounds(bounds, color);
			}

			var superVoxelTree = voxelTree.GetOwnerNode().GetTree();

			foreach (var node in superVoxelTree.DepthFirst())
			{
				Color color;

				if (node.IsLeafNode())
				{
					color = Color.green;
					if (node.GetItem() != null) {

						foreach (var subNode in node.GetItem().DepthFirst())
						{
							Color color2;

							if (subNode.IsLeafNode())
							{
								color2 = Color.red;
							}
							else {
								//                    continue;
								color2 = Color.white;
							}
							var bounds2 = subNode.GetBounds();

							DrawBounds(bounds2, color2);
						}

					}
				}
				else {
					//                    continue;
					color = Color.blue;
				}
				var bounds = node.GetBounds();

				DrawBounds(bounds, color);
			}
		}
	}

	private void DrawBounds(Bounds bounds, Color color, bool gizmos = true) {
		var min = bounds.min;
		var max = bounds.max;

		if (gizmos) {
			Gizmos.color = color;
		}

		DrawLocalLine(min, new Vector3(min.x, min.y, max.z), gizmos, color);
		DrawLocalLine(min, new Vector3(min.x, max.y, min.z), gizmos, color);
		DrawLocalLine(min, new Vector3(max.x, min.y, min.z), gizmos, color);

		DrawLocalLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), gizmos, color);

		DrawLocalLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), gizmos, color);
		DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z), gizmos, color);
		DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z), gizmos, color);

		DrawLocalLine(max, new Vector3(max.x, max.y, min.z), gizmos, color);
		DrawLocalLine(max, new Vector3(max.x, min.y, max.z), gizmos, color);
		DrawLocalLine(max, new Vector3(min.x, max.y, max.z), gizmos, color);

		DrawLocalLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), gizmos, color);
		DrawLocalLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), gizmos, color);
	}
}