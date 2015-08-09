using System;
using UnityEditor;
using UnityEngine;

public class MeshModification : IDisposable
{
    private readonly MeshCache _instance;

    public MeshModification(Mesh mesh, string undoMessage)
    {
        _instance = ScriptableObject.CreateInstance<MeshCache>();
        _instance.hideFlags = HideFlags.HideAndDontSave;
        _instance.mesh = mesh;

        Undo.RegisterCreatedObjectUndo(_instance, undoMessage);

        Undo.RegisterCompleteObjectUndo(_instance, undoMessage);
    }

    public void Dispose()
    {
        EditorUtility.SetDirty(_instance);

        Undo.DestroyObjectImmediate(_instance);
    }
}