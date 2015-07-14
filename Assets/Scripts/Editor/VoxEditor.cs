using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Random = UnityEngine.Random;

[CustomEditor(typeof (Vox))]
public class VoxEditor : Editor {
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>(); 

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
        } else
        {
            if (GUILayout.Button("Regenerate"))
            {
                vox.octree = new Octree<int>(new Bounds(Vector3.zero, Vector3.one*5));

                vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.BotBackLeft);
                vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.BotFwdLeft);
                vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.BotBackRight);
                vox.octree.GetRoot().AddChild(OctreeNode.ChildIndex.TopFwdLeft);

                using (new MeshModification(sharedMesh, "Regenerate")) {
                                    
                    var vertices = new List<Vector3>();
                    var normals = new List<Vector3>();

                    foreach (var node in vox.octree.DepthFirst().Where(node => node.IsLeafNode()))
                    {
                        node.GetVertices(vertices, normals);
                    }

                    sharedMesh.MarkDynamic();

                    var numSides = vertices.Count/4;

                    var tris = new int[numSides*6];
                    var uvs = new Vector2[vertices.Count];

                    for (var i = 0; i < numSides; i++)
                    {
                        var v1 = i*4;
                        var v2 = v1 + 1;
                        var v3 = v1 + 2;
                        var v4 = v1 + 3;

                        tris[i*6] = v1;
                        tris[i*6+1] = v2;
                        tris[i*6+2] = v3;

                        tris[i*6+3] = v1;
                        tris[i*6+4] = v3;
                        tris[i*6+5] = v4;

                        uvs[i*4] = Vector2.zero;
                        uvs[i*4+1] = Vector2.up;
                        uvs[i*4+2] = Vector2.one;
                        uvs[i*4+3] = Vector2.right;
                    }

                    sharedMesh.vertices = vertices.ToArray();
                    sharedMesh.normals = normals.ToArray();
                    sharedMesh.triangles = tris;
                    sharedMesh.uv = uvs;
                }


                foreach (var node in vox.octree.DepthFirst())
                {
                    if (!node.IsLeafNode()) continue;

//                    var nodeBounds = node.GetBounds();
//
//                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//
//                    cube.transform.position = nodeBounds.center;
//                    cube.transform.localScale = nodeBounds.size;
//
//                    Undo.RegisterCreatedObjectUndo(cube, "Harp darp");
                    
//                    DrawBounds(nodeBounds, node.IsLeafNode() ? Color.red : Color.white);

                    //            var firstChildBounds = node.GetChildBounds(0);
                    //
                    //            DrawBounds(node.GetChildBounds(0), Color.red);
                    //            DrawBounds(node.GetChildBounds(1), Color.green);
                    //            DrawBounds(node.GetChildBounds(2), Color.blue);
                    //            DrawBounds(node.GetChildBounds(3), Color.yellow);
                }
                //                using (new MeshModification(meshFilter.sharedMesh, "Regenerate")) {
                //                    
                ////                    var vertices = new Vector3[vox.width * vox.height * vox.depth * 24];
                //
                //                    
                //                }
            }
        }
    }

    private void DrawBounds(Bounds bounds)
    {
        DrawBounds(bounds, Color.white);
    }

    private void DrawBounds(Bounds bounds, Color color)
    {
        var center = bounds.center;
        var extents = bounds.extents;

//        Debug.DrawLine(center - extents * 0.15f, center - extents * 0.125f, color);
//        Debug.DrawLine(center, center + extents, color);
//        return;

        var topBackLeft = center + new Vector3(-extents.x, extents.y, -extents.z);
        var topBackRight = topBackLeft + Vector3.right * extents.x * 2;
        var topFwdLeft = topBackLeft + Vector3.forward * extents.z * 2;
        var topFwdRight = topBackRight + Vector3.forward * extents.z * 2;

        var botBackLeft = topBackLeft + Vector3.down * extents.y * 2;
        var botBackRight = topBackRight + Vector3.down * extents.y * 2;
        var botFwdLeft = topFwdLeft + Vector3.down * extents.y * 2;
        var botFwdRight = topFwdRight + Vector3.down * extents.y * 2;

        Debug.DrawLine(topBackLeft, topBackRight, color);
        Debug.DrawLine(topBackRight, topFwdRight, color);
        Debug.DrawLine(topFwdRight, topFwdLeft, color);
        Debug.DrawLine(topFwdLeft, topBackLeft, color);

        Debug.DrawLine(topBackLeft, botBackLeft, color);
        Debug.DrawLine(topBackRight, botBackRight, color);
        Debug.DrawLine(topFwdRight, botFwdRight, color);
        Debug.DrawLine(topFwdLeft, botFwdLeft, color);

        Debug.DrawLine(botBackLeft,  botBackRight, color);
        Debug.DrawLine(botBackRight, botFwdRight, color);
        Debug.DrawLine(botFwdRight,  botFwdLeft, color);
        Debug.DrawLine(botFwdLeft,   botBackLeft, color);
    }

    public void OnSceneGUI()
    {
        var vox = target as Vox;

        if (vox == null) return;

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