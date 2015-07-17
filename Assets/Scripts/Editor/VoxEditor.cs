using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (Vox))]
public class VoxEditor : Editor {
    private Vector3[] normals;
    private int[] triangles;
    private Vector2[] uv;
    private Vector3[] vertices;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var vox = target as Vox;
        if (!vox) {
            return;
        }

        var meshFilter = vox.GetComponent<MeshFilter>();

        if (!meshFilter) {
            if (GUILayout.Button("New Mesh")) {
                Undo.RecordObject(vox.gameObject, "add new mesh");
                meshFilter = Undo.AddComponent<MeshFilter>(vox.gameObject);

                meshFilter.sharedMesh = new Mesh {
                    triangles = new int[0]
                };


                Undo.RegisterCreatedObjectUndo(meshFilter.sharedMesh, "add new mesh");

                EditorUtility.SetDirty(meshFilter);
                EditorUtility.SetDirty(vox.gameObject);
            }
            return;
        }
        var sharedMesh = meshFilter.sharedMesh;

        //        var currentNode = vox.octree.;
        //        int indent = EditorGUI.indentLevel;
        //        string foldoutString = "";
        //        if (currentNode != null) {
        //            if (!foldouts.ContainsKey(foldoutString)) {
        //                foldouts[foldoutString] = false;
        //            }
        //            
        //            foldouts[foldoutString] = EditorGUILayout.Foldout(foldouts[foldoutString], "Root Node");
        //
        //            if (foldouts[foldoutString]) {
        //                
        //            }
        //        }

        if (!sharedMesh) {
            if (GUILayout.Button("New Mesh")) {
                Undo.RecordObject(meshFilter, "add new mesh");
                meshFilter.sharedMesh = new Mesh {
                    triangles = new int[0]
                };
                EditorUtility.SetDirty(meshFilter);
            }
        }
        else {
            if (GUILayout.Button("Modify")) {
                var topFwdLeft = vox.octree.GetRoot().GetChild(OctreeNode.ChildIndex.TopFwdLeft);

                topFwdLeft.SubDivide();

                topFwdLeft.RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);
            }
            if (GUILayout.Button("Apply")) {
                using (new MeshModification(sharedMesh, "Modify")) {
                    sharedMesh.Clear();

                    sharedMesh.vertices = vertices;
                    sharedMesh.normals = normals;
                    sharedMesh.triangles = triangles;
                    sharedMesh.uv = uv;
                }
            }
            if (GUILayout.Button("GetVertices")) {
                var verts = new List<Vector3>();
                var norms = new List<Vector3>();
                var uvs = new List<Vector2>();
                var indices = new List<int>();

                foreach (var node in vox.octree.BreadthFirst().Where(node => node.IsLeafNode() && node.HasItem()))
                {
                    node.GetVertices(verts, indices, norms, uvs);
                }

                vertices = verts.ToArray();
                normals = norms.ToArray();
                uv = uvs.ToArray();
                triangles = indices.ToArray();
            }
            if (GUILayout.Button("Regenerate")) {
                vox.octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one*7.5f));

                vox.octree.AddBounds(new Bounds(new Vector3(0, 0.1f, -0.4f), Vector3.one), 5, 8);
                vox.octree.AddBounds(new Bounds(new Vector3(0, -.75f, -0.35f), Vector3.one*0.5f), 6, 8);
                vox.octree.AddBounds(new Bounds(new Vector3(0.25f, -.35f, -0.93f), Vector3.one*0.7f), 7, 8);

                vox.octree.GetRoot().RemoveChild(OctreeNode.ChildIndex.TopFwdLeft);
                var topFwdLeft = vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopFwdLeft);

                topFwdLeft.SetItem(4);
            }
        }
    }

    private void DrawBounds(Bounds bounds) {
        DrawBounds(bounds, Color.white);
    }

    private void DrawBounds(Bounds bounds, Color color) {
        var center = bounds.center;
        var extents = bounds.extents;

//        Debug.DrawLine(center - extents * 0.15f, center - extents * 0.125f, color);
//        Debug.DrawLine(center, center + extents, color);
//        return;

        var topBackLeft = center + new Vector3(-extents.x, extents.y, -extents.z);
        var topBackRight = topBackLeft + Vector3.right*extents.x*2;
        var topFwdLeft = topBackLeft + Vector3.forward*extents.z*2;
        var topFwdRight = topBackRight + Vector3.forward*extents.z*2;

        var botBackLeft = topBackLeft + Vector3.down*extents.y*2;
        var botBackRight = topBackRight + Vector3.down*extents.y*2;
        var botFwdLeft = topFwdLeft + Vector3.down*extents.y*2;
        var botFwdRight = topFwdRight + Vector3.down*extents.y*2;

        Debug.DrawLine(topBackLeft, topBackRight, color);
        Debug.DrawLine(topBackRight, topFwdRight, color);
        Debug.DrawLine(topFwdRight, topFwdLeft, color);
        Debug.DrawLine(topFwdLeft, topBackLeft, color);

        Debug.DrawLine(topBackLeft, botBackLeft, color);
        Debug.DrawLine(topBackRight, botBackRight, color);
        Debug.DrawLine(topFwdRight, botFwdRight, color);
        Debug.DrawLine(topFwdLeft, botFwdLeft, color);

        Debug.DrawLine(botBackLeft, botBackRight, color);
        Debug.DrawLine(botBackRight, botFwdRight, color);
        Debug.DrawLine(botFwdRight, botFwdLeft, color);
        Debug.DrawLine(botFwdLeft, botBackLeft, color);
    }

    public void OnSceneGUI() {
        var vox = target as Vox;

        if (vox == null) {}

//        DrawBounds(new Bounds(Vector3.zero, Vector3.one));

//        foreach (var node in vox.octree.DepthFirst())
//        {
//            if (!node.IsLeafNode()) continue;
//
//            var nodeBounds = node.GetBounds();
//            DrawBounds(nodeBounds, node.IsLeafNode() ? Color.red : Color.white);
//
////            var firstChildBounds = node.GetChildBounds(0);
////
////            DrawBounds(node.GetChildBounds(0), Color.red);
////            DrawBounds(node.GetChildBounds(1), Color.green);
////            DrawBounds(node.GetChildBounds(2), Color.blue);
////            DrawBounds(node.GetChildBounds(3), Color.yellow);
//        }
    }
}