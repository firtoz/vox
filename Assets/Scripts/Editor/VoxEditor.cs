using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Vox))]
public class VoxEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }
}
