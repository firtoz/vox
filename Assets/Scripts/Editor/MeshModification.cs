using System;
using UnityEditor;
using UnityEngine;

public class MeshModification : IDisposable
{
    private readonly MeshCache instance;

    public MeshModification(Mesh mesh, string undoMessage)
    {
        instance = ScriptableObject.CreateInstance<MeshCache>();
        instance.hideFlags = HideFlags.HideAndDontSave;
        instance.mesh = mesh;

        Undo.RegisterCreatedObjectUndo(instance, undoMessage);

        Undo.RegisterCompleteObjectUndo(instance, undoMessage);
    }

    public void Dispose()
    {
        EditorUtility.SetDirty(instance);

        Undo.DestroyObjectImmediate(instance);
    }
}