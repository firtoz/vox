using System;
using UnityEditor;
using UnityEngine;

public class MeshModification : IDisposable
{
    private readonly MeshCache instance;

    public MeshModification(Mesh mesh, string undoMessage)
    {
        Debug.Log("Creating modification!");
        instance = ScriptableObject.CreateInstance<MeshCache>();
        instance.hideFlags = HideFlags.HideAndDontSave;
        Debug.Log("Assigned mesh!");
        instance.mesh = mesh;

        Undo.RegisterCreatedObjectUndo(instance, undoMessage);

        Debug.Log("Registering complete object undo!");
        Undo.RegisterCompleteObjectUndo(instance, undoMessage);
        Debug.Log("Registered complete object undo!");
    }

    public void Dispose()
    {
        Debug.Log("Disposing! Marking dirty!");
        EditorUtility.SetDirty(instance);

        Debug.Log("Destroying object!");
        Undo.DestroyObjectImmediate(instance);
        Debug.Log("object destroyed!");
    }
}