using UnityEngine;
using System.Linq;
using UnityEditor;
using Random = UnityEngine.Random;

[CustomEditor(typeof (Vox))]
public class VoxEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var vox = target as Vox;
        if (!vox) {
            return;
        }

        var meshFilter = vox.GetComponent<MeshFilter>();

        if (!meshFilter.sharedMesh) {
            if (GUILayout.Button("New Mesh")) {
                Undo.RecordObject(meshFilter, "add new mesh");
                meshFilter.sharedMesh = new Mesh {
                    triangles = new int[0]
                };
                EditorUtility.SetDirty(meshFilter);
            }
        } else {
            var voxMesh = meshFilter.sharedMesh;



            if (GUILayout.Button("clone")) {
                Undo.RecordObject(meshFilter, "Clone Mesh");
                meshFilter.sharedMesh = Instantiate(voxMesh);
                EditorUtility.SetDirty(meshFilter);
            }

            if (GUILayout.Button("add triangle")) {
                using (new MeshModification(voxMesh, "Add Triangle")) {
                    var a = Random.insideUnitSphere;
                    var b = Random.insideUnitSphere;
                    var c = Random.insideUnitSphere;

                    var aI = voxMesh.vertices.Length;
                    var bI = aI + 1;
                    var cI = aI + 2;

                    var normals = voxMesh.normals;

                    voxMesh.vertices = voxMesh.vertices.Concat(new[] { a, b, c }).ToArray();
                    voxMesh.normals = normals.Concat(new []{Vector3.one, Vector3.one, Vector3.one}).ToArray();
                    voxMesh.triangles = voxMesh.triangles.Concat(new[] { aI, bI, cI }).ToArray();
                }
            }
        }
    }
}